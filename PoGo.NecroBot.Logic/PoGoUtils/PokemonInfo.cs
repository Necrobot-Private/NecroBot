#region using directives

using System;
using System.Collections.Generic;
using System.Linq;
using POGOProtos.Data;
using POGOProtos.Enums;
using POGOProtos.Inventory;
using POGOProtos.Settings.Master;
using PokemonGo.RocketAPI.Helpers;

#endregion

namespace PoGo.NecroBot.Logic.PoGoUtils
{
    public struct BaseStats
    {
        public int BaseAttack, BaseDefense, BaseStamina;

        public BaseStats(int baseStamina, int baseAttack, int baseDefense)
        {
            BaseAttack = baseAttack;
            BaseDefense = baseDefense;
            BaseStamina = baseStamina;
        }

        public override string ToString()
        {
            return $"({BaseAttack} atk,{BaseDefense} def,{BaseStamina} sta)";
        }
    }

    public static class PokemonInfo
    {
        public static int CalculateCp(PokemonData poke)
        {
            return PokemonCpUtils.GetCp(poke);
        }
        
        public static double CalculateMaxCpMultiplier(PokemonId pokemonId)
        {
            return PokemonCpUtils.GetAbsoluteMaxCp(pokemonId);
        }

        public static int CalculateMaxCp(PokemonData poke)
        {
            return PokemonCpUtils.GetMaxCp(poke);
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

        public static int GetCandy(PokemonData pokemon, List<Candy> PokemonFamilies,
            IEnumerable<PokemonSettings> PokemonSettings)
        {
            if (PokemonFamilies == null || PokemonSettings == null) return 0;

            var setting = PokemonSettings.FirstOrDefault(q => pokemon != null && q.PokemonId == pokemon.PokemonId);
            var family = PokemonFamilies.FirstOrDefault(q => setting != null && q.FamilyId == setting.FamilyId);

            return family != null ? family.Candy_ : 0;
        }

        public static int GetPowerUpLevel(PokemonData poke)
        {
            return (int) (GetLevel(poke) * 2.0);
        }
    }
}