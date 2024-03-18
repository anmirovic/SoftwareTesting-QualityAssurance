using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Newtonsoft.Json;
using RestSharp;

namespace PlaywrightTests.TestsAPI
{
    public class OrderTestsAPI : PlaywrightTest
    {
        private IAPIRequestContext Request;

        [SetUp]
        public async Task SetUpAPITesting()
        {
            var headers = new Dictionary<string, string>
            {
                { "Accept", "application/json" }
            };

            Request = await Playwright.APIRequest.NewContextAsync(new()
            {
                BaseURL = "https://localhost:7050",
                ExtraHTTPHeaders = headers,
                IgnoreHTTPSErrors = true
            });
        }

        [Test]
        public async Task PreuzmiNarudzbine()
        {
            await using var response = await Request.GetAsync("/api/Order/GetAllOrders");

            Assert.That(response.Status, Is.EqualTo(200));
            var jsonResponse = await response.JsonAsync();
            Assert.That(jsonResponse, Is.Not.Null);
        }

        [Test]
        public async Task PreuzmiNarudzbinu_Uspesno()
        {
            var noviKorisnik = new 
            {

                name = "user12",
                surname = "user12",
                username = "user12",
                email = "user122@gmail.com", //pri ponovnom testiranju, promeniti email
                password = "user12",
                phoneNumber = "0625467",
                role = "user"
            };

            await using var userResponse = await Request.PostAsync("/api/User/Register", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = noviKorisnik
            }); ;

            if (userResponse.Status != 200)
            {
                Assert.Fail($"Code: {userResponse.Status} - {userResponse.StatusText}");
            }

            Assert.That(userResponse.Status, Is.EqualTo(200));
            var userTextResponse = await userResponse.TextAsync();
            Assert.IsNotNull(userTextResponse);
            var createUserJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(userTextResponse);
            var userId = createUserJsonResponse["id"].ToString();

            var restaurant = new
            {
                Name = "Restoran 11",
                Address = "Adresa 11",
                Meals = new List<object>
                {
                    new { Name = "Pizza", Description = "Capricoza", Price = 1700 },
                    new { Name = "Pasta", Description = "Bolonjeze", Price = 1000 }
                }
            };

            await using var restaurantResponse = await Request.PostAsync("/api/Restaurant/CreateRestaurant", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = restaurant
            }); ;

            if (restaurantResponse.Status != 200)
            {
                Assert.Fail($"Code: {restaurantResponse.Status} - {restaurantResponse.StatusText}");
            }

            Assert.That(restaurantResponse.Status, Is.EqualTo(200));
            var restaurantTextResponse = await restaurantResponse.TextAsync();
            Assert.IsNotNull(restaurantTextResponse);
            var createRestaurantJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(restaurantTextResponse);
            var restaurantId = createRestaurantJsonResponse["id"].ToString();

            var mealsJsonElement = (JsonElement)createRestaurantJsonResponse["meals"];

            var mealIds = new List<string>();
            foreach (var mealJson in mealsJsonElement.EnumerateArray())
            {
                var mealId = mealJson.GetProperty("id").GetString();
                mealIds.Add(mealId);
            }

