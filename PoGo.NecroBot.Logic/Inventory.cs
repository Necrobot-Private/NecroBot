#region using directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Event.Inventory;
using PoGo.NecroBot.Logic.Exceptions;
using PoGo.NecroBot.Logic.Interfaces.Configuration;
using PoGo.NecroBot.Logic.Model.Settings;
using PoGo.NecroBot.Logic.PoGoUtils;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Utils;
using PokemonGo.RocketAPI;
using POGOProtos.Data;
using POGOProtos.Data.Player;
using POGOProtos.Enums;
using POGOProtos.Inventory;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Responses;
using POGOProtos.Settings.Master;
using PokemonGo.RocketAPI.Helpers;
using TinyIoC;
using static PoGo.NecroBot.Logic.Model.Settings.FilterUtil;
using POGOProtos.Settings.Master.Pokemon;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Common;

#endregion

namespace PoGo.NecroBot.Logic
{
    public class Inventory
    {
        private readonly Client _client;
        private readonly ILogicSettings _logicSettings;
        private GetPlayerResponse _player = null;
        private int _level = 0;
        private IEnumerable<PokemonSettings> _pokemonSettings = null;
        private DateTime _lastLuckyEggTime;
        private readonly List<ItemId> _revives = new List<ItemId> { ItemId.ItemRevive, ItemId.ItemMaxRevive };
        private ISession ownerSession;

        public async Task<Candy> GetCandyFamily(PokemonId id)
        {
            var setting = (await GetPokemonSettings().ConfigureAwait(false)).FirstOrDefault(x => x.PokemonId == id);
            var family = (await GetPokemonFamilies().ConfigureAwait(false)).FirstOrDefault(x => x.FamilyId == setting.FamilyId);

            if (family == null) return null;
            return family;
        }

        internal async Task<PokemonSettings> GetPokemonSetting(PokemonId pokemonId)
        {
            return (await GetPokemonSettings().ConfigureAwait(false)).FirstOrDefault(p => p.PokemonId == pokemonId);
        }

        public async Task<int> GetCandyCount(PokemonId id)
        {
            Candy candy = await GetCandyFamily(id).ConfigureAwait(false);
            if (candy != null)
                return candy.Candy_;
            return 0;
        }

        public Inventory(ISession session, Client client, ILogicSettings logicSettings,
            Action onUpdated = null)
        {
            ownerSession = session;
            _client = client;
            _logicSettings = logicSettings;
            // Inventory update will be called everytime GetMapObject is called.
            client.Inventory.OnInventoryUpdated += () =>
            {
                if (onUpdated != null && _player != null)
                {
                    onUpdated();
                }
            };
        }

        private readonly List<ItemId> _pokeballs = new List<ItemId>
        {
            ItemId.ItemPokeBall,
            ItemId.ItemGreatBall,
            ItemId.ItemUltraBall,
            ItemId.ItemMasterBall
        };

        private readonly List<ItemId> _potions = new List<ItemId>
        {
            ItemId.ItemPotion,
            ItemId.ItemSuperPotion,
            ItemId.ItemHyperPotion,
            ItemId.ItemMaxPotion
        };

        public async Task UpdateInventoryItem(ItemId itemId)
        {
            var inventory = await GetCachedInventory().ConfigureAwait(false);
            foreach (var item in inventory)
            {
                if (item.InventoryItemData != null
                    && item.InventoryItemData.Item != null
                    && item.InventoryItemData.Item.ItemId == itemId)
                {
                    ownerSession.EventDispatcher.Send(new InventoryItemUpdateEvent()
                    {
                        Item = item.InventoryItemData.Item
                    });
                }
            }
        }

        public async Task<LevelUpRewardsResponse> GetLevelUpRewards(Inventory inv)
        {
            return await GetLevelUpRewards((await inv.GetPlayerStats().ConfigureAwait(false)).FirstOrDefault().Level).ConfigureAwait(false);
        }

        public async Task<IEnumerable<InventoryItem>> GetCachedInventory()
        {
            if (_player == null)
            {
                _player = await GetPlayerData().ConfigureAwait(false);
            }
            return _client.Inventory.InventoryItems.Select(kvp => kvp.Value);
        }

        public async Task<IEnumerable<AppliedItems>> GetAppliedItems()
        {
            var inventory = await GetCachedInventory().ConfigureAwait(false);
            return inventory
                .Select(i => i.InventoryItemData?.AppliedItems)
                .Where(p => p != null);
        }

        public async Task<IEnumerable<PokemonData>> GetSlashedPokemonToTransfer()
        {
            var session = TinyIoCContainer.Current.Resolve<ISession>();
            var myPokemon = await GetPokemons().ConfigureAwait(false);
            var pokemonToTransfer = myPokemon.Where(p => p.IsBad);

            try
            {
                pokemonToTransfer =
                    pokemonToTransfer.Where(
                            p =>
                            {
                                var pokemonTransferFilter = session.LogicSettings.PokemonsTransferFilter.GetFilter<TransferFilter>(p.PokemonId);
                                return true;
                            })
                        .ToList();
            }
            catch (ActiveSwitchByRuleException e)
            {
                throw e;
            }
            return pokemonToTransfer.OrderBy(poke => poke.Cp);
        }

