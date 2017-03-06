#region using directives

using System.Linq;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Model;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Utils;
using POGOProtos.Enums;
using POGOProtos.Inventory.Item;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public class EvolveSpecificPokemonTask
    {
        public static async Task Execute(ISession session, ulong pokemonId, PokemonId evolveToId = PokemonId.Missingno)
        {
            using (var blocker = new BlockableScope(session, BotActions.Envolve))
            {
                if (!await blocker.WaitToRun())
                {
                    session.EventDispatcher.Send(new PokemonEvolveEvent
                    {
                        OriginalId = pokemonId,
                        Cancelled = true
                    });
                    return;
                }
                
                var all = session.Inventory.GetPokemons();
                var pokemons = all.OrderByDescending(x => x.Cp).ThenBy(n => n.StaminaMax);
                var pokemon = pokemons.FirstOrDefault(p => p.Id == pokemonId);

                if (pokemon == null) return;

                if (!await session.Inventory.CanEvolvePokemon(pokemon, new Model.Settings.EvolveFilter() {
                    EvolveTo = evolveToId.ToString()
                }))
                    return;
                ItemId itemToEvolve = ItemId.ItemUnknown;
                var pkmSetting = session.Inventory.GetPokemonSetting(pokemon.PokemonId);

                if (evolveToId != PokemonId.Missingno && pkmSetting != null)
                {
                    var evolution = pkmSetting.EvolutionBranch.FirstOrDefault(x => x.Evolution == evolveToId);
                    if(evolution!= null)
                    {
                        itemToEvolve = evolution.EvolutionItemRequirement;
                        if(itemToEvolve !=  ItemId.ItemUnknown && session.Inventory.GetItemAmountByType(itemToEvolve) ==0)
                        {
                            session.EventDispatcher.Send(new PokemonEvolveEvent
                            {
                                OriginalId = pokemonId,
                                Cancelled = true
                            });
                            return;
                        }
                    }
                }

                var evolveResponse = await session.Client.Inventory.EvolvePokemon(pokemon.Id, itemToEvolve);

                session.EventDispatcher.Send(new PokemonEvolveEvent
                {
                    OriginalId = pokemonId,
                    Id = pokemon.PokemonId,
                    Exp = evolveResponse.ExperienceAwarded,
                    UniqueId = pokemon.Id,
                    Result = evolveResponse.Result,
                    EvolvedPokemon = evolveResponse.EvolvedPokemonData
                });

            }
        }
    }
}