using System.Collections.Generic;
using Newtonsoft.Json;
using POGOProtos.Enums;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    [JsonObject(Title = "Catch Settings", Description = "", ItemRequired = Required.DisallowNull)]
    public class CatchSettings
    {
        public CatchSettings()
        {
        }

        public CatchSettings(List<Location> locations, List<PokemonId> pokemon)
        {
            Pokemon = pokemon;
        }

        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Ignore, Order = 2)]
        public List<PokemonId> Pokemon = new List<PokemonId>();

        internal static CatchSettings Default()
        {
            return new CatchSettings
            {
                Pokemon = new List<PokemonId>
                {
                    PokemonId.Venusaur,
                    PokemonId.Charizard,
                    PokemonId.Blastoise,
                    PokemonId.Beedrill,
                    PokemonId.Raichu,
                    PokemonId.Sandslash,
                    PokemonId.Nidoking,
                    PokemonId.Nidoqueen,
                    PokemonId.Clefable,
                    PokemonId.Ninetales,
                    PokemonId.Golbat,
                    PokemonId.Vileplume,
                    PokemonId.Golduck,
                    PokemonId.Primeape,
                    PokemonId.Arcanine,
                    PokemonId.Poliwrath,
                    PokemonId.Alakazam,
                    PokemonId.Machamp,
                    PokemonId.Golem,
                    PokemonId.Rapidash,
                    PokemonId.Slowbro,
                    PokemonId.Farfetchd,
                    PokemonId.Muk,
                    PokemonId.Cloyster,
                    PokemonId.Gengar,
                    PokemonId.Exeggutor,
                    PokemonId.Marowak,
                    PokemonId.Hitmonchan,
                    PokemonId.Lickitung,
                    PokemonId.Rhydon,
                    PokemonId.Chansey,
                    PokemonId.Kangaskhan,
                    PokemonId.Starmie,
                    PokemonId.MrMime,
                    PokemonId.Scyther,
                    PokemonId.Magmar,
                    PokemonId.Electabuzz,
                    PokemonId.Jynx,
                    PokemonId.Gyarados,
                    PokemonId.Lapras,
                    PokemonId.Ditto,
                    PokemonId.Vaporeon,
                    PokemonId.Jolteon,
                    PokemonId.Flareon,
                    PokemonId.Porygon,
                    PokemonId.Kabutops,
                    PokemonId.Aerodactyl,
                    PokemonId.Snorlax,
                    PokemonId.Articuno,
                    PokemonId.Zapdos,
                    PokemonId.Moltres,
                    PokemonId.Dragonite,
                    PokemonId.Mewtwo,
                    PokemonId.Mew
                }
            };
        }
    }
}