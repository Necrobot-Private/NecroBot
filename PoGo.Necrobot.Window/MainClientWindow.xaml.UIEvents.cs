using MahApps.Metro.Controls;
using PoGo.Necrobot.Window.Model;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Event.Inventory;
using PoGo.NecroBot.Logic.Event.Player;
using PoGo.NecroBot.Logic.Event.Snipe;
using PoGo.NecroBot.Logic.Event.UI;
using PoGo.NecroBot.Logic.State;
using POGOProtos.Inventory.Item;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.Necrobot.Window
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
            this.datacontext.PokemonList.OnFavorited(ev);
        }
        public void OnBotEvent(BotSwitchedEvent ex)
        {
            //this.botMap.Reset();
            this.datacontext.Reset();
            this.popSwithAccount.IsOpen = true;
            //show popup...
        }
        public void OnBotEvent(FinishUpgradeEvent e)
        {
            this.datacontext.PokemonList.OnUpgradeEnd(e);
        }
        public void OnBotEvent(UpgradePokemonEvent e)
        {
            this.datacontext.PokemonList.OnUpgraded(e);
        }
        public void OnBotEvent(PokemonEvolveEvent ev)
        {
            this.datacontext.PokemonList.OnEvolved(ev);
        }
        public void OnBotEvent(PokemonCaptureEvent inventory)
        {
            this.datacontext.Sidebar.AddOrUpdate(new CatchPokemonViewModel(inventory));
        }
        public void OnBotEvent(LoginEvent ev)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.datacontext.Reset();

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
        }
        public void OnBotEvent(InventoryRefreshedEvent e)
        {
            if (currentSession.Profile == null || currentSession.Profile.PlayerData == null) return;

            var maxPokemonStorage = currentSession.Profile?.PlayerData?.MaxPokemonStorage;
            var maxItemStorage = currentSession.Profile?.PlayerData?.MaxItemStorage;
            var pokemons = currentSession.Inventory.GetPokemons();

            var inventory = currentSession.Inventory.GetCachedInventory();
            datacontext.SnipeList.OnInventoryRefreshed(inventory);
            datacontext.PlayerInfo.OnInventoryRefreshed(inventory);
            datacontext.EggsList.OnInventoryRefreshed(inventory);

            var items = inventory.Select(x => x.InventoryItemData?.Item).Where(x => x != null).ToList();
            this.datacontext.MaxItemStorage = maxItemStorage.Value;
            this.datacontext.RaisePropertyChanged("MaxItemStorage");

            this.datacontext.ItemsList.Update(items);
            this.datacontext.RaisePropertyChanged("ItemsTabHeader");

            this.datacontext.PokemonList.Update(pokemons);
            this.datacontext.RaisePropertyChanged("PokemonTabHeader");
            
            UIUpdateSafe(() =>
             {
                 tabPokemons.Header = $"   Pokemons ({this.datacontext.Pokemons.Count}/{maxPokemonStorage})   ";
             });
        }

        public void OnBotEvent(InventoryItemUpdateEvent e)
        {
            this.datacontext.ItemsList.Update(new List<ItemData> { e.Item });
            this.datacontext.RaisePropertyChanged("ItemsTabHeader");
        }

        public void OnBotEvent(EncounteredEvent e)
        {
            this.datacontext.SnipeList.OnSnipeData(e);
            this.botMap.HandleEncounterEvent(e);
        }

        public void OnBotEvent(LoggedEvent userLogged)
        {
            this.datacontext.UI.PlayerStatus = "Playing";
            this.datacontext.UI.PlayerName = userLogged.Profile.PlayerData.Username;
            this.datacontext.RaisePropertyChanged("UI");

            this.Dispatcher.Invoke(() =>
            {
                this.popSwithAccount.IsOpen = false;
                lblAccount.Content = $"{this.datacontext.UI.PlayerStatus} as : {this.datacontext.UI.PlayerName}";

            });
        }
        public void OnBotEvent(ProfileEvent profile)
        {
            var stats = profile.Stats;

            this.datacontext.PlayerInfo.OnProfileUpdate(profile);

            this.datacontext.UI.PlayerStatus = "Playing";
            this.datacontext.UI.PlayerName = profile.Profile.PlayerData.Username;

            this.datacontext.RaisePropertyChanged("UI");

            lblAccount.Content = $"{this.datacontext.UI.PlayerStatus} as : {this.datacontext.UI.PlayerName}";

        }
        public void OnBotEvent(TransferPokemonEvent transferedPkm)
        {
            this.datacontext.PokemonList.OnTransfer(transferedPkm);
        }
        public void OnBotEvent(StatusBarEvent e)
        {
            lblStatus.Text = e.Message;
        }
        public void OnBotEvent(FortUsedEvent ev)
        {
            this.botMap.MarkFortAsLooted(ev.Id);
        }
        public void OnBotEvent(PokeStopListEvent ev)
        {
            this.botMap.OnPokestopEvent(ev);
        }
        public void OnBotEvent(UpdatePositionEvent ev)
        {
            this.botMap.UpdatePlayerPosition(ev.Latitude, ev.Longitude);
        }
        public void OnBotEvent(AutoSnipePokemonAddedEvent ev)
        {                            
            datacontext.SnipeList.OnSnipeItemQueue(ev.EncounteredEvent);
        }
        public void OnBotEvent(IEvent evt)
        {
        }
        internal void HandleBotEvent(IEvent evt)
        {
            dynamic eve = evt;

            Task.Run(() =>
            {
                this.Dispatcher.Invoke(() =>
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
