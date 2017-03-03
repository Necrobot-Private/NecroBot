#region using directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Utils;
using PokemonGo.RocketAPI.Extensions;
using POGOProtos.Map.Fort;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Responses;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    //add delegate
    public delegate void PokemonsEncounterLureDelegate(List<MapPokemon> pokemons);
    public static class CatchLurePokemonsTask
    {
        public static async Task Execute(ISession session, FortData currentFortData,
            CancellationToken cancellationToken)
        {
            TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();
            cancellationToken.ThrowIfCancellationRequested();
            if (!session.LogicSettings.CatchPokemon ||
                session.CatchBlockTime > DateTime.Now) return;

            if (session.KnownLongitudeBeforeSnipe != 0 && session.KnownLatitudeBeforeSnipe != 0 &&
                LocationUtils.CalculateDistanceInMeters(session.KnownLatitudeBeforeSnipe,
                session.KnownLongitudeBeforeSnipe,
                session.Client.CurrentLatitude,
                session.Client.CurrentLongitude) > 1000)
            {
                Logger.Write($"ERROR - Bot stucked at snipe location({session.Client.CurrentLatitude},{session.Client.CurrentLongitude}). Teleport him back home - if you see this message please PM samuraitruong");

                session.Client.Player.SetCoordinates(session.KnownLatitudeBeforeSnipe, session.KnownLongitudeBeforeSnipe, session.Client.CurrentAltitude);
                return;
            }


            Logger.Write(session.Translation.GetTranslation(TranslationString.LookingForLurePokemon), LogLevel.Debug);

            var fortId = currentFortData.Id;

            var pokemonId = currentFortData.LureInfo.ActivePokemonId;

            if ((session.LogicSettings.UsePokemonToNotCatchFilter &&
                 session.LogicSettings.PokemonsNotToCatch.Contains(pokemonId)))
            {
                session.EventDispatcher.Send(new NoticeEvent
                {
                    Message = session.Translation.GetTranslation(TranslationString.PokemonSkipped, pokemonId)
                });
            }
            else
            {
                var encounterId = currentFortData.LureInfo.EncounterId;
                if (session.Cache.Get(currentFortData.LureInfo.EncounterId.ToString()) != null)
                    return; //pokemon been ignore before

                var encounter = await session.Client.Encounter.EncounterLurePokemon(encounterId, fortId);

                if (encounter.Result == DiskEncounterResponse.Types.Result.Success &&
                    session.LogicSettings.CatchPokemon)
                {
                    var pokemon = new MapPokemon
                    {
                        EncounterId = encounterId,
                        ExpirationTimestampMs = currentFortData.LureInfo.LureExpiresTimestampMs,
                        Latitude = currentFortData.Latitude,
                        Longitude = currentFortData.Longitude,
                        PokemonId = currentFortData.LureInfo.ActivePokemonId,
                        SpawnPointId = currentFortData.Id
                    };

                    //add delegate function
                    OnPokemonEncounterEvent(new List<MapPokemon> { pokemon });

                    // Catch the Pokemon
                    await CatchPokemonTask.Execute(session, cancellationToken, encounter, pokemon,
                        currentFortData, sessionAllowTransfer: true);
                }
                else if (encounter.Result == DiskEncounterResponse.Types.Result.PokemonInventoryFull)
                {
                    if (session.LogicSettings.TransferDuplicatePokemon || session.LogicSettings.TransferWeakPokemon)
                    {
                        session.EventDispatcher.Send(new WarnEvent
                        {
                            Message = session.Translation.GetTranslation(TranslationString.InvFullTransferring)
                        });
                        if (session.LogicSettings.TransferDuplicatePokemon)
                            await TransferDuplicatePokemonTask.Execute(session, cancellationToken);
                        if (session.LogicSettings.TransferWeakPokemon)
                            await TransferWeakPokemonTask.Execute(session, cancellationToken);
                        if (session.LogicSettings.EvolveAllPokemonAboveIv ||
                            session.LogicSettings.EvolveAllPokemonWithEnoughCandy ||
                            session.LogicSettings.UseLuckyEggsWhileEvolving ||
                            session.LogicSettings.KeepPokemonsThatCanEvolve)
                        {
                            await EvolvePokemonTask.Execute(session, cancellationToken);
                        }
                    }
                    else
                        session.EventDispatcher.Send(new WarnEvent
                        {
                            Message = session.Translation.GetTranslation(TranslationString.InvFullTransferManually)
                        });
                }
                else
                {
                    if (encounter.Result.ToString().Contains("NotAvailable")) return;
                    session.EventDispatcher.Send(new WarnEvent
                    {
                        Message =
                            session.Translation.GetTranslation(TranslationString.EncounterProblemLurePokemon,
                                encounter.Result)
                    });
                }
            }
        }

        public static async Task Execute(ISession session, CancellationToken cancellationToken)
        {
            // Looking for any lure pokestop neaby

            var mapObjects = await session.Client.Map.GetMapObjects();
            var pokeStops = mapObjects.MapCells.SelectMany(i => i.Forts)
                .Where(
                    i =>
                        (i.Type == FortType.Checkpoint) &&
                        i.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime()
                );

            session.AddForts(pokeStops.ToList());

            var forts = session.Forts.Where(p => p.Type == FortType.Checkpoint);
            List<FortData> luredNearBy = new List<FortData>();

            foreach (FortData fort in forts)
            {
                var distance = LocationUtils.CalculateDistanceInMeters(session.Client.CurrentLatitude,
                    session.Client.CurrentLongitude, fort.Latitude, fort.Longitude);
                if (distance < 40 && fort.LureInfo != null)
                {
                    luredNearBy.Add(fort);
                    await Execute(session, fort, cancellationToken);
                }
            }
            ;
        }
        //add delegate event
        public static event PokemonsEncounterLureDelegate PokemonEncounterEvent;

        private static void OnPokemonEncounterEvent(List<MapPokemon> pokemons)
        {
            PokemonEncounterEvent?.Invoke(pokemons);
        }
    }
}