        public async Task<IEnumerable<PokemonData>> GetWeakPokemonToTransfer(
            IEnumerable<PokemonId> pokemonsNotToTransfer, Dictionary<PokemonId, EvolveFilter> pokemonEvolveFilters,
            bool keepPokemonsThatCanEvolve = false)
        {
            var session = TinyIoCContainer.Current.Resolve<ISession>();
            var myPokemon = await GetPokemons().ConfigureAwait(false);
            var pokemonToTransfer = myPokemon.Where(p => !pokemonsNotToTransfer.Contains(p.PokemonId) && CanTransferPokemon(p));

            try
            {
                pokemonToTransfer =
                    pokemonToTransfer.Where(
                            p =>
                            {
                                var pokemonTransferFilter = session.LogicSettings.PokemonsTransferFilter.GetFilter<TransferFilter>(p.PokemonId);

                                return !pokemonTransferFilter.MovesOperator.BoolFunc(
                                    pokemonTransferFilter.MovesOperator.ReverseBoolFunc(
                                        pokemonTransferFilter.MovesOperator.InverseBool(
                                            pokemonTransferFilter.Moves.Count > 0),
                                        pokemonTransferFilter.Moves.Any(moveset =>
                                            pokemonTransferFilter.MovesOperator.ReverseBoolFunc(
                                                pokemonTransferFilter.MovesOperator.InverseBool(moveset.Count > 0),
                                                moveset.Intersect(new[] { p.Move1, p.Move2 }).Count() ==
                                                Math.Max(Math.Min(moveset.Count, 2), 0)))),
                                    pokemonTransferFilter.KeepMinOperator.BoolFunc(
                                        p.Cp >= pokemonTransferFilter.KeepMinCp,
                                        PokemonInfo.CalculatePokemonPerfection(p) >=
                                        pokemonTransferFilter.KeepMinIvPercentage,
                                        pokemonTransferFilter.KeepMinOperator.ReverseBoolFunc(
                                            pokemonTransferFilter.KeepMinOperator.InverseBool(pokemonTransferFilter
                                                .UseKeepMinLvl),
                                            PokemonInfo.GetLevel(p) >= pokemonTransferFilter.KeepMinLvl)));
                            })
                        .ToList();
            }
            catch (ActiveSwitchByRuleException e)
            {
                throw e;
            }

            if (keepPokemonsThatCanEvolve)
            {
                var pokemonToEvolve = await session.Inventory.GetPokemonToEvolve(session.LogicSettings.PokemonEvolveFilters).ConfigureAwait(false);

                pokemonToTransfer = pokemonToTransfer.Where(pokemon => !pokemonToEvolve.Any(p => p.Id == pokemon.Id));
            }
            return pokemonToTransfer.OrderBy(poke => poke.Cp);
        }

