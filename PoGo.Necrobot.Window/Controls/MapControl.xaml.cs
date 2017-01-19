using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using PoGo.Necrobot.Window.Controls.MapMarkers;
using PoGo.Necrobot.Window.Model;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Tasks;
using POGOProtos.Map.Fort;
using PoGo.NecroBot.Logic.Event;
using POGOProtos.Map.Pokemon;
using System.Diagnostics;

namespace PoGo.Necrobot.Window.Controls
{
    /// <summary>
    /// Interaction logic for MapControl.xaml
    /// </summary>
    public partial class MapControl : UserControl
    {
        MapViewModel model;
        Dictionary<string, GMapMarker> allMarkers = new Dictionary<string, GMapMarker>();
        public ISession Session { get; set; }

        public MapControl()
        {
            InitializeComponent();
            this.forts = new List<FortData>();
            InitMap();
            this.model = this.DataContext as MapViewModel;
        }

        GMapMarker selectedMarker;
        List<PointLatLng> track = new List<PointLatLng>();

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
            gmap.MapProvider = OpenStreetMapProvider.Instance;
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            //gmap.SetPositionByKeywords("Melbourne, 3000");
            gmap.Position = new PointLatLng(54.6961334816182, 25.2985095977783);
            gmap.Zoom = 16;
            var m = new GMapMarker(gmap.Position);

            m.Shape = new CustomMarkerDemo(null, m, "xxx");
            //var x = new CustomMarkerDemo(null, m, "xxx");
            gmap.Markers.Add(m);

            selectedMarker = new GMapMarker(new PointLatLng(0, 0));
            selectedMarker.Shape = new TargetMarker(null, selectedMarker, popSelect);
            gmap.Markers.Add(selectedMarker);
        }

        private List<FortData> forts;

        internal void OnPokestopEvent(PokeStopListEvent ev)
        {
            if (ev.Forts == null) return;

            foreach (var item in ev.Forts)
            {
                AddPokestopMarker(item);
            }
            if (ev.NearbyPokemons == null) return;
            foreach (var item in ev.NearbyPokemons)
            {
                var fort = ev.Forts.FirstOrDefault(x => x.Id == item.FortId);
                AddNearByPokemonMarker(item, fort);
            }
        }
        List<GMapMarker> nearbyPokemons = new List<GMapMarker>();

        private void AddNearByPokemonMarker(NearbyPokemon item, FortData fort)
        {
            var existing = model.NearbyPokemons.FirstOrDefault(x => x.EncounterId == item.EncounterId);
            if (existing != null) return;

            var nearbyModel = new MapPokemonViewModel(item, fort);

            var marker = new GMapMarker(new PointLatLng(nearbyModel.Latitude, nearbyModel.Longitude) );
            marker.Shape = new MapPokemonMarker(null, marker, Session, nearbyModel);
            nearbyPokemons.Add(marker);
            gmap.Markers.Add(marker);
            this.model.NearbyPokemons.Add(nearbyModel);
          
        }

        private GMapMarker playerMarker;

        internal void MarkFortAsLooted(string id)
        {
            var marker = allMarkers[id];
            marker.Shape = new ImageMarker(null, marker, "", "pokestop-used.png");
        }

        //var track = new List<PointLatLngpos
        public void UpdatePlayerPosition(double lat, double lng)
        {
            if (playerMarker == null)
            {
                playerMarker = new GMapMarker(new PointLatLng(lat, lng));
                playerMarker.ZIndex = 9999;
                playerMarker.Shape = new PlayerMarker(null, playerMarker, $"");
                this.gmap.Markers.Add(this.playerMarker);
            }

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

        private void gmap_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(gmap);
            var pos = gmap.FromLocalToLatLng((int) p.X, (int) p.Y);
            model.CurrentLatitude = pos.Lat;
            model.CurrentLongitude = pos.Lng;
            var currentXY = this.gmap.FromLatLngToLocal(this.selectedMarker.Position);

            //TODO : Need to find the better way to stop event then we can get rid of this hack
            if (Math.Abs(p.X - currentXY.X) > 30 || Math.Abs(p.Y - currentXY.Y) > 30)
            {
                this.selectedMarker.Position = pos;
                popSelect.IsOpen = false;
            }
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.model = DataContext as MapViewModel;
        }

        private void btnWalkHere_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(async () =>
            {
                await SetMoveToTargetTask.Execute(this.Session, model.CurrentLatitude, model.CurrentLongitude);
            });

            popSelect.IsOpen = false;
        }

        internal void HandleEncounterEvent(EncounteredEvent encounteredEvent)
        {
            if (encounteredEvent.IsRecievedFromSocket) return;
            var marker = this.nearbyPokemons.FirstOrDefault(x => ((MapPokemonMarker)x.Shape).IsMarkerOf(encounteredEvent.EncounterId));
            if(marker != null)
            {
                this.Dispatcher.Invoke(() =>
                {
                    gmap.Markers.Remove(marker);
                    this.nearbyPokemons.Remove(marker);
                    var find = this.model.NearbyPokemons.FirstOrDefault(x => x.EncounterId.ToString() == encounteredEvent.EncounterId);
                    if (find != null)
                        this.model.NearbyPokemons.Remove(find);
                });
            }
        }

        private void gmap_LayoutUpdated(object sender, EventArgs e)
        {
            Debug.Write("gmap_LayoutUpdated");
        }

        internal void EnsureContent()
        {
            //foreach (var item in this.allMarkers)
            //{
            //    gmap.Markers.Remove(item.Value);
            //}

            //foreach (var item in this.allMarkers)
            //{
            //    gmap.Markers.Add(item.Value);
            //}
        }
    }
}