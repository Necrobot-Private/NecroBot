using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using PoGo.Necrobot.Window.Model;
using PoGo.NecroBot.Logic.DataDumper;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Tasks;
using TinyIoC;
using PoGo.NecroBot.Logic;
using PoGo.NecroBot.Logic.Model.Settings;

namespace PoGo.Necrobot.Window.Controls
{
    public delegate void PokemonItemSelected(PokemonDataViewModel selected);

    /// <summary>
    /// Interaction logic for PokemonInventory.xaml
    /// </summary>
    public partial class PokemonInventory : UserControl
    {
        public ISession Session { get; set; }
        public event PokemonItemSelected OnPokemonItemSelected;

        public PokemonInventory()
        {
            InitializeComponent();
        }

        private void gridData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // The bulk selection only works when 2 or more rows are selected.  This is to work around
            // issues with the checkbox and row selection when only one row is clicked.
            if ((sender as DataGrid).SelectedItems.Count > 1)
            {
                foreach (PokemonDataViewModel pokemon in (sender as DataGrid).SelectedItems)
                {
                    pokemon.IsSelected = true;
                }
            }

            UpdateTransferAllButtonState();

            OnPokemonItemSelected?.Invoke(null);
        }

        private void UpdateTransferAllButtonState()
        {
            var data = DataContext as PokemonListViewModel;
            var count = data.Pokemons.Count(x => x.IsSelected && Session.Inventory.CanTransferPokemon(x.PokemonData));
            //TODO : Thought it will better to use binding.
            btnTransferAll.Content = $"Transfer all ({count})";
            btnTransferAll.IsEnabled = count > 0;
        }

        private void btnTransfer_Click(object sender, RoutedEventArgs e)
        {
            var model = this.DataContext as PokemonListViewModel;

            ulong pokemonId = (ulong) ((Button) sender).CommandParameter;
            model.Transfer(pokemonId);
            var button = sender as Button;
            //button.Content = "Transfering...";
            //button.IsEnabled = false;

            Task.Run(async () =>
            {
                await TransferPokemonTask.Execute(
                    Session, Session.CancellationTokenSource.Token, new List<ulong> {pokemonId}
                );
            });
        }

        private void btnTransferAll_Click(object sender, RoutedEventArgs e)
        {
            var data = DataContext as PokemonListViewModel;
            var pokemonToTransfer = data.Pokemons
                .Where(x => x.IsSelected && !x.IsTransfering && Session.Inventory.CanTransferPokemon(x.PokemonData))
                .Select(x => x.Id)
                .ToList();
            if (pokemonToTransfer.Count > 0)
            {
                data.Transfer(pokemonToTransfer);
                if (MessageBox.Show("Do you want to transfer all selected pokemon", "Bulk transfer", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Task.Run(async () =>
                    {
                        await TransferPokemonTask.Execute(
                            Session, Session.CancellationTokenSource.Token, pokemonToTransfer
                        );
                    });
                }
            }
            else
            {
                // There are no transferrable pokemon selected.
            }
        }

        private void btnEvolve_Click(object sender, RoutedEventArgs e)
        {
            var model = this.DataContext as PokemonListViewModel;

            EvolutionToPokemon evolveTo = (EvolutionToPokemon) ((Button) sender).CommandParameter;
            model.Evolve(evolveTo.OriginPokemonId);

            Task.Run(async () => { await EvolveSpecificPokemonTask.Execute(Session, evolveTo.OriginPokemonId, evolveTo.Pokemon); });
        }

        private void btnFavorite_Click(object sender, RoutedEventArgs e)
        {
            var model = this.DataContext as PokemonListViewModel;

            ulong pokemonId = (ulong) ((Button) sender).CommandParameter;
            bool state = model.Favorite(pokemonId);

            Task.Run(async () => { await FavoritePokemonTask.Execute(Session, pokemonId, !state); });
        }

        private void Select_Checked(object sender, RoutedEventArgs e)
        {
            ulong pokemonId = (ulong)((CheckBox)sender).CommandParameter;

            UpdateTransferAllButtonState();

            OnPokemonItemSelected?.Invoke(null);
        }

        private void btnPowerup_Click(object sender, RoutedEventArgs e)
        {
            var model = this.DataContext as PokemonListViewModel;

            ulong pokemonId = (ulong) ((Button) sender).CommandParameter;
            model.Powerup(pokemonId);

            Task.Run(async () => { await UpgradeSinglePokemonTask.Execute(Session, pokemonId, false, 1 /* Only upgrade 1 time */); });
        }

        private void btnPokedexView_Click(object sender, RoutedEventArgs e)
        {
            PokedexWindow dexWindow = new PokedexWindow(this.Session);
            dexWindow.Show();
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog()
            {
                Filter = "Excel (.xlsx)|*.xlsx",
            };

            if (dlg.ShowDialog() == true)
            {
                Dumper.SaveAsExcel(this.Session, dlg.FileName).ContinueWith((t) => { Process.Start(dlg.FileName); });
            }
        }

        private void btnMaxPowerUp_Click(object sender, RoutedEventArgs e)
        {
            var model = this.DataContext as PokemonListViewModel;

            ulong pokemonId = (ulong) ((Button) sender).CommandParameter;
            model.Powerup(pokemonId);

            Task.Run(async () => { await UpgradeSinglePokemonTask.Execute(Session, pokemonId, true); });
        }

        private void mnuBuddy_Click(object sender, RoutedEventArgs e)
        {
            //Get the clicked MenuItem
            var menuItem = (MenuItem) sender;

            //Get the ContextMenu to which the menuItem belongs
            var contextMenu = (ContextMenu) menuItem.Parent;

            //Find the placementTarget
            var item = (DataGrid) contextMenu.PlacementTarget;

            //Get the underlying item, that you cast to your object that is bound
            //to the DataGrid (and has subject and state as property)
            var buddySelect = (PokemonDataViewModel) item.SelectedCells[0].Item;

            Task.Run(async () =>
            {
                await SelectBuddyPokemonTask.Execute(
                    this.Session,
                    this.Session.CancellationTokenSource.Token,
                    buddySelect.Id);
            });
        }

        private void applyFilter_Click(object sender, RoutedEventArgs e)
        {
            var model = (PokemonListViewModel)this.DataContext;

            model.ApplyFilter();
        }

        private void applySearchSelect_Click(object sender, RoutedEventArgs e)
        {
            var model = (PokemonListViewModel)this.DataContext;

            model.ApplyFilter(true);
        }

        private void mnuTransferSetting_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;

            //Get the ContextMenu to which the menuItem belongs
            var contextMenu = (ContextMenu)menuItem.Parent;

            //Find the placementTarget
            var item = (DataGrid)contextMenu.PlacementTarget;
            var selectItem = (PokemonDataViewModel)item.SelectedCells[0].Item;
            var filter = TinyIoCContainer.Current.Resolve<ISession>().LogicSettings.PokemonsTransferFilter.GetFilter<TransferFilter>(selectItem.PokemonId);
            var setting = new FilterSetting(selectItem.PokemonId, filter, "PokemonsTransferFilter", (id, f)=> {

                var globalSettings = GlobalSettings.Load("", false);
                FilterUtil.UpdateFilterSetting<TransferFilter>(globalSettings,globalSettings.PokemonsTransferFilter, id, (TransferFilter)f);
                
            });
            setting.ShowDialog();
        }