        public async Task<IEnumerable<PokemonData>> GetDuplicatePokemonToTransfer(
            IEnumerable<PokemonId> pokemonsNotToTransfer, Dictionary<PokemonId, EvolveFilter> pokemonEvolveFilters,
            bool keepPokemonsThatCanEvolve = false, bool prioritizeIVoverCp = false
        )
        {
            var session = TinyIoCContainer.Current.Resolve<ISession>();
            var myPokemon = await GetPokemons().ConfigureAwait(false);
            var pokemonToTransfer = myPokemon.Where(p => !pokemonsNotToTransfer.Contains(p.PokemonId) && CanTransferPokemon(p));

            try
            {
                pokemonToTransfer =
                    pokemonToTransfer.Where(
                            p =>
                            {
                                var pokemonTransferFilter = session.LogicSettings.PokemonsTransferFilter.GetFilter<TransferFilter>(p.PokemonId);

                                return !pokemonTransferFilter.MovesOperator.BoolFunc(
                                    pokemonTransferFilter.MovesOperator.ReverseBoolFunc(
                                        pokemonTransferFilter.MovesOperator.InverseBool(
                                            pokemonTransferFilter.Moves.Count > 0),
                                        pokemonTransferFilter.Moves.Any(moveset =>
                                            pokemonTransferFilter.MovesOperator.ReverseBoolFunc(
                                                pokemonTransferFilter.MovesOperator.InverseBool(moveset.Count > 0),
                                                moveset.Intersect(new[] { p.Move1, p.Move2 }).Count() ==
                                                Math.Max(Math.Min(moveset.Count, 2), 0)))),
                                    pokemonTransferFilter.KeepMinOperator.BoolFunc(
                                        p.Cp >= pokemonTransferFilter.KeepMinCp,
                                        PokemonInfo.CalculatePokemonPerfection(p) >=
                                        pokemonTransferFilter.KeepMinIvPercentage,
                                        pokemonTransferFilter.KeepMinOperator.ReverseBoolFunc(
                                            pokemonTransferFilter.KeepMinOperator.InverseBool(pokemonTransferFilter
                                                .UseKeepMinLvl),
                                            PokemonInfo.GetLevel(p) >= pokemonTransferFilter.KeepMinLvl)));
                            })
                        .ToList();
            }
            catch (ActiveSwitchByRuleException e)
            {
                throw e;
            }

            var results = new List<PokemonData>();
            var pokemonToEvolve = await GetPokemonToEvolve(pokemonEvolveFilters).ConfigureAwait(false);

            foreach (var pokemonGroupToTransfer in pokemonToTransfer.GroupBy(p => p.PokemonId).ToList())
            {
                var amountToKeepInStorage = Math.Max(GetApplyFilter<TransferFilter>(session.LogicSettings.PokemonsTransferFilter, pokemonGroupToTransfer.Key).KeepMinDuplicatePokemon, 0);
                var inStorage = myPokemon.Count(data => data.PokemonId == pokemonGroupToTransfer.Key);
                var needToRemove = inStorage - amountToKeepInStorage;

                if (needToRemove <= 0)
                    continue;

                var weakPokemonCount = pokemonGroupToTransfer.Count();
                var canBeRemoved = Math.Min(needToRemove, weakPokemonCount);

                // Adjust canBeRemoved by subtracting the number of evolve pokemon we are saving.
                var pokemonToEvolveForThisGroup = pokemonToEvolve.Where(p => p.PokemonId == pokemonGroupToTransfer.Key);
                var numToSaveForEvolve = pokemonGroupToTransfer.Count(p => pokemonToEvolveForThisGroup.Any(p2 => p2.Id == p.Id));
                canBeRemoved -= numToSaveForEvolve;
                var PokemonId = session.Translation.GetPokemonTranslation(pokemonGroupToTransfer.Key);

                Logger.Write($"Saving {numToSaveForEvolve,2:#0} {PokemonId,-12} for evolve. Number of {PokemonId,-12} to be transferred: {canBeRemoved,2:#0}", Logic.Logging.LogLevel.Info);

                if (prioritizeIVoverCp)
                {
                    results.AddRange(pokemonGroupToTransfer
                        .Where(p => !pokemonToEvolveForThisGroup.Any(p2 => p2.Id == p.Id)) // Remove pokemon to evolve from the transfer list.
                        .OrderBy(PokemonInfo.CalculatePokemonPerfection)
                        .ThenBy(n => n.Cp)
                        .Take(canBeRemoved));
                }
                else
                {
                    results.AddRange(pokemonGroupToTransfer
                        .Where(p => !pokemonToEvolveForThisGroup.Any(p2 => p2.Id == p.Id)) // Remove pokemon to evolve from the transfer list.
                        .OrderBy(x => x.Cp)
                        .ThenBy(PokemonInfo.CalculatePokemonPerfection)
                        .Take(canBeRemoved));
                }
            }
            return results;
        }

        public async Task<IEnumerable<PokemonData>> GetMaxPokemonToTransfer(
            IEnumerable<PokemonId> pokemonsNotToTransfer, bool prioritizeIVoverCp = false)
        {
            var session = TinyIoCContainer.Current.Resolve<ISession>();
            var myPokemon = await GetPokemons().ConfigureAwait(false);
            var transferrablePokemon = myPokemon.Where(p => !pokemonsNotToTransfer.Contains(p.PokemonId) && CanTransferPokemon(p));
            var results = new List<PokemonData>();

            foreach (var pokemonGroupToTransfer in transferrablePokemon.GroupBy(p => p.PokemonId).ToList())
            {
                var amountToKeepInStorage = GetApplyFilter<TransferFilter>(session.LogicSettings.PokemonsTransferFilter, pokemonGroupToTransfer.Key).KeepMaxDuplicatePokemon;

                if (amountToKeepInStorage < 0)
                    continue;

                var inStorage = pokemonGroupToTransfer.Count();
                if (amountToKeepInStorage >= inStorage)
                    continue;

                var needToRemove = inStorage - amountToKeepInStorage;

                Logger.Write($"Duplicate Pokemon Allowed: {amountToKeepInStorage,2:0}. Transferring {needToRemove,2:0} {pokemonGroupToTransfer.Key} now.", Logic.Logging.LogLevel.Info);

                if (prioritizeIVoverCp)
                {
                    results.AddRange(pokemonGroupToTransfer
                        .OrderBy(PokemonInfo.CalculatePokemonPerfection)
                        .ThenBy(n => n.Cp)
                        .Take(needToRemove));
                }
                else
                {
                    results.AddRange(pokemonGroupToTransfer
                        .OrderBy(x => x.Cp)
                        .ThenBy(PokemonInfo.CalculatePokemonPerfection)
                        .Take(needToRemove));
                }
            }
            return results;
        }

