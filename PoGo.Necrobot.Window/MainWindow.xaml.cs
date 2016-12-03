using MahApps.Metro.Controls;
using PoGo.NecroBot.Logic.Model.Settings;
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

namespace PoGo.Necrobot.Window
{
       /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            InitForm();
            this.WindowState = WindowState.Maximized;

        }
        public void InitForm1()
        {
            GlobalSettings setting = new GlobalSettings();
            foreach (var item in setting.GetType().GetFields())
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
        public StackPanel BuildObjectForm(object source)
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
            if(enumDataTypeAtt != null)
            {
                DropDownButton ddrop = new DropDownButton()
                {
                    AllowDrop = true,
                    Width = 150,
                    HorizontalAlignment = HorizontalAlignment.Left
                    
                };
                foreach (var v in Enum.GetValues(enumDataTypeAtt.EnumType))
                {
                    ddrop.Items.Add(v.ToString());

                }
                //BindingOperations.SetBinding(ddrop, DropDownButton.selec)
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

        GlobalSettings setting;
        public void InitForm()
        {
            setting = new GlobalSettings();
            this.DataContext = setting;
            foreach (var item in setting.GetType().GetFields())
            {
                var att = item.GetCustomAttributes<ExcelConfigAttribute>(true).FirstOrDefault();
                if (att != null)
                {
                    string name = string.IsNullOrEmpty(att.Key) ? item.Name : att.Key;
                    var tabItem = new TabItem() { Content = BuildObjectForm(item.GetValue(setting)), Header = name, FontSize = 10 };

                    tabControl.Items.Add(tabItem);


                    //var button = new Button()
                    //{
                    //    Content = name,
                    //    HorizontalAlignment = HorizontalAlignment.Left,
                    //    Width = 128
                    //};
                    //DockPanel.SetDock(button, Dock.Top);
                    //stackProps.Children.Add(button);
                }
            }
        }


        private void button_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
