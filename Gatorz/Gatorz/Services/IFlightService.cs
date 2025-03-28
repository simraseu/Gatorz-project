﻿using Gatorz.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gatorz.Services
{
    public interface IFlightService
    {
        Task<List<FlightInfo>> SearchFlightsAsync(string origin, string destination, DateTime departureDate);
        Task<FlightInfo> GetFlightDetailAsync(string flightId);
    }
}