        public class EvolutionCalculations
        {
            public int Transfers { get; set; }
            public int Evolves { get; set; }
            public int CandiesLeft { get; set; }
            public int PokemonLeft { get; set; }
        }

        // Calculates the number of pokemon evolutions possible given number of pokemon, candies, and candies to evolve.
        // Implementation is taken from https://www.pidgeycalc.com and double-checked with calculator at https://pokeassistant.com/main/pidgeyspam
        public EvolutionCalculations CalculatePokemonEvolution(int pokemonLeft, int candiesLeft, int candiesToEvolve, int candiesGainedOnEvolve)
        {
            int transferCandiesGained = 1;
            int evolveCount = 0;
            int transferCount = 0;

            // Evolutions without transferring
            while (true)
            {
                // Not enough Pokemon or candies
                if (candiesLeft / candiesToEvolve == 0 || pokemonLeft == 0)
                {
                    break;
                }
                else
                {
                    // Evolve a Pokemon
                    pokemonLeft--;
                    candiesLeft -= candiesToEvolve;
                    candiesLeft += candiesGainedOnEvolve;
                    evolveCount++;
                    // Break if out of Pokemon
                    if (pokemonLeft == 0)
                    {
                        break;
                    }
                }
            }

            // Evolutions after transferring
            while (true)
            {
                // Not enough Pokemon or candies
                if ((candiesLeft + (pokemonLeft * transferCandiesGained)) < (candiesToEvolve + transferCandiesGained) || pokemonLeft == 0)
                {
                    break;
                }

                // Keep transferring until enough candies for an evolve
                while (candiesLeft < candiesToEvolve)
                {
                    transferCount++;
                    pokemonLeft--;
                    candiesLeft += transferCandiesGained;
                }

                // Evolve a Pokemon
                pokemonLeft--;
                candiesLeft -= candiesToEvolve;
                candiesLeft += candiesGainedOnEvolve;
                evolveCount++;
            }

            return new EvolutionCalculations
            {
                Transfers = transferCount,
                Evolves = evolveCount,
                CandiesLeft = candiesLeft,
                PokemonLeft = pokemonLeft
            };
        }

        public async Task<IEnumerable<EggIncubator>> GetEggIncubators()
        {
            var inventory = await GetCachedInventory().ConfigureAwait(false);
            return
                inventory
                    .Where(x => x.InventoryItemData.EggIncubators != null)
                    .SelectMany(i => i.InventoryItemData.EggIncubators.EggIncubator)
                    .Where(i => i != null);
        }

        public async Task<IEnumerable<PokemonData>> GetEggs()
        {
            var inventory = await GetCachedInventory().ConfigureAwait(false);
            return
                inventory.Select(i => i.InventoryItemData?.PokemonData)
                    .Where(p => p != null && p.IsEgg);
        }

        public async Task<PokemonData> GetHighestPokemonOfTypeByCp(PokemonData pokemon)
        {
            var myPokemon = await GetPokemons().ConfigureAwait(false);
            if (myPokemon != null)
            {
                var pokemons = myPokemon.ToList();
                if (pokemons != null && 0 < pokemons.Count)
                {
                    return pokemons.Where(x => x.PokemonId == pokemon.PokemonId)
                        .OrderByDescending(x => x.Cp)
                        .FirstOrDefault();
                }
            }
            return null;
        }

        public int UpdateStarDust(int startdust)
        {
            GetPlayerData().Wait();
            _player.PlayerData.Currencies[1].Amount += startdust;

            return _player.PlayerData.Currencies[1].Amount;
        }

        public int GetStarDust()
        {
            GetPlayerData().Wait();
            return _player.PlayerData.Currencies[1].Amount;
        }

        public async Task<GetPlayerResponse> GetPlayerData()
        {
            if (_player == null)
            {
                _player = await _client.Player.GetPlayer().ConfigureAwait(false);
            }
            return _player;
        }

        public async Task<PokemonData> GetHighestPokemonOfTypeByIv(PokemonData pokemon)
        {
            var myPokemon = await GetPokemons().ConfigureAwait(false);
            if (myPokemon != null)
            {
                var pokemons = myPokemon.ToList();
                if (pokemons != null && 0 < pokemons.Count)
                {
                    return pokemons.Where(x => x.PokemonId == pokemon.PokemonId)
                        .OrderByDescending(PokemonInfo.CalculatePokemonPerfection)
                        .FirstOrDefault();
                }
            }
            return null;
        }

        public async Task<IEnumerable<PokemonData>> GetHighestsCp(int limit)
        {
            var myPokemon = await GetPokemons().ConfigureAwait(false);
            var pokemons = myPokemon.ToList();
            return pokemons.OrderByDescending(x => x.Cp).ThenBy(n => n.StaminaMax).Take(limit);
        }

