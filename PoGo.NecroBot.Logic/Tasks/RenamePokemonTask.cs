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
using System.Text.RegularExpressions;
using PoGo.NecroBot.Logic.Logging;
using System.Linq;
using System.Collections.Generic;
using POGOProtos.Data;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public class RenamePokemonTask
    {
        public static async Task Execute(ISession session, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();
            var pokemons = session.Inventory.GetPokemons();
            if (session.LogicSettings.TransferDuplicatePokemon &&
                session.LogicSettings.RenamePokemonRespectTransferRule)
            {
                var duplicatePokemons =
                   await
                       session.Inventory.GetDuplicatePokemonToTransfer(
                           session.LogicSettings.PokemonsNotToTransfer,
                           session.LogicSettings.PokemonsToEvolve,
                           session.LogicSettings.KeepPokemonsThatCanEvolve,
                           session.LogicSettings.PrioritizeIvOverCp);

                pokemons = pokemons.Where(x => !duplicatePokemons.Any(p => p.Id == x.Id));
            }

            if (session.LogicSettings.TransferWeakPokemon && session.LogicSettings.RenamePokemonRespectTransferRule)
            {

                var pokemonsFiltered =
                    pokemons.Where(pokemon => !session.LogicSettings.PokemonsNotToTransfer.Contains(pokemon.PokemonId) &&
                        pokemon.Favorite == 0 &&
                        pokemon.DeployedFortId == string.Empty)
                        .ToList().OrderBy(poke => poke.Cp);


                var weakPokemons = new List<PokemonData>();
                foreach (var pokemon in pokemonsFiltered)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if ((pokemon.Cp >= session.LogicSettings.KeepMinCp) ||
                        (PokemonInfo.CalculatePokemonPerfection(pokemon) >= session.LogicSettings.KeepMinIvPercentage &&
                         session.LogicSettings.PrioritizeIvOverCp) ||
                         (PokemonInfo.GetLevel(pokemon) >= session.LogicSettings.KeepMinLvl && session.LogicSettings.UseKeepMinLvl) ||
                        pokemon.Favorite == 1)
                        continue;

                    weakPokemons.Add(pokemon);
                }
                pokemons = pokemons.Where(x => !weakPokemons.Any(p => p.Id == x.Id));
            }
            foreach (var pokemon in pokemons)
            {
                cancellationToken.ThrowIfCancellationRequested();
                TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();
                var perfection = Math.Round(PokemonInfo.CalculatePokemonPerfection(pokemon));
                var level = PokemonInfo.GetLevel(pokemon);
                var pokemonName = session.Translation.GetPokemonTranslation(pokemon.PokemonId);
                var cp = PokemonInfo.CalculateCp(pokemon);
                // iv number + templating part + pokemonName <= 12

                var newNickname = session.LogicSettings.RenameTemplate.ToUpper();
                newNickname = newNickname.Replace("{IV}", Math.Round(perfection, 0).ToString());
                newNickname = newNickname.Replace("{LEVEL}", Math.Round(level, 0).ToString());
                newNickname = newNickname.Replace("{CP}", cp.ToString());

                var nameLength = 18 - newNickname.Length;
                if (pokemonName.Length > nameLength && nameLength > 0)
                {
                    pokemonName = pokemonName.Substring(0, nameLength);
                }

                newNickname = newNickname.Replace("{NAME}", pokemonName);

                //verify
                if (nameLength <= 0)
                {
                    Logger.Write($"Your rename template : {session.LogicSettings.RenameTemplate} incorrect. : {pokemonName} / {newNickname}");
                    continue;
                }
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

                        session.EventDispatcher.Send(new RenamePokemonEvent
                        {
                            Id = pokemon.Id,
                            PokemonId = pokemon.PokemonId,
                            OldNickname = oldNickname,
                            NewNickname = newNickname
                        });
                    }
                    //Delay only if the pokemon was really renamed!
                    DelayingUtils.Delay(session.LogicSettings.RenamePokemonActionDelay, 500);
                }
            }
        }
    }
}
