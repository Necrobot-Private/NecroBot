#region using directives

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Event.Inventory;
using PoGo.NecroBot.Logic.Model;
using PoGo.NecroBot.Logic.PoGoUtils;
using PoGo.NecroBot.Logic.State;
using POGOProtos.Networking.Responses;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public class FavoritePokemonTask
    {
        public static async Task Execute(ISession session, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            //await session.Inventory.RefreshCachedInventory();

            var pokemons = session.Inventory.GetPokemons();

            foreach (var pokemon in pokemons)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var perfection = Math.Round(PokemonInfo.CalculatePokemonPerfection(pokemon));

                if (session.LogicSettings.AutoFavoritePokemon &&
                    perfection >= session.LogicSettings.FavoriteMinIvPercentage && pokemon.Cp >= session.LogicSettings.FavoriteMinCp && pokemon.Favorite != 1)
                {
                    var response = await session.Client.Inventory.SetFavoritePokemon(pokemon.Id, true);
                    if (response.Result == SetFavoritePokemonResponse.Types.Result.Success)
                    {
                        session.EventDispatcher.Send(new NoticeEvent
                        {
                            Message =
                                session.Translation.GetTranslation(TranslationString.PokemonFavorite, perfection,
                                    session.Translation.GetPokemonTranslation(pokemon.PokemonId), pokemon.Cp)
                        });
                    }
                }
            }
        }

        public static async Task Execute(ISession session, ulong pokemonId, bool favorite)
        {
            //using (var blocker = new BlockableScope(session, BotActions.Favorite))
            //{
                //if (!await blocker.WaitToRun()) return;
                
                var pokemon = session.Inventory.GetPokemons().FirstOrDefault(p => p.Id == pokemonId);
                if (pokemon != null)
                {
                    var perfection = Math.Round(PokemonInfo.CalculatePokemonPerfection(pokemon));

                    var response = await session.Client.Inventory.SetFavoritePokemon(pokemonId, favorite);
                    if (response.Result == SetFavoritePokemonResponse.Types.Result.Success)
                    {
                        // Reload pokemon to refresh favorite flag.
                        pokemon = session.Inventory.GetPokemons().FirstOrDefault(p => p.Id == pokemonId);

                        session.EventDispatcher.Send(new FavoriteEvent(pokemon, response));

                        string message;
                        if (favorite)
                            message = session.Translation.GetTranslation(TranslationString.PokemonFavorite, perfection,
                                session.Translation.GetPokemonTranslation(pokemon.PokemonId), pokemon.Cp);
                        else
                            message = session.Translation.GetTranslation(TranslationString.PokemonUnFavorite, perfection,
                                session.Translation.GetPokemonTranslation(pokemon.PokemonId), pokemon.Cp);
                        
                        session.EventDispatcher.Send(new NoticeEvent
                        {
                            Message = message
                        });
                    }
                }
            //}
        }
    }
}