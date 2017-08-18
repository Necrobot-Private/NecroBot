using System.Linq;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using PoGo.NecroBot.Window.Controls.MapMarkers;
using PoGo.NecroBot.Window.Model;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Tasks;
using POGOProtos.Map.Fort;
using PoGo.NecroBot.Logic.Event;
using POGOProtos.Map.Pokemon;
using PoGo.NecroBot.Logic.Utils;
using PoGo.NecroBot.Window.Properties;

namespace PoGo.NecroBot.Window.Controls
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
            forts = new List<FortData>();
            InitMap();
            model = DataContext as MapViewModel;
        }

        internal void Reset()
        {
            foreach (var item in allMarkers)
            {
                gmap.Markers.Remove(item.Value);
                item.Value.Shape = null;
                item.Value.Clear();
            }
            allMarkers = new Dictionary<string, GMapMarker>();
        }

        GMapMarker selectedMarker;
        List<PointLatLng> track = new List<PointLatLng>();

        public void SetDefaultPosition(double lat, double lng)
        {
            Dispatcher.Invoke(() =>
            {
                gmap.Position = new PointLatLng(lat, lng);
                gmap.Zoom = Settings.Default.MapZoom;
            });
        }

        public void GetStyle()
        {
            if (Settings.Default.MapMode == "Normal")
                gmap.MapProvider = GoogleMapProvider.Instance;
            else if (Settings.Default.MapMode == "Satellite")
                gmap.MapProvider = GoogleSatelliteMapProvider.Instance;
            gmap.Zoom = Settings.Default.MapZoom;
        }

        public void InitMap()
        {
            GetStyle();
            gmap.DragButton = MouseButton.Left;
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            //gmap.SetPositionByKeywords("Melbourne, 3000");
            gmap.Position = new PointLatLng(54.6961334816182, 25.2985095977783);
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

            foreach (var item in nearbyPokemonMarkers)
            {
                var pokeMarker = item.Shape as MapPokemonMarker;
                if (ev.NearbyPokemons.Any(x => pokeMarker.IsMarkerOf(x.EncounterId.ToString()))) continue;

                needRemoveMarkers.Add(item);
            }
            //remove map markers
            foreach (var item in needRemoveMarkers)
            {
                gmap.Markers.Remove(item);
            }
              
            model.NearbyPokemons.RemoveAll(x => needRemoveMarkers.Any(y => ((MapPokemonMarker)y.Shape).IsMarkerOf(x.EncounterId.ToString())));
            nearbyPokemonMarkers.RemoveAll(x=> needRemoveMarkers.Contains(x));
            
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
            model.NearbyPokemons.Add(nearbyModel);
          
        }

        private GMapMarker playerMarker;

        internal void MarkFortAsLooted(FortData fortData)
        {
            if (allMarkers.ContainsKey(fortData.Id))
            {
                GMapMarker marker = allMarkers[fortData.Id];
                var fort = forts.Where(x => x.Id == fortData.Id).FirstOrDefault();
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
                playerMarker = new GMapMarker(new PointLatLng(lat, lng))
                {
                    ZIndex = 9999
                };
                playerMarker.Shape = new PlayerMarker(null, playerMarker, $"");
                gmap.Markers.Add(playerMarker);
            }

            playerMarker.Position = new PointLatLng(lat, lng);

            foreach (var item in allMarkers)
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
            var existingFort = forts.FirstOrDefault(x => x.Id == item.Id);
            if (existingFort == null)
            {
                forts.Add(item);

                var m = new GMapMarker(new PointLatLng()
                {
                    Lat = item.Latitude,
                    Lng = item.Longitude,
                });

                Dispatcher.Invoke(() =>
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

        private void Gmap_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(gmap);
            var pos = gmap.FromLocalToLatLng((int) p.X, (int) p.Y);
            model.CurrentLatitude = pos.Lat;
            model.CurrentLongitude = pos.Lng;
            var currentXY = gmap.FromLatLngToLocal(selectedMarker.Position);

            //TODO : Need to find the better way to stop event then we can get rid of this hack
            if (Math.Abs(p.X - currentXY.X) > 30 || Math.Abs(p.Y - currentXY.Y) > 30)
            {
                selectedMarker.Position = pos;
                popSelect.IsOpen = false;
            }
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            model = DataContext as MapViewModel;
        }

        private async void BtnWalkHere_Click(object sender, RoutedEventArgs e)
        {
            await SetMoveToTargetTask.Execute(model.CurrentLatitude, model.CurrentLongitude);
            
            popSelect.IsOpen = false;
        }

        internal void HandleEncounterEvent(EncounteredEvent encounteredEvent)
        {
            if (encounteredEvent.IsRecievedFromSocket) return;
            var marker = nearbyPokemonMarkers.FirstOrDefault(x => ((MapPokemonMarker)x.Shape).IsMarkerOf(encounteredEvent.EncounterId));
            if(marker != null)
            {
                Dispatcher.Invoke(() =>
                {
                    gmap.Markers.Remove(marker);
                    nearbyPokemonMarkers.Remove(marker);
                    var find = model.NearbyPokemons.FirstOrDefault(x => x.EncounterId.ToString() == encounteredEvent.EncounterId);
                    if (find != null)
                        model.NearbyPokemons.Remove(find);
                });
            }
        }

        private void MnuClear_Click(object sender, RoutedEventArgs e)
        {
            foreach (var nearby in nearbyPokemonMarkers)
            {
                gmap.Markers.Remove(nearby);
            };
            model.NearbyPokemons.Clear();
        }

        private void MnuZoomOut_Click(object sender, RoutedEventArgs e)
        {
            if (gmap.Zoom <= gmap.MinZoom) return;

            gmap.Zoom--;
            Settings.Default.MapZoom = gmap.Zoom;
            Settings.Default.Save();
        }

        private void MnuZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if (gmap.Zoom >= gmap.MaxZoom) return;

            gmap.Zoom++;
            Settings.Default.MapZoom = gmap.Zoom;
            Settings.Default.Save();
        }
    }
}