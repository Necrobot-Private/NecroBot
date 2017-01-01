using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using POGOProtos.Enums;
using PoGo.NecroBot.Logic;
using PoGo.Necrobot.Window;
using PoGo.NecroBot.Logic.Model.Settings;
using PoGo.Necrobot.Window.Converters;

namespace PoGo.Necrobot.Window
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        public GlobalSettings Settings { get; set; }
        MetroWindow main;
        private string fileName;
        public MainWindow(MetroWindow parent, string filename)
        {
            main = parent;
            this.Settings = GlobalSettings.Load(filename);
            BackwardCompitableUpdate(this.Settings);
            this.fileName = filename;
            InitializeComponent();
            InitForm();
            this.WindowState = WindowState.Maximized;
        }
        private static void BackwardCompitableUpdate(GlobalSettings setting)
        {
            //foreach (var item in setting.PokemonsTransferFilter)
            //{
            //    setting.PokemonsTransferFilter[item].AllowTransfer = true;
            //    item.Value.AllowTransfer = true;
            //}
            foreach (var item in setting.PokemonsNotToTransfer)
            {
                if (setting.PokemonsTransferFilter.ContainsKey(item))
                {
                    setting.PokemonsTransferFilter[item].DoNotTransfer = true;
                }
                else
                {
                    setting.PokemonsTransferFilter.Add(item, new TransferFilter()
                    {
                        AllowTransfer = true,
                        DoNotTransfer = true
                    });
                }
            }
            foreach (var item in setting.PokemonsToEvolve)
            {
                if (!setting.EvolvePokemonFilter.ContainsKey(item))
                {
                    setting.EvolvePokemonFilter.Add(item, new EvolveFilter()
                    {

                    });
                }
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            InitForm();
            this.WindowState = WindowState.Maximized;
        }
        

        public void InitForm1()
        {
            GlobalSettings Settings = new GlobalSettings();
            foreach (var item in Settings.GetType().GetFields())
            {
                var att = item.GetCustomAttributes<ExcelConfigAttribute>(true).FirstOrDefault();
                if (att != null)
                {
                    string name = string.IsNullOrEmpty(att.Key) ? item.Name : att.Key;
                    var button = new Button()
                    {
                        Content = name,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Width = 128
                    };
                    DockPanel.SetDock(button, Dock.Top);
                    //stackProps.Children.Add(button);
                }
            }
        }

        Dictionary<FieldInfo, object> map = new Dictionary<FieldInfo, object>();

        public UIElement BuildDictionaryForm<T>(FieldInfo pi, Dictionary<PokemonId, T> dictionary)
        {
            ObservablePairCollection<PokemonId, T> dataSource = new ObservablePairCollection<PokemonId, T>(dictionary);
            map.Add(pi, dataSource);

            DataGrid grid = new DataGrid()
            {
                IsReadOnly = false,
                AutoGenerateColumns = false
            };
            grid.ItemsSource = dataSource;

            var col1 = new DataGridComboBoxColumn() { Header = "Pokemon Name" };
            col1.ItemsSource = Enum.GetValues(typeof(PokemonId)).Cast<PokemonId>();

            col1.SelectedItemBinding = new Binding("Key")
            {
                Mode = BindingMode.TwoWay
            };
            grid.Columns.Add(col1);
            var type = typeof(T);
            foreach (var item in type.GetProperties())
            {
                var att = item.GetCustomAttribute<ExcelConfigAttribute>(true);
                if (att != null && !att.IsPrimaryKey)
                {
                    var dataGridControl = GetDataGridInputControl(item);
                    grid.Columns.Add(dataGridControl);
                }
            }
            return grid;
        }
        private static GlobalSettings ConvertToBackwardCompitable(GlobalSettings setting)
        {
            if (setting.PokemonsTransferFilter != null)
            {
                setting.PokemonsNotToTransfer = setting.PokemonsTransferFilter.Where(p => p.Value.DoNotTransfer).Select(p => p.Key).ToList();
            }
            setting.PokemonsToEvolve = setting.EvolvePokemonFilter.Select(x => x.Key).ToList();

            if (setting.SnipePokemonFilter != null)
            {
                setting.PokemonToSnipe.Pokemon = setting.SnipePokemonFilter.Select(p => p.Key).ToList();
            }
            return setting;

        }
        private DataGridColumn GetDataGridInputControl(PropertyInfo item)
        {
            var att = item.GetCustomAttribute<ExcelConfigAttribute>(true);

            var binding = new Binding($"Value.{item.Name}")
            {
                Mode = BindingMode.TwoWay,
                
                // Converter = new   ObservableCollectionConverter()
            };
            string header = $"{att.Key}";
            var enumDataTypeAtt = item.GetCustomAttribute<EnumDataTypeAttribute>(true);
            if (enumDataTypeAtt != null)
            {
                binding.Converter = new OperatorConverter();

                DataGridComboBoxColumn ddrop = new DataGridComboBoxColumn()
                {
                    Header = header
                };
                ddrop.ItemsSource = Enum.GetValues(enumDataTypeAtt.EnumType);
                ddrop.SelectedItemBinding = binding;
                ddrop.SelectedValueBinding = binding;
                return ddrop;
            }

            if(item.PropertyType == typeof(List<List<PokemonMove>>))
            {
                binding.Converter = new MoveConverter();

                DataGridTextColumn txt = new DataGridTextColumn()
                {
                    Binding = binding,
                    Header = header,
                    Width = 120,
                    IsReadOnly = false
                };

                return txt;

            }

            if (item.PropertyType == typeof(string) ||
            item.PropertyType == typeof(int) ||
            item.PropertyType == typeof(double))
            {
                DataGridTextColumn txt = new DataGridTextColumn()
                {
                    Binding = binding,
                    Header = header,
                    IsReadOnly = false
                };

                return txt;
            }

            if (item.PropertyType == typeof(bool))
            {
                DataGridCheckBoxColumn checkbox = new DataGridCheckBoxColumn()
                {
                    Binding = binding,
                    Header = header,
                    IsReadOnly = false,
                };

                return checkbox;
            }

            return new DataGridTextColumn()
            {
                Header = header,
                IsReadOnly = false,
            };
        }
        private DataGridTemplateColumn BuildComboMoves()
        {
            // Create The Column
            DataGridTemplateColumn accountColumn = new DataGridTemplateColumn();
            accountColumn.Header = "Moves";

            Binding bind = new Binding("Moves");
            bind.Mode = BindingMode.OneWay;

            // Create the TextBlock
            FrameworkElementFactory textFactory = new FrameworkElementFactory(typeof(TextBlock));
            textFactory.SetBinding(TextBlock.TextProperty, bind);
            DataTemplate textTemplate = new DataTemplate();
            textTemplate.VisualTree = textFactory;

            // Create the ComboBox
            Binding comboBind = new Binding("Move");
            comboBind.Mode = BindingMode.OneWay;

            FrameworkElementFactory comboFactory = new FrameworkElementFactory(typeof(ComboBox));
            comboFactory.SetValue(ComboBox.IsTextSearchEnabledProperty, true);
           // comboFactory.SetValue(ComboBox.ItemsSourceProperty, this.Accounts);
            comboFactory.SetBinding(ComboBox.SelectedItemProperty, comboBind);

            DataTemplate comboTemplate = new DataTemplate();
            comboTemplate.VisualTree = comboFactory;

            // Set the Templates to the Column
            accountColumn.CellTemplate = textTemplate;
            accountColumn.CellEditingTemplate = comboTemplate;

            return accountColumn;
        }
        public UIElement BuildForm(FieldInfo fi, object source)
        {
            var type = source.GetType();

            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Dictionary<,>)))
            {
                Type keyType = type.GetGenericArguments()[0];
                Type valueType = type.GetGenericArguments()[1];

                MethodInfo method = typeof(MainWindow).GetMethod("BuildDictionaryForm");
                MethodInfo genericMethod = method.MakeGenericMethod(valueType);
                return (UIElement)genericMethod.Invoke(this, new object[] { fi, source });
            }
            return BuildObjectForm(source);


        }
        public UIElement BuildObjectForm(object source)
        {
            StackPanel panel = new StackPanel();
            var type = source.GetType();
            foreach (var item in type.GetProperties())
            {
                var att = item.GetCustomAttributes<ExcelConfigAttribute>(true).FirstOrDefault();
                if (att != null)
                {
                    panel.Children.Add(new Label() { Content = item.Name, FontSize = 15, ToolTip = att.Description });
                    panel.Children.Add(GetInputControl(item, source));
                }
            }

            return panel;
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

        public void InitForm()
        {
            this.DataContext = Settings;
            foreach (var item in Settings.GetType().GetFields())
            {
                var att = item.GetCustomAttributes<ExcelConfigAttribute>(true).FirstOrDefault();
                if (att != null)
                {
                    string name = string.IsNullOrEmpty(att.Key) ? item.Name : att.Key;
                    var tabItem = new TabItem() { Content = BuildForm(item, item.GetValue(Settings)), Header = name, FontSize = 11 };
                    tabControl.Items.Add(tabItem);
                }
            }
        }

        private void btnSave_click(object sender, RoutedEventArgs e)
        {
            foreach (var item in Settings.GetType().GetFields())
            {
                var att = item.GetCustomAttributes<ExcelConfigAttribute>(true).FirstOrDefault();
                if (att != null)
                {
                    var type = item.FieldType;

                    if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Dictionary<,>)))
                    {
                        var obj = map[item];
                        Type keyType = type.GetGenericArguments()[0];
                        Type valueType = type.GetGenericArguments()[1];

                        var t = typeof(Window.ObservablePairCollection<,>);
                        var genericType = t.MakeGenericType(keyType, valueType);
                        var method = genericType.GetMethod("GetDictionary");
                        var dict = method.Invoke(obj, null);
                        item.SetValue(Settings, dict);
                    }
                }
            }
            //code to back compitable.
            var backCombitable = ConvertToBackwardCompitable(this.Settings);
            if (!string.IsNullOrEmpty(fileName))  {
                if (MessageBox.Show("Do you want to overwrite file : ", "Save config", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                {
                    Settings.Save(fileName);
                    this.Close();

                }
            }

        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            main.Visibility =  Visibility.Visible;
           // this.Owner.Visibility = Visibility.Visible;
        }
    }
}
