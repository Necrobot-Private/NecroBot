using System.Windows.Controls;

namespace PoGo.NecroBot.Window.Controls
{
    /// <summary>
    /// Interaction logic for TrolleyTooltip.xaml
    /// </summary>
    public partial class TrolleyTooltip : UserControl
   {
      public TrolleyTooltip()
      {
         InitializeComponent();
      }

      public void SetValues(string type, object vl)
      {
         //Device.Text = vl.Id.ToString();
         //LineNum.Text = type + " " + vl.Line;
         //StopName.Text = vl.LastStop;
         //TrackType.Text = vl.TrackType;
         //TimeGps.Text = vl.Time;
         //Area.Text = vl.AreaName;
         //Street.Text = vl.StreetName;
      }
   }
}
