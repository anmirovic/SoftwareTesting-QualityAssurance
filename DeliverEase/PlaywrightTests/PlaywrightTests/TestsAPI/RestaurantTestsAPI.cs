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
    [TestFixture]
    public class RestaurantTestsAPI : PlaywrightTest
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
        public async Task PreuzmiRestorane_Uspesno()
        {
            await using var response = await Request.GetAsync("/api/Restaurant/GetAllRestaurants");

            Assert.That(response.Status, Is.EqualTo(200));
            var jsonResponse = await response.JsonAsync();
            Assert.That(jsonResponse, Is.Not.Null);
        }

        [Test]
        public async Task DodajRestoran_Uspesno()
        {
            var restaurant = new
            {
                Name = "Zelja",
                Address = "Cairska",
                Meals = new List<object>
                {
                    new { Name = "Pizza", Description = "Capricoza", Price = 1700 },
                    new { Name = "Pasta", Description = "Bolonjeze", Price = 1000 }
                }
            };

            await using var response = await Request.PostAsync("/api/Restaurant/CreateRestaurant", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = restaurant
            }); ;

            if (response.Status != 200)
            {
                Assert.Fail($"Code: {response.Status} - {response.StatusText}");
            }

            Assert.That(response.Status, Is.EqualTo(200));
            var textResponse = await response.TextAsync();
            Assert.IsNotNull(textResponse);
            Assert.That(textResponse, Does.Contain(restaurant.Name));
        }

        [Test]
        public async Task DodajRestoran_Neuspesno()
        {
            var restaurant = new
            {
                Address = "Cairska",
                Meals = new List<object>
                {
                    new { Name = "Pizza", Description = "Capricoza", Price = 1700 },
                    new { Name = "Pasta", Description = "Bolonjeze", Price = 1000 }
                }
            };

            await using var response = await Request.PostAsync("/api/Restaurant/CreateRestaurant", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = restaurant
            });

            Assert.That(response.Status, Is.EqualTo(400));
            var textResponse = await response.TextAsync();
            Assert.IsNotNull(textResponse);
            Assert.That(textResponse, Does.Contain("The Name field is required."));
        }

        [Test]
        public async Task PreuzmiRestoran_Uspesno()
        {
            var newRestaurant = new
            {
                Name = "Test restoran",
                Address = "Test adresa",
                Meals = new List<object>
                {
                    new { Name = "Test jelo 1", Description = "Opis test jela 1", Price = 100 },
                    new { Name = "Test jelo 2", Description = "Opis test jela 2", Price = 150 }
                }
            };

            await using var createResponse = await Request.PostAsync("/api/Restaurant/CreateRestaurant", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = newRestaurant
            });

            if (createResponse.Status != 200)
            {
                Assert.Fail($"Code: {createResponse.Status} - {createResponse.StatusText}");
            }

            Assert.That(createResponse.Status, Is.EqualTo(200));
            var createTextResponse = await createResponse.TextAsync();
            var createJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createTextResponse);
            var restaurantId = createJsonResponse["id"].ToString();


            await using var response = await Request.GetAsync($"/api/Restaurant/GetRestaurantById?id={restaurantId}");

            Assert.That(response.Status, Is.EqualTo(200));
            var textResponse = await createResponse.TextAsync();
            Assert.IsNotNull(textResponse);

            var jsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(textResponse);

            Assert.That(jsonResponse["id"].ToString(), Is.EqualTo(restaurantId));
            Assert.That(jsonResponse["name"].ToString(), Is.EqualTo("Test restoran"));

        }

        [Test]
        public async Task PreuzmiRestoran_Neuspesno()
        {
            string nepostojeciId = "65ed984c454619ba306c8c63";

            await using var response = await Request.GetAsync($"/api/Restaurant/GetRestaurantById?id={nepostojeciId}");

            Assert.That(response.Status, Is.EqualTo(400));
            var textResponse = await response.TextAsync();
            Assert.IsNotNull(textResponse);

            Assert.That(textResponse, Does.Contain($"Restaurant with ID {nepostojeciId} does not exist."));
        }

        [Test]
        public async Task AzurirajRestoran_Uspesno()
        {
            var newRestaurant = new
            {
                Name = "Test restoran 2",
                Address = "Test adresa 2",
                Meals = new List<object>
                {
                    new { Name = "Test jelo 1", Description = "Opis test jela 1", Price = 100 },
                    new { Name = "Test jelo 2", Description = "Opis test jela 2", Price = 150 }
                }
            };

            await using var createResponse = await Request.PostAsync("/api/Restaurant/CreateRestaurant", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = newRestaurant
            });

            if (createResponse.Status != 200)
            {
                Assert.Fail($"Code: {createResponse.Status} - {createResponse.StatusText}");
            }

            Assert.That(createResponse.Status, Is.EqualTo(200));
            var createTextResponse = await createResponse.TextAsync();
            var createJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createTextResponse);
            var restaurantId = createJsonResponse["id"].ToString();

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


            await using var response = await Request.PutAsync($"/api/Restaurant/UpdateRestaurant?id={restaurantId}", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = updatedRestaurant
            });

            Assert.That(response.Status, Is.EqualTo(200));
            var textResponse = await response.TextAsync();
            Assert.IsNotNull(textResponse);
            Assert.That(textResponse, Does.Contain("Restaurant successfully updated."));

        }

        [Test]
        public async Task AzuirajRestoran_Neuspesno()
        {
            string nepostojeciId = "65ed984c454619ba306c8c63";

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


            await using var response = await Request.PutAsync($"/api/Restaurant/UpdateRestaurant?id={nepostojeciId}", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = updatedRestaurant
            });

            Assert.That(response.Status, Is.EqualTo(400));
            var textResponse = await response.TextAsync();
            Assert.IsNotNull(textResponse);

            Assert.That(textResponse, Does.Contain($"Restaurant with ID {nepostojeciId} does not exist."));
        }

        [Test]
        public async Task ObrisiRestoran_Uspesno()
        {
            var newRestaurant = new
            {
                Name = "Test restoran",
                Address = "Test adresa",
                Meals = new List<object>
                {
                    new { Name = "Test jelo 1", Description = "Opis test jela 1", Price = 100 },
                    new { Name = "Test jelo 2", Description = "Opis test jela 2", Price = 150 }
                }
            };

            await using var createResponse = await Request.PostAsync("/api/Restaurant/CreateRestaurant", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = newRestaurant
            });

            if (createResponse.Status != 200)
            {
                Assert.Fail($"Code: {createResponse.Status} - {createResponse.StatusText}");
            }

            Assert.That(createResponse.Status, Is.EqualTo(200));
            var createTextResponse = await createResponse.TextAsync();
            var createJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createTextResponse);
            var restaurantId = createJsonResponse["id"].ToString();


            await using var response = await Request.DeleteAsync($"/api/Restaurant/DeleteRestaurant?id={restaurantId}");

            Assert.That(response.Status, Is.EqualTo(200));
            var textResponse = await response.TextAsync();
            Assert.IsNotNull(textResponse);
            Assert.That(textResponse, Does.Contain($"Izbrisan je restoran: {restaurantId}"));
        }

        [Test]
        public async Task ObrisiRestoran_Neuspesno()
        {
            string nepostojeciId = "65ed984c454619ba306c8c63";

            await using var response = await Request.DeleteAsync($"/api/Restaurant/DeleteRestaurant?id={nepostojeciId}");

            Assert.That(response.Status, Is.EqualTo(400));
            var textResponse = await response.TextAsync();
            Assert.IsNotNull(textResponse);
            Assert.That(textResponse, Does.Contain($"Restaurant with ID {nepostojeciId} does not exist."));
        }

        [Test]
        public async Task DodajJelo_Uspesno()
        {
            
            var newRestaurant = new
            {
                Name = "Kineski restoran",
                Address = "Dimitrija Tucovica",
                Meals = new List<object>
                {
                    new { Name = "Nudle sa piletinom", Description = "500g", Price = 800 }
                }
            };

            await using var createRestaurantResponse = await Request.PostAsync("/api/Restaurant/CreateRestaurant", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = newRestaurant
            });

            Assert.That(createRestaurantResponse.Status, Is.EqualTo(200));
            var createRestaurantTextResponse = await createRestaurantResponse.TextAsync();
            var createRestaurantJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createRestaurantTextResponse);
            var restaurantId = createRestaurantJsonResponse["id"].ToString();

            var newMeal = new
            {
                Name = "Cufte",
                Description = "U sosu",
                Price = 1000
            };

            await using var response = await Request.PostAsync($"/api/Restaurant/AddMeal?restaurantId={restaurantId}", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = newMeal
            });

            if (response.Status != 200)
            {
                Assert.Fail($"Code: {response.Status} - {response.StatusText}");
            }

            Assert.That(response.Status, Is.EqualTo(200));
            var textResponse = await response.TextAsync();
            Assert.IsNotNull(textResponse);
            Assert.That(textResponse, Does.Contain(newMeal.Name));

        }


        [Test]
        public async Task DodajJelo_Neuspesno()
        {

            string nepostojeciId = "65ed984c454619ba306c8c63";

            var newMeal = new
            {
                Name = "Cufte",
                Description = "U sosu",
                Price = 1000
            };

           
            await using var response = await Request.PostAsync($"/api/Restaurant/AddMeal?restaurantId={nepostojeciId}", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = newMeal
            });

           
            Assert.That(response.Status, Is.EqualTo(400));
            var textResponse = await response.TextAsync();
            Assert.That(textResponse, Does.Contain($"Restaurant with ID {nepostojeciId} not found."));
        }

        [Test]
        public async Task ObrisiMeal_Uspesno()
        {
            
            var newRestaurant = new
            {
                Name = "t",
                Address = "t",
                Meals = new List<object>
                {
                    new { Name = "Nudle sa piletinom", Description = "500g", Price = 800 },
                    new { Name = "Cufte", Description = "U sosu", Price = 1000 }
                }
            };


            await using var createRestaurantResponse = await Request.PostAsync("/api/Restaurant/CreateRestaurant", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = newRestaurant
            });


            Assert.That(createRestaurantResponse.Status, Is.EqualTo(200));

            var createRestaurantTextResponse = await createRestaurantResponse.TextAsync();
            var createRestaurantJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createRestaurantTextResponse);
            var restaurantId = createRestaurantJsonResponse["id"].ToString();

            var meals = (JsonElement)createRestaurantJsonResponse["meals"];
            var firstMealId = meals.EnumerateArray().First().GetProperty("id").GetString();

            await using var deleteResponse = await Request.DeleteAsync($"/api/Restaurant/DeleteMeal?restaurantId={restaurantId}&mealId={firstMealId}");

            Assert.That(deleteResponse.Status, Is.EqualTo(200));
            var deleteTextResponse = await deleteResponse.TextAsync();
            Assert.That(deleteTextResponse, Does.Contain($"Izbrisano je jelo: {firstMealId}"));
        }

        [Test]
        public async Task ObrisiMeal_Neuspesno()
        {

            var newRestaurant = new
            {
                Name = "novo",
                Address = "novo",
                Meals = new List<object>
                {
                    new { Name = "Nudle sa piletinom", Description = "500g", Price = 800 },
                    new { Name = "Cufte", Description = "U sosu", Price = 1000 }
                }
            };


            await using var createRestaurantResponse = await Request.PostAsync("/api/Restaurant/CreateRestaurant", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = newRestaurant
            });


            Assert.That(createRestaurantResponse.Status, Is.EqualTo(200));

            var createRestaurantTextResponse = await createRestaurantResponse.TextAsync();
            var createRestaurantJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createRestaurantTextResponse);
            var restaurantId = createRestaurantJsonResponse["id"].ToString();

            string nepostojeciId = "65ed984c454619ba306c8c63";

            await using var deleteResponse = await Request.DeleteAsync($"/api/Restaurant/DeleteMeal?restaurantId={restaurantId}&mealId={nepostojeciId}");

            Assert.That(deleteResponse.Status, Is.EqualTo(400));
            var deleteTextResponse = await deleteResponse.TextAsync();
            Assert.That(deleteTextResponse, Does.Contain($"Meal with ID {nepostojeciId} not found"));
        }

        [Test]
        public async Task PreuzmiMeal_Uspesno()
        {

            var newRestaurant = new
            {
                Name = "Restoran",
                Address = "Adresa",
                Meals = new List<object>
                {
                    new { Name = "Nudle sa piletinom", Description = "500g", Price = 800 },
                    new { Name = "Cufte", Description = "U sosu", Price = 1000 }
                }
            };


            await using var createRestaurantResponse = await Request.PostAsync("/api/Restaurant/CreateRestaurant", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = newRestaurant
            });


            Assert.That(createRestaurantResponse.Status, Is.EqualTo(200));

            var createRestaurantTextResponse = await createRestaurantResponse.TextAsync();
            var createRestaurantJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createRestaurantTextResponse);
            var restaurantId = createRestaurantJsonResponse["id"].ToString();

            var meals = (JsonElement)createRestaurantJsonResponse["meals"];
            var firstMealId = meals.EnumerateArray().First().GetProperty("id").GetString();


            await using var response = await Request.GetAsync($"/api/Restaurant/GetMealById?restaurantId={restaurantId}&mealId={firstMealId}");

            Assert.That(response.Status, Is.EqualTo(200));
            var jsonResponse = await response.JsonAsync();
            Assert.IsNotNull(jsonResponse);
            var jsonString = jsonResponse.ToString();
            Assert.That(jsonString, Does.Contain(firstMealId));

        }

        [Test]
        public async Task PreuzmiMeal_Neuspesno()
        {

            var newRestaurant = new
            {
                Name = "Test restoran",
                Address = "Test adresa",
                Meals = new List<object>
                {
                    new { Name = "Test jelo", Description = "Opis test jela", Price = 100 }
                }
            };

            await using var createRestaurantResponse = await Request.PostAsync("/api/Restaurant/CreateRestaurant", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = newRestaurant
            });

            Assert.That(createRestaurantResponse.Status, Is.EqualTo(200));
            var createRestaurantTextResponse = await createRestaurantResponse.TextAsync();
            var createRestaurantJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createRestaurantTextResponse);
            var restaurantId = createRestaurantJsonResponse["id"].ToString();

            string nepostojeciMealId = "65ed984c454619ba306c8c63";

            await using var response = await Request.GetAsync($"/api/Restaurant/GetMealById?restaurantId={restaurantId}&mealId={nepostojeciMealId}");

            Assert.That(response.Status, Is.EqualTo(400));
            var textResponse = await response.TextAsync();
            Assert.IsNotNull(textResponse);
            Assert.That(textResponse, Does.Contain($"Meal with ID {nepostojeciMealId} not found."));

        }

        [Test]
        public async Task PreuzmiSvaJelaUrestoranu()
        {

            var newRestaurant = new
            {
                Name = "Test restoran",
                Address = "Test adresa",
                Meals = new List<object>
                {
                    new { Name = "Test jelo 1", Description = "Opis test jela 1", Price = 100 },
                    new { Name = "Test jelo 2", Description = "Opis test jela 2", Price = 150 }
                }
            };


            await using var createRestaurantResponse = await Request.PostAsync("/api/Restaurant/CreateRestaurant", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = newRestaurant
            });

            Assert.That(createRestaurantResponse.Status, Is.EqualTo(200));

            var createRestaurantTextResponse = await createRestaurantResponse.TextAsync();
            var createRestaurantJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createRestaurantTextResponse);
            var restaurantId = createRestaurantJsonResponse["id"].ToString();

            await using var response = await Request.GetAsync($"/api/Restaurant/GetAllMealsInRestaurant?restaurantId={restaurantId}");

            Assert.That(response.Status, Is.EqualTo(200));
            var jsonResponse = await response.JsonAsync();
            Assert.IsNotNull(jsonResponse);
            var jsonString = jsonResponse.ToString();
            Assert.That(jsonString, Does.Contain("Test jelo 1"));
            Assert.That(jsonString, Does.Contain("Test jelo 2"));
        }



        [Test]
        public async Task SortirajRestoranePoOceni_Rastuce()
        {
            var restaurants = new List<object>
                   {
                       new
                       {
                           Name = "Test restoran 1",
                           Address = "Test adresa 1",
                           Meals = new List<object>
                           {
                               new { Name = "Test jelo 1", Description = "Opis test jela 1", Price = 100 },
                               new { Name = "Test jelo 2", Description = "Opis test jela 2", Price = 150 }
                           }
                       },
                       new
                       {
                           Name = "Test restoran 2",
                           Address = "Test adresa 2",
                           Meals = new List<object>
                           {
                               new { Name = "Test jelo 1", Description = "Opis test jela 1", Price = 100 },
                               new { Name = "Test jelo 2", Description = "Opis test jela 2", Price = 150 }
                           }
                       }
                   };


            foreach (var restaurant in restaurants)
            {

                await using var createResponse = await Request.PostAsync("/api/Restaurant/CreateRestaurant", new APIRequestContextOptions
                {
                    Headers = new Dictionary<string, string>
                           {
                               { "Content-Type", "application/json" }
                           },
                    DataObject = restaurant
                });

                if (createResponse.Status != 200)
                {
                    Assert.Fail($"Code: {createResponse.Status} - {createResponse.StatusText}");
                }

                Assert.That(createResponse.Status, Is.EqualTo(200));
            }


            bool ascending = true;
            await using var response = await Request.GetAsync($"/api/Restaurant/SortByRating?ascending={ascending}");

            Assert.That(response.Status, Is.EqualTo(200));
            var textResponse = await response.TextAsync();
            Assert.IsNotNull(textResponse);

        }
    }

}
