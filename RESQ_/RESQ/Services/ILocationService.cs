using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RESQ.Services
{
    public interface ILocationService
    {
        Task<(double Latitude, double Longitude)> GetCurrentLocationAsync();
    }
}
