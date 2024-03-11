using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using DeliverEase.Models;
using DeliverEase.Controllers;
using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;
using DeliverEase.Controllers;
using MongoDB.Driver;
using System.ComponentModel.DataAnnotations;
using DeliverEase.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace NUnitTests
{
    public class RestaurantTests
    {
        private DeliverEase.Controllers.RestaurantController _restaurantController;

        [SetUp]
        public void Setup()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var mongoConnectionString = "mongodb://localhost:27017";

            var mongoClient = new MongoClient(mongoConnectionString);
            var database = mongoClient.GetDatabase("DeliverEase");

            var restaurantService = new RestaurantService(database);
            var reviewService = new ReviewService(database);

            _restaurantController = new DeliverEase.Controllers.RestaurantController(restaurantService, reviewService);


        }

        [Test]
        public async Task DodajRestoran_saJelima()
        {
            Restaurant restaurant = new Restaurant
            { 
                Name = "NightandDay",
                Address = "Obrenoviceva",
                Meals = new List<Meal>
                {
                    new Meal { Name = "Pizza", Description = "Capricoza", Price = 1700 },
                    new Meal { Name = "Pasta", Description = "Bolonjeze", Price = 1000 }
                }
            };

            var result = await _restaurantController.CreateRestaurant(restaurant);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;

            Assert.IsNotNull(okResult, "Povratna vrednost nije instanca okObjectResult-a.");

            var addedRestaurant = okResult.Value as Restaurant;
            Assert.IsNotNull(addedRestaurant, "Dodati restoran nije instanciran.");
            Assert.IsNotNull(addedRestaurant.Id, "ID nije postavljen nakon dodavanja restorana.");
            Assert.AreEqual(restaurant.Id, addedRestaurant.Id, "ID dodatog restorana se ne podudara sa ocekivanim ID-em.");

        }

        [Test]
        public async Task DodajRestoran_bezJela()
        {
            Restaurant restaurant = new Restaurant
            {
                Name = "PrazanRestoran",
                Address = "Obilicev Venac",
                Meals = new List<Meal>()
                
            };

            var result = await _restaurantController.CreateRestaurant(restaurant);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;

            Assert.IsNotNull(okResult, "Povratna vrednost nije instanca okObjectResult-a.");

            var addedRestaurant = okResult.Value as Restaurant;
            Assert.IsNotNull(addedRestaurant, "Dodati restoran nije instanciran.");
            Assert.IsNotNull(addedRestaurant.Id, "ID nije postavljen nakon dodavanja restorana.");
            Assert.AreEqual(restaurant.Id, addedRestaurant.Id, "ID dodatog restorana se ne podudara sa ocekivanim ID-em.");
        }

        [Test]
        public async Task DodajRestoran_NeuspesnoDodavanje()
        {
            Restaurant RestoranSaGreskom = new Restaurant
            {
                Name = "Kod Rajka",
                Meals = new List<Meal>
                {
                    new Meal { Name = "Rostilj", Description = "1 Porcija", Price = 1300 },
                    new Meal { Name = "Palacinke", Description = "Krem, krem i plazma", Price = 500 }
                }
            };

            var result = await _restaurantController.CreateRestaurant(RestoranSaGreskom);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task ObrisiRestoran_UspesnoBrisanje()
        {
           var  restoran = new Restaurant
           {
                Name = "Kod Rajka",
                Address= "Toplicina",
                Meals = new List<Meal>
                {
                    new Meal { Name = "Rostilj", Description = "1 Porcija", Price = 1300 },
                    new Meal { Name = "Palacinke", Description = "Krem, krem i plazma", Price = 500 }
                }
           };

            await _restaurantController.CreateRestaurant(restoran);

            var result = await _restaurantController.DeleteRestaurant(restoran.Id);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.AreEqual($"Izbrisan je restoran: {restoran.Id}", okResult.Value);
        }

        [Test]
        public async Task ObrisiRestoran_NepostojeciRestoran()
        {
            string nepostojeciId = "65ed984c454619ba306c8c63";

            var result = await _restaurantController.DeleteRestaurant(nepostojeciId);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;

            Assert.IsNotNull(badRequestResult.Value);

            Assert.AreEqual($"Restaurant with ID {nepostojeciId} does not exist.", badRequestResult.Value);
        }

        [Test]
        public async Task ObrisiRestoran_ProveraBrisanjaIzBaze()
        {
            var restoran = new Restaurant
            {
                Name = "Kod Rajka",
                Address = "Toplicina",
                Meals = new List<Meal>
                {
                    new Meal { Name = "Rostilj", Description = "1 Porcija", Price = 1300 },
                    new Meal { Name = "Palacinke", Description = "Krem, krem i plazma", Price = 500 }
                }
            };

            await _restaurantController.CreateRestaurant(restoran);

            await _restaurantController.DeleteRestaurant(restoran.Id);
            var restoranIzBaze = await _restaurantController.GetRestaurantById(restoran.Id);

            Assert.IsNotNull(restoranIzBaze, "Restoran nije obrisan iz baze.");
        }

        [Test]
        public async Task PreuzmiRestorane_UspesnoPreuzimanje()
        {

            var restaurants = new List<Restaurant>
            {
                new Restaurant
                {
                Name = "Galija",
                Address = "Nikole Pasica",
                Meals = new List<Meal>
                {
                    new Meal { Name = "Biftek", Description = "srednje pecen", Price = 1700 },
                    new Meal { Name = "Pasta", Description = "Bolonjeze", Price = 1000 }
                }},

                new Restaurant
                {
                Name = "Komuna",
                Address = "Nikole Pasica",
                Meals = new List<Meal>
                {
                    new Meal { Name = "Burger", Description = "juneci burger", Price = 1000 }

                }}
            };

            foreach (var restaurant in restaurants)
            {
                await _restaurantController.CreateRestaurant(restaurant);
            }

            var actionResult = await _restaurantController.GetAllRestaurants();
            var result = actionResult.Result as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            var getRestaurants = result.Value as List<Restaurant>;
            Assert.That(getRestaurants, Is.Not.Null);
            Assert.That(getRestaurants.Count + restaurants.Count, Is.EqualTo(restaurants.Count + getRestaurants.Count));

        }

        [Test]
        public async Task PreuzmiRestorane_PraznaLista()
        {
            var mongoClient = new MongoClient("mongodb://localhost:27017");

            var database = mongoClient.GetDatabase("TestDatabase");

            var restaurantsCollection = database.GetCollection<Restaurant>("Restaurants");
            await restaurantsCollection.DeleteManyAsync(Builders<Restaurant>.Filter.Empty);

            var restaurantService = new RestaurantService(database);
            var reviewService = new ReviewService(database);
            var restaurantController = new DeliverEase.Controllers.RestaurantController(restaurantService, reviewService);

            var restaurants = new List<Restaurant>();

            foreach (var restaurant in restaurants)
            {
                await restaurantController.CreateRestaurant(restaurant);
            }

            var actionResult = await restaurantController.GetAllRestaurants();

            Assert.IsInstanceOf<OkObjectResult>(actionResult.Result);
            var result = (OkObjectResult)actionResult.Result;

            Assert.IsNotNull(result);

            var getRestaurants = result.Value as List<Restaurant>;

            Assert.IsNotNull(getRestaurants);
            Assert.AreEqual(restaurants.Count, getRestaurants.Count);
        }

        [Test]
        public async Task PreuzmiRestoran_UspesnoPreuzimanje()
        {
            var restoran = new Restaurant
            {
                Name = "Sangaj",
                Address = "Obrenoviceva",
                Meals = new List<Meal>
                {
                    new Meal { Name = "Nudle sa piletinom", Description = "500g", Price = 800 },
                    new Meal { Name = "Rizoto sa kikirikijem", Description = "300g", Price = 500 }
                }
            };

            await _restaurantController.CreateRestaurant(restoran);

            var actionResult = await _restaurantController.GetRestaurantById(restoran.Id);

            Assert.IsInstanceOf<OkObjectResult>(actionResult.Result);
            var result = (OkObjectResult)actionResult.Result;

        
            Assert.IsNotNull(result);

            
            var getRestaurant = result.Value as Restaurant;

            
            Assert.IsNotNull(getRestaurant);
            Assert.AreEqual(restoran.Id, getRestaurant.Id);
        }

        [Test]
        public async Task PreuzmiNepostojeciRestoran()
        {
            string nepostojeciId = "65ed984c454619ba306c8c10";

            var rezultat = await _restaurantController.GetRestaurantById(nepostojeciId);


            Assert.IsInstanceOf<BadRequestObjectResult>(rezultat.Result);
            var result = (BadRequestObjectResult)rezultat.Result;

            Assert.IsNotNull(result);


        }

        [Test]
        public async Task AzurirajRestoran_UspesnoAzuriranje()
        {

            var restaurant = new Restaurant
            {
                Name = "Na cosku",
                Address = "Kazandzijsko",
                Meals = new List<Meal>
                {
                    new Meal { Name = "Cizkejk", Description = "dodatak: sladoled", Price = 500 }
                }
            };

            await _restaurantController.CreateRestaurant(restaurant);

            restaurant.Name = "Novo ime";

            var result = await _restaurantController.UpdateRestaurant(restaurant.Id, restaurant);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.AreEqual("Restaurant successfully updated.", okResult.Value);
        }

        [Test]
        public async Task AzurirajRestoran_NepostojeciId()
        {
            var nepostojeciId = "65ed984c454619ba306c8c63";

            var restaurant = new Restaurant
            {
                Name = "Menza",
                Address = "Bulevar Nikole Tesle",
                Meals = new List<Meal>
                {
                    new Meal { Name = "Cizkejk", Description = "dodatak: sladoled", Price = 500 }
                }
            };

            var result = await _restaurantController.UpdateRestaurant(nepostojeciId, restaurant);


            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;

            Assert.IsNotNull(badRequestResult.Value);

            Assert.AreEqual($"Restaurant with ID {nepostojeciId} does not exist.", badRequestResult.Value);
        }

        [Test]
        public async Task SortByRating_UspesnoSortiranje_Descending()
        {
          
            var restaurants = new List<Restaurant>
            {
                new Restaurant
                {
                    Name = "Restaurant1",
                    Address = "Adresa 1",
                    Rating = 4.5,
                    Meals = new List<Meal>
                    {
                        new Meal { Name = "Cizkejk", Description = "dodatak: sladoled", Price = 600 }
                    }

                },
                new Restaurant
                {
                    Name = "Restaurant2",
                    Address = "Adresa 2",
                    Rating = 4.0,
                    Meals = new List<Meal>
                    {
                        new Meal { Name = "Sufle", Description = "dodatak: sladoled", Price = 500 }
                    }

                },
                new Restaurant
                {
                    Name = "Restaurant3",
                    Address = "Adresa 3",
                    Rating = 4.8,
                    Meals = new List<Meal>
                    {
                        new Meal { Name = "Tiramisu", Description = "dodatak: sladoled", Price = 700 }
                    }

                }
            };

            foreach (var restaurant in restaurants)
            {
                await _restaurantController.CreateRestaurant(restaurant);
            }


            var actionResult = await _restaurantController.GetRestaurantsSortedByRating(ascending: false);

            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOf<OkObjectResult>(actionResult.Result);

            var result = actionResult.Result as OkObjectResult;
            Assert.IsNotNull(result);

            var sortedRestaurants = result.Value as List<Restaurant>;
            Assert.IsNotNull(sortedRestaurants);

            for (int i = 1; i < sortedRestaurants.Count; i++)
            {
                Assert.GreaterOrEqual(sortedRestaurants[i - 1].Rating, sortedRestaurants[i].Rating);
            }
        }

        [Test]
        public async Task SortByRating_UspesnoSortiranje_Ascending()
        {

            var restaurants = new List<Restaurant>
            {
                new Restaurant
                {
                    Name = "Restaurant4",
                    Address = "Adresa 4",
                    Rating = 5.0,
                    Meals = new List<Meal>
                    {
                        new Meal { Name = "Cizkejk", Description = "dodatak: sladoled", Price = 600 }
                    }

                },
                new Restaurant
                {
                    Name = "Restaurant5",
                    Address = "Adresa 5",
                    Rating = 4.0,
                    Meals = new List<Meal>
                    {
                        new Meal { Name = "Sufle", Description = "dodatak: sladoled", Price = 500 }
                    }

                },
                new Restaurant
                {
                    Name = "Restaurant6",
                    Address = "Adresa 6",
                    Rating = 3.8,
                    Meals = new List<Meal>
                    {
                        new Meal { Name = "Tiramisu", Description = "dodatak: sladoled", Price = 700 }
                    }

                }
            };

            foreach (var restaurant in restaurants)
            {
                await _restaurantController.CreateRestaurant(restaurant);
            }

            var actionResult = await _restaurantController.GetRestaurantsSortedByRating(ascending: true);

            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOf<OkObjectResult>(actionResult.Result);

            var result = actionResult.Result as OkObjectResult;
            Assert.IsNotNull(result);

            var sortedRestaurants = result.Value as List<Restaurant>;
            Assert.IsNotNull(sortedRestaurants);

            for (int i = 1; i < sortedRestaurants.Count; i++)
            {
                Assert.LessOrEqual(sortedRestaurants[i - 1].Rating, sortedRestaurants[i].Rating);
            }
        }

        [Test]
        public async Task SortByRating_PraznaLista_Ascending()
        {
            var mongoClient = new MongoClient("mongodb://localhost:27017");
            var database = mongoClient.GetDatabase("TestDatabase");
            var restaurantsCollection = database.GetCollection<Restaurant>("Restaurants");

            // Delete existing restaurants from the collection
            await restaurantsCollection.DeleteManyAsync(Builders<Restaurant>.Filter.Empty);

            var restaurantService = new RestaurantService(database);
            var reviewService = new ReviewService(database);
            var restaurantController = new DeliverEase.Controllers.RestaurantController(restaurantService, reviewService);

            var restaurants = new List<Restaurant>();

            var actionResult = await restaurantController.GetRestaurantsSortedByRating(ascending: true);

            Assert.IsInstanceOf<OkObjectResult>(actionResult.Result);
            var result = (OkObjectResult)actionResult.Result;

            Assert.IsNotNull(result);

            var sortedRestaurants = result.Value as List<Restaurant>;
            Assert.IsNotNull(sortedRestaurants);

            Assert.AreEqual(0, sortedRestaurants.Count);
        }


    }
}