        public async Task<IEnumerable<PokemonData>> GetHighestCpForGym(int limit)
        {
            var myPokemon = await GetPokemons().ConfigureAwait(false);
            // var pokemons = myPokemon.Where(i => !i.DeployedFortId.Any() && i.Stamina == i.StaminaMax);
            var pokemons = myPokemon.Where(i => !i.DeployedFortId.Any());
            return pokemons.OrderByDescending(x => x.Cp).ThenBy(n => n.StaminaMax).Take(limit);
        }

        public async Task<IEnumerable<PokemonData>> GetHighestsPerfect(int limit)
        {
            var myPokemon = await GetPokemons().ConfigureAwait(false);
            var pokemons = myPokemon.ToList();
            return pokemons.OrderByDescending(PokemonInfo.CalculatePokemonPerfection).Take(limit);
        }

        public async Task<int> GetItemAmountByType(ItemId type)
        {
            var pokeballs = await GetItems().ConfigureAwait(false);
            return pokeballs.FirstOrDefault(i => i.ItemId == type)?.Count ?? 0;
        }

        public async Task<IEnumerable<ItemData>> GetItems()
        {
            var inventory = await GetCachedInventory().ConfigureAwait(false);
            return inventory
                .Select(i => i.InventoryItemData?.Item)
                .Where(p => p != null);
        }

        public async Task<int> GetTotalItemCount()
        {
            var myItems = (await GetItems().ConfigureAwait(false)).ToList();
            int myItemCount = 0;
            foreach (var myItem in myItems) myItemCount += myItem.Count;
            return myItemCount;
        }

        public async Task<IEnumerable<ItemData>> GetItemsToRecycle(ISession session)
        {
            var itemsToRecycle = new List<ItemData>();
            var myItems = (await GetItems().ConfigureAwait(false)).ToList();
            if (myItems == null)
                return itemsToRecycle;

            var otherItemsToRecycle = myItems
                .Where(x => _logicSettings.ItemRecycleFilter.Any(f => f.Key == x.ItemId && x.Count > f.Value))
                .Select(
                    x =>
                        new ItemData
                        {
                            ItemId = x.ItemId,
                            Count = x.Count - _logicSettings.ItemRecycleFilter.FirstOrDefault(f => f.Key == x.ItemId).Value,
                            Unseen = x.Unseen
                        });

            itemsToRecycle.AddRange(otherItemsToRecycle);

            return itemsToRecycle;
        }

        public double GetPerfect(PokemonData poke)
        {
            var result = PokemonInfo.CalculatePokemonPerfection(poke);
            return result;
        }

        public async Task<IEnumerable<PlayerStats>> GetPlayerStats()
        {
            var inventory = await GetCachedInventory().ConfigureAwait(false);
            return inventory
                .Select(i => i.InventoryItemData?.PlayerStats)
                .Where(p => p != null);
        }

        public async Task UseLuckyEgg()
        {
            var inventoryContent = await ownerSession.Inventory.GetItems().ConfigureAwait(false);
            var luckyEgg = inventoryContent.FirstOrDefault(p => p.ItemId == ItemId.ItemLuckyEgg);

            if (luckyEgg == null || luckyEgg.Count == 0) // We tried to use egg but we don't have any more. Just return.
            {
                Logger.Write(ownerSession.Translation.GetTranslation(TranslationString.NoEggsAvailable));
                return;
            }

            if (_lastLuckyEggTime.AddMinutes(30).Ticks > DateTime.Now.Ticks)
            {
                TimeSpan duration = _lastLuckyEggTime.AddMinutes(30) - DateTime.Now;
                Logger.Write(ownerSession.Translation.GetTranslation(TranslationString.UseLuckyEggActive, duration.Minutes, duration.Seconds));
                return;
            }

            var responseLuckyEgg = await ownerSession.Client.Inventory.UseItemXpBoost().ConfigureAwait(false);
            switch (responseLuckyEgg.Result)
            {
                case UseItemXpBoostResponse.Types.Result.Success:
                    {
                        _lastLuckyEggTime = DateTime.Now;
                        ownerSession.EventDispatcher.Send(new UseLuckyEggEvent { Count = luckyEgg.Count - 1 });
                        TinyIoCContainer.Current.Resolve<MultiAccountManager>().DisableSwitchAccountUntil(DateTime.Now.AddMinutes(30));
                        Logger.Write(ownerSession.Translation.GetTranslation(TranslationString.UsedLuckyEgg));
                    }
                    break;
                case UseItemXpBoostResponse.Types.Result.ErrorNoItemsRemaining:
                    {
                        Logger.Write(ownerSession.Translation.GetTranslation(TranslationString.NoEggsAvailable));
                    }
                    break;
                case UseItemXpBoostResponse.Types.Result.ErrorXpBoostAlreadyActive:
                    {
                        TimeSpan duration = _lastLuckyEggTime.AddMinutes(30) - DateTime.Now;
                        Logger.Write(ownerSession.Translation.GetTranslation(TranslationString.UseLuckyEggActive, duration.Minutes, duration.Seconds));
                    }
                    break;
                default:
                    Logger.Write($"Failed to use a Lucky Egg!", LogLevel.Error);
                    break;
            }
            await DelayingUtils.DelayAsync(ownerSession.LogicSettings.DelayBetweenPlayerActions, 0, ownerSession.CancellationTokenSource.Token).ConfigureAwait(false);
        }

