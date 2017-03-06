using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Event.Player;
using PoGo.NecroBot.Logic.State;
using POGOProtos.Networking.Responses;
using PoGo.NecroBot.Logic.PoGoUtils;
using PoGo.NecroBot.Logic.Logging;
using POGOProtos.Data;
using POGOProtos.Enums;
using System;

namespace PoGo.NecroBot.Logic.Tasks
{
    public class SelectBuddyPokemonTask
    {
        public static async Task Execute(ISession session, CancellationToken cancellationToken, ulong pokemonId = 0)
        {
            PokemonData newBuddy = null;
            if (pokemonId == 0)
            {
                if (string.IsNullOrEmpty(session.LogicSettings.DefaultBuddyPokemon))
                    return;

                PokemonId buddyPokemonId;
                bool success = Enum.TryParse(session.LogicSettings.DefaultBuddyPokemon, out buddyPokemonId);
                if (!success)
                {
                    // Invalid buddy pokemon type
                    Logger.Write($"The DefaultBuddyPokemon ({session.LogicSettings.DefaultBuddyPokemon}) is not a valid pokemon.", LogLevel.Error);
                    return;
                }

                if (session.Profile.PlayerData.BuddyPokemon?.Id > 0)
                {
                    var currentBuddy = session.Inventory.GetPokemons().FirstOrDefault(x => x.Id == session.Profile.PlayerData.BuddyPokemon.Id);
                    if (currentBuddy.PokemonId == buddyPokemonId)
                    {
                        //dont change same buddy
                        return;
                    }
                }

                var buddy = session.Inventory.GetPokemons().Where(x => x.PokemonId == buddyPokemonId)
                .OrderByDescending(x => PokemonInfo.CalculateCp(x));

                if (session.LogicSettings.PrioritizeIvOverCp)
                {
                    buddy = buddy.OrderByDescending(x => PokemonInfo.CalculatePokemonPerfection(x));
                }
                newBuddy = buddy.FirstOrDefault();

                if (newBuddy == null)
                {
                    Logger.Write($"You don't have pokemon {session.LogicSettings.DefaultBuddyPokemon} to set as buddy");
                    return;
                }
            }
            if (newBuddy == null)
            {
                newBuddy = session.Inventory.GetPokemons().FirstOrDefault(x => x.Id == pokemonId);
            }
            if (newBuddy == null) return;

            var response = await session.Client.Player.SelectBuddy(newBuddy.Id);

            if (response.Result == SetBuddyPokemonResponse.Types.Result.Success)
            {
                session.EventDispatcher.Send(new BuddyUpdateEvent(response.UpdatedBuddy, newBuddy));
            }
        }
    }
}