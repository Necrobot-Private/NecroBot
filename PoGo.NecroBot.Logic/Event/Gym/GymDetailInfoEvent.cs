using POGOProtos.Enums;

namespace PoGo.NecroBot.Logic.Event.Gym
{
    public class GymDetailInfoEvent : IEvent
    {
        public string Name { get; internal set; }
        public int Players { get; internal set; }
        public TeamColor Team { get; internal set; }
    }
}