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
    public class ReviewTestsAPI : PlaywrightTest
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
        public async Task PreuzmiReviews_Uspesno()
        {
            await using var response = await Request.GetAsync("/api/Review/GetAllReviews");

            Assert.That(response.Status, Is.EqualTo(200));
            var jsonResponse = await response.JsonAsync();
            Assert.That(jsonResponse, Is.Not.Null);
        }

        [Test]
        public async Task DodajReview_Uspesno()
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
            var createRestaurantTextResponse = await response.TextAsync();
            var createRestaurantJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createRestaurantTextResponse);
            var restaurantId = createRestaurantJsonResponse["id"].ToString();


            var korisnik = new //obavezno promeni email
            {

                name = "Nemanja",
                surname = "Tadic",
                username = "Nemanja",
                email = "nemanja@gmail.com",
                password = "nemanja",
                phoneNumber = "0627596267",
                role = "user"
            };

            await using var response2 = await Request.PostAsync("/api/User/Register", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = korisnik
            }); ;

            if (response2.Status != 200)
            {
                Assert.Fail($"Code: {response2.Status} - {response2.StatusText}");
            }

            Assert.That(response2.Status, Is.EqualTo(200));
            var createUserTextResponse = await response2.TextAsync();
            var createUserJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createUserTextResponse);
            var userId = createUserJsonResponse["id"].ToString();

            var review = new 
            {
                rating = 3,
                userId = userId,
                restaurantId = restaurantId
            };

            await using var response3 = await Request.PostAsync("/api/Review/AddReview", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = review
            }); ;

            if (response3.Status != 200)
            {
                Assert.Fail($"Code: {response3.Status} - {response3.StatusText}");
            }

            Assert.That(response3.Status, Is.EqualTo(200));
            var textResponse = await response3.TextAsync();
            var jsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(textResponse);
            Assert.That(jsonResponse["rating"].ToString(), Is.EqualTo("3"));
            
        }

        [Test]
        public async Task DodajReview_Neuspesno_NedostajeIdKorisnika()
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
            var createRestaurantTextResponse = await response.TextAsync();
            var createRestaurantJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createRestaurantTextResponse);
            var restaurantId = createRestaurantJsonResponse["id"].ToString();


            var review = new
            {
                rating = 3,
                restaurantId = restaurantId
            };

            await using var response3 = await Request.PostAsync("/api/Review/AddReview", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = review
            }); ;

            Assert.That(response3.Status, Is.EqualTo(400));
            var textResponse = await response3.TextAsync();
            Assert.IsNotNull(textResponse);
            Assert.That(textResponse, Does.Contain("The UserId field is required."));

        }

        [Test]
        public async Task DodajReview_Neuspesno_NedostajeIdRestorana()
        {

            var korisnik = new //obavezno promeni email
            {
                name = "Uros",
                surname = "Tadic",
                username = "Uros",
                email = "uros@gmail.com",
                password = "uros",
                phoneNumber = "0627596267",
                role = "user"
            };

            await using var response2 = await Request.PostAsync("/api/User/Register", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = korisnik
            }); ;

            if (response2.Status != 200)
            {
                Assert.Fail($"Code: {response2.Status} - {response2.StatusText}");
            }

            Assert.That(response2.Status, Is.EqualTo(200));
            var createUserTextResponse = await response2.TextAsync();
            var createUserJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createUserTextResponse);
            var userId = createUserJsonResponse["id"].ToString();

            var review = new
            {
                rating = 3,
                userId = userId
            };

            await using var response3 = await Request.PostAsync("/api/Review/AddReview", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = review
            }); ;

            Assert.That(response3.Status, Is.EqualTo(400));
            var textResponse = await response3.TextAsync();
            Assert.IsNotNull(textResponse);
            Assert.That(textResponse, Does.Contain("The RestaurantId field is required."));
           
        }

        [Test]
        public async Task DodajReview_Neuspesno_NedostajeIdKorisnika_NedostajeIdRestorana()
        { 
            var review = new
            {
                rating = 3,
            };

            await using var response3 = await Request.PostAsync("/api/Review/AddReview", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = review
            }); ;

            Assert.That(response3.Status, Is.EqualTo(400));
            var textResponse = await response3.TextAsync();
            Assert.IsNotNull(textResponse);
            Assert.That(textResponse, Does.Contain("The UserId field is required."));
            Assert.That(textResponse, Does.Contain("The RestaurantId field is required."));

        }

        [Test]
        public async Task PreuzmiReview_Uspesno()
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
            var createRestaurantTextResponse = await response.TextAsync();
            var createRestaurantJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createRestaurantTextResponse);
            var restaurantId = createRestaurantJsonResponse["id"].ToString();


            var korisnik = new //obavezno promeni email
            {

                name = "Nikola",
                surname = "Jankovic",
                username = "Nikola",
                email = "nikola@gmail.com",
                password = "nikola",
                phoneNumber = "0627596267",
                role = "user"
            };

            await using var response2 = await Request.PostAsync("/api/User/Register", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = korisnik
            }); ;

            if (response2.Status != 200)
            {
                Assert.Fail($"Code: {response2.Status} - {response2.StatusText}");
            }

            Assert.That(response2.Status, Is.EqualTo(200));
            var createUserTextResponse = await response2.TextAsync();
            var createUserJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createUserTextResponse);
            var userId = createUserJsonResponse["id"].ToString();

            var review = new
            {
                rating = 3,
                userId = userId,
                restaurantId = restaurantId
            };

            await using var response3 = await Request.PostAsync("/api/Review/AddReview", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = review
            }); ;

            if (response3.Status != 200)
            {
                Assert.Fail($"Code: {response3.Status} - {response3.StatusText}");
            }

            Assert.That(response3.Status, Is.EqualTo(200));
            var textResponse = await response3.TextAsync();
            var jsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(textResponse);
            var reviewId = jsonResponse["id"].ToString();

            await using var response4 = await Request.GetAsync($"/api/Review/GetReviewById?id={reviewId}");
            Assert.That(response4.Status, Is.EqualTo(200));
            var textResponse4 = await response4.TextAsync();
            Assert.IsNotNull(textResponse4);

            var jsonResponse4 = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(textResponse4);

            Assert.That(jsonResponse["id"].ToString(), Is.EqualTo(reviewId));
            
        }

        [Test]
        public async Task PreuzmiReview_Neuspesno()
        {
            string nepostojeciId = "65ed984c454619ba306c8c63";

            await using var response = await Request.GetAsync($"/api/Review/GetReviewById?id={nepostojeciId}");

            Assert.That(response.Status, Is.EqualTo(400));
            var textResponse = await response.TextAsync();
            Assert.IsNotNull(textResponse);

            Assert.That(textResponse, Does.Contain($"Review with ID {nepostojeciId} not found."));
        }

        [Test]
        public async Task AzurirajReview_Uspesno()
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
            var createRestaurantTextResponse = await response.TextAsync();
            var createRestaurantJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createRestaurantTextResponse);
            var restaurantId = createRestaurantJsonResponse["id"].ToString();


            var korisnik = new //obavezno promeni email
            {

                name = "Srdjan",
                surname = "Jankovic",
                username = "Srdjan",
                email = "srdjan@gmail.com",
                password = "srdjan",
                phoneNumber = "0627596267",
                role = "user"
            };

            await using var response2 = await Request.PostAsync("/api/User/Register", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = korisnik
            }); ;

            if (response2.Status != 200)
            {
                Assert.Fail($"Code: {response2.Status} - {response2.StatusText}");
            }

            Assert.That(response2.Status, Is.EqualTo(200));
            var createUserTextResponse = await response2.TextAsync();
            var createUserJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createUserTextResponse);
            var userId = createUserJsonResponse["id"].ToString();

            var review = new
            {
                rating = 3,
                userId = userId,
                restaurantId = restaurantId
            };

            await using var response3 = await Request.PostAsync("/api/Review/AddReview", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = review
            }); ;

            if (response3.Status != 200)
            {
                Assert.Fail($"Code: {response3.Status} - {response3.StatusText}");
            }

            Assert.That(response3.Status, Is.EqualTo(200));
            var textResponse = await response3.TextAsync();
            var jsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(textResponse);
            var reviewId = jsonResponse["id"].ToString();

            var updatedReview = new
            {
                rating = 5,
                userId = userId,
                restaurantId = restaurantId
            };


            await using var response4 = await Request.PutAsync($"/api/Review/UpdateReview?id={reviewId}", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = updatedReview
            });

            Assert.That(response4.Status, Is.EqualTo(200));
            var textResponse4 = await response4.TextAsync();
            Assert.IsNotNull(textResponse4);
            Assert.That(textResponse4, Does.Contain("Review updated successfully."));

        }

        [Test]
        public async Task AzuirajReview_Neuspesno()
        {
            string nepostojeciId = "65ed984c454619ba306c8c63";

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
            var createRestaurantTextResponse = await response.TextAsync();
            var createRestaurantJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createRestaurantTextResponse);
            var restaurantId = createRestaurantJsonResponse["id"].ToString();


            var korisnik = new //obavezno promeni email
            {

                name = "Vasilije",
                surname = "Markovic",
                username = "Vasilije",
                email = "vasilije@gmail.com",
                password = "vasilije",
                phoneNumber = "0627596267",
                role = "user"
            };

            await using var response2 = await Request.PostAsync("/api/User/Register", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = korisnik
            }); ;

            if (response2.Status != 200)
            {
                Assert.Fail($"Code: {response2.Status} - {response2.StatusText}");
            }

            Assert.That(response2.Status, Is.EqualTo(200));
            var createUserTextResponse = await response2.TextAsync();
            var createUserJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createUserTextResponse);
            var userId = createUserJsonResponse["id"].ToString();

            var review = new
            {
                rating = 3,
                userId = userId,
                restaurantId = restaurantId
            };

            await using var response3 = await Request.PostAsync("/api/Review/AddReview", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = review
            }); ;

            if (response3.Status != 200)
            {
                Assert.Fail($"Code: {response3.Status} - {response3.StatusText}");
            }

            Assert.That(response3.Status, Is.EqualTo(200));
            var textResponse = await response3.TextAsync();
            var jsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(textResponse);
            var reviewId = jsonResponse["id"].ToString();

            var updatedReview = new
            {
                rating = 5,
                userId = userId,
                restaurantId = restaurantId
            };


            await using var response4 = await Request.PutAsync($"/api/Review/UpdateReview?id={nepostojeciId}", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = updatedReview
            });

            Assert.That(response4.Status, Is.EqualTo(400));
            var textResponse4 = await response4.TextAsync();
            Assert.IsNotNull(textResponse4);

            Assert.That(textResponse4, Does.Contain($"Review with ID {nepostojeciId} not found."));
        }

        [Test]
        public async Task ObrisiReview_Uspesno()
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
            var createRestaurantTextResponse = await response.TextAsync();
            var createRestaurantJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createRestaurantTextResponse);
            var restaurantId = createRestaurantJsonResponse["id"].ToString();


            var korisnik = new //obavezno promeni email
            {

                name = "Kosta",
                surname = "Kostic",
                username = "Kosta",
                email = "kosta@gmail.com",
                password = "kosta",
                phoneNumber = "0627596267",
                role = "user"
            };

            await using var response2 = await Request.PostAsync("/api/User/Register", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = korisnik
            }); ;

            if (response2.Status != 200)
            {
                Assert.Fail($"Code: {response2.Status} - {response2.StatusText}");
            }

            Assert.That(response2.Status, Is.EqualTo(200));
            var createUserTextResponse = await response2.TextAsync();
            var createUserJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createUserTextResponse);
            var userId = createUserJsonResponse["id"].ToString();

            var review = new
            {
                rating = 3,
                userId = userId,
                restaurantId = restaurantId
            };

            await using var response3 = await Request.PostAsync("/api/Review/AddReview", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = review
            }); ;

            if (response3.Status != 200)
            {
                Assert.Fail($"Code: {response3.Status} - {response3.StatusText}");
            }

            Assert.That(response3.Status, Is.EqualTo(200));
            var textResponse = await response3.TextAsync();
            var jsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(textResponse);
            var reviewId = jsonResponse["id"].ToString();

            await using var response4 = await Request.DeleteAsync($"/api/Review/DeleteReview?id={reviewId}");

            Assert.That(response4.Status, Is.EqualTo(200));
            var textResponse4 = await response4.TextAsync();
            Assert.IsNotNull(textResponse4);
            Assert.That(textResponse4, Does.Contain("Review deleted successfully."));
        }

        [Test]
        public async Task ObrisiReview_Neuspesno()
        {
            string nepostojeciId = "65ed984c454619ba306c8c63";

            await using var response = await Request.DeleteAsync($"/api/Review/DeleteReview?id={nepostojeciId}");

            Assert.That(response.Status, Is.EqualTo(400));
            var textResponse = await response.TextAsync();
            Assert.IsNotNull(textResponse);
            Assert.That(textResponse, Does.Contain($"Review with ID {nepostojeciId} not found."));
        }

        [Test]
        public async Task PreuzmiReviewRestorana()
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
            var createRestaurantTextResponse = await response.TextAsync();
            var createRestaurantJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createRestaurantTextResponse);
            var restaurantId = createRestaurantJsonResponse["id"].ToString();


            var korisnik = new //obavezno promeni email
            {

                name = "Bozidar",
                surname = "Kostic",
                username = "Bozidar",
                email = "bozidar@gmail.com",
                password = "bozidar",
                phoneNumber = "0627596267",
                role = "user"
            };

            await using var response2 = await Request.PostAsync("/api/User/Register", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = korisnik
            }); ;

            if (response2.Status != 200)
            {
                Assert.Fail($"Code: {response2.Status} - {response2.StatusText}");
            }

            Assert.That(response2.Status, Is.EqualTo(200));
            var createUserTextResponse = await response2.TextAsync();
            var createUserJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createUserTextResponse);
            var userId = createUserJsonResponse["id"].ToString();

            var review = new
            {
                rating = 3,
                userId = userId,
                restaurantId = restaurantId
            };

            await using var response3 = await Request.PostAsync("/api/Review/AddReview", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = review
            }); ;

            if (response3.Status != 200)
            {
                Assert.Fail($"Code: {response3.Status} - {response3.StatusText}");
            }

            Assert.That(response3.Status, Is.EqualTo(200));
            var textResponse = await response3.TextAsync();
            var jsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(textResponse);
            var reviewId = jsonResponse["id"].ToString();

            await using var response4 = await Request.GetAsync($"/api/Review/GetReviewsForRestaurant?restaurantId={restaurantId}");

            Assert.That(response4.Status, Is.EqualTo(200));
           
        }

        [Test]
        public async Task PreuzmiReviewKorisnika()
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
            var createRestaurantTextResponse = await response.TextAsync();
            var createRestaurantJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createRestaurantTextResponse);
            var restaurantId = createRestaurantJsonResponse["id"].ToString();


            var korisnik = new //obavezno promeni email
            {

                name = "Nenad",
                surname = "Petrovic",
                username = "Nenad",
                email = "nenad@gmail.com",
                password = "nenad",
                phoneNumber = "0627596267",
                role = "user"
            };

            await using var response2 = await Request.PostAsync("/api/User/Register", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = korisnik
            }); ;

            if (response2.Status != 200)
            {
                Assert.Fail($"Code: {response2.Status} - {response2.StatusText}");
            }

            Assert.That(response2.Status, Is.EqualTo(200));
            var createUserTextResponse = await response2.TextAsync();
            var createUserJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createUserTextResponse);
            var userId = createUserJsonResponse["id"].ToString();

            var review = new
            {
                rating = 3,
                userId = userId,
                restaurantId = restaurantId
            };

            await using var response3 = await Request.PostAsync("/api/Review/AddReview", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = review
            }); ;

            if (response3.Status != 200)
            {
                Assert.Fail($"Code: {response3.Status} - {response3.StatusText}");
            }

            Assert.That(response3.Status, Is.EqualTo(200));
            var textResponse = await response3.TextAsync();
            var jsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(textResponse);
            var reviewId = jsonResponse["id"].ToString();

            await using var response4 = await Request.GetAsync($"/api/Review/GetReviewsForUser?userId={userId}");

            Assert.That(response4.Status, Is.EqualTo(200));

        }


    }    

}
