using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.Service.Elevation
{
    public interface IElevationService
    {
        string GetServiceId();
        Task<double> GetElevation(double lat, double lng);
    }
}