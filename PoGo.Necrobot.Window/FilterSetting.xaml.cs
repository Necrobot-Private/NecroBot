using MahApps.Metro.Controls;
using PoGo.Necrobot.Window.Model;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Model.Settings;
using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TinyIoC;

namespace PoGo.Necrobot.Window
{
    /// <summary>
    /// Interaction logic for FilterSetting.xaml
    /// </summary>
    public partial class FilterSetting : MetroWindow
    {
        UITranslation translator;
        string FilterName;
        IPokemonFilter data;
        PokemonId PokemonId;
        ObservableCollectionExt<AffectPokemonViewModel> pokemonList;
        Action<PokemonId, IPokemonFilter> onSaveFilter;
        public FilterSetting(PokemonId pokemonId, IPokemonFilter filter, string filterName, Action<PokemonId, IPokemonFilter> onSave)
        {
            this.data = filter;
            translator = TinyIoCContainer.Current.Resolve<UITranslation>();

            this.Title = string.Format(translator.TransferFilterFormTitle, pokemonId.ToString());

            this.PokemonId = pokemonId;
            this.FilterName = filterName;
            this.onSaveFilter = onSave;
            this.DataContext = data;
            InitializeComponent();
            pnlFilters.Children.Add(BuildObjectForm(filter));
            DisplayListPokemons(filter.AffectToPokemons);
        }

        private void DisplayListPokemons(List<PokemonId> affectToPokemons)
        {
            this.pokemonList = new ObservableCollectionExt<AffectPokemonViewModel>();

            foreach (var item in Enum.GetValues(typeof(PokemonId)))
            {
                if ((PokemonId)item == PokemonId.Missingno) continue;
                this.pokemonList.Add(new AffectPokemonViewModel()
                {
                    Pokemon = (PokemonId)item,
                    Selected = affectToPokemons != null && affectToPokemons.Contains((PokemonId)item)
                });
            }
            this.lslAllPokemons.ItemsSource = this.pokemonList;
        }

        public UIElement BuildObjectForm(IPokemonFilter source)
        {

            StackPanel panelWrap = new StackPanel();
            Border border = new Border()
            {
                BorderBrush = Brushes.CadetBlue,
                BorderThickness = new Thickness(2, 2, 3, 3)
            };

            StackPanel panel = new StackPanel()
            {
                Margin = new Thickness(20, 20, 20, 20),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            border.Child = panel;
            panelWrap.Children.Add(border);

            var type = source.GetType();

            var fieldName = type.Name;

            foreach (var item in type.GetProperties())
            {
                if (item.Name == "AffectToPokemons") continue;

                var att = item.GetCustomAttributes<NecrobotConfigAttribute>(true).FirstOrDefault();
                if (att != null && !att.HiddenOnGui)
                {
                    string resKey = $"Setting.{FilterName}.{item.Name}";
                    string DescKey = $"Setting.{FilterName}.{item.Name}Desc";

                    panel.Children.Add(new Label() { Content = translator.GetTranslation(resKey), FontSize = 15, ToolTip = translator.GetTranslation(DescKey) });
                    panel.Children.Add(GetInputControl(item, source));
                }
            }

            return panelWrap;
        }

        private UIElement GetInputControl(PropertyInfo item, object source)
        {
            Binding binding = new Binding();
            binding.Source = source;  // view model?
            binding.Path = new PropertyPath(item.Name);
            binding.Mode = BindingMode.TwoWay;
            binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            var enumDataTypeAtt = item.GetCustomAttribute<EnumDataTypeAttribute>(true);
            if (enumDataTypeAtt != null)
            {

                ComboBox ddrop = new ComboBox()
                {
                    AllowDrop = true,
                    Width = 150,
                    HorizontalAlignment = HorizontalAlignment.Left

                };
                foreach (var v in Enum.GetValues(enumDataTypeAtt.EnumType))
                {
                    ddrop.Items.Add(v.ToString());
                }
                BindingOperations.SetBinding(ddrop, ComboBox.SelectedValueProperty, binding);
                return ddrop;
            }

            if (item.PropertyType == typeof(string))
            {
                var textbox = new TextBox()
                {
                    MaxWidth = 400
                };

                BindingOperations.SetBinding(textbox, TextBox.TextProperty, binding);
                return textbox;
            }

            if (item.PropertyType == typeof(bool))
            {
                var checkbox = new CheckBox();
                BindingOperations.SetBinding(checkbox, CheckBox.IsCheckedProperty, binding);
                return checkbox;
            }

            if (item.PropertyType == typeof(int) || item.PropertyType == typeof(double))
            {
                var range = item.GetCustomAttributes<RangeAttribute>(true).FirstOrDefault();
                if (range != null)
                {
                    NumericUpDown numberic = new NumericUpDown()
                    {
                        Minimum = Convert.ToDouble(range.Minimum),
                        Maximum = Convert.ToDouble(range.Maximum),
                        InterceptArrowKeys = true,
                        Interval = 10,
                        Width = 150,
                        HorizontalAlignment = HorizontalAlignment.Left,
                    };

                    BindingOperations.SetBinding(numberic, NumericUpDown.ValueProperty, binding);
                    return numberic;
                }

                var numberTextbox = new TextBox()
                {
                    Width = 150,
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                BindingOperations.SetBinding(numberTextbox, TextBox.TextProperty, binding);
                return numberTextbox;
            }

            return new TextBox();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            data.AffectToPokemons = this.pokemonList.Where(x => x.Selected).Select(x => x.Pokemon).ToList();
            this.onSaveFilter(PokemonId, data);
            this.Close();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var cmd = (AffectPokemonViewModel)button.CommandParameter;
            cmd.Selected = !cmd.Selected;
            cmd.RaisePropertyChanged("Selected");
        }

        private void PokemonSelected_Click(object sender, RoutedEventArgs e)
        {
            data.AffectToPokemons = this.pokemonList.Where(x => x.Selected).Select(x => x.Pokemon).ToList();
        }
    }
}
