using System;
using System.Threading;
using System.Threading.Tasks;
using GeoCoordinatePortable;
using PoGo.NecroBot.Logic.State;
using POGOProtos.Networking.Responses;

namespace PoGo.NecroBot.Logic.Service.Elevation
{
    public interface IElevationService
    {
        string GetServiceId();
        double GetElevation(double lat, double lng);
    }
}