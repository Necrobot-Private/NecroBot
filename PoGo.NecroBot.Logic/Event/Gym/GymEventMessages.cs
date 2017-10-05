using System;

namespace PoGo.NecroBot.Logic.Event.Gym
{
    public class GymEventMessages : IEvent
    {
        public string Message { get; internal set; }
        public ConsoleColor consoleColor { get; internal set; }
    }
}