        public async Task<UseIncenseResponse> UseIncenseConstantly()
        {
            var UseIncense = await _client.Inventory.UseIncense(ItemId.ItemIncenseOrdinary).ConfigureAwait(false);
            return UseIncense;
        }

        public async Task<List<InventoryItem>> GetPokeDexItems()
        {
            var inventory = await GetCachedInventory().ConfigureAwait(false);

            return (from items in inventory
                    where items.InventoryItemData?.PokedexEntry != null
                    select items).ToList();
        }

        public async Task<List<Candy>> GetPokemonFamilies(int retries = 0)
        {
            if (retries > 3) return null;

            IEnumerable<Candy> families = null;
            var inventory = await GetCachedInventory().ConfigureAwait(false);

            try
            {
                families = from item in inventory
                           where item.InventoryItemData?.Candy != null
                           where item.InventoryItemData?.Candy.FamilyId != PokemonFamilyId.FamilyUnset
                           group item by item.InventoryItemData?.Candy.FamilyId
                    into family
                           select new Candy
                           {
                               FamilyId = family.First().InventoryItemData.Candy.FamilyId,
                               Candy_ = family.First().InventoryItemData.Candy.Candy_
                           };
            }
            catch (NullReferenceException)
            {
                await DelayingUtils.DelayAsync(3000, 3000, ownerSession.CancellationTokenSource.Token).ConfigureAwait(false);
                return await GetPokemonFamilies(++retries).ConfigureAwait(false);
            }
            return families.ToList();
        }

        public async Task<PokemonData> GetSinglePokemon(ulong id)
        {
            var inventory = await GetCachedInventory().ConfigureAwait(false);
            return
                inventory.Select(i => i.InventoryItemData?.PokemonData)
                    .FirstOrDefault(p => p != null && p.PokemonId > 0 && p.Id == id);
        }

        public async Task<IEnumerable<PokemonData>> GetPokemons()
        {
            var inventory = await GetCachedInventory().ConfigureAwait(false);
            return inventory
                    .Select(i => i.InventoryItemData?.PokemonData)
                    .Where(p => p != null && p.PokemonId > 0);
        }

        public async Task<IEnumerable<PokemonData>> GetFavoritePokemons()
        {
            var inventory = await GetPokemons().ConfigureAwait(false);
            return inventory.Where(i => i.Favorite == 1);
        }

        public async Task<IEnumerable<PokemonData>> GetDeployedPokemons()
        {
            var inventory = await GetPokemons().ConfigureAwait(false);
            return inventory.Where(i => !string.IsNullOrEmpty(i.DeployedFortId));
        }

        public async Task<MoveSettings> GetMoveSetting(PokemonMove move)
        {
            if (_client.Download.ItemTemplates == null)
                await GetPokemonSettings().ConfigureAwait(false);

            var moveSettings = _client.Download.ItemTemplates.Where(x => x.MoveSettings != null)
                .Select(x => x.MoveSettings)
                .ToList();

            return moveSettings.FirstOrDefault(x => x.MovementId == move);
        }

        public async Task<IEnumerable<PokemonSettings>> GetPokemonSettings()
        {
            if (_client.Download.ItemTemplates == null)
                await _client.Download.GetItemTemplates().ConfigureAwait(false);

            if (_pokemonSettings == null)
            {
                var moveSettings = _client.Download.ItemTemplates.Where(x => x.MoveSettings != null)
                    .Select(x => x.MoveSettings)
                    .ToList();

                _pokemonSettings = _client.Download.ItemTemplates.Select(i => i.PokemonSettings)
                    .Where(p => p != null && p.FamilyId != PokemonFamilyId.FamilyUnset);
            }
            return _pokemonSettings;
        }

        public async Task<IEnumerable<MoveSettings>> GetMoveSettings()
        {
            if (_client.Download.ItemTemplates == null)
                await _client.Download.GetItemTemplates().ConfigureAwait(false);

            var moveSettings = _client.Download.ItemTemplates.Where(x => x.MoveSettings != null)
                .Select(x => x.MoveSettings);

            return moveSettings;
        }

        public bool CanTransferPokemon(PokemonData pokemon)
        {
            // Can't transfer pokemon in gyms.
            if (!string.IsNullOrEmpty(pokemon.DeployedFortId))
                return false;

            // Can't transfer buddy pokemon
            var buddy = ownerSession.Profile.PlayerData.BuddyPokemon;
            if (buddy != null && buddy.Id == pokemon.Id)
                return false;

            // Can't transfer favorite
            if (pokemon.Favorite == 1)
                return false;

            return true;
        }

