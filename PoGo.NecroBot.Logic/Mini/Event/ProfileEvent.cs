#region using directives

using POGOProtos.Networking.Responses;

#endregion

namespace PoGo.NecroBot.Logic.Mini.Event
{
    public class ProfileEvent : IEvent
    {
        public GetPlayerResponse Profile;
    }
}