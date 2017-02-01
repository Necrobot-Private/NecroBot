#region using directives

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.PoGoUtils;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Utils;
using POGOProtos.Networking.Responses;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public class RenamePokemonTask
    {
        public static async Task Execute(ISession session, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var pokemons = session.Inventory.GetPokemons();

            foreach (var pokemon in pokemons)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var perfection = Math.Round(PokemonInfo.CalculatePokemonPerfection(pokemon));
                var level = PokemonInfo.GetLevel(pokemon);
                var pokemonName = session.Translation.GetPokemonTranslation(pokemon.PokemonId);
                // iv number + templating part + pokemonName <= 12
                
                var newNickname = session.LogicSettings.RenameTemplate;
                newNickname.Replace("{IV}", Math.Round(perfection, 0).ToString());
                newNickname.Replace("{Level}", Math.Round(level, 0).ToString());

                var nameLength = 18 - newNickname.Length;
                if (pokemonName.Length > nameLength)
                {
                    pokemonName = pokemonName.Substring(0, nameLength);
                }

                newNickname.Replace("{Name}", pokemonName);


                var oldNickname = pokemon.Nickname.Length != 0 ? pokemon.Nickname : pokemon.PokemonId.ToString();

                // If "RenameOnlyAboveIv" = true only rename pokemon with IV over "KeepMinIvPercentage"
                if ((!session.LogicSettings.RenameOnlyAboveIv ||
                     perfection >= session.LogicSettings.KeepMinIvPercentage) &&
                    newNickname != oldNickname)
                {
                    var result = await session.Client.Inventory.NicknamePokemon(pokemon.Id, newNickname);

                    if (result.Result == NicknamePokemonResponse.Types.Result.Success)
                    {
                        pokemon.Nickname = newNickname;

                        session.EventDispatcher.Send(new NoticeEvent
                        {
                            Message = session.Translation.GetTranslation(
                                TranslationString.PokemonRename,
                                session.Translation.GetPokemonTranslation(pokemon.PokemonId),
                                pokemon.Id,
                                oldNickname,
                                newNickname
                            )
                        });
                    }
                    //Delay only if the pokemon was really renamed!
                    DelayingUtils.Delay(session.LogicSettings.RenamePokemonActionDelay, 500);
                }
            }
        }
    }
}