using POGOProtos.Data;
using POGOProtos.Settings.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.State
{
    public class GymTeamState: IDisposable
    {
        public List<MyPokemonStat> myPokemons { get; private set; }

        public List<MyPokemonStat> otherDefenders { get; private set; }

        public List<GymPokemon> myTeam { get; private set; }

        public IEnumerable<MoveSettings> moveSettings { get; set; }

        public GymTeamState()
        {
            myTeam = new List<GymPokemon>();
            myPokemons = new List<MyPokemonStat>();
        }

        public void addPokemon(ISession session, PokemonData pokemon, bool isMine=true)
        {
            if (isMine && myPokemons.Any(a => a.data.Id == pokemon.Id))
                return;
            if (!isMine && otherDefenders.Any(a => a.data.Id == pokemon.Id))
                return;

            if (isMine)
                myPokemons.Add(new MyPokemonStat(session, pokemon));
            else
                otherDefenders.Add(new MyPokemonStat(session, pokemon));
        }

        public void Dispose()
        {
            if (myTeam != null)
                myTeam.Clear();

            if (myPokemons != null)
                myPokemons.Clear();
        }
    }

    public class GymPokemon : IDisposable
    {
        public PokemonData attacker { get; set; }

        public int HpState { get; set; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public class MyPokemonStat: IDisposable
    {
        public PokemonData data { get; set; }

        public POGOProtos.Settings.Master.MoveSettings Attack { get; set; }

        public POGOProtos.Settings.Master.MoveSettings SpecialAttack { get; set; }

        public POGOProtos.Enums.PokemonType MainType { get; set; }

        public POGOProtos.Enums.PokemonType ExtraType { get; set; }

        public Dictionary<POGOProtos.Enums.PokemonType, int> TypeFactor { get; private set; }

        public MyPokemonStat(ISession session, PokemonData pokemon)
        {
            data = pokemon;

            var pokemonsSetting = session.Inventory.GetPokemonSettings();
            pokemonsSetting.Wait();
            MainType = pokemonsSetting.Result.Where(f => f.PokemonId == data.PokemonId).Select(s => s.Type).FirstOrDefault();
            ExtraType = pokemonsSetting.Result.Where(f => f.PokemonId == data.PokemonId).Select(s => s.Type2).FirstOrDefault();

            var attack = session.Inventory.GetMoveSetting(data.Move1);
            attack.Wait();
            Attack = attack.Result;
            
            var specialMove = session.Inventory.GetMoveSetting(data.Move2);
            specialMove.Wait();
            SpecialAttack = specialMove.Result;
        }

        public void Dispose()
        {
            
        }
    }
}
