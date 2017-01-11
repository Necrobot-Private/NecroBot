using GMap.NET;
using GMap.NET.WindowsPresentation;
using PoGo.Necrobot.Window.Controls.MapMarkers;
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
using POGOProtos.Map.Fort;
using PoGo.NecroBot.Logic.State;

namespace PoGo.Necrobot.Window.Controls
{
    /// <summary>
    /// Interaction logic for MapControl.xaml
    /// </summary>
    public partial class MapControl : UserControl
    {
        Dictionary<string, GMapMarker> allMarkers = new Dictionary<string, GMapMarker>();
        public MapControl()
        {
            InitializeComponent();
            this.forts = new List<FortData>();
            InitMap();
        }

        public void SetDefaultPosition(double lat, double lng)
        {
            this.Dispatcher.Invoke(() =>
            {
                gmap.Position = new PointLatLng(lat, lng);
                gmap.Zoom = 16;
            });

        }
        public void InitMap()
        {
            gmap.MapProvider = GMap.NET.MapProviders.OpenStreetMapProvider.Instance;
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;
            //gmap.SetPositionByKeywords("Melbourne, 3000");
            gmap.Position = new PointLatLng(54.6961334816182, 25.2985095977783);
            gmap.Zoom = 16;
            var m = new GMapMarker(gmap.Position);

            //{
            //if (checkBoxPlace.IsChecked.Value)
            //{
            //    GeoCoderStatusCode status;
            //    var plret = GMapProviders.GoogleMap.GetPlacemark(currentMarker.Position, out status);
            //    if (status == GeoCoderStatusCode.G_GEO_SUCCESS && plret != null)
            //    {
            //        p = plret;
            //    }
            //}
            m.Shape = new CustomMarkerDemo(null, m, "xxx");
            //var x = new CustomMarkerDemo(null, m, "xxx");
            gmap.Markers.Add(m);

        }
        private ISession session;
        private List<FortData> forts;

        internal void OnPokestopEvent(List<FortData> forts)
        {
            foreach (var item in forts)
            {
                AddPokestopMarker(item);
            }
        }
        private GMapMarker playerMarker;

        internal void MarkFortAsLooted(string id)
        {
            var marker = allMarkers[id];
            marker.Shape = new ImageMarker(null, marker, "", "pokestop-used.png");
        }

        public void UpdatePlayerPosition(double lat, double lng)
        {
            if (playerMarker == null)
            {
                playerMarker = new GMapMarker(new PointLatLng(lat, lng));
                playerMarker.ZIndex = 9999;
                playerMarker.Shape = new PlayerMarker(null, playerMarker, $"");
                this.gmap.Markers.Add(this.playerMarker);
            };

            this.playerMarker.Position = new PointLatLng(lat, lng);
        }

        private void AddPokestopMarker(FortData item)
        {
            if (!this.forts.Exists(x => x.Id == item.Id))
            {
                this.forts.Add(item);

                var m = new GMapMarker(new PointLatLng()
                {
                    Lat = item.Latitude,
                    Lng = item.Longitude,
                });

                this.Dispatcher.Invoke(() =>
                {
                    m.Shape = new ImageMarker(null, m, $"[{item.Latitude},{item.Longitude}]", "pokestop-normal.png");
                    allMarkers.Add(item.Id, m);
                    gmap.Markers.Add(m);
                });
            }

        }
    }
}
