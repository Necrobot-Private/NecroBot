#region using directives

using System;
using System.Threading;
using System.Threading.Tasks;
using PoGo.NecroBot.LIB.Common;
using PoGo.NecroBot.LIB.Event;
using PoGo.NecroBot.LIB.PoGoUtils;
using PoGo.NecroBot.LIB.State;

#endregion

namespace PoGo.NecroBot.LIB.Tasks
{
    public class FavoritePokemonTask
    {
        public static async Task Execute(ISession session, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var pokemons = await session.Inventory.GetPokemons();

            foreach (var pokemon in pokemons)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var perfection = Math.Round(PokemonInfo.CalculatePokemonPerfection(pokemon));

                if (session.LogicSettings.AutoFavoritePokemon &&
                    perfection >= session.LogicSettings.FavoriteMinIvPercentage && pokemon.Favorite != 1)
                {
                    await session.Client.Inventory.SetFavoritePokemon(pokemon.Id, true);

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
}