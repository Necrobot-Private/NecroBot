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
using PokemonGo.RocketAPI.Extensions;
using PoGo.NecroBot.Logic.Utils;
using POGOProtos.Enums;

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

        internal void Reset()
        {
            foreach (var item in this.allMarkers)
            {
                gmap.Markers.Remove(item.Value);
                item.Value.Shape = null;
                item.Value.Clear();
            }
            this.allMarkers = new Dictionary<string, GMapMarker>();
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
            gmap.DragButton = MouseButton.Left;
            gmap.MapProvider = GoogleMapProvider.Instance;
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            //gmap.SetPositionByKeywords("Melbourne, 3000");
            gmap.Position = new PointLatLng(54.6961334816182, 25.2985095977783);
            gmap.Zoom = 16;
            gmap.ShowCenter = false;

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
            //remove pokemon not in the list
            List<GMapMarker> needRemoveMarkers = new List<GMapMarker>();

            foreach (var item in this.nearbyPokemonMarkers)
            {
                var pokeMarker = item.Shape as MapPokemonMarker;
                if (ev.NearbyPokemons.Any(x => pokeMarker.IsMarkerOf(x.EncounterId.ToString()))) continue;

                needRemoveMarkers.Add(item);
            }
            //remove map markers
            foreach (var item in needRemoveMarkers)
            {
                this.gmap.Markers.Remove(item);
            }
              
            model.NearbyPokemons.RemoveAll(x => needRemoveMarkers.Any(y => ((MapPokemonMarker)y.Shape).IsMarkerOf(x.EncounterId.ToString())));
            this.nearbyPokemonMarkers.RemoveAll(x=> needRemoveMarkers.Contains(x));
            
            foreach (var item in ev.NearbyPokemons)
            {
                var fort = ev.Forts.FirstOrDefault(x => x.Id == item.FortId);
                AddNearByPokemonMarker(item, fort);
            }
        }
        List<GMapMarker> nearbyPokemonMarkers = new List<GMapMarker>();

        private void AddNearByPokemonMarker(NearbyPokemon item, FortData fort)
        {
            var existing = model.NearbyPokemons.FirstOrDefault(x => x.EncounterId == item.EncounterId);
            if (existing != null) return;

            var nearbyModel = new MapPokemonViewModel(item, fort);

            var marker = new GMapMarker(new PointLatLng(nearbyModel.Latitude, nearbyModel.Longitude) );
            marker.Shape = new MapPokemonMarker(null, marker, Session, nearbyModel);
            nearbyPokemonMarkers.Add(marker);
            gmap.Markers.Add(marker);
            this.model.NearbyPokemons.Add(nearbyModel);
          
        }

        private GMapMarker playerMarker;

        internal void MarkFortAsLooted(FortData fortData)
        {
            if (allMarkers.ContainsKey(fortData.Id))
            {
                GMapMarker marker = allMarkers[fortData.Id];
                var fort = this.forts.Where(x => x.Id == fortData.Id).FirstOrDefault();
                if (fort.Type == FortType.Checkpoint)
                {
                    if (marker.Shape is FortMarker)
                    {
                        ((FortMarker)marker.Shape).UpdateFortData(fortData);
                    }
                }
            }
        }

        //var track = new List<PointLatLngpos
        public void UpdatePlayerPosition(double lat, double lng)
        {
            var distance = LocationUtils.CalculateDistanceInMeters(lat, lng, Session.Settings.DefaultLatitude, Session.Settings.DefaultLongitude);
            //Snipe location update, return without update
            if (distance > 5000) return;

            if (playerMarker == null)
            {
                playerMarker = new GMapMarker(new PointLatLng(lat, lng));
                playerMarker.ZIndex = 9999;
                playerMarker.Shape = new PlayerMarker(null, playerMarker, $"");
                this.gmap.Markers.Add(this.playerMarker);
            }

            this.playerMarker.Position = new PointLatLng(lat, lng);

            foreach (var item in this.allMarkers)
            {
                if(item.Value.Shape is FortMarker)
                {
                    var fortMaker = item.Value.Shape as FortMarker;
                    fortMaker.UpdateDistance(lat, lng);
                }

            }
        }

        private void AddPokestopMarker(FortData item)
        {
            var existingFort = this.forts.FirstOrDefault(x => x.Id == item.Id);
            if (existingFort == null)
            {
                this.forts.Add(item);

                var m = new GMapMarker(new PointLatLng()
                {
                    Lat = item.Latitude,
                    Lng = item.Longitude,
                });

                this.Dispatcher.Invoke(() =>
                {
                    switch (item.Type)
                    {
                        case FortType.Checkpoint:
                            m.Shape = new FortMarker(null, m, item);
                            break;
                        case FortType.Gym:
                           
                            m.Shape = new GymMarker(null, m, item);

                            break;
                    }
                    allMarkers.Add(item.Id, m);
                    gmap.Markers.Add(m);
                });
            }
            else
            {
                // Update state of fort
                GMapMarker gmapMarker = allMarkers[item.Id];
                if (gmapMarker.Shape is FortMarker)
                {
                    ((FortMarker)gmapMarker.Shape).UpdateFortData(item);
                }
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

        private async void btnWalkHere_Click(object sender, RoutedEventArgs e)
        {
            await SetMoveToTargetTask.Execute(model.CurrentLatitude, model.CurrentLongitude);
            
            popSelect.IsOpen = false;
        }

        internal void HandleEncounterEvent(EncounteredEvent encounteredEvent)
        {
            if (encounteredEvent.IsRecievedFromSocket) return;
            var marker = this.nearbyPokemonMarkers.FirstOrDefault(x => ((MapPokemonMarker)x.Shape).IsMarkerOf(encounteredEvent.EncounterId));
            if(marker != null)
            {
                this.Dispatcher.Invoke(() =>
                {
                    gmap.Markers.Remove(marker);
                    this.nearbyPokemonMarkers.Remove(marker);
                    var find = this.model.NearbyPokemons.FirstOrDefault(x => x.EncounterId.ToString() == encounteredEvent.EncounterId);
                    if (find != null)
                        this.model.NearbyPokemons.Remove(find);
                });
            }
        }

        private void mnuClear_Click(object sender, RoutedEventArgs e)
        {
            foreach (var nearby in this.nearbyPokemonMarkers)
            {
                gmap.Markers.Remove(nearby);
            };
            this.model.NearbyPokemons.Clear();
        }

        private void mnuZoomOut_Click(object sender, RoutedEventArgs e)
        {
            if (gmap.Zoom <= gmap.MinZoom) return;

            gmap.Zoom--;
        }

        private void mnuZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if (gmap.Zoom >= gmap.MaxZoom) return;

            gmap.Zoom++;
        }
    }
}