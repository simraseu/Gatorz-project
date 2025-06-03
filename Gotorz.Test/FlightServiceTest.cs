using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Gotorz.Services;
using Microsoft.Extensions.Logging;
using Moq.Protected;
using Gotorz.Models;


namespace Gotorz.Tests
{
    [TestClass]
    public class FlightServiceTests
    {
        private Mock<ITokenService> _tokenServiceMock;
        private Mock<IHttpClientFactory> _httpClientFactoryMock;
        private Mock<ILogger<FlightService>> _loggerMock;

        [TestInitialize]
        public void Setup()
        {
            _tokenServiceMock = new Mock<ITokenService>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _loggerMock = new Mock<ILogger<FlightService>>();
        }

        [TestMethod]
        public async Task SearchFlightsAsync_WithEmptyOrigin_ReturnsMockData()
        {
            var service = new FlightService(_httpClientFactoryMock.Object, _tokenServiceMock.Object, _loggerMock.Object);
            var result = await service.SearchFlightsAsync("", "BCN", DateTime.Today);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0);
        }
        [TestMethod]
        public async Task SearchFlightsAsync_WithValidApiResponse_ParsesFlightCorrectly()
        {
            // JSON svar fra API med korrekt format
            var json = @"{
        'data': [{
            'itineraries': [{
                'segments': [{
                    'carrierCode': 'SK',
                    'number': '123',
                    'departure': { 'iataCode': 'CPH', 'at': '2025-07-01T08:00:00' },
                    'arrival': { 'iataCode': 'BCN', 'at': '2025-07-01T11:00:00' }
                }]
            }],
            'price': { 'total': '199.99' }
        }]
    }";

            _tokenServiceMock.Setup(t => t.GetTokenAsync()).ReturnsAsync("dummy-token");

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json)
                });

            var client = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://fake.api")
            };

            _httpClientFactoryMock.Setup(f => f.CreateClient("AmadeusAPI")).Returns(client);

            var service = new FlightService(_httpClientFactoryMock.Object, _tokenServiceMock.Object, _loggerMock.Object);

            // Act
            var result = await service.SearchFlightsAsync("CPH", "BCN", new DateTime(2025, 7, 1));

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            var flight = result[0];
            Assert.AreEqual("SK123", flight.FlightNumber);
            Assert.AreEqual("CPH", flight.DepartureAirport);
            Assert.AreEqual("BCN", flight.ArrivalAirport);
            Assert.AreEqual(199.99m, flight.Price);
        }

    }
}
