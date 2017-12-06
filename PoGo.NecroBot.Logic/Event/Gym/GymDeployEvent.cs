using POGOProtos.Enums;
using POGOProtos.Networking.Responses;

namespace PoGo.NecroBot.Logic.Event.Gym
{
    public class GymDeployEvent : IEvent
    {
        public GymGetInfoResponse GymGetInfo { get; internal set; }
        public PokemonId PokemonId { get; internal set; }
    }
}