#region using directives

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.State;
using POGOProtos.Enums;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Responses;
using PoGo.NecroBot.Logic.Utils;
#endregion

namespace PoGo.NecroBot.CLI
{
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    internal class PushNotificationListener
    {
        private static void HandleEvent(ErrorEvent errorEvent, ISession session)
        {
            PushNotificationClient.SendNotification(session, "Error occured", errorEvent.Message);
        }

        public static void HandleEvent(SnipePokemonFoundEvent ev, ISession session)
        {
        }

        public static void HandleEvent(EncounteredEvent ev, ISession session)
        {
        }

        internal void Listen(IEvent evt, ISession session)
        {
            dynamic eve = evt;

            try
            { HandleEvent(eve, session); }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
