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
    public class HotelServiceTests
    {
        private Mock<ITokenService> _tokenServiceMock;
        private Mock<IHttpClientFactory> _httpClientFactoryMock;
        private Mock<ILogger<HotelService>> _loggerMock;

        [TestInitialize]
        public void Setup()
        {
            _tokenServiceMock = new Mock<ITokenService>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _loggerMock = new Mock<ILogger<HotelService>>();
        }

        [TestMethod]
        public async Task SearchHotelsAsync_WithEmptyCityCode_ReturnsMockHotels()
        {
            // Arrange
            var service = new HotelService(_httpClientFactoryMock.Object, _tokenServiceMock.Object, _loggerMock.Object);

            // Act
            var result = await service.SearchHotelsAsync("", DateTime.Today, DateTime.Today.AddDays(2));

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0, "Der blev ikke returneret nogen hoteller (mock).");
        }

        [TestMethod]
        public async Task SearchHotelsAsync_WhenApiFails_ReturnsMockHotels()
        {
            // Arrange
            _tokenServiceMock.Setup(t => t.GetTokenAsync()).ReturnsAsync("fake-token");

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent("Fejl fra API"),
                });

            var client = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://fake.api")
            };

            _httpClientFactoryMock.Setup(f => f.CreateClient("AmadeusAPI")).Returns(client);

            var service = new HotelService(_httpClientFactoryMock.Object, _tokenServiceMock.Object, _loggerMock.Object);

            // Act
            var result = await service.SearchHotelsAsync("BCN", DateTime.Today, DateTime.Today.AddDays(2));

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0, "Der blev ikke returneret nogen hoteller ved API-fejl.");
        }
        [TestMethod]
        public async Task SearchHotelsAsync_WithValidApiResponse_ParsesHotelCorrectly()
        {
            // Step 1: hotel lookup (by-city → hotelId)
            var lookupJson = @"{
        'data': [{ 'hotelId': 'H123' }]
    }";

            // Step 2: hotel offers (med priser og info)
            var offersJson = @"{
        'data': [{
            'hotel': {
                'name': 'Test Hotel',
                'address': {
                    'cityName': 'Barcelona',
                    'countryCode': 'ES',
                    'lines': ['Carrer de Test 1']
                },
                'rating': '4'
            },
            'offers': [{
                'room': {
                    'typeEstimated': { 'category': 'Deluxe Room' }
                },
                'price': {
                    'base': '220.00'
                }
            }]
        }]
    }";

            _tokenServiceMock.Setup(t => t.GetTokenAsync()).ReturnsAsync("fake-token");

            int callCount = 0;

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() =>
                {
                    callCount++;
                    var content = callCount == 1 ? lookupJson : offersJson;
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(content)
                    };
                });

            var client = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://fake.api")
            };

            _httpClientFactoryMock.Setup(f => f.CreateClient("AmadeusAPI")).Returns(client);

            var service = new HotelService(_httpClientFactoryMock.Object, _tokenServiceMock.Object, _loggerMock.Object);

            // Act
            var result = await service.SearchHotelsAsync("BCN", DateTime.Today, DateTime.Today.AddDays(2));

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            var hotel = result[0];
            Assert.AreEqual("Test Hotel", hotel.HotelName);
            Assert.AreEqual("Barcelona", hotel.City);
            Assert.AreEqual("ES", hotel.Country);
            Assert.AreEqual("Deluxe Room", hotel.RoomType);
            Assert.AreEqual(220.00m, hotel.PricePerNight);
        }

    }
}
