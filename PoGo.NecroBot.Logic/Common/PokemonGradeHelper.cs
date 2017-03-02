using System;
using System.Collections.Generic;
using System.Linq;
using PoGo.NecroBot.Logic.Model;
using POGOProtos.Enums;

namespace PoGo.NecroBot.Logic.Common
{
    public static class PokemonGradeHelper
    {
        private static Dictionary<PokemonGrades, string> RarityColors = new Dictionary<PokemonGrades, string>()
        {
            { PokemonGrades.Common,"#7FFF8E" },
            { PokemonGrades.Epic,"#E16FA2" },
            { PokemonGrades.Legendary,"#B05895" },
            { PokemonGrades.Popular,"#DBFE80" },
            { PokemonGrades.Rare,"#FFB382" },
            { PokemonGrades.Special,"Red" },
            { PokemonGrades.VeryCommon,"#DBFE80" },
            { PokemonGrades.VeryRare,"#FF807F" }
        };

        public static string GetGradeColor(PokemonId pokemonId)
        {
            var g = GetPokemonGrade(pokemonId);
            return RarityColors[g];
        }

        public static string GetGradeColor(PokemonGrades g)
        {
            return RarityColors[g];
        }
        public static PokemonGrades GetPokemonGrade(PokemonId id)
        {
            var first = pokemonByGrades.FirstOrDefault(p => p.Value.Contains(id));
            return first.Key;
        }

