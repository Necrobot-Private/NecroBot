using System;
using System.Threading;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Model;
using PoGo.NecroBot.Logic.State;
using GeoCoordinatePortable;
using System.Collections.Generic;

namespace PoGo.NecroBot.Logic.Strategies.Walk
{
    public interface IWalkStrategy
    {
        string RouteName { get; }
        List<GeoCoordinate> Points { get; set; }
        event UpdatePositionDelegate UpdatePositionEvent;
        event GetRouteDelegate GetRouteEvent;

        Task Walk(IGeoLocation destinationLocation, Func<Task> functionExecutedWhileWalking,
            ISession session, CancellationToken cancellationToken, double customWalkingSpeed = 0.0);

        double CalculateDistance(double sourceLat, double sourceLng, double destinationLat, double destinationLng,
            ISession session = null);
    }
}