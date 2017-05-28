using System;

namespace PoGo.NecroBot.Logic.Exceptions
{
    public class PauseStateException : Exception
    {
        public static bool IsRunning;

        public PauseStateException(bool isRunning)
        {
            IsRunning = isRunning;
        }
    }
}
