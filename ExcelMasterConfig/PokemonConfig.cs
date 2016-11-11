using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelMasterConfig
{
    public class PokemonConfigCore
    {
        public bool Enable { get; set; }
        public int MinIV { get; set; }
        public int MinCP { get; set; }
        public int MinLevel { get; set; }

        public List<PokemonMove> Moves { get; set; }

        public string LogicOperator { get; set; }
    }
     public class UpgradePokemonConfig : PokemonConfigCore {
        public bool FavoriteOnly { get; set; }
    }

    public class CatchPokemonConfig : PokemonConfigCore
    {
    }

    public class TransferPokemonConfig : PokemonConfigCore
    {
        public int KeepDuplicates { get; set; }
    }

    public class PokemonConfig
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public UpgradePokemonConfig Upgrade { get; set; }

        public TransferPokemonConfig Transfer { get; set; }

        public string CatchPokemonConfig { get; set; }

        public PokemonId PokemonId
        {
            get { return (PokemonId)Id; }
        }
    }
}