        public int GetCandyToEvolve(PokemonSettings settings, EvolveFilter appliedFilter = null)
        {
            EvolutionBranch branch;
            PokemonId evolveTo = PokemonId.Missingno;
            if (appliedFilter != null && !string.IsNullOrEmpty(appliedFilter.EvolveTo) && Enum.TryParse<PokemonId>(appliedFilter.EvolveTo, true, out evolveTo))
            {
                branch = settings.EvolutionBranch.FirstOrDefault(x => x.Evolution == evolveTo);
            }
            else
            {
                branch = settings.EvolutionBranch.FirstOrDefault(x => x.EvolutionItemRequirement == ItemId.ItemUnknown);
            }

            if (branch == null)
                return -1;

            return branch.CandyCost;
        }

        public async Task<bool> CanEvolvePokemon(PokemonData pokemon, EvolveFilter appliedFilter = null, bool checkEvolveFilterRequirements = false)
        {
            // Can't evolve pokemon in gyms.
            if (!string.IsNullOrEmpty(pokemon.DeployedFortId))
                return false;

            IEnumerable<PokemonSettings> pokemonSettings = await GetPokemonSettings().ConfigureAwait(false);
            var settings = pokemonSettings.SingleOrDefault(x => x.PokemonId == pokemon.PokemonId);

            // Can't evolve pokemon that are not evolvable.
            if (settings.EvolutionIds.Count == 0 && settings.EvolutionBranch.Count == 0)
                return false;

            int familyCandy = await GetCandyCount(pokemon.PokemonId).ConfigureAwait(false);

            if (checkEvolveFilterRequirements)
            {
                if (!appliedFilter.Operator.BoolFunc(
                    appliedFilter.MinIV <= pokemon.Perfection(),
                    appliedFilter.MinLV <= pokemon.Level(),
                    appliedFilter.MinCP <= pokemon.CP(),
                    (appliedFilter.Moves == null ||
                    appliedFilter.Moves.Count == 0 ||
                    appliedFilter.Moves.Any(x => x[0] == pokemon.Move1 && x[1] == pokemon.Move2)
                    )
                ))
                {
                    return false;
                }

                if (appliedFilter.MinCandiesBeforeEvolve > 0 && familyCandy < appliedFilter.MinCandiesBeforeEvolve)
                    return false;
            }

            PokemonId evolveTo = PokemonId.Missingno;
            if (appliedFilter != null && !string.IsNullOrEmpty(appliedFilter.EvolveTo) && Enum.TryParse<PokemonId>(appliedFilter.EvolveTo, true, out evolveTo))
            {
                var branch = settings.EvolutionBranch.FirstOrDefault(x => x.Evolution == evolveTo);
                if (branch == null)
                    return false; //wrong setting, do not evolve this pokemon

                if (branch.EvolutionItemRequirement != ItemId.ItemUnknown)
                {
                    var itemCount = (await GetItems().ConfigureAwait(false)).Count(x => x.ItemId == branch.EvolutionItemRequirement);

                    if (itemCount == 0)
                        return false;
                }

                if (familyCandy < branch.CandyCost)
                    return false;
            }
            else
            {
                bool canEvolve = false;
                // Check requirements for all branches, if we meet the requirements for any of them then we return true.
                foreach (var branch in settings.EvolutionBranch)
                {
                    var itemCount = (await GetItems().ConfigureAwait(false)).Count(x => x.ItemId == branch.EvolutionItemRequirement);
                    var Candies2Evolve = branch.CandyCost; // GetCandyToEvolve(settings);
                    var Evolutions = familyCandy / Candies2Evolve;

                    if (branch.EvolutionItemRequirement != ItemId.ItemUnknown)
                    {
                        if (itemCount == 0)
                            continue;  // Cannot evolve so check next branch
                    }

                    if (familyCandy < branch.CandyCost)
                        continue;  // Cannot evolve so check next branch

                    // If we got here, then we can evolve so break out of loop.
                    canEvolve = true;
                }
                return canEvolve;
            }
            return true;
        }

