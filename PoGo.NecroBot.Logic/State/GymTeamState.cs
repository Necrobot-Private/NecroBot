using PoGo.NecroBot.Logic.Tasks;
using POGOProtos.Data;
using POGOProtos.Settings.Master;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PoGo.NecroBot.Logic.State
{
    public class GymTeamState : IDisposable
    {
        public List<MyPokemonStat> myPokemons { get; private set; }

        public List<AnyPokemonStat> otherDefenders { get; private set; }

        public List<GymPokemon> myTeam { get; private set; }

        public IEnumerable<MoveSettings> moveSettings { get; set; }

        public long TimeToDodge { get; set; }
        public long LastWentDodge { get; set; }

        public GymTeamState()
        {
            myTeam = new List<GymPokemon>();
            myPokemons = new List<MyPokemonStat>();
            otherDefenders = new List<AnyPokemonStat>();
            TimeToDodge = 0;
        }

        public void addPokemon(ISession session, PokemonData pokemon, bool isMine = true)
        {
            if (isMine && myPokemons.Any(a => a.data.Id == pokemon.Id))
                return;

            if (!isMine && otherDefenders.Any(a => a.data.Id == pokemon.Id))
                return;

            if (isMine)
                myPokemons.Add(new MyPokemonStat(session, pokemon));
            else
                otherDefenders.Add(new AnyPokemonStat(session, pokemon));
        }

        public void addToTeam(ISession session, PokemonData pokemon)
        {
            if (!myPokemons.Any(a => a.data.Id == pokemon.Id))
                myPokemons.Add(new MyPokemonStat(session, pokemon));

            if (!myTeam.Any(a => a.attacker.Id == pokemon.Id))
                myTeam.Add(new GymPokemon() { attacker = pokemon, HpState = pokemon.StaminaMax });
        }

        public void LoadMyPokemons(ISession session)
        {
            myPokemons.Clear();
            foreach (var pokemon in session.Inventory.GetPokemons().Where(w => w.Cp >= session.LogicSettings.GymConfig.MinCpToUseInAttack))
            {
                MyPokemonStat mps = new MyPokemonStat(session, pokemon);
                myPokemons.Add(mps);
            }
        }

        public void Dispose()
        {
            if (myTeam != null)
                myTeam.Clear();

            if (myPokemons != null)
                myPokemons.Clear();

            if (otherDefenders != null)
                otherDefenders.Clear();
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

    public class AnyPokemonStat : IDisposable
    {
        public PokemonData data { get; set; }

        public MoveSettings Attack { get; set; }

        public MoveSettings SpecialAttack { get; set; }

        public POGOProtos.Enums.PokemonType MainType { get; set; }

        public POGOProtos.Enums.PokemonType ExtraType { get; set; }

        public AnyPokemonStat(ISession session, PokemonData pokemon)
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

    public class MyPokemonStat : AnyPokemonStat
    {

        public Dictionary<POGOProtos.Enums.PokemonType, int> TypeFactor { get; private set; }

        public MyPokemonStat(ISession session, PokemonData pokemon) : base(session, pokemon)
        {
            TypeFactor = new Dictionary<POGOProtos.Enums.PokemonType, int>();

            foreach (var type in Enum.GetValues(typeof(POGOProtos.Enums.PokemonType)))
            {
                getFactorAgainst((POGOProtos.Enums.PokemonType)type);
            }
        }

        private int getFactorAgainst(POGOProtos.Enums.PokemonType type)
        {
            if (TypeFactor.Keys.Contains(type))
                return TypeFactor[type];

            int factor = 0;
            if (UseGymBattleTask.GetBestTypes(type).Any(a => a == Attack.PokemonType))
            {
                factor += 2;
                if (MainType == Attack.PokemonType || ExtraType == Attack.PokemonType)
                    factor += 1;
            }
            if (UseGymBattleTask.GetWorstTypes(type).Any(a => a == Attack.PokemonType)) factor -= 2;

            if (UseGymBattleTask.GetBestTypes(type).Any(a => a == SpecialAttack.PokemonType))
            {
                factor += 2;
                if (MainType == SpecialAttack.PokemonType || ExtraType == SpecialAttack.PokemonType)
                    factor += 1;
            }
            if (UseGymBattleTask.GetWorstTypes(type).Any(a => a == SpecialAttack.PokemonType)) factor -= 2;

            TypeFactor.Add(type, factor);

            return factor;
        }

        public int getFactorAgainst(ISession session, int cp, bool isTraining)
        {
            decimal percent = 0.0M;
            if (cp > data.Cp)
                percent = (decimal)data.Cp / (decimal)cp * -100.0M;
            else
                percent = (decimal)cp / (decimal)data.Cp * 100.0M;

            int factor = (int)((100.0M - Math.Abs(percent)) / 5.0M) * Math.Sign(percent);

            if (isTraining)
                factor *= -1;

            if (session.LogicSettings.GymConfig.NotUsedSkills.Any(a => a.Key == data.PokemonId && a.Value == Attack.MovementId))
                factor -= 6;

            if (session.LogicSettings.GymConfig.NotUsedSkills.Any(a => a.Key == data.PokemonId && a.Value == SpecialAttack.MovementId))
                factor -= 6;

            return factor;
        }

        private int getFactorAgainst(PokemonSettings pokemon)
        {
            int factor = getFactorAgainst(pokemon.Type);
            factor += getFactorAgainst(pokemon.Type2);
            return factor;
        }

        // jjskuld - Ignore CS0108 warning for now.
#pragma warning disable 0108
        public void Dispose()
        {
            if (TypeFactor != null)
                TypeFactor.Clear();
        }
#pragma warning restore 0108
    }
}