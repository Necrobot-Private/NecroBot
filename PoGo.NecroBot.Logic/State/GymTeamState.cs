using PoGo.NecroBot.Logic.Tasks;
using POGOProtos.Data;
using POGOProtos.Map.Fort;
using POGOProtos.Networking.Responses;
using POGOProtos.Settings.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.State
{
    public class GymTeamState : IDisposable
    {
        /// <summary>
        /// Cache time in seconds
        /// </summary>
        private const long _cacheTime = 2 * 60;

        private Dictionary<string, CachedGymGetails> _gymDetails { get; set; }

        public List<MyPokemonStat> MyPokemons { get; private set; }

        public List<AnyPokemonStat> OtherDefenders { get; private set; }

        public List<GymPokemon> MyTeam { get; private set; }

        public IEnumerable<MoveSettings> MoveSettings { get; set; }

        public long TimeToDodge { get; set; }

        public long LastWentDodge { get; set; }

        public SwitchPokemonData SwithAttacker { get; set; }

        public int TrainingRound { get; set; }

        public string TrainingGymId { get; set; }

        public string CapturedGymId { get; set; }

        public GymTeamState()
        {
            MyTeam = new List<GymPokemon>();
            MyPokemons = new List<MyPokemonStat>();
            OtherDefenders = new List<AnyPokemonStat>();
            _gymDetails = new Dictionary<string, CachedGymGetails>();
            TimeToDodge = 0;
            SwithAttacker = null;
        }

        public void AddPokemon(ISession session, PokemonData pokemon, bool isMine = true)
        {
            if (isMine && MyPokemons.Any(a => a.Data.Id == pokemon.Id))
                return;

            if (!isMine && OtherDefenders.Any(a => a.Data.Id == pokemon.Id))
                return;

            if (isMine)
                MyPokemons.Add(new MyPokemonStat(session, pokemon));
            else
                OtherDefenders.Add(new AnyPokemonStat(session, pokemon));
        }

        public void AddToTeam(ISession session, PokemonData pokemon)
        {
            if (!MyPokemons.Any(a => a.Data.Id == pokemon.Id))
                MyPokemons.Add(new MyPokemonStat(session, pokemon));

            if (!MyTeam.Any(a => a.Attacker.Id == pokemon.Id))
                MyTeam.Add(new GymPokemon() { Attacker = pokemon, HpState = pokemon.StaminaMax });
        }

        public async Task LoadMyPokemons(ISession session)
        {
            MyPokemons.Clear();
            var pokemons = await session.Inventory.GetPokemons().ConfigureAwait(false);
            foreach (var pokemon in pokemons.Where(w => w.Cp >= session.LogicSettings.GymConfig.MinCpToUseInAttack))
            {
                MyPokemonStat mps = new MyPokemonStat(session, pokemon);
                MyPokemons.Add(mps);
            }
        }

        public GymGetInfoResponse GetGymDetails(ISession session, FortData fort, bool force = false)
        {
            CachedGymGetails gymDetails = null;

            if (_gymDetails.Keys.Contains(fort.Id))
                gymDetails = _gymDetails[fort.Id];
            else
            {
                gymDetails = new CachedGymGetails(session, fort);
                _gymDetails.Add(fort.Id, gymDetails);
                force = false;
            }

            if (force || gymDetails.LastCall.AddSeconds(_cacheTime) < DateTime.UtcNow)
            {
                gymDetails.LoadData(session, fort);
                _gymDetails[fort.Id] = gymDetails;
            }

            return gymDetails.GymDetails;
        }

        public void Dispose()
        {
            if (MyTeam != null)
                MyTeam.Clear();

            if (MyPokemons != null)
                MyPokemons.Clear();

            if (OtherDefenders != null)
                OtherDefenders.Clear();
        }
    }

    public class GymPokemon : IDisposable
    {
        public PokemonData Attacker { get; set; }

        public int HpState { get; set; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public class AnyPokemonStat : IDisposable
    {
        public PokemonData Data { get; set; }

        public MoveSettings Attack { get; set; }

        public MoveSettings SpecialAttack { get; set; }

        public POGOProtos.Enums.PokemonType MainType { get; set; }

        public POGOProtos.Enums.PokemonType ExtraType { get; set; }

        public AnyPokemonStat(ISession session, PokemonData pokemon)
        {
            Data = pokemon;

            var pokemonsSetting = session.Inventory.GetPokemonSettings().Result;
            
            MainType = pokemonsSetting.Where(f => f.PokemonId == Data.PokemonId).Select(s => s.Type).FirstOrDefault();
            ExtraType = pokemonsSetting.Where(f => f.PokemonId == Data.PokemonId).Select(s => s.Type2).FirstOrDefault();

            Attack = session.Inventory.GetMoveSetting(Data.Move1).Result;
            SpecialAttack = session.Inventory.GetMoveSetting(Data.Move2).Result;
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
                GetFactorAgainst((POGOProtos.Enums.PokemonType)type);
            }
        }

        private int GetFactorAgainst(POGOProtos.Enums.PokemonType type)
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

        public int GetFactorAgainst(ISession session, int cp, bool isTraining)
        {
            decimal percent = 0.0M;
            if (cp > Data.Cp)
                percent = (decimal)Data.Cp / (decimal)cp * -100.0M;
            else
                percent = (decimal)cp / (decimal)Data.Cp * 100.0M;

            int factor = (int)((100.0M - Math.Abs(percent)) / 5.0M) * Math.Sign(percent);

            if (isTraining && cp <= Data.Cp)
                factor -= 100;

            if (session.LogicSettings.GymConfig.NotUsedSkills.Any(a => a.Key == Data.PokemonId && a.Value == Attack.MovementId))
                factor -= 20;

            if (session.LogicSettings.GymConfig.NotUsedSkills.Any(a => a.Key == Data.PokemonId && a.Value == SpecialAttack.MovementId))
                factor -= 20;

            return factor;
        }

        private int GetFactorAgainst(PokemonSettings pokemon)
        {
            int factor = GetFactorAgainst(pokemon.Type);
            factor += GetFactorAgainst(pokemon.Type2);
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

    public class SwitchPokemonData
    {
        public ulong OldAttacker { get; private set; }
        public ulong NewAttacker { get; private set; }

        public int AttackDuration
        {
            get { return 1000; }
        }

        public SwitchPokemonData(ulong Old, ulong New)
        {
            OldAttacker = Old;
            NewAttacker = New;
        }
    }

    public class CachedGymGetails
    {
        public DateTime LastCall { get; set; }

        public GymGetInfoResponse GymDetails { get; set; }

        public CachedGymGetails(ISession session, FortData fort)
        {
            LoadData(session, fort);
        }

        public void LoadData(ISession session, FortData fort)
        {
            var task = session.Client.Fort.GymGetInfo(fort.Id, fort.Latitude, fort.Longitude);
            task.Wait();
            if (task.IsCompleted && task.Result.Result == GymGetInfoResponse.Types.Result.Success)
            {
                var state = new POGOProtos.Data.Gym.GymState()
                {
                    FortData = fort
                };
                fort = state.FortData;
                GymDetails = task.Result;
                LastCall = DateTime.UtcNow;
            }
        }
    }

}