        private static Dictionary<PokemonGrades, List<PokemonId>> pokemonByGrades =
            new Dictionary<PokemonGrades, List<PokemonId>>()
            {
                {
                    PokemonGrades.VeryCommon, new List<PokemonId>()
                    {
                        PokemonId.Caterpie,
                        PokemonId.Weedle,
                        PokemonId.Pidgey,
                        PokemonId.Rattata,
                        PokemonId.Ekans,
                        PokemonId.Sandshrew,
                        PokemonId.NidoranFemale,
                        PokemonId.NidoranMale,
                        PokemonId.Zubat,
                        PokemonId.Oddish,
                        PokemonId.Paras,
                        PokemonId.Venonat,
                        PokemonId.Mankey,
                        PokemonId.Poliwag,
                        PokemonId.Machop,
                        PokemonId.Bellsprout,
                        PokemonId.Geodude,
                        PokemonId.Slowpoke,
                        PokemonId.Magnemite,
                        PokemonId.Gastly,
                        PokemonId.Krabby,
                        PokemonId.Voltorb,
                        PokemonId.Goldeen,
                        PokemonId.Eevee,
                        PokemonId.Magikarp,
                        PokemonId.Natu
                    }
                },
                {
                    PokemonGrades.Common, new List<PokemonId>
                    {
                        PokemonId.Bulbasaur,
                        PokemonId.Charmander,
                        PokemonId.Squirtle,
                        PokemonId.Kakuna,
                        PokemonId.Pidgeotto,
                        PokemonId.Raticate,
                        PokemonId.Spearow,
                        PokemonId.Arbok,
                        PokemonId.Pikachu,
                        PokemonId.Sandslash,
                        PokemonId.Clefable,
                        PokemonId.Jigglypuff,
                        PokemonId.Golbat,
                        PokemonId.Diglett,
                        PokemonId.Persian,
                        PokemonId.Psyduck,
                        PokemonId.Growlithe,
                        PokemonId.Abra,
                        PokemonId.Machoke,
                        PokemonId.Graveler,
                        PokemonId.Ponyta,
                        PokemonId.Magneton,
                        PokemonId.Doduo,
                        PokemonId.Seel,
                        PokemonId.Grimer,
                        PokemonId.Shellder,
                        PokemonId.Haunter,
                        PokemonId.Electrode,
                        PokemonId.Exeggcute,
                        PokemonId.Cubone,
                        PokemonId.Hitmonlee,
                        PokemonId.Koffing,
                        PokemonId.Rhyhorn,
                        PokemonId.Horsea,
                        PokemonId.Staryu,
                        PokemonId.Jynx,
                       PokemonId.Chikorita,
                       PokemonId.Totodile,
                       PokemonId.Sentret,
                       PokemonId.Hoothoot,
                       PokemonId.Ledyba,
                       PokemonId.Spinarak,
                       PokemonId.Sunkern,
                       PokemonId.Wooper,
                       PokemonId.Murkrow,
                       PokemonId.Snubbull,
                       PokemonId.Teddiursa ,
                       PokemonId.Slugma       ,
                       PokemonId.Swinub       ,
                       PokemonId.Remoraid     ,
                       PokemonId.Houndour     ,
                       PokemonId.Phanpy       ,
                       PokemonId.Marill
                    }
                },
                {
                    PokemonGrades.Popular, new List<PokemonId>()
                    {
                        PokemonId.Dratini,
                        PokemonId.Butterfree,
                        PokemonId.Spearow,
                        PokemonId.Nidorina,
                        PokemonId.Nidorino,
                        PokemonId.Ninetales,
                        PokemonId.Wigglytuff,
                        PokemonId.Gloom,
                        PokemonId.Parasect,
                        PokemonId.Golduck,
                        PokemonId.Primeape,
                        PokemonId.Chansey,
                        PokemonId.Poliwhirl,
                        PokemonId.Kadabra,
                        PokemonId.Machamp,
                        PokemonId.Tentacruel,
                        PokemonId.Golem,
                        PokemonId.Kabuto,
                        PokemonId.Dodrio,
                        PokemonId.Cloyster,
                        PokemonId.Scyther,
                        PokemonId.Hypno,
                        PokemonId.Seadra,
                        PokemonId.Hitmonchan,
                        PokemonId.Lickitung,
                        PokemonId.Weezing,
                        PokemonId.Seaking,
                        PokemonId.Starmie    ,
                        PokemonId.Bayleef,
                            PokemonId.Cyndaquil,
                            PokemonId.Croconaw,
                            PokemonId.Furret,
                            PokemonId.Ledian,
                            PokemonId.Ariados,
                            PokemonId.Chinchou,
                            PokemonId.Sudowoodo,
                            PokemonId.Hoppip,
                            PokemonId.Aipom,
                            PokemonId.Yanma,
                            PokemonId.Misdreavus,
                            PokemonId.Wobbuffet,
                            PokemonId.Dunsparce,
                            PokemonId.Gligar,
                            PokemonId.Granbull,
                            PokemonId.Qwilfish,
                            PokemonId.Shuckle,
                            PokemonId.Heracross,
                            PokemonId.Sneasel,
                            PokemonId.Magcargo,
                            PokemonId.Piloswine,
                            PokemonId.Corsola,
                            PokemonId.Octillery,
                            PokemonId.Mantine,
                            PokemonId.Skarmory,
                            PokemonId.Stantler,
                            PokemonId.Larvitar,

                    }
                },
                {
                    PokemonGrades.Rare, new List<PokemonId>()
                    {
                        PokemonId.Beedrill,
                        PokemonId.Pidgeot,
                        PokemonId.Pinsir,
                        PokemonId.Snorlax,
                        PokemonId.Slowbro,
                        PokemonId.MrMime,
                        PokemonId.Farfetchd,
                        PokemonId.Onix,
                        PokemonId.Jolteon,
                        PokemonId.Flareon,
                        PokemonId.Magmar,
                        PokemonId.Kingler,
                        PokemonId.Rhydon,
                        PokemonId.Rapidash,
                        PokemonId.Arcanine,
                        PokemonId.Muk,
                        PokemonId.Exeggutor,
                        PokemonId.Tangela,
                        PokemonId.Meganium,
                        PokemonId.Quilava,
                        PokemonId.Feraligatr,
                        PokemonId.Noctowl,
                        PokemonId.Lanturn,
                        PokemonId.Skiploom,
                        PokemonId.Quagsire,
                        PokemonId.Unown,
                        PokemonId.Girafarig,
                        PokemonId.Pineco,
                        PokemonId.Ursaring,
                        PokemonId.Delibird,
                        PokemonId.Houndoom,
                        PokemonId.Donphan,
                        PokemonId.Smeargle,
                        PokemonId.Hitmontop,
                        PokemonId.Miltank,
                        PokemonId.Blissey,
                        PokemonId.Pupitar,
                        PokemonId.Xatu    ,
                        PokemonId.Mareep    ,
                        PokemonId.Flaaffy ,
                        PokemonId.Azumarill

                    }
                },
                {
                    PokemonGrades.VeryRare, new List<PokemonId>()
                    {

                        PokemonId.Gyarados,
                        PokemonId.Lapras,
                        PokemonId.Vaporeon,
                        PokemonId.Kabutops,
                        PokemonId.Dragonair,
                        PokemonId.Dragonite,
                        PokemonId.Raichu,
                        PokemonId.Nidoqueen,
                        PokemonId.Nidoking,
                        PokemonId.Vileplume,
                        PokemonId.Venomoth,
                        PokemonId.Poliwrath,
                        PokemonId.Alakazam,
                        PokemonId.Electabuzz,
                        PokemonId.Victreebel,
                        PokemonId.Kangaskhan,
                        PokemonId.Dewgong,
                        PokemonId.Marowak,
                        PokemonId.Gengar,
                        PokemonId.Tyranitar ,
                        PokemonId.Pupitar,
                        PokemonId.Togetic,
                        PokemonId.Blissey,
                        PokemonId.Steelix    ,
                        PokemonId.Crobat,
                        PokemonId.Espeon,
                        PokemonId.Umbreon,
                        PokemonId.Typhlosion,
                        PokemonId.Jumpluff,
                        PokemonId.Forretress,
                        PokemonId.Ampharos
                    }
                },
                {
                    PokemonGrades.Epic, new List<PokemonId>()
                    {
                        PokemonId.Venusaur,
                        PokemonId.Charmeleon,
                        PokemonId.Wartortle,
                        PokemonId.Porygon,
                        PokemonId.Porygon2,
                        PokemonId.Omanyte,
                        PokemonId.Aerodactyl,
                        PokemonId.Charizard,
                        PokemonId.Blastoise,
                        PokemonId.Ivysaur   ,
                        PokemonId.Politoed,
                        PokemonId.Sunflora,
                        PokemonId.Steelix,
                        PokemonId.Scizor,
                        PokemonId.Kingdra,
                        PokemonId.Porygon,

                    }
                },
                {
                    PokemonGrades.Legendary, new List<PokemonId>()
                    {
                        PokemonId.Ditto,
                        PokemonId.Articuno,
                        PokemonId.Zapdos,
                        PokemonId.Moltres,
                        PokemonId.Mewtwo,
                    }
                }
            };
    }
}