        public async Task<IEnumerable<PokemonData>> GetPokemonToEvolve(Dictionary<PokemonId, EvolveFilter> filters = null)
        {
            var buddy = (await GetPlayerData().ConfigureAwait(false)).PlayerData.BuddyPokemon?.Id;

            IEnumerable<PokemonData> myPokemons = (await GetPokemons().ConfigureAwait(false)).OrderByDescending(p => p.Cp);

            List<PokemonData> possibleEvolvePokemons = new List<PokemonData>();

            foreach (var pokemon in myPokemons)
            {
                if (!filters.ContainsKey(pokemon.PokemonId)) continue;
                var filter = filters[pokemon.PokemonId];

                if (await CanEvolvePokemon(pokemon, filter, true).ConfigureAwait(false))
                {
                    possibleEvolvePokemons.Add(pokemon);
                }
            }

            var pokemonToEvolve = new List<PokemonData>();

            // Group pokemon by their PokemonId
            var groupedPokemons = possibleEvolvePokemons.GroupBy(p => p.PokemonId);

            foreach (var g in groupedPokemons)
            {
                PokemonId pokemonId = g.Key;

                var orderedGroup = g.OrderByDescending(p => p.Cp);

                //if (!filters.ContainsKey(pokemon.PokemonId)) continue;
                var filter = filters[pokemonId];

                int candiesLeft = await GetCandyCount(pokemonId).ConfigureAwait(false);
                PokemonSettings settings = (await GetPokemonSettings().ConfigureAwait(false)).FirstOrDefault(x => x.PokemonId == pokemonId);
                int pokemonLeft = orderedGroup.Count();
                int candyNeed = GetCandyToEvolve(settings, filter);

                if (candyNeed == -1)
                    continue; // If we were unable to determine which branch to use, then skip this pokemon.

                // Calculate the number of evolutions possible (taking into account +1 candy for evolve and +1 candy for transfer)
                EvolutionCalculations evolutionInfo = CalculatePokemonEvolution(pokemonLeft, candiesLeft, candyNeed, 1);

                if (evolutionInfo.Evolves > 0)
                {
                    // Add only the number of pokemon we can evolve.
                    pokemonToEvolve.AddRange(orderedGroup.Take(evolutionInfo.Evolves).ToList());
                }
            }
            return pokemonToEvolve;
        }

        public async Task<LevelUpRewardsResponse> GetLevelUpRewards(int level)
        {
            var rewards = new LevelUpRewardsResponse();
            if (_level == 0 || level > _level)
            {
                _level = level;

                rewards = await _client.Player.GetLevelUpRewards(level).ConfigureAwait(false);
                foreach (var item in rewards.ItemsAwarded)
                {
                    await UpdateInventoryItem(item.ItemId).ConfigureAwait(false);
                }
            }
            return rewards;
        }

        public async Task<bool> CanUpgradePokemon(PokemonData pokemon)
        {
            // Can't upgrade pokemon in gyms.
            if (!string.IsNullOrEmpty(pokemon.DeployedFortId))
                return false;

            var playerLevel = (await GetPlayerStats().ConfigureAwait(false)).FirstOrDefault().Level;
            var pokemonLevel = PokemonInfo.GetLevel(pokemon);

            // Can't evolve unless pokemon level is lower than trainer.
            if (pokemonLevel >= playerLevel + 2)
                return false;

            int familyCandy = await GetCandyCount(pokemon.PokemonId).ConfigureAwait(false);

            // Can't evolve if not enough candy.
            int pokemonCandyNeededAlready = PokemonCpUtils.GetCandyCostsForPowerup(pokemon.CpMultiplier + pokemon.AdditionalCpMultiplier);
            if (familyCandy < pokemonCandyNeededAlready)
                return false;

            // Can't evolve if not enough stardust.
            var stardustToUpgrade = PokemonCpUtils.GetStardustCostsForPowerup(pokemon.CpMultiplier + pokemon.AdditionalCpMultiplier);
            if (GetStarDust() < stardustToUpgrade)
                return false;

            return true;
        }

        public async Task<List<PokemonData>> GetPokemonToUpgrade()
        {
            var upgradePokemon = new List<PokemonData>();

            if (!_logicSettings.AutomaticallyLevelUpPokemon)
                return upgradePokemon;

            List<PokemonData> upgradeablePokemons = new List<PokemonData>();
            var pokemons = await GetPokemons().ConfigureAwait(false);
            foreach (var pokemon in pokemons)
            {
                if (await CanUpgradePokemon(pokemon).ConfigureAwait(false))
                    upgradeablePokemons.Add(pokemon);
            }

            foreach (var pokemon in upgradeablePokemons)
            {
                var appliedFilter = _logicSettings.PokemonUpgradeFilters.GetFilter<UpgradeFilter>(pokemon.PokemonId);

                if ((appliedFilter.OnlyUpgradeFavorites && pokemon.Favorite == 1) ||
                     (!appliedFilter.OnlyUpgradeFavorites &&
                        appliedFilter.UpgradePokemonMinimumStatsOperator.BoolFunc(
                        pokemon.CP() >= appliedFilter.UpgradePokemonCpMinimum,
                        pokemon.Level() >= appliedFilter.UpgradePokemonLvlMinimum,
                        pokemon.Perfection() >= appliedFilter.UpgradePokemonIvMinimum,
                        ((appliedFilter.UpgradePokemonMinimumStatsOperator == "and" && (appliedFilter.Moves == null || appliedFilter.Moves.Count == 0)) ||
                        (appliedFilter.Moves != null && appliedFilter.Moves.Count > 0 && appliedFilter.Moves.Any(x => x[0] == pokemon.Move1 && x[1] == pokemon.Move2))
                    ))))
                {
                    upgradePokemon.Add(pokemon);
                }
            }
            return upgradePokemon;
        }

        public async Task<UpgradePokemonResponse> UpgradePokemon(ulong pokemonid)
        {
            return await _client.Inventory.UpgradePokemon(pokemonid).ConfigureAwait(false);
        }
    }
}
