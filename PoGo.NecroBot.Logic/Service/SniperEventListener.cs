#region using directives

using System;
using System.Diagnostics.CodeAnalysis;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Tasks;
using System.Threading.Tasks;

#endregion

namespace PoGo.NecroBot.Logic.Service
{
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    public class SniperEventListener
    {
        private static void HandleEvent(PokemonCaptureEvent pokemonCaptureEvent, ISession session)
        {
            //remove pokemon from list
            HumanWalkSnipeTask.UpdateCatchPokemon(pokemonCaptureEvent.Latitude,
                pokemonCaptureEvent.Longitude, pokemonCaptureEvent.Id);
        }

        public static async Task HandleEventAsync(EncounteredEvent ev, ISession session)
        {
            if (!ev.IsRecievedFromSocket) return;

            await HumanWalkSnipeTask.AddSnipePokemon("mypogosnipers.com",
                ev.PokemonId,
                ev.Latitude,
                ev.Longitude,
                ev.Expires,
                ev.IV,
                session
            ).ConfigureAwait(false);
        }

        public static void HandleEvent(IEvent evt, ISession session)
        {
        }

        public void Listen(IEvent evt, ISession session)
        {
            dynamic eve = evt;

            try
            {
                HandleEvent(eve, session);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}