        private void MenuEvolve_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;

            //Get the ContextMenu to which the menuItem belongs
            var contextMenu = (ContextMenu)menuItem.Parent;

            //Find the placementTarget
            var item = (DataGrid)contextMenu.PlacementTarget;
            var selectItem = (PokemonDataViewModel)item.SelectedCells[0].Item;
            var filter = TinyIoCContainer.Current.Resolve<ISession>().LogicSettings.PokemonEvolveFilters.GetFilter<EvolveFilter>(selectItem.PokemonId);
            var setting = new FilterSetting(selectItem.PokemonId, filter, "PokemonEvolveFilter", (id, f) => {

                var globalSettings = GlobalSettings.Load("", false);
                FilterUtil.UpdateFilterSetting<EvolveFilter>(globalSettings, globalSettings.PokemonEvolveFilter, id, (EvolveFilter)f);

            });
            setting.ShowDialog();

        }

        private void MenuUpgrade_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;

            //Get the ContextMenu to which the menuItem belongs
            var contextMenu = (ContextMenu)menuItem.Parent;

            //Find the placementTarget
            var item = (DataGrid)contextMenu.PlacementTarget;
            var selectItem = (PokemonDataViewModel)item.SelectedCells[0].Item;
            var filter = TinyIoCContainer.Current.Resolve<ISession>().LogicSettings.PokemonUpgradeFilters.GetFilter<UpgradeFilter>(selectItem.PokemonId);
            var setting = new FilterSetting(selectItem.PokemonId, filter, "PokemonUpgradeFilters", (id, f) => {

                var globalSettings = GlobalSettings.Load("", false);
                FilterUtil.UpdateFilterSetting<UpgradeFilter>(globalSettings, globalSettings.PokemonUpgradeFilters, id, (UpgradeFilter)f);

            });
            setting.ShowDialog();
        }

        private void MenuSnipe_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;

            //Get the ContextMenu to which the menuItem belongs
            var contextMenu = (ContextMenu)menuItem.Parent;

            //Find the placementTarget
            var item = (DataGrid)contextMenu.PlacementTarget;
            var selectItem = (PokemonDataViewModel)item.SelectedCells[0].Item;
            var filter = TinyIoCContainer.Current.Resolve<ISession>().LogicSettings.PokemonSnipeFilters.GetFilter<SnipeFilter>(selectItem.PokemonId);
            var setting = new FilterSetting(selectItem.PokemonId, filter, "SnipePokemonFilter", (id, f) => {

                var globalSettings = GlobalSettings.Load("", false);
                FilterUtil.UpdateFilterSetting<SnipeFilter>(globalSettings, globalSettings.SnipePokemonFilter, id, (SnipeFilter)f);

            });
            setting.ShowDialog();

        }

        //ICommand transferPokemonCommand;
        //public ICommand TransferPokemonCommand
        //{
        //    get
        //    {
        //        if (transferPokemonCommand == null)
        //        {
        //            transferPokemonCommand = new RelayCommand(param => this.ShowCustomer());
        //        }
        //        return transferPokemonCommand;
        //    }
        //}
    }
}