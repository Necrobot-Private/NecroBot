using MahApps.Metro.Controls;
using PoGo.NecroBot.Window.Model;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Event.Inventory;
using PoGo.NecroBot.Logic.Event.Player;
using PoGo.NecroBot.Logic.Event.Snipe;
using PoGo.NecroBot.Logic.Event.UI;
using PoGo.NecroBot.Logic.Logging;
using POGOProtos.Inventory.Item;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PoGo.NecroBot.Window
{
    public partial class MainClientWindow
    {
        #region UI Event handler

        public void UIUpdateSafe(Action action)
        {
            try
            {
                this.Invoke(() => action);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        public void OnBotEvent(FavoriteEvent ev)
        {
            datacontext.PokemonList.OnFavorited(ev);
        }
        public void OnBotEvent(BotSwitchedEvent ex)
        {
            Dispatcher.Invoke(() =>
            {
                datacontext.Reset();
                lblAccount.Content = "Switching Account...";
            });
        }
        public void OnBotEvent(FinishUpgradeEvent e)
        {
            datacontext.PokemonList.OnUpgradeEnd(e);
        }
        public void OnBotEvent(UpgradePokemonEvent e)
        {
            datacontext.PokemonList.OnUpgraded(e);
        }
        public void OnBotEvent(PokemonEvolveEvent ev)
        {
            datacontext.PokemonList.OnEvolved(ev);
        }
        public void OnBotEvent(PokemonCaptureEvent capture)
        {
            datacontext.Sidebar.AddOrUpdate(new CatchPokemonViewModel(capture));
        }
        public void OnBotEvent(LoginEvent ev)
        {
            Dispatcher.Invoke(() =>
            {
                datacontext.Reset();
                lblAccount.Content = currentSession.Translation.GetTranslation(NecroBot.Logic.Common.TranslationString.LoggingIn, ev.AuthType, ev.Username);
            });
        }
        public void OnBotEvent(SnipePokemonStarted ex)
        {
            datacontext.SnipeList.OnPokemonSnipeStarted(ex.Pokemon);
        }
        public void OnBotEvent(EggIncubatorStatusEvent e)
        {
            datacontext.EggsList.OnEggIncubatorStatus(e);
            //datacontext.PlayerInfo.UpdateEggs(e.KmRemaining); //Still in the works(TheWizard1328)
            datacontext.PlayerInfo.RaisePropertyChanged("KmRemaining");
            datacontext.PlayerInfo.RaisePropertyChanged("KmToWalk");
            datacontext.PlayerInfo.RaisePropertyChanged("EggPerc");
        }
        public void OnBotEvent(InventoryRefreshedEvent e)
        {
            if (currentSession.Profile == null || currentSession.Profile.PlayerData == null) return;

            var maxPokemonStorage = currentSession.Profile?.PlayerData?.MaxPokemonStorage;
            var maxItemStorage = currentSession.Profile?.PlayerData?.MaxItemStorage;
            var pokemons = currentSession.Inventory.GetPokemons().Result;

            var inventory = currentSession.Inventory.GetCachedInventory().Result;
            datacontext.SnipeList.OnInventoryRefreshed(inventory);
            datacontext.PlayerInfo.OnInventoryRefreshed(inventory);
            datacontext.EggsList.OnInventoryRefreshed(inventory);
            datacontext.RaisePropertyChanged("EggsTabHeader");

            datacontext.MaxPokemonStorage = maxPokemonStorage.Value;
            datacontext.RaisePropertyChanged("MaxPokemonStorage");

            var items = inventory.Select(x => x.InventoryItemData?.Item).Where(x => x != null).ToList();
            datacontext.MaxItemStorage = maxItemStorage.Value;
            datacontext.RaisePropertyChanged("MaxItemStorage");

            datacontext.ItemsList.Update(items);
            datacontext.RaisePropertyChanged("ItemsTabHeader");

            datacontext.PokemonList.Update(pokemons);
            datacontext.RaisePropertyChanged("PokemonTabHeader");
        }

        public void OnBotEvent(InventoryItemUpdateEvent e)
        {
            datacontext.ItemsList.Update(new List<ItemData> { e.Item });
            datacontext.RaisePropertyChanged("ItemsTabHeader");
        }

        public void OnBotEvent(EncounteredEvent e)
        {
            datacontext.SnipeList.OnSnipeData(e);
            botMap.HandleEncounterEvent(e);
        }

        public void OnBotEvent(LoggedEvent userLogged)
        {
            datacontext.UI.PlayerStatus = "Playing";
            datacontext.UI.PlayerName = userLogged.Profile.PlayerData.Username;
            datacontext.RaisePropertyChanged("UI");

            Dispatcher.Invoke(() =>
            {
                lblAccount.Content = $"{datacontext.UI.PlayerStatus} as : {datacontext.UI.PlayerName}";
            });
        }
        public void OnBotEvent(ProfileEvent profile)
        {
            var stats = profile.Stats;

            datacontext.PlayerInfo.OnProfileUpdate(profile);

            datacontext.UI.PlayerStatus = "Playing";
            datacontext.UI.PlayerName = profile.Profile.PlayerData.Username;

            datacontext.RaisePropertyChanged("UI");

            lblAccount.Content = $"{datacontext.UI.PlayerStatus} as : {datacontext.UI.PlayerName}";

        }
        public void OnBotEvent(RenamePokemonEvent renamePokemonEvent)
        {
            datacontext.PokemonList.OnRename(renamePokemonEvent);
        }
        public void OnBotEvent(TransferPokemonEvent transferedPkm)
        {
            datacontext.PokemonList.OnTransfer(transferedPkm);
        }
        public void OnBotEvent(StatusBarEvent e)
        {
            lblStatus.Text = e.Message;
        }
        public void OnBotEvent(FortUsedEvent ev)
        {
            datacontext.Sidebar.AddOrUpdate(new PokestopItemViewModel(ev));
            botMap.MarkFortAsLooted(ev.Fort);
        }
        public void OnBotEvent(PokeStopListEvent ev)
        {
            botMap.OnPokestopEvent(ev);
        }
        public void OnBotEvent(UpdatePositionEvent ev)
        {
            botMap.UpdatePlayerPosition(ev.Latitude, ev.Longitude);
            datacontext.PlayerInfo.UpdateSpeed(ev.Speed);
        }
        public void OnBotEvent(AutoSnipePokemonAddedEvent ev)
        {
            datacontext.SnipeList.OnSnipeItemQueue(ev.EncounteredEvent);
        }
        public void OnBotEvent(PokestopLimitUpdate ev)
        {
            datacontext.PlayerInfo.UpdatePokestopLimit(ev);
        }
        public void OnBotEvent(CatchLimitUpdate ev)
        {
            datacontext.PlayerInfo.UpdateCatchLimit(ev);
        }
        public void OnBotEvent(ErrorEvent ev)
        {
            Dispatcher.Invoke(() =>
            {
                if (ev.RequireExit)
                {
                    lblAccount.Content = "An Error has been Detected and the bot will Shut Down in 10 Seconds (Info in Console/Logs)";
                    Logger.Write($"Error Detected! ({ev.Message})", LogLevel.Error);
                    var ExitTime = DateTime.Now.AddSeconds(10);
                    if (DateTime.Now > ExitTime)
                        Application.Current.Shutdown();
                }
            });
        }
        public void OnBotEvent(IEvent evt)
        {
        }
        internal void HandleBotEvent(IEvent evt)
        {
            dynamic eve = evt;

            Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        OnBotEvent(eve);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                });
            });
        }
        #endregion
    }
}