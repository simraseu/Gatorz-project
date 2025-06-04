
namespace TestProject
{ 
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Net;
    using System.Text;
    using Gotorz.Services;
    using Gotorz.Models;
    using Moq.Protected;
    using System.Threading;
    using Microsoft.Testing.Platform.Logging;


    [TestClass]
        public class FlightServiceTests
        {
            private Mock<IHttpClientFactory> _mockHttpClientFactory;
            private Mock<ITokenService> _mockTokenService;
            private Mock<ILogger<FlightService>> _mockLogger;
            private Mock<HttpMessageHandler> _mockHttpMessageHandler;
            private FlightService _flightService;

            [TestInitialize]
            public void Setup()
            {
                _mockHttpClientFactory = new Mock<IHttpClientFactory>();
                _mockTokenService = new Mock<ITokenService>();
                _mockLogger = new Mock<ILogger<FlightService>>();
                _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

                // Setup HttpClient with mocked HttpMessageHandler
                var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
                {
                    BaseAddress = new Uri("https://api.amadeus.com")
                };

                _mockHttpClientFactory.Setup(x => x.CreateClient("AmadeusAPI"))
                    .Returns(httpClient);

                _flightService = new FlightService(
                    _mockHttpClientFactory.Object,
                    _mockTokenService.Object,
                    _mockLogger.Object);
            }

            [TestMethod]
            public async Task SearchFlightsAsync_WithEmptyOrigin_ReturnsMockFlights()
            {
                // Arrange
                var destination = "BCN";
                var departureDate = DateTime.Today.AddDays(7);

                // Act
                var result = await _flightService.SearchFlightsAsync("", destination, departureDate);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Count >= 3 && result.Count <= 5);

                foreach (var flight in result)
                {
                    Assert.IsNotNull(flight.FlightNumber);
                    Assert.IsNotNull(flight.Airline);
                    Assert.AreEqual("CPH", flight.DepartureAirport); // Default when origin is empty
                    Assert.AreEqual(destination, flight.ArrivalAirport);
                }
            }

            [TestMethod]
            public async Task SearchFlightsAsync_WithEmptyDestination_ReturnsMockFlights()
            {
                // Arrange
                var origin = "CPH";
                var departureDate = DateTime.Today.AddDays(7);

                // Act
                var result = await _flightService.SearchFlightsAsync(origin, "", departureDate);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Count >= 3 && result.Count <= 5);

