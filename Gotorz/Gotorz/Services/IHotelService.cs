using Gotorz.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gotorz.Services
{
    public interface IHotelService
    {
        Task<List<HotelInfo>> SearchHotelsAsync(string location, DateTime checkIn, DateTime checkOut);
        Task<HotelInfo> GetHotelDetailAsync(string hotelId);
    }
}