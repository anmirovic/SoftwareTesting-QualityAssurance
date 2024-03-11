using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using DeliverEase.Models;
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
    public class MealTests
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
        public async Task DodajMeal_UspesnoDodavanje()
        {
            var restoran = new Restaurant
            {
                Name = "Kineski restoran",
                Address = "Dimitrija Tucovica",
                Meals = new List<Meal>
                {
                    new Meal { Name = "Nudle sa piletinom", Description = "500g", Price = 800 }

                }
            };

            await _restaurantController.CreateRestaurant(restoran);

            var meal = new Meal
            {
                Name = "Cufte",
                Description = "U sosu",
                Price = 1000
            };

            var result = await _restaurantController.AddMealToRestaurant(restoran.Id, meal);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;

            Assert.IsNotNull(okResult, "Povratna vrednost nije instanca okObjectResult-a.");

            var updatedRestaurantResult = await _restaurantController.GetRestaurantById(restoran.Id);
            var updatedRestaurant = (updatedRestaurantResult.Result as OkObjectResult).Value as Restaurant;

            Assert.IsTrue(updatedRestaurant.Meals.Any(m => m.Id == meal.Id), "Dodati obrok nije pronađen u listi obroka restorana.");


        }

        [Test]
        public async Task DodajMeal_NepostojeciRestaurantId()
        {
            var meal = new Meal
            {
                Name = "TestMeal",
                Description = "Opis test meal-a",
                Price = 1000
            };

            var nepostojeciId = "65ed984c454619ba306c8c63";
            var result = await _restaurantController.AddMealToRestaurant(nepostojeciId, meal);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;

            Assert.IsNotNull(badRequestResult.Value);

            Assert.AreEqual($"Restaurant with ID {nepostojeciId} not found.", badRequestResult.Value);
        }

        [Test]
        public async Task DodajMeal_NeuspesnoDodavanje()
        {
            var restoran = new Restaurant
            {
                Name = "Julija",
                Address = "Jovana Kolarovica",
                Meals = new List<Meal>
                {
                    new Meal { Name = "Pasta", Description = "Carbonara", Price = 800 }

                }
            };

            await _restaurantController.CreateRestaurant(restoran);

            var MealsaGreskom = new Meal
            {
                Name = "Pasta",
                Description = "Bolonjeze"
            };

            var result = await _restaurantController.AddMealToRestaurant(restoran.Id, MealsaGreskom);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);


        }

        [Test]
        public async Task ObrisiMeal_UspesnoBrisanje()
        {

            var restoran = new Restaurant
            {
                Name = "Castello",
                Address = "Veljka Vlahovica",
                Meals = new List<Meal>
                {
                    new Meal { Name = "Pasta", Description = "Carbonara", Price = 800 },
                    new Meal { Name = "Pizza", Description = "Capricoza 25 cm", Price = 700 }

                }
            };

            await _restaurantController.CreateRestaurant(restoran);
            var mealToDelete = restoran.Meals.FirstOrDefault();

            var result = await _restaurantController.DeleteMealFromRestaurant(restoran.Id, mealToDelete.Id);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.AreEqual($"Izbrisano je jelo: {mealToDelete.Id}", okResult.Value);
        }

        [Test]
        public async Task ObrisiMeal_NepostojeciIdMeal()
        {
            var restoran = new Restaurant
            {
                Name = "Novi restoran",
                Address = "Ulica 4",
                Meals = new List<Meal>
                {
                    new Meal { Name = "Pasta", Description = "Carbonara", Price = 800 }

                }

            };

            await _restaurantController.CreateRestaurant(restoran);

            string nepostojeciId = "65ed984c454619ba306c8c63";


            var result = await _restaurantController.DeleteMealFromRestaurant(restoran.Id, nepostojeciId);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;

            Assert.IsNotNull(badRequestResult.Value);

            Assert.AreEqual($"Meal with ID {nepostojeciId} not found", badRequestResult.Value);
        }

        [Test]
        public async Task ObrisiMeal_NepostojeciIdRestaurant()
        {

            var restoran = new Restaurant
            {
                Name = "Gepetto",
                Address = "Ulica 26",
                Meals = new List<Meal>
                {
                    new Meal { Name = "Tost", Description = "Sunka i kackavalj", Price = 300 }

                }

            };

            await _restaurantController.CreateRestaurant(restoran);

            string nepostojeciId = "65ed984c454619ba306c8c63";
            var mealToDelete = restoran.Meals.FirstOrDefault();

            var result = await _restaurantController.DeleteMealFromRestaurant(nepostojeciId, mealToDelete.Id);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;

            Assert.IsNotNull(badRequestResult.Value);

            Assert.AreEqual($"Restaurant with ID {nepostojeciId} not found.", badRequestResult.Value);
        }

        [Test]
        public async Task ObrisiMeal_ProveraBrisanjaIzBaze()
        {
            var restoran = new Restaurant
            {
                Name = "Castello",
                Address = "Veljka Vlahovica",
                Meals = new List<Meal>
                {
                    new Meal { Name = "Pasta", Description = "Carbonara", Price = 800 },
                    new Meal { Name = "Pizza", Description = "Capricoza 25 cm", Price = 700 }

                }
            };

            await _restaurantController.CreateRestaurant(restoran);
            var mealToDelete = restoran.Meals.FirstOrDefault();

            await _restaurantController.DeleteMealFromRestaurant(restoran.Id, mealToDelete.Id);
            var mealIzBaze = await _restaurantController.GetMealById(restoran.Id, mealToDelete.Id);

            Assert.IsNotNull(mealIzBaze, "Jelo nije obrisano iz baze.");
        }


        [Test]
        public async Task PreuzmiMealsNekogRestorana_UspesnoPreuzimanje()
        {


            var restoran = new Restaurant
            {
                Name = "Cezar",
                Address = "Kralja Aleksandra",
                Meals = new List<Meal>
                {
                    new Meal { Name = "Biftek", Description = "srednje pecen", Price = 1700 },
                    new Meal { Name = "Pasta", Description = "Bolonjeze", Price = 1000 }
                }
            };
            
            await _restaurantController.CreateRestaurant(restoran);
            

            var actionResult = await _restaurantController.GetAllMealsInRestaurant(restoran.Id);
            var result = actionResult.Result as OkObjectResult;

            Assert.IsNotNull(result);

            var meals = result.Value as List<Meal>;
            Assert.IsNotNull(meals);


        }

        [Test]
        public async Task PreuzmiMealsNekogRestorana_PraznaLista()
        {


            var restoran = new Restaurant
            {
                Name = "Cezar",
                Address = "Kralja Aleksandra",
                Meals = new List<Meal>()
                          
            };

            await _restaurantController.CreateRestaurant(restoran);


            var actionResult = await _restaurantController.GetAllMealsInRestaurant(restoran.Id);
            var result = actionResult.Result as OkObjectResult;

            Assert.IsNotNull(result);

            var meals = result.Value as List<Meal>;
            Assert.IsNotNull(meals);
            Assert.AreEqual(0, meals.Count);

        }

        [Test]
        public async Task PreuzmiMeal_UspesnoPreuzimanje()
        {
            var restoran = new Restaurant
            {
                Name = "Bolji Zivot",
                Address = "Generala Pavla Ilica",
                Meals = new List<Meal>
                {
                    new Meal { Name = "Jelo 1", Description = "Opis jela 1", Price = 1000 },
                    new Meal { Name = "Jelo 2", Description = "Opis jela 2", Price = 1200 }
                }
            };

            await _restaurantController.CreateRestaurant(restoran);

            var obrok = restoran.Meals.FirstOrDefault();

            var actionResult = await _restaurantController.GetMealById(restoran.Id, obrok.Id);
           
            Assert.IsInstanceOf<OkObjectResult>(actionResult.Result);
            var result = (OkObjectResult)actionResult.Result;

            
            Assert.IsNotNull(result);

            
            var getMeal = result.Value as Meal;

            
            Assert.IsNotNull(getMeal);
            Assert.AreEqual(obrok.Id, getMeal.Id);
        }

        [Test]
        public async Task PreuzmiMeal_NepostojeciIdMeal()
        {
            var restoran = new Restaurant
            {
                Name = "Stari Fijaker",
                Address = "Samarinovacki put bb",
                Meals = new List<Meal>
                {
                    new Meal { Name = "Jelo 3", Description = "Opis jela 1", Price = 1000 },
                    new Meal { Name = "Jelo 4", Description = "Opis jela 2", Price = 1200 }
                }
            };

            await _restaurantController.CreateRestaurant(restoran);

            string nepostojeciMealId = "65ed984c454619ba306c8c63";

            var rezultat = await _restaurantController.GetMealById(restoran.Id, nepostojeciMealId);
            
            Assert.IsInstanceOf<BadRequestObjectResult>(rezultat.Result);
            var result = (BadRequestObjectResult)rezultat.Result;


            Assert.IsNotNull(result);
            Assert.AreEqual($"Meal with ID {nepostojeciMealId} not found.", result.Value);
        }

        [Test]
        public async Task PreuzmiMeal_NepostojeciIdRestaurant()
        {
            var restoran = new Restaurant
            {
                Name = "Forma",
                Address = "Hajduk Veljkova",
                Meals = new List<Meal>
                {
                    new Meal { Name = "Jelo 5", Description = "Opis jela 1", Price = 1000 },
                    new Meal { Name = "Jelo 6", Description = "Opis jela 2", Price = 1200 }
                }
            };

            await _restaurantController.CreateRestaurant(restoran);

            string nepostojeciRestaurantId = "65ed984c454619ba306c8c63";
            var obrok = restoran.Meals.FirstOrDefault();

            var rezultat = await _restaurantController.GetMealById(nepostojeciRestaurantId, obrok.Id);

            Assert.IsInstanceOf<BadRequestObjectResult>(rezultat.Result);
            var result = (BadRequestObjectResult)rezultat.Result;


            Assert.IsNotNull(result);
            Assert.AreEqual($"Restaurant with ID {nepostojeciRestaurantId} not found.", result.Value);
        }

    }
}
