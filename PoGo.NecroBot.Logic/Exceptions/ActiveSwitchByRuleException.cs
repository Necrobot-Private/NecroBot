using System;

namespace PoGo.NecroBot.Logic.Exceptions
{
    public enum SwitchRules
    {
        Pokestop,
        Pokemon,
        EXP,
        Runtime,
        PokestopSoftban,
        CatchFlee,
        CatchLimitReached,
        SpinPokestopReached,
        EmptyMap
    }

    public class ActiveSwitchByRuleException : Exception
    {
        public ActiveSwitchByRuleException()
        {
        }

        public ActiveSwitchByRuleException(SwitchRules rule, double value)
        {
            MatchedRule = rule;
            ReachedValue = value;
        }

        public SwitchRules MatchedRule { get; set; }
        public double ReachedValue { get; set; }
    }
}