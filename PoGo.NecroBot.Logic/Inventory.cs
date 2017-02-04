#region using directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caching;
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
using System.Collections.Concurrent;

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

        private readonly List<ItemId> _revives = new List<ItemId> { ItemId.ItemRevive, ItemId.ItemMaxRevive };
        private ISession ownerSession;

        public Candy GetCandyFamily(PokemonId id)
        {
            var setting = GetPokemonSettings().Result.FirstOrDefault(x => x.PokemonId == id);
            var family = GetPokemonFamilies().Result.FirstOrDefault(x => x.FamilyId == setting.FamilyId);

            if (family == null) return null;
            return family;
        }

        public int GetCandyCount(PokemonId id)
        {
            Candy candy = GetCandyFamily(id);
            if (candy != null)
                return candy.Candy_;
            return 0;
        }

        public Inventory(ISession session, Client client, ILogicSettings logicSettings,
            Action onUpdated = null)
        {
            this.ownerSession = session;
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
            await Task.Run(() =>
            {
                foreach (var item in GetCachedInventory())
                {
                    if (item.InventoryItemData != null
                        && item.InventoryItemData.Item != null
                        && item.InventoryItemData.Item.ItemId == itemId)
                    {
                        this.ownerSession.EventDispatcher.Send(new InventoryItemUpdateEvent()
                        {
                            Item = item.InventoryItemData.Item
                        });
                    }
                }
            });
        }

        public async Task<LevelUpRewardsResponse> GetLevelUpRewards(Inventory inv)
        {
            return await GetLevelUpRewards(inv.GetPlayerStats().FirstOrDefault().Level);
        }

        public IEnumerable<InventoryItem> GetCachedInventory()
        {
            lock (_player)
            {
                if (_player == null)
                {
                    _player = GetPlayerData().Result;
                }
            }

            return _client.Inventory.InventoryItems.Select(kvp => kvp.Value);
        }

        public IEnumerable<AppliedItems> GetAppliedItems()
        {
            var inventory = GetCachedInventory();
            return inventory
                .Select(i => i.InventoryItemData?.AppliedItems)
                .Where(p => p != null);
        }

        public async Task<IEnumerable<PokemonData>> GetDuplicatePokemonToTransfer(
            IEnumerable<PokemonId> pokemonsNotToTransfer, IEnumerable<PokemonId> pokemonsToEvolve,
            bool keepPokemonsThatCanEvolve = false, bool prioritizeIVoverCp = false
        )
        {
            var myPokemon = GetPokemons();

            var pokemonToTransfer = myPokemon
                .Where(p => !pokemonsNotToTransfer.Contains(p.PokemonId) && p.DeployedFortId == string.Empty &&
                            p.Favorite == 0 && p.BuddyTotalKmWalked == 0)
                .ToList();

            try
            {
                pokemonToTransfer =
                    pokemonToTransfer.Where(
                            p =>
                            {
                                var pokemonTransferFilter = GetPokemonTransferFilter(p.PokemonId);

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
            catch (Exception)
            {
                //throw e; 
            }

            var myPokemonSettings = await GetPokemonSettings();
            var pokemonSettings = myPokemonSettings.ToList();

            var myPokemonFamilies = await GetPokemonFamilies();
            var pokemonFamilies = myPokemonFamilies.ToArray();

            var results = new List<PokemonData>();

            foreach (var pokemonGroupToTransfer in pokemonToTransfer.GroupBy(p => p.PokemonId).ToList())
            {
                var amountToKeepInStorage = Math.Max(GetPokemonTransferFilter(pokemonGroupToTransfer.Key).KeepMinDuplicatePokemon, 0);

                var inStorage = myPokemon.Count(data => data.PokemonId == pokemonGroupToTransfer.Key);
                var needToRemove = inStorage - amountToKeepInStorage;

                if (needToRemove <= 0)
                    continue;

                var weakPokemonCount = pokemonGroupToTransfer.Count();
                var canBeRemoved = Math.Min(needToRemove, weakPokemonCount);

                var settings = pokemonSettings.FirstOrDefault(x => x.PokemonId == pokemonGroupToTransfer.Key);

                if(settings != null && 
                    pokemonsToEvolve.Contains(pokemonGroupToTransfer.Key) &&
                    settings.CandyToEvolve > 0 &&
                    settings.EvolutionIds.Count != 0)
                {
                    var familyCandy = pokemonFamilies.FirstOrDefault(x => settings.FamilyId == x.FamilyId);
                    if (familyCandy != null)
                    {
                        // Calculate the number of evolutions possible (taking into account +1 candy for evolve and +1 candy for transfer)
                        var evolutionCalcs = CalcuatePokemonEvolution(canBeRemoved, familyCandy.Candy_, settings.CandyToEvolve);

                        // Subtract the number of evolutions possible from the number that can be transferred.
                        canBeRemoved -= evolutionCalcs.Evolves;
                    }
                }

                if (canBeRemoved <= 0)
                    continue;

                if (prioritizeIVoverCp)
                {
                    results.AddRange(pokemonGroupToTransfer
                        .OrderBy(PokemonInfo.CalculatePokemonPerfection)
                        .ThenBy(n => n.Cp)
                        .Take(canBeRemoved));
                }
                else
                {
                    results.AddRange(pokemonGroupToTransfer
                        .OrderBy(x => x.Cp)
                        .ThenBy(PokemonInfo.CalculatePokemonPerfection)
                        .Take(canBeRemoved));
                }
            }

            #region For testing

            /*
                        results.ForEach(data =>
                        {
                            var allpokemonoftype = myPokemonList.Where(x => x.PokemonId == data.PokemonId);
                            var bestPokemonOfType = 
                                (_logicSettings.PrioritizeIvOverCp
                                     ? allpokemonoftype
                                    .OrderByDescending(PokemonInfo.CalculatePokemonPerfection)
                                    .FirstOrDefault()
                                     : allpokemonoftype
                                    .OrderByDescending(x => x.Cp)
                                    .FirstOrDefault()) 
                                ?? data;

                            var perfection = PokemonInfo.CalculatePokemonPerfection(data);
                            var cp = data.Cp;

                            var bestPerfection = PokemonInfo.CalculatePokemonPerfection(bestPokemonOfType);
                            var bestCp = bestPokemonOfType.Cp;
                        });
            */

            #endregion

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
        public EvolutionCalculations CalcuatePokemonEvolution(int pokemonLeft, int candiesLeft, int candiesToEvolve)
        {
            int transferCandiesGained = 1;
            int candiesGainedOnEvolve = candiesToEvolve;
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

        public IEnumerable<EggIncubator> GetEggIncubators()
        {
            var inventory = GetCachedInventory();
            return
                inventory
                    .Where(x => x.InventoryItemData.EggIncubators != null)
                    .SelectMany(i => i.InventoryItemData.EggIncubators.EggIncubator)
                    .Where(i => i != null);
        }

        public IEnumerable<PokemonData> GetEggs()
        {
            var inventory = GetCachedInventory();
            return
                inventory.Select(i => i.InventoryItemData?.PokemonData)
                    .Where(p => p != null && p.IsEgg);
        }

        public PokemonData GetHighestPokemonOfTypeByCp(PokemonData pokemon)
        {
            var myPokemon = GetPokemons();
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
                _player = await _client.Player.GetPlayer();
            }

            return _player;
        }

        public PokemonData GetHighestPokemonOfTypeByIv(PokemonData pokemon)
        {
            var myPokemon = GetPokemons();
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

        public IEnumerable<PokemonData> GetHighestsCp(int limit)
        {
            var myPokemon = GetPokemons();
            var pokemons = myPokemon.ToList();
            return pokemons.OrderByDescending(x => x.Cp).ThenBy(n => n.StaminaMax).Take(limit);
        }

        public IEnumerable<PokemonData> GetHighestCpForGym(int limit)
        {
            var myPokemon = GetPokemons();
            // var pokemons = myPokemon.Where(i => !i.DeployedFortId.Any() && i.Stamina == i.StaminaMax);
            var pokemons = myPokemon.Where(i => !i.DeployedFortId.Any());
            return pokemons.OrderByDescending(x => x.Cp).ThenBy(n => n.StaminaMax).Take(limit);
        }

        public IEnumerable<PokemonData> GetHighestsPerfect(int limit)
        {
            var myPokemon = GetPokemons();
            var pokemons = myPokemon.ToList();
            return pokemons.OrderByDescending(PokemonInfo.CalculatePokemonPerfection).Take(limit);
        }

        public int GetItemAmountByType(ItemId type)
        {
            var pokeballs = GetItems();
            return pokeballs.FirstOrDefault(i => i.ItemId == type)?.Count ?? 0;
        }

        public IEnumerable<ItemData> GetItems()
        {
            var inventory = GetCachedInventory();
            return inventory
                .Select(i => i.InventoryItemData?.Item)
                .Where(p => p != null);
        }

        public int GetTotalItemCount()
        {
            var myItems = GetItems().ToList();
            int myItemCount = 0;
            foreach (var myItem in myItems) myItemCount += myItem.Count;
            return myItemCount;
        }

        public IEnumerable<ItemData> GetItemsToRecycle(ISession session)
        {
            //await RefreshCachedInventory();
            var itemsToRecycle = new List<ItemData>();
            var myItems = GetItems().ToList();
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

        public IEnumerable<PlayerStats> GetPlayerStats()
        {
            var inventory = GetCachedInventory();
            return inventory
                .Select(i => i.InventoryItemData?.PlayerStats)
                .Where(p => p != null);
        }

        public async Task<UseItemXpBoostResponse> UseLuckyEggConstantly()
        {
            var UseLuckyEgg = await _client.Inventory.UseItemXpBoost();
            return UseLuckyEgg;
        }

        public async Task<UseIncenseResponse> UseIncenseConstantly()
        {
            var UseIncense = await _client.Inventory.UseIncense(ItemId.ItemIncenseOrdinary);
            return UseIncense;
        }

        public List<InventoryItem> GetPokeDexItems()
        {
            var inventory = GetCachedInventory();

            return (from items in inventory
                    where items.InventoryItemData?.PokedexEntry != null
                    select items).ToList();
        }

        public async Task<List<Candy>> GetPokemonFamilies(int retries = 0)
        {
            if (retries > 3) return null;

            IEnumerable<Candy> families = null;
            var inventory = GetCachedInventory();

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
                DelayingUtils.Delay(3000, 3000);
                return await GetPokemonFamilies(++retries);
            }

            return families.ToList();
        }

        public PokemonData GetSinglePokemon(ulong id)
        {
            var inventory = GetCachedInventory();
            return
                inventory.Select(i => i.InventoryItemData?.PokemonData)
                    .FirstOrDefault(p => p != null && p.PokemonId > 0 && p.Id == id);
        }

        public IEnumerable<PokemonData> GetPokemons()
        {
            var inventory = GetCachedInventory();
            return inventory
                    .Select(i => i.InventoryItemData?.PokemonData)
                    .Where(p => p != null && p.PokemonId > 0);
        }

        public IEnumerable<PokemonData> GetFavoritePokemons()
        {
            var inventory = GetPokemons();
            return
                inventory.Where(i => i.Favorite == 1);
        }

        public IEnumerable<PokemonData> GetDeployedPokemons()
        {
            var inventory = GetPokemons();
            return
                inventory.Where(i => !string.IsNullOrEmpty(i.DeployedFortId));
        }

        public async Task<MoveSettings> GetMoveSetting(PokemonMove move)
        {
            if (_client.Download.ItemTemplates == null)
                await GetPokemonSettings();

            var moveSettings = _client.Download.ItemTemplates.Where(x => x.MoveSettings != null)
                .Select(x => x.MoveSettings)
                .ToList();

            return moveSettings.FirstOrDefault(x => x.MovementId == move);
        }

        public async Task<IEnumerable<PokemonSettings>> GetPokemonSettings()
        {
            if (_client.Download.ItemTemplates == null)
                await _client.Download.GetItemTemplates();

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
                await _client.Download.GetItemTemplates();

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
            var buddy = this.ownerSession.Profile.PlayerData.BuddyPokemon;
            if (buddy != null && buddy.Id == pokemon.Id)
                return false;

            // Can't transfer favorite
            if (pokemon.Favorite == 1)
                return false;

            return true;
        }

        public async Task<bool> CanEvolvePokemon(PokemonData pokemon)
        {
            // Can't evolve pokemon in gyms.
            if (!string.IsNullOrEmpty(pokemon.DeployedFortId))
                return false;

            IEnumerable<PokemonSettings> pokemonSettings = await GetPokemonSettings();
            var settings = pokemonSettings.SingleOrDefault(x => x.PokemonId == pokemon.PokemonId);

            // Can't evolve pokemon that are not evolvable.
            if (settings.EvolutionIds.Count == 0)
                return false;

            int familyCandy = GetCandyCount(pokemon.PokemonId);
            // Can't evolve if not enough candy.
            if (familyCandy < settings.CandyToEvolve)
                return false;

            return true;
        }

        public IEnumerable<PokemonData> GetPokemonToEvolve(IEnumerable<PokemonId> filter = null)
        {
            IEnumerable<PokemonData> myPokemon = GetPokemons().OrderByDescending(p => p.Cp);

            IEnumerable<PokemonId> pokemonIds = filter as PokemonId[] ?? filter.ToArray();
            if (pokemonIds.Any())
            {
                myPokemon =
                    myPokemon.Where(
                        p => (pokemonIds.Contains(p.PokemonId)) ||
                             (_logicSettings.EvolveAllPokemonAboveIv &&
                              (PokemonInfo.CalculatePokemonPerfection(p) >= _logicSettings.EvolveAboveIvValue)));
            }
            else if (_logicSettings.EvolveAllPokemonAboveIv)
            {
                myPokemon =
                    myPokemon.Where(
                        p => PokemonInfo.CalculatePokemonPerfection(p) >= _logicSettings.EvolveAboveIvValue);
            }

            // Only get evolvable pokemon (not in gym, enough candy, etc.)
            var evolvablePokemon = myPokemon.Where(p => CanEvolvePokemon(p).Result).ToList();

            var pokemonToEvolve = new List<PokemonData>();

            // Group pokemon by their PokemonId
            var groupedPokemons = evolvablePokemon.GroupBy(p => p.PokemonId);
            foreach (var group in groupedPokemons)
            {
                PokemonId pokemonId = group.Key;
                int candiesLeft = GetCandyCount(pokemonId);
                PokemonSettings settings = GetPokemonSettings().Result.FirstOrDefault(x => x.PokemonId == pokemonId);
                int pokemonLeft = group.Count();

                // Calculate the number of evolutions possible (taking into account +1 candy for evolve and +1 candy for transfer)
                EvolutionCalculations evolutionInfo = CalcuatePokemonEvolution(pokemonLeft, candiesLeft, settings.CandyToEvolve);

                if (evolutionInfo.Evolves > 0)
                {
                    // Add only the number of pokemon we can evolve.
                    pokemonToEvolve.AddRange(group.Take(evolutionInfo.Evolves).ToList());
                }
            }

            return pokemonToEvolve;
        }

        public async Task<LevelUpRewardsResponse> GetLevelUpRewards(int level)
        {
            if (_level == 0 || level > _level)
            {
                _level = level;

                var rewards = await _client.Player.GetLevelUpRewards(level);
                foreach (var item in rewards.ItemsAwarded)
                {
                    await UpdateInventoryItem(item.ItemId);
                }
            }

            return new LevelUpRewardsResponse();
        }

        public bool CanUpgradePokemon(PokemonData pokemon)
        {
            // Can't upgrade pokemon in gyms.
            if (!string.IsNullOrEmpty(pokemon.DeployedFortId))
                return false;

            var playerLevel = GetPlayerStats().FirstOrDefault().Level;
            var pokemonLevel = PokemonInfo.GetLevel(pokemon);

            // Can't evolve unless pokemon level is lower than trainer.
            if (pokemonLevel >= playerLevel)
                return false;

            int familyCandy = GetCandyCount(pokemon.PokemonId);

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

        public List<PokemonData> GetPokemonToUpgrade()
        {
            var upgradePokemon = new List<PokemonData>();

            if (!_logicSettings.AutomaticallyLevelUpPokemon)
                return upgradePokemon;

            var myPokemon = GetPokemons().Where(p => CanUpgradePokemon(p));

            var grouped = myPokemon.GroupBy(p => p.PokemonId);

            Parallel.ForEach(grouped, (group) =>
            {
                var appliedFilter = _logicSettings.PokemonUpgradeFilters.ContainsKey(group.Key)
                    ? _logicSettings.PokemonUpgradeFilters[group.Key]
                    : new UpgradeFilter(_logicSettings.LevelUpByCPorIv, _logicSettings.UpgradePokemonCpMinimum,
                        _logicSettings.UpgradePokemonIvMinimum, _logicSettings.UpgradePokemonMinimumStatsOperator,
                        _logicSettings.OnlyUpgradeFavorites);

                IEnumerable<PokemonData> highestPokemonForUpgrade =
                    (appliedFilter.UpgradePokemonMinimumStatsOperator.ToLower().Equals("and"))
                        ? group.Where(
                                p => (p.Cp >= appliedFilter.UpgradePokemonCpMinimum &&
                                      PokemonInfo.CalculatePokemonPerfection(p) >=
                                      appliedFilter.UpgradePokemonIvMinimum))
                            .OrderByDescending(p => p.Cp)
                            .ToList()
                        : group.Where(
                                p => (p.Cp >= appliedFilter.UpgradePokemonCpMinimum ||
                                      PokemonInfo.CalculatePokemonPerfection(p) >=
                                      appliedFilter.UpgradePokemonIvMinimum))
                            .OrderByDescending(p => p.Cp)
                            .ToList();

                if (appliedFilter.OnlyUpgradeFavorites)
                {
                    highestPokemonForUpgrade = highestPokemonForUpgrade.Where(i => i.Favorite == 1);
                }

                var upgradeableList = (appliedFilter.LevelUpByCPorIv.ToLower().Equals("iv"))
                    ? highestPokemonForUpgrade.OrderByDescending(PokemonInfo.CalculatePokemonPerfection).ToList()
                    : highestPokemonForUpgrade.OrderByDescending(p => p.Cp).ToList();
                lock (upgradePokemon)
                {
                    upgradePokemon.AddRange(upgradeableList);
                }
            });
            return upgradePokemon;
            //IEnumerable<PokemonData> highestPokemonForUpgrade = (_logicSettings.UpgradePokemonMinimumStatsOperator.ToLower().Equals("and")) ?
            //    myPokemon.Where(
            //            p => (p.Cp >= _logicSettings.UpgradePokemonCpMinimum &&
            //                PokemonInfo.CalculatePokemonPerfection(p) >= _logicSettings.UpgradePokemonIvMinimum)).OrderByDescending(p => p.Cp).ToList() :
            //    myPokemon.Where(
            //        p => (p.Cp >= _logicSettings.UpgradePokemonCpMinimum ||
            //            PokemonInfo.CalculatePokemonPerfection(p) >= _logicSettings.UpgradePokemonIvMinimum)).OrderByDescending(p => p.Cp).ToList();

            //return upgradePokemon = (_logicSettings.LevelUpByCPorIv.ToLower().Equals("iv")) ?
            //        highestPokemonForUpgrade.OrderByDescending(PokemonInfo.CalculatePokemonPerfection).ToList() :
            //        highestPokemonForUpgrade.OrderByDescending(p => p.Cp).ToList();
        }

        public TransferFilter GetPokemonTransferFilter(PokemonId pokemon)
        {
            if (_logicSettings.PokemonsTransferFilter != null &&
                _logicSettings.PokemonsTransferFilter.ContainsKey(pokemon))
            {
                var filter = _logicSettings.PokemonsTransferFilter[pokemon];
                if (filter.Moves == null)
                {
                    filter.Moves = new List<List<PokemonMove>>();
                }
                return filter;
            }
            return new TransferFilter(_logicSettings.KeepMinCp, _logicSettings.KeepMinLvl, _logicSettings.UseKeepMinLvl,
                _logicSettings.KeepMinIvPercentage,
                _logicSettings.KeepMinOperator, _logicSettings.KeepMinDuplicatePokemon);
        }

        public async Task<UpgradePokemonResponse> UpgradePokemon(ulong pokemonid)
        {
            return await _client.Inventory.UpgradePokemon(pokemonid);
        }
    }
}