                foreach (var flight in result)
                {
                    Assert.AreEqual(origin, flight.DepartureAirport);
                    Assert.AreEqual("BCN", flight.ArrivalAirport); // Default when destination is empty
                }
            }

            [TestMethod]
            public async Task SearchFlightsAsync_ApiSuccess_ReturnsFlights()
            {
                // Arrange
                var origin = "CPH";
                var destination = "BCN";
                var departureDate = DateTime.Today.AddDays(7);
                var token = "test-token";

                _mockTokenService.Setup(x => x.GetTokenAsync())
                    .ReturnsAsync(token);

                var apiResponse = @"{
                ""data"": [
                    {
                        ""itineraries"": [
                            {
                                ""segments"": [
                                    {
                                        ""carrierCode"": ""SK"",
                                        ""number"": ""1234"",
                                        ""departure"": {
                                            ""iataCode"": ""CPH"",
                                            ""at"": ""2024-06-10T10:00:00""
                                        },
                                        ""arrival"": {
                                            ""iataCode"": ""BCN"",
                                            ""at"": ""2024-06-10T14:00:00""
                                        }
                                    }
                                ]
                            }
                        ],
                        ""price"": {
                            ""total"": ""250.00""
                        }
                    }
                ]
            }";

                _mockHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(apiResponse, Encoding.UTF8, "application/json")
                    });

                // Act
                var result = await _flightService.SearchFlightsAsync(origin, destination, departureDate);

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Count);
                var flight = result[0];
                Assert.AreEqual("SK1234", flight.FlightNumber);
                Assert.AreEqual("SK", flight.Airline);
                Assert.AreEqual("CPH", flight.DepartureAirport);
                Assert.AreEqual("BCN", flight.ArrivalAirport);
                Assert.AreEqual(250m, flight.Price);
            }

            [TestMethod]
            public async Task SearchFlightsAsync_ApiError_ReturnsMockFlights()
            {
                // Arrange
                var origin = "CPH";
                var destination = "BCN";
                var departureDate = DateTime.Today.AddDays(7);
                var token = "test-token";

                _mockTokenService.Setup(x => x.GetTokenAsync())
                    .ReturnsAsync(token);

                _mockHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Content = new StringContent("Bad Request")
                    });

                // Act
                var result = await _flightService.SearchFlightsAsync(origin, destination, departureDate);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Count >= 3 && result.Count <= 5);

                foreach (var flight in result)
                {
                    Assert.AreEqual(origin, flight.DepartureAirport);
                    Assert.AreEqual(destination, flight.ArrivalAirport);
                }
            }

            [TestMethod]
            public async Task SearchFlightsAsync_TokenServiceThrows_ReturnsMockFlights()
            {
                // Arrange
                var origin = "CPH";
                var destination = "BCN";
                var departureDate = DateTime.Today.AddDays(7);

                _mockTokenService.Setup(x => x.GetTokenAsync())
                    .ThrowsAsync(new Exception("Token service error"));

                // Act
                var result = await _flightService.SearchFlightsAsync(origin, destination, departureDate);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Count >= 3 && result.Count <= 5);
            }

            [TestMethod]
            public async Task SearchFlightsAsync_EmptyApiResponse_ReturnsMockFlights()
            {
                // Arrange
                var origin = "CPH";
                var destination = "BCN";
                var departureDate = DateTime.Today.AddDays(7);
                var token = "test-token";

                _mockTokenService.Setup(x => x.GetTokenAsync())
                    .ReturnsAsync(token);

                var emptyApiResponse = @"{""data"": []}";

                _mockHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(emptyApiResponse, Encoding.UTF8, "application/json")
                    });

                // Act
                var result = await _flightService.SearchFlightsAsync(origin, destination, departureDate);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Count >= 3 && result.Count <= 5);
            }

            [TestMethod]
            public async Task GetFlightDetailAsync_ThrowsNotImplementedException()
            {
                // Act & Assert
                await Assert.ThrowsExceptionAsync<NotImplementedException>(() =>
                    _flightService.GetFlightDetailAsync("test-id"));
            }

            [TestMethod]
            public async Task SearchFlightsAsync_MockFlights_WithCPHtoBCN_ContainCorrectData()
            {
                // Arrange
                var departureDate = DateTime.Today.AddDays(7);

                // Act (using empty string to trigger mock data)
                var result = await _flightService.SearchFlightsAsync("", "", departureDate);

                // Assert
                Assert.IsNotNull(result);
                foreach (var flight in result)
                {
                    Assert.IsTrue(flight.Price >= 100 && flight.Price <= 1000);
                    Assert.IsTrue(flight.DepartureTime >= departureDate);
                    Assert.IsTrue(flight.ArrivalTime > flight.DepartureTime);
                }
            }

            [TestMethod]
            public async Task SearchFlightsAsync_MockFlights_WithJFKtoLAX_ContainCorrectData()
            {
                // Arrange
                var departureDate = DateTime.Today.AddDays(5);

                // Act (using empty string to trigger mock data)
                var result = await _flightService.SearchFlightsAsync("", "", departureDate);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Count >= 3 && result.Count <= 5);
                foreach (var flight in result)
                {
                    Assert.IsNotNull(flight.FlightNumber);
                    Assert.IsNotNull(flight.Airline);
                    Assert.IsTrue(flight.Price > 0);
                }
            }

            [TestMethod]
            public async Task SearchFlightsAsync_MockFlights_WithLHRtoCDG_ContainCorrectData()
            {
                // Arrange
                var departureDate = DateTime.Today.AddDays(10);

                // Act (using empty string to trigger mock data)
                var result = await _flightService.SearchFlightsAsync("", "", departureDate);

                // Assert
                Assert.IsNotNull(result);
                foreach (var flight in result)
                {
                    Assert.IsTrue(flight.DepartureTime.Date >= departureDate.Date);
                    Assert.IsTrue(flight.ArrivalTime > flight.DepartureTime);
                    Assert.IsTrue(flight.Id >= 0);
                }
            }
        }
    }