            await using var orderResponse = await Request.PostAsync($"/api/Order/CreateOrder?restaurantId={restaurantId}&userId={userId}", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                Data = System.Text.Json.JsonSerializer.Serialize(mealIds)
            });

            Assert.That(orderResponse.Status, Is.EqualTo(200));
            var orderTextResponse = await orderResponse.TextAsync();
            Assert.IsNotNull(orderTextResponse);
            var createOrderJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(orderTextResponse);
            var orderId = createOrderJsonResponse["id"].ToString();


            await using var response = await Request.GetAsync($"/api/Order/GetOrderById?id={orderId}");

            Assert.That(response.Status, Is.EqualTo(200));
            var textResponse = await response.TextAsync();
            Assert.IsNotNull(textResponse);
            var jsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(orderTextResponse);
            Assert.IsNotNull(jsonResponse["orderTime"]);

        }

        [Test]
        public async Task PreuzmiNarudzbinu_Neuspesno()
        {
            string nepostojeciId = "65ed984c454619ba306c8c63";

            await using var response = await Request.GetAsync($"/api/Order/GetOrderById?id={nepostojeciId}");

            Assert.That(response.Status, Is.EqualTo(400));
            var textResponse = await response.TextAsync();
            Assert.IsNotNull(textResponse);

            Assert.That(textResponse, Does.Contain($"Order with ID {nepostojeciId} does not exist."));
        }

        [Test]
        public async Task DodajNarudzbinu_Uspesno()
        {
            var noviKorisnik = new //obavezno promeni email
            {

                name = "user10",
                surname = "user10",
                username = "user10",
                email = "user101@gmail.com",
                password = "user10",
                phoneNumber = "0625467",
                role = "user"
            };

            await using var userResponse = await Request.PostAsync("/api/User/Register", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = noviKorisnik
            }); ;

            if (userResponse.Status != 200)
            {
                Assert.Fail($"Code: {userResponse.Status} - {userResponse.StatusText}");
            }

            Assert.That(userResponse.Status, Is.EqualTo(200));
            var userTextResponse = await userResponse.TextAsync();
            Assert.IsNotNull(userTextResponse);
            var createUserJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(userTextResponse);
            var userId = createUserJsonResponse["id"].ToString();

            var restaurant = new
            {
                Name = "Restoran 11",
                Address = "Adresa 11",
                Meals = new List<object>
                {
                    new { Name = "Pizza", Description = "Capricoza", Price = 1700 },
                    new { Name = "Pasta", Description = "Bolonjeze", Price = 1000 }
                }
            };

            await using var restaurantResponse = await Request.PostAsync("/api/Restaurant/CreateRestaurant", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = restaurant
            }); ;

            if (restaurantResponse.Status != 200)
            {
                Assert.Fail($"Code: {restaurantResponse.Status} - {restaurantResponse.StatusText}");
            }

            Assert.That(restaurantResponse.Status, Is.EqualTo(200));
            var restaurantTextResponse = await restaurantResponse.TextAsync();
            Assert.IsNotNull(restaurantTextResponse);
            var createRestaurantJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(restaurantTextResponse);
            var restaurantId = createRestaurantJsonResponse["id"].ToString();

            var mealsJsonElement = (JsonElement)createRestaurantJsonResponse["meals"];

            var mealIds = new List<string>();
            foreach (var mealJson in mealsJsonElement.EnumerateArray())
            {
                var mealId = mealJson.GetProperty("id").GetString();
                mealIds.Add(mealId);
            }

            await using var orderResponse = await Request.PostAsync($"/api/Order/CreateOrder?restaurantId={restaurantId}&userId={userId}", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                Data = System.Text.Json.JsonSerializer.Serialize(mealIds) 
            });

            Assert.That(orderResponse.Status, Is.EqualTo(200));
            var orderTextResponse = await orderResponse.TextAsync();
            Assert.IsNotNull(orderTextResponse);
            var jsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(orderTextResponse);
            Assert.IsNotNull(jsonResponse["orderTime"]);
        }


        [Test]
        public async Task DodajNarudzbinu_Neuspesno()
        {
            var restaurant = new
            {
                Name = "Restoran 12",
                Address = "Adresa 12",
                Meals = new List<object>
                {
                    new { Name = "Pizza", Description = "Capricoza", Price = 1700 },
                    new { Name = "Pasta", Description = "Bolonjeze", Price = 1000 }
                }
            };

            await using var restaurantResponse = await Request.PostAsync("/api/Restaurant/CreateRestaurant", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = restaurant
            }); ;

            if (restaurantResponse.Status != 200)
            {
                Assert.Fail($"Code: {restaurantResponse.Status} - {restaurantResponse.StatusText}");
            }

            Assert.That(restaurantResponse.Status, Is.EqualTo(200));
            var restaurantTextResponse = await restaurantResponse.TextAsync();
            Assert.IsNotNull(restaurantTextResponse);
            var createRestaurantJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(restaurantTextResponse);
            var restaurantId = createRestaurantJsonResponse["id"].ToString();

            var mealsJsonElement = (JsonElement)createRestaurantJsonResponse["meals"];

            var mealIds = new List<string>();
            foreach (var mealJson in mealsJsonElement.EnumerateArray())
            {
                var mealId = mealJson.GetProperty("id").GetString();
                mealIds.Add(mealId);
            }

            string nepostojeciId = "65ed984c454619ba306c8c63";

            await using var orderResponse = await Request.PostAsync($"/api/Order/CreateOrder?restaurantId={restaurantId}&userId={nepostojeciId}", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                Data = System.Text.Json.JsonSerializer.Serialize(mealIds)
            });

            Assert.That(orderResponse.Status, Is.EqualTo(400));
            var orderTextResponse = await orderResponse.TextAsync();
            Assert.IsNotNull(orderTextResponse);
            Assert.That(orderTextResponse, Does.Contain("Restaurant or user not found, or some meals were not added to the order."));
        }

        [Test]
        public async Task AzurirajNarudzbinu_Uspesno()
        {
            var noviKorisnik = new //obavezno promeni email
            {

                name = "user13",
                surname = "user13",
                username = "user13",
                email = "user133@gmail.com",
                password = "user13",
                phoneNumber = "0625467",
                role = "user"
            };

            await using var userResponse = await Request.PostAsync("/api/User/Register", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = noviKorisnik
            }); ;

            if (userResponse.Status != 200)
            {
                Assert.Fail($"Code: {userResponse.Status} - {userResponse.StatusText}");
            }

            Assert.That(userResponse.Status, Is.EqualTo(200));
            var userTextResponse = await userResponse.TextAsync();
            Assert.IsNotNull(userTextResponse);
            var createUserJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(userTextResponse);
            var userId = createUserJsonResponse["id"].ToString();

            var restaurant = new
            {
                Name = "Restoran 13",
                Address = "Adresa 13",
                Meals = new List<object>
                {
                    new { Name = "Pizza", Description = "Capricoza", Price = 1700 },
                    new { Name = "Pasta", Description = "Bolonjeze", Price = 1000 }
                }
            };

            await using var restaurantResponse = await Request.PostAsync("/api/Restaurant/CreateRestaurant", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = restaurant
            }); ;

            if (restaurantResponse.Status != 200)
            {
                Assert.Fail($"Code: {restaurantResponse.Status} - {restaurantResponse.StatusText}");
            }

            Assert.That(restaurantResponse.Status, Is.EqualTo(200));
            var restaurantTextResponse = await restaurantResponse.TextAsync();
            Assert.IsNotNull(restaurantTextResponse);
            var createRestaurantJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(restaurantTextResponse);
            var restaurantId = createRestaurantJsonResponse["id"].ToString();

            var mealsJsonElement = (JsonElement)createRestaurantJsonResponse["meals"];

            var mealIds = new List<string>();
            foreach (var mealJson in mealsJsonElement.EnumerateArray())
            {
                var mealId = mealJson.GetProperty("id").GetString();
                mealIds.Add(mealId);
            }

            await using var orderResponse = await Request.PostAsync($"/api/Order/CreateOrder?restaurantId={restaurantId}&userId={userId}", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                Data = System.Text.Json.JsonSerializer.Serialize(mealIds)
            });

            Assert.That(orderResponse.Status, Is.EqualTo(200));
            var orderTextResponse = await orderResponse.TextAsync();
            Assert.IsNotNull(orderTextResponse);
            var createOrderJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(orderTextResponse);
            var orderId = createOrderJsonResponse["id"].ToString();

            var updatedRestaurant = new
            {
                Name = "Novi naziv restorana",
                Address = "Nova adresa restorana",
                Meals = new List<object>
                {
                    new { Name = "Novo jelo 1", Description = "Opis novog jela 1", Price = 120 },
                    new { Name = "Novo jelo 2", Description = "Opis novog jela 2", Price = 180 }
                }
            };

            var selectedMealId = mealIds.FirstOrDefault();

            var updatedMealIds = new List<string> { selectedMealId };

            await using var response = await Request.PutAsync($"/api/Order/UpdateOrder?id={orderId}", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                Data = System.Text.Json.JsonSerializer.Serialize(updatedMealIds)
            });

            Assert.That(response.Status, Is.EqualTo(200));
            var textResponse = await response.TextAsync();
            Assert.IsNotNull(textResponse);
            Assert.That(textResponse, Does.Contain("Order successfully updated."));

        }

        [Test]
        public async Task AzuirajNarudzbinu_Neuspesno()
        {
            string nepostojeciId = "65ed984c454619ba306c8c63";

            var restaurant = new
            {
                Name = "Novi naziv restorana",
                Address = "Nova adresa restorana",
                Meals = new List<object>
                {
                    new { Name = "Novo jelo 1", Description = "Opis novog jela 1", Price = 120 },
                    new { Name = "Novo jelo 2", Description = "Opis novog jela 2", Price = 180 }
                }
            };
            await using var restaurantResponse = await Request.PostAsync("/api/Restaurant/CreateRestaurant", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = restaurant
            }); ;

            if (restaurantResponse.Status != 200)
            {
                Assert.Fail($"Code: {restaurantResponse.Status} - {restaurantResponse.StatusText}");
            }

            Assert.That(restaurantResponse.Status, Is.EqualTo(200));
            var restaurantTextResponse = await restaurantResponse.TextAsync();
            Assert.IsNotNull(restaurantTextResponse);
            var createRestaurantJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(restaurantTextResponse);

            var mealsJsonElement = (JsonElement)createRestaurantJsonResponse["meals"];

            var mealIds = new List<string>();
            foreach (var mealJson in mealsJsonElement.EnumerateArray())
            {
                var mealId = mealJson.GetProperty("id").GetString();
                mealIds.Add(mealId);
            }

            await using var response = await Request.PutAsync($"/api/Order/UpdateOrder?id={nepostojeciId}", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                Data = System.Text.Json.JsonSerializer.Serialize(mealIds)
            });

            Assert.That(response.Status, Is.EqualTo(400));
            var textResponse = await response.TextAsync();
            Assert.IsNotNull(textResponse);

            Assert.That(textResponse, Does.Contain($"Order with ID {nepostojeciId} not found."));
        }

        [Test]
        public async Task ObrisiNarudzbinu_Uspesno()
        {
            var noviKorisnik = new //obavezno promeni email
            {

                name = "user14",
                surname = "user14",
                username = "user14",
                email = "user144@gmail.com",
                password = "user14",
                phoneNumber = "0625467",
                role = "user"
            };

            await using var userResponse = await Request.PostAsync("/api/User/Register", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = noviKorisnik
            }); ;

            if (userResponse.Status != 200)
            {
                Assert.Fail($"Code: {userResponse.Status} - {userResponse.StatusText}");
            }

            Assert.That(userResponse.Status, Is.EqualTo(200));
            var userTextResponse = await userResponse.TextAsync();
            Assert.IsNotNull(userTextResponse);
            var createUserJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(userTextResponse);
            var userId = createUserJsonResponse["id"].ToString();

            var restaurant = new
            {
                Name = "Restoran 14",
                Address = "Adresa 14",
                Meals = new List<object>
                {
                    new { Name = "Pizza", Description = "Capricoza", Price = 1700 },
                    new { Name = "Pasta", Description = "Bolonjeze", Price = 1000 }
                }
            };

            await using var restaurantResponse = await Request.PostAsync("/api/Restaurant/CreateRestaurant", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = restaurant
            }); ;

            if (restaurantResponse.Status != 200)
            {
                Assert.Fail($"Code: {restaurantResponse.Status} - {restaurantResponse.StatusText}");
            }

            Assert.That(restaurantResponse.Status, Is.EqualTo(200));
            var restaurantTextResponse = await restaurantResponse.TextAsync();
            Assert.IsNotNull(restaurantTextResponse);
            var createRestaurantJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(restaurantTextResponse);
            var restaurantId = createRestaurantJsonResponse["id"].ToString();

            var mealsJsonElement = (JsonElement)createRestaurantJsonResponse["meals"];

            var mealIds = new List<string>();
            foreach (var mealJson in mealsJsonElement.EnumerateArray())
            {
                var mealId = mealJson.GetProperty("id").GetString();
                mealIds.Add(mealId);
            }

            await using var orderResponse = await Request.PostAsync($"/api/Order/CreateOrder?restaurantId={restaurantId}&userId={userId}", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                Data = System.Text.Json.JsonSerializer.Serialize(mealIds)
            });

            Assert.That(orderResponse.Status, Is.EqualTo(200));
            var orderTextResponse = await orderResponse.TextAsync();
            Assert.IsNotNull(orderTextResponse);
            var createOrderJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(orderTextResponse);
            var orderId = createOrderJsonResponse["id"].ToString();

            await using var response = await Request.DeleteAsync($"/api/Order/DeleteOrder?id={orderId}");

            Assert.That(response.Status, Is.EqualTo(200));
            var textResponse = await response.TextAsync();
            Assert.IsNotNull(textResponse);
            Assert.That(textResponse, Does.Contain("Order successfully removed."));
        }

        [Test]
        public async Task ObrisiNarudzbinu_Neuspesno()
        {
            string nepostojeciId = "65ed984c454619ba306c8c63";

            await using var response = await Request.DeleteAsync($"/api/Order/DeleteOrder?id={nepostojeciId}");

            Assert.That(response.Status, Is.EqualTo(400));
            var textResponse = await response.TextAsync();
            Assert.IsNotNull(textResponse);
            Assert.That(textResponse, Does.Contain($"Order with ID {nepostojeciId} does not exist."));
        }

        [Test]
        public async Task PreuzmiNarudzbineNekogKorisnika_Neuspesno()
        {
            string nepostojeciId = "65ed984c454619ba306c8c63";

            await using var response = await Request.GetAsync($"/api/Order/GetOrdersForUser?userId={nepostojeciId}");

            Assert.That(response.Status, Is.EqualTo(400));
            var textResponse = await response.TextAsync();
            Assert.IsNotNull(textResponse);

            Assert.That(textResponse, Does.Contain($"User with ID {nepostojeciId} does not exist."));
        }

        [Test]
        public async Task PreuzmiNarudzbineNekogKorisnika_Uspesno()
        {
            var noviKorisnik = new //obavezno promeni email
            {

                name = "user15",
                surname = "user15",
                username = "user15",
                email = "user155@gmail.com",
                password = "user15",
                phoneNumber = "0625467",
                role = "user"
            };

            await using var userResponse = await Request.PostAsync("/api/User/Register", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = noviKorisnik
            }); ;

            if (userResponse.Status != 200)
            {
                Assert.Fail($"Code: {userResponse.Status} - {userResponse.StatusText}");
            }

            Assert.That(userResponse.Status, Is.EqualTo(200));
            var userTextResponse = await userResponse.TextAsync();
            Assert.IsNotNull(userTextResponse);
            var createUserJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(userTextResponse);
            var userId = createUserJsonResponse["id"].ToString();

            var restaurant = new
            {
                Name = "Restoran 15",
                Address = "Adresa 15",
                Meals = new List<object>
                {
                    new { Name = "Pizza", Description = "Capricoza", Price = 1700 },
                    new { Name = "Pasta", Description = "Bolonjeze", Price = 1000 }
                }
            };

            await using var restaurantResponse = await Request.PostAsync("/api/Restaurant/CreateRestaurant", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = restaurant
            }); ;

            if (restaurantResponse.Status != 200)
            {
                Assert.Fail($"Code: {restaurantResponse.Status} - {restaurantResponse.StatusText}");
            }

            Assert.That(restaurantResponse.Status, Is.EqualTo(200));
            var restaurantTextResponse = await restaurantResponse.TextAsync();
            Assert.IsNotNull(restaurantTextResponse);
            var createRestaurantJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(restaurantTextResponse);
            var restaurantId = createRestaurantJsonResponse["id"].ToString();

            var mealsJsonElement = (JsonElement)createRestaurantJsonResponse["meals"];

            var mealIds1 = new List<string>();
            foreach (var mealJson in mealsJsonElement.EnumerateArray())
            {
                var mealId = mealJson.GetProperty("id").GetString();
                mealIds1.Add(mealId);
            }


            await using var order1Response = await Request.PostAsync($"/api/Order/CreateOrder?restaurantId={restaurantId}&userId={userId}", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                Data = System.Text.Json.JsonSerializer.Serialize(mealIds1)
            });

            Assert.That(order1Response.Status, Is.EqualTo(200));
            var order1TextResponse = await order1Response.TextAsync();
            Assert.IsNotNull(order1TextResponse);

            var selectedMealId = mealIds1.FirstOrDefault();

            var mealIds2 = new List<string> { selectedMealId };

            await using var order2Response = await Request.PostAsync($"/api/Order/CreateOrder?restaurantId={restaurantId}&userId={userId}", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                Data = System.Text.Json.JsonSerializer.Serialize(mealIds2)
            });

            Assert.That(order2Response.Status, Is.EqualTo(200));
            var order2TextResponse = await order2Response.TextAsync();
            Assert.IsNotNull(order2TextResponse);

            await using var response = await Request.GetAsync($"/api/Order/GetOrdersForUser?userId={userId}");

            Assert.That(response.Status, Is.EqualTo(200));
            var jsonResponse = await response.JsonAsync();
            Assert.That(jsonResponse, Is.Not.Null);
        }


    }
}
