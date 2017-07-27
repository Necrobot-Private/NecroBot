﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using GMap.NET.WindowsPresentation;

namespace PoGo.NecroBot.Window.Controls.MapMarkers
{
   /// <summary>
   /// Interaction logic for CustomMarkerDemo.xaml
   /// </summary>
   public partial class PlayerMarker
    {
      Popup Popup;
      Label Label;
      GMapMarker Marker;
      MainClientWindow MainWindow;

      public PlayerMarker(MainClientWindow window, GMapMarker marker, string title)
      {
            InitializeComponent();

            MainWindow = window;
            Marker = marker;

         Popup = new Popup();
         Label = new Label();

            Loaded += new RoutedEventHandler(CustomMarkerDemo_Loaded);
            SizeChanged += new SizeChangedEventHandler(CustomMarkerDemo_SizeChanged);
            MouseEnter += new MouseEventHandler(MarkerControl_MouseEnter);
            MouseLeave += new MouseEventHandler(MarkerControl_MouseLeave);
            MouseMove += new MouseEventHandler(CustomMarkerDemo_MouseMove);
            MouseLeftButtonUp += new MouseButtonEventHandler(CustomMarkerDemo_MouseLeftButtonUp);
            MouseLeftButtonDown += new MouseButtonEventHandler(CustomMarkerDemo_MouseLeftButtonDown);

         Popup.Placement = PlacementMode.Mouse;
         {
            Label.Background = Brushes.Blue;
            Label.Foreground = Brushes.White;
            Label.BorderBrush = Brushes.WhiteSmoke;
            Label.BorderThickness = new Thickness(2);
            Label.Padding = new Thickness(5);
            Label.FontSize = 22;
            Label.Content = title;
         }
         Popup.Child = Label;
      }

      void CustomMarkerDemo_Loaded(object sender, RoutedEventArgs e)
      {
         if(icon.Source.CanFreeze)
         {
            icon.Source.Freeze();
         }
      }

      void CustomMarkerDemo_SizeChanged(object sender, SizeChangedEventArgs e)
      {
         Marker.Offset = new Point(-e.NewSize.Width/2, -e.NewSize.Height);
      }

      void CustomMarkerDemo_MouseMove(object sender, MouseEventArgs e)
      {
         if(e.LeftButton == MouseButtonState.Pressed && IsMouseCaptured)
         {
           // Point p = e.GetPosition(MainWindow.MainMap);
            //Marker.Position = MainWindow.MainMap.FromLocalToLatLng((int) p.X, (int) p.Y);
         }
      }

      void CustomMarkerDemo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
      {
         if(!IsMouseCaptured)
         {
            Mouse.Capture(this);
         }
      }

      void CustomMarkerDemo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
      {
         if(IsMouseCaptured)
         {
            Mouse.Capture(null);
         }
      }

      void MarkerControl_MouseLeave(object sender, MouseEventArgs e)
      {
         Marker.ZIndex -= 10000;
         Popup.IsOpen = false;
      }

      void MarkerControl_MouseEnter(object sender, MouseEventArgs e)
      {
         Marker.ZIndex += 10000;
         Popup.IsOpen = true;
      }
   }
}