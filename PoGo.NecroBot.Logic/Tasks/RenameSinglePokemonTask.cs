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

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public class RenameSinglePokemonTask
    {
        public static async Task Execute(ISession session, ulong pokemonId, string newNickname, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();
            var pokemon = session.Inventory.GetPokemons().Where(x => x.Id == pokemonId).FirstOrDefault();

            if (pokemon == null || pokemon.Nickname == newNickname)
                return;

            if (newNickname.Length > 12)
                newNickname = newNickname.Substring(0, 12);

            var oldNickname = string.IsNullOrEmpty(pokemon.Nickname) ? pokemon.PokemonId.ToString() : pokemon.Nickname;

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
        }
    }
}