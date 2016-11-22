#region using directives

using POGOProtos.Networking.Responses;

#endregion

namespace PoGo.NecroBot.LIB.Event
{
    public class ProfileEvent : IEvent
    {
        public GetPlayerResponse Profile;
    }
}