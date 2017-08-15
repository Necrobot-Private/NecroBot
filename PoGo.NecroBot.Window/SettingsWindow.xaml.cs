using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Windows.Controls.Primitives;
using POGOProtos.Enums;
using PoGo.NecroBot.Logic.Model.Settings;
using PoGo.NecroBot.Window.Converters;
using System.Collections.ObjectModel;
using PoGo.NecroBot.Logic.Common;
using TinyIoC;

namespace PoGo.NecroBot.Window
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class SettingsWindow : MetroWindow
    {

        public GlobalSettings Settings { get; set; }
        MetroWindow main;
        private string fileName;
        public SettingsWindow(MetroWindow parent, string filename)
        {
            main = parent;
            Settings = GlobalSettings.Load(filename);
            BackwardCompitableUpdate(Settings);
            fileName = filename;
            InitializeComponent();
            InitForm();
            WindowState = WindowState.Maximized;
        }
        private static void BackwardCompitableUpdate(GlobalSettings setting)
        {
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
        }
        public SettingsWindow()
        {
            InitializeComponent();
            InitForm();
            WindowState = WindowState.Maximized;
        }

        Dictionary<FieldInfo, object> map = new Dictionary<FieldInfo, object>();

        public UIElement BuildDictionaryForm<Y, T>(FieldInfo pi, Dictionary<Y, T> dictionary)
        {
            var natt = pi.GetCustomAttribute<NecroBotConfigAttribute>();

            string resKey = $"Setting.{pi.Name}";

            ObservablePairCollection<Y, T> dataSource = new ObservablePairCollection<Y, T>(dictionary);
            map.Add(pi, dataSource);

            DataGrid grid = new DataGrid()
            {
                IsReadOnly = false,
                AutoGenerateColumns = false
            };
            grid.ItemsSource = dataSource;

            var col1 = new DataGridComboBoxColumn() { Header = translator.PokemonName };
            col1.ItemsSource = Enum.GetValues(typeof(Y)).Cast<Y>();

            col1.SelectedItemBinding = new Binding("Key")
            {
                Mode = BindingMode.TwoWay
            };
            grid.Columns.Add(col1);
            var type = typeof(T);
            foreach (var item in type.GetProperties())
            {
                var att = item.GetCustomAttribute<NecroBotConfigAttribute>(true);
                if (att != null && !att.IsPrimaryKey)
                {
                    string headerKey = $"{resKey}.{item.Name}";
                    var dataGridControl = GetDataGridInputControl(item);
                    dataGridControl.Header = translator.GetTranslation(headerKey);

                    grid.Columns.Add(dataGridControl);
                }
            }
            return grid;
        }

        public UIElement BuildListObjectForm<T>(FieldInfo pi, List<T> list)
        {
            ObservableCollection<T> dataSource = new ObservableCollection<T>(list);

            DataGrid grid = new DataGrid()
            {
                IsReadOnly = false,
                AutoGenerateColumns = true
            };
            grid.ItemsSource = dataSource;


            var type = typeof(T);
            foreach (var item in type.GetProperties())
            {
                var att = item.GetCustomAttribute<NecroBotConfigAttribute>(true);
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

            return setting;

        }
        private DataGridColumn GetDataGridInputControl(PropertyInfo item)
        {
            var att = item.GetCustomAttribute<NecroBotConfigAttribute>(true);

            var binding = new Binding($"Value.{item.Name}")
            {
                Mode = BindingMode.TwoWay
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

            if (item.PropertyType == typeof(List<PokemonId>))
            {
                binding.Converter = new ListPokemonIdToTextConverter();

                DataGridTextColumn txt = new DataGridTextColumn()
                {
                    Binding = binding,
                    Header = header,
                    Width = 220,
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
            DataGridTemplateColumn accountColumn = new DataGridTemplateColumn()
            {
                Header = "Moves"
            };
            Binding bind = new Binding("Moves")
            {
                Mode = BindingMode.OneWay
            };

            // Create the TextBlock
            FrameworkElementFactory textFactory = new FrameworkElementFactory(typeof(TextBlock));
            textFactory.SetBinding(TextBlock.TextProperty, bind);
            DataTemplate textTemplate = new DataTemplate()
            {
                VisualTree = textFactory
            };

            // Create the ComboBox
            Binding comboBind = new Binding("Move")
            {
                Mode = BindingMode.OneWay
            };
            FrameworkElementFactory comboFactory = new FrameworkElementFactory(typeof(ComboBox));
            comboFactory.SetValue(ItemsControl.IsTextSearchEnabledProperty, true);
            // comboFactory.SetValue(ComboBox.ItemsSourceProperty, this.Accounts);
            comboFactory.SetBinding(ComboBox.SelectedItemProperty, comboBind);

            DataTemplate comboTemplate = new DataTemplate()
            {
                VisualTree = comboFactory
            };

            // Set the Templates to the Column
            accountColumn.CellTemplate = textTemplate;
            accountColumn.CellEditingTemplate = comboTemplate;

            return accountColumn;
        }
        public UIElement BuildForm(FieldInfo fi, object source, NecroBotConfigAttribute propAtt)
        {
            var type = source.GetType();

            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Dictionary<,>)))
            {
                Type keyType = type.GetGenericArguments()[0];
                Type valueType = type.GetGenericArguments()[1];

                MethodInfo method = typeof(SettingsWindow).GetMethod("BuildDictionaryForm");
                MethodInfo genericMethod = method.MakeGenericMethod(keyType, valueType);
                return (UIElement)genericMethod.Invoke(this, new object[] { fi, source });
                
            }

            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(List<>)))
            {
                Type objectType= type.GetGenericArguments()[0];
                //Type valueType = type.GetGenericArguments()[1];

                MethodInfo method = typeof(SettingsWindow).GetMethod("BuildListObjectForm");
                MethodInfo genericMethod = method.MakeGenericMethod(objectType);
                return (UIElement)genericMethod.Invoke(this, new object[] { fi, source });
            }

            return BuildObjectForm(fi,source, propAtt);

        }
        public UIElement BuildObjectForm(FieldInfo fi,object source, NecroBotConfigAttribute configAttibute)
        {

            StackPanel panelWrap = new StackPanel() {
              
            };

            Border border= new Border()
            {
                BorderBrush = Brushes.CadetBlue,
                
                BorderThickness = new Thickness(2, 2, 3, 3)
            };

            StackPanel panel = new StackPanel() {
                Margin = new Thickness(20, 20, 20, 20),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            border.Child = panel;
            panelWrap.Children.Add(border);

            var type = source.GetType();

            var fieldName =  type.Name;

            foreach (var item in type.GetProperties())
            {
                var att = item.GetCustomAttributes<NecroBotConfigAttribute>(true).FirstOrDefault();
                if (att != null)
                {

                    string resKey = $"Setting.{fi.Name}.{item.Name}";
                    string DescKey = $"Setting.{fi.Name}.{item.Name}Desc";

                    panel.Children.Add(new Label() { Content = translator.GetTranslation(resKey), FontSize = 15, ToolTip = translator.GetTranslation(DescKey)});
                    panel.Children.Add(GetInputControl(item, source));
                }
            }

            return panelWrap;
        }

        private UIElement GetInputControl(PropertyInfo item, object source)
        {
            Binding binding = new Binding()
            {
                Source = source,  // view model?
                Path = new PropertyPath(item.Name),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
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
                BindingOperations.SetBinding(ddrop, Selector.SelectedValueProperty, binding);
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
                BindingOperations.SetBinding(checkbox, ToggleButton.IsCheckedProperty, binding);
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

            if(item.PropertyType == typeof(List<PokemonId>))
            {
                var  txt = new TextBox()
                {
                    MinWidth = 600,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                binding.Converter = new ListPokemonIdConverter();
                BindingOperations.SetBinding(txt, TextBox.TextProperty, binding);
                return txt;
            }
            return new TextBox();
        }

        private UITranslation translator; 

        public void InitForm()
        {
            translator = TinyIoCContainer.Current.Resolve<UITranslation>();
            DataContext = Settings;
            foreach (var item in Settings.GetType().GetFields())
            {
                var att = item.GetCustomAttributes<NecroBotConfigAttribute>(true).FirstOrDefault();
                if (att != null)
                {
                    string resKey = $"Setting.{item.Name}";

                    string name = string.IsNullOrEmpty(att.Key) ? item.Name : att.Key;
                    var tabItem = new TabItem() { Content = BuildForm(item, item.GetValue(Settings), att), FontSize = 11, Header = translator.GetTranslation(resKey) };

                    tabControl.Items.Add(tabItem);
                }
            }
        }

        private void BtnSave_click(object sender, RoutedEventArgs e)
        {
            foreach (var item in Settings.GetType().GetFields())
            {
                var att = item.GetCustomAttributes<NecroBotConfigAttribute>(true).FirstOrDefault();
                if (att != null)
                {
                    var type = item.FieldType;

                    if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Dictionary<,>)))
                    {
                        var obj = map[item];
                        Type keyType = type.GetGenericArguments()[0];
                        Type valueType = type.GetGenericArguments()[1];

                        var t = typeof(ObservablePairCollection<,>);
                        var genericType = t.MakeGenericType(keyType, valueType);
                        var method = genericType.GetMethod("GetDictionary");
                        var dict = method.Invoke(obj, null);
                        item.SetValue(Settings, dict);
                    }
                }
            }
            //code to back compatable.
            var backCombitable = ConvertToBackwardCompitable(Settings);
            if (!string.IsNullOrEmpty(fileName))  {
                if (MessageBox.Show($"Do you want to overwrite file : {fileName}", "Save Config", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                {
                    Settings.Save(fileName);
                    Close();
                }
            }

        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            main.Visibility =  Visibility.Visible;
        }
    }
}
