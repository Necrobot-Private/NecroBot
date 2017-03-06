#region using directives

using System;
using System.Collections.Generic;
using System.Linq;
using POGOProtos.Data;
using POGOProtos.Enums;
using POGOProtos.Inventory;
using POGOProtos.Settings.Master;
using PokemonGo.RocketAPI.Helpers;
using PoGo.NecroBot.Logic.State;

#endregion

namespace PoGo.NecroBot.Logic.PoGoUtils
{
    public static class PokemonInfo
    {
        public static int CalculateCp(PokemonData poke)
        {
            return PokemonCpUtils.GetCp(poke);
        }
        
        public static int CalculateMaxCp(PokemonId pokemonId, int level = 40)
        {
            return PokemonCpUtils.GetAbsoluteMaxCp(pokemonId, level);
        }

        public static double CalculatePokemonPerfection(PokemonData poke)
        {
            if (poke == null)
                return 0;

            //TODO : Lets use the simple formulat att+def+sta /45

            //if (Math.Abs(poke.CpMultiplier + poke.AdditionalCpMultiplier) <= 0)
                return Math.Round((poke.IndividualAttack + poke.IndividualDefense + poke.IndividualStamina) / 45.0 * 100.0, 2);

            //GetBaseStats(poke.PokemonId);
            //var maxCp = CalculateMaxCpMultiplier(poke.PokemonId);
            //var minCp = CalculateMinCpMultiplier(poke);
            //var curCp = CalculateCpMultiplier(poke);

            //return (curCp - minCp) / (maxCp - minCp) * 100.0;
        }

        public static double GetLevel(PokemonData poke)
        {
            return PokemonCpUtils.GetLevel(poke);
        }

        public static PokemonMove GetPokemonMove1(PokemonData poke)
        {
            var move1 = poke.Move1;
            return move1;
        }

        public static PokemonMove GetPokemonMove2(PokemonData poke)
        {
            var move2 = poke.Move2;
            return move2;
        }

        public static int GetCandy(ISession session, PokemonData pokemon)
        {
            return session.Inventory.GetCandyCount(pokemon.PokemonId);
        }

        public static int GetPowerUpLevel(PokemonData poke)
        {
            return (int) (GetLevel(poke) * 2.0);
        }
    }
}