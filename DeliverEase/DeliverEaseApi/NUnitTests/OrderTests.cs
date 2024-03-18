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
    public class OrderTests
    {
        private DeliverEase.Controllers.OrderController _orderController;
        private DeliverEase.Controllers.UserController _userController;
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

            var userService = new UserService(database);
            var restaurantService = new RestaurantService(database);
            var reviewService = new ReviewService(database);
            var orderService = new OrderService(database, restaurantService, userService);


            _orderController = new DeliverEase.Controllers.OrderController(orderService);
            _userController = new DeliverEase.Controllers.UserController(userService);
            _restaurantController = new DeliverEase.Controllers.RestaurantController(restaurantService, reviewService);


        }

        [Test]
        public async Task DodajOrder_saJelima()
        {
            Restaurant newRestaurant = new Restaurant
            {
                Name = "Carneval",
                Address = "Branka Copica",
                Meals = new List<Meal>
                {
                    new Meal { Name = "Pizza", Description = "Capricoza", Price = 1700 },
                    new Meal { Name = "Pasta", Description = "Bolonjeze", Price = 1000 }
                }
            };

            User newUser = new User
            {
                Name = "Mirka",
                Surname = "Mirkovic",
                Username = "Mirka",
                Email = "mirka@gmail.com", //pri ponovnom pokretanju testa, promeniti email 
                Password = "mirka",
                PhoneNumber = "0647239567",
                Role = "user"
            };

            var restaurantActionResult = await _restaurantController.CreateRestaurant(newRestaurant);
            var restaurantObjectResult = restaurantActionResult as ObjectResult;
            var restaurant = restaurantObjectResult.Value as Restaurant;

            var userActionResult = await _userController.Register(newUser);
            var userObjectResult = userActionResult as ObjectResult;
            var user = userObjectResult.Value as User;

            var selectedMeals = restaurant.Meals.Take(2).ToList();

            var mealIds = selectedMeals.Select(meal => meal.Id).ToList();

            var createOrderResult = await _orderController.CreateOrder(restaurant.Id, user.Id, mealIds);
            
            Assert.IsInstanceOf<OkObjectResult>(createOrderResult);
            var okResult = createOrderResult as OkObjectResult;

            Assert.IsNotNull(okResult, "Povratna vrednost nije instanca okObjectResult-a.");

            var addedOrder = okResult.Value as Order;
            Assert.IsNotNull(addedOrder, "Dodata narudzbina nije instancirana.");
            Assert.IsNotNull(addedOrder.Id, "ID nije postavljen nakon dodavanja narudzbine.");
        }

        [Test]
        public async Task DodajOrder_NeuspesnoDodavanje()
        {
            Restaurant newRestaurant = new Restaurant
            {
                Name = "Kod Zike",
                Address = "Bulevar Nemanjica",
                Meals = new List<Meal>
                {
                    new Meal { Name = "Rostilj", Description = "1kg", Price = 1700 },
                    new Meal { Name = "Salata", Description = "Kupus salata", Price = 500 }
                }
            };

            User newUser = new User
            {
                Name = "Milica",
                Surname = "Andric",
                Username = "Milica",
                Email = "milicaandric@gmail.com", //pri ponovnom pokretanju testa, promeniti email
                Password = "milica",
                PhoneNumber = "0642839577",
                Role = "user"
            };

            var restaurantActionResult = await _restaurantController.CreateRestaurant(newRestaurant);
            var restaurantObjectResult = restaurantActionResult as ObjectResult;
            var restaurant = restaurantObjectResult.Value as Restaurant;

            var userActionResult = await _userController.Register(newUser);
            var userObjectResult = userActionResult as ObjectResult;
            var user = userObjectResult.Value as User;

            var mealIds = new List<string>();

            var result = await _orderController.CreateOrder(restaurant.Id, user.Id, mealIds);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task ObrisiOrder_UspesnoBrisanje()
        {
            Restaurant newRestaurant = new Restaurant
            {
                Name = "Carneval",
                Address = "Branka Copica",
                Meals = new List<Meal>
                {
                    new Meal { Name = "Pizza", Description = "Capricoza", Price = 1700 },
                    new Meal { Name = "Pasta", Description = "Bolonjeze", Price = 1000 }
                }
            };

            User newUser = new User
            {
                Name = "Mirka",
                Surname = "Mirkovic",
                Username = "Mirka",
                Email = "mirkamirk@gmail.com", //pri ponovnom pokretanju testa, promeniti email
                Password = "mirka",
                PhoneNumber = "0647239567",
                Role = "user"
            };

            var restaurantActionResult = await _restaurantController.CreateRestaurant(newRestaurant);
            var restaurantObjectResult = restaurantActionResult as ObjectResult;
            var restaurant = restaurantObjectResult.Value as Restaurant;

            var userActionResult = await _userController.Register(newUser);
            var userObjectResult = userActionResult as ObjectResult;
            var user = userObjectResult.Value as User;

            var selectedMeals = restaurant.Meals.Take(1).ToList();

            var mealIds = selectedMeals.Select(meal => meal.Id).ToList();

            var orderActionResult = await _orderController.CreateOrder(restaurant.Id, user.Id, mealIds);
            var orderObjectResult = orderActionResult as ObjectResult;
            var order = orderObjectResult.Value as Order;

            var result = await _orderController.DeleteOrder(order.Id);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.AreEqual("Order successfully removed.", okResult.Value);
        }

        [Test]
        public async Task ObrisiOrder_NepostojeciOrder()
        {
            string nepostojeciId = "65ed984c454619ba306c8c63";

            var result = await _orderController.DeleteOrder(nepostojeciId);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;

            Assert.IsNotNull(badRequestResult.Value);

            Assert.AreEqual($"Order with ID {nepostojeciId} does not exist.", badRequestResult.Value);
        }

        [Test]
        public async Task ObrisiOrder_ProveraBrisanjaIzBaze()
        {
            Restaurant newRestaurant = new Restaurant
            {
                Name = "Carneval",
                Address = "Branka Copica",
                Meals = new List<Meal>
                {
                    new Meal { Name = "Pizza", Description = "Capricoza", Price = 1700 },
                    new Meal { Name = "Pasta", Description = "Bolonjeze", Price = 1000 }
                }
            };

            User newUser = new User
            {
                Name = "Mirko",
                Surname = "Bojanic",
                Username = "Mirko",
                Email = "mirkomirko@gmail.com", //pri ponovnom pokretanju testa, promeniti email
                Password = "mirko",
                PhoneNumber = "0667239667",
                Role = "user"
            };

            var restaurantActionResult = await _restaurantController.CreateRestaurant(newRestaurant);
            var restaurantObjectResult = restaurantActionResult as ObjectResult;
            var restaurant = restaurantObjectResult.Value as Restaurant;

            var userActionResult = await _userController.Register(newUser);
            var userObjectResult = userActionResult as ObjectResult;
            var user = userObjectResult.Value as User;

            var selectedMeals = restaurant.Meals.Take(1).ToList();

            var mealIds = selectedMeals.Select(meal => meal.Id).ToList();

            var orderActionResult = await _orderController.CreateOrder(restaurant.Id, user.Id, mealIds);
            var orderObjectResult = orderActionResult as ObjectResult;
            var order = orderObjectResult.Value as Order;

            var result = await _orderController.DeleteOrder(order.Id);

            var orderIzBaze = await _orderController.GetOrderById(order.Id);

            Assert.IsNotNull(orderIzBaze, "Narudzbina nije obrisana iz baze.");
        }

        [Test]
        public async Task PreuzmiOrder_UspesnoPreuzimanje()
        {
            Restaurant newRestaurant = new Restaurant
            {
                Name = "Tron",
                Address = "Branka Copica",
                Meals = new List<Meal>
                {
                    new Meal { Name = "Pizza", Description = "Capricoza", Price = 1700 },
                    new Meal { Name = "Pasta", Description = "Bolonjeze", Price = 1000 }
                }
            };

            User newUser = new User
            {
                Name = "Mihajlo",
                Surname = "Mihajlovic",
                Username = "Mihajlo",
                Email = "mihajlo@gmail.com", //pri ponovnom pokretanju testa, promeniti email
                Password = "mihajlo",
                PhoneNumber = "0620230567",
                Role = "user"
            };

            var restaurantActionResult = await _restaurantController.CreateRestaurant(newRestaurant);
            var restaurantObjectResult = restaurantActionResult as ObjectResult;
            var restaurant = restaurantObjectResult.Value as Restaurant;

            var userActionResult = await _userController.Register(newUser);
            var userObjectResult = userActionResult as ObjectResult;
            var user = userObjectResult.Value as User;

            var selectedMeals = restaurant.Meals.Take(2).ToList();

            var mealIds = selectedMeals.Select(meal => meal.Id).ToList();

            var orderActionResult = await _orderController.CreateOrder(restaurant.Id, user.Id, mealIds);
            var orderObjectResult = orderActionResult as ObjectResult;
            var order = orderObjectResult.Value as Order;

            var actionResult = await _orderController.GetOrderById(order.Id);

            Assert.IsInstanceOf<OkObjectResult>(actionResult.Result);
            var result = (OkObjectResult)actionResult.Result;
            Assert.IsNotNull(result);
            var getOrder = result.Value as Order;
            Assert.IsNotNull(getOrder);
            Assert.AreEqual(order.Id, getOrder.Id);
        }

        [Test]
        public async Task PreuzmiOrder_NepostojeciOrder()
        {
            string nepostojeciId = "65ed984c454619ba306c8c10";

            var rezultat = await _orderController.GetOrderById(nepostojeciId);


            Assert.IsInstanceOf<BadRequestObjectResult>(rezultat.Result);
            var result = (BadRequestObjectResult)rezultat.Result;

            Assert.IsNotNull(result);

        }

        [Test]
        public async Task AzurirajOrder_UspesnoAzuriranje()
        {

            Restaurant newRestaurant = new Restaurant
            {
                Name = "Na cosku",
                Address = "Branka Radicevica 15",
                Meals = new List<Meal>
                {
                    new Meal { Name = "Sufle", Description = "Cokolada", Price = 700 },
                    new Meal { Name = "Sah torta", Description = "Dodatak: sladoled", Price = 600 },
                    new Meal { Name = "Plazma torta", Description = "Dodatak: sladoled", Price = 600 }
                }
            };

            User newUser = new User
            {
                Name = "Julija",
                Surname = "Brankic",
                Username = "Julija",
                Email = "julija@gmail.com", //pri ponovnom pokretanju testa, promeniti email
                Password = "julija",
                PhoneNumber = "0663339667",
                Role = "user"
            };

            var restaurantActionResult = await _restaurantController.CreateRestaurant(newRestaurant);
            var restaurantObjectResult = restaurantActionResult as ObjectResult;
            var restaurant = restaurantObjectResult.Value as Restaurant;

            var userActionResult = await _userController.Register(newUser);
            var userObjectResult = userActionResult as ObjectResult;
            var user = userObjectResult.Value as User;

            var selectedMeals = restaurant.Meals.Take(1).ToList();

            var mealIds = selectedMeals.Select(meal => meal.Id).ToList();

            var orderActionResult = await _orderController.CreateOrder(restaurant.Id, user.Id, mealIds);
            var orderObjectResult = orderActionResult as ObjectResult;
            var order = orderObjectResult.Value as Order;

            var updatedSelectedMeals = restaurant.Meals.Take(2).ToList();

            var updatedMealIds = updatedSelectedMeals.Select(meal => meal.Id).ToList();

            var result = await _orderController.UpdateOrder(order.Id, updatedMealIds);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ActionResult>(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.AreEqual("Order successfully updated.", okResult.Value);
        }

        [Test]
        public async Task AzurirajOrder_NepostojeciId()
        {
            var nepostojeciId = "65ed984c454619ba306c8c63";

            Restaurant newRestaurant = new Restaurant
            {
                Name = "Nasa prica",
                Address = "Branka Radicevica 22",
                Meals = new List<Meal>
                {
                    new Meal { Name = "Sufle", Description = "Cokolada", Price = 700 },
                    new Meal { Name = "Sah torta", Description = "Dodatak: sladoled", Price = 600 },
                    new Meal { Name = "Plazma torta", Description = "Dodatak: sladoled", Price = 600 }
                }
            };

            User newUser = new User
            {
                Name = "Dragana",
                Surname = "Brankic",
                Username = "Dragana",
                Email = "dragana@gmail.com", //pri ponovnom pokretanju testa, promeniti email
                Password = "dragana",
                PhoneNumber = "0663323667",
                Role = "user"
            };

            var restaurantActionResult = await _restaurantController.CreateRestaurant(newRestaurant);
            var restaurantObjectResult = restaurantActionResult as ObjectResult;
            var restaurant = restaurantObjectResult.Value as Restaurant;

            var userActionResult = await _userController.Register(newUser);
            var userObjectResult = userActionResult as ObjectResult;
            var user = userObjectResult.Value as User;

            var selectedMeals = restaurant.Meals.Take(1).ToList();

            var mealIds = selectedMeals.Select(meal => meal.Id).ToList();

            var orderActionResult = await _orderController.CreateOrder(restaurant.Id, user.Id, mealIds);
            var orderObjectResult = orderActionResult as ObjectResult;
            var order = orderObjectResult.Value as Order;

            var updatedSelectedMeals = restaurant.Meals.Take(2).ToList();

            var updatedMealIds = updatedSelectedMeals.Select(meal => meal.Id).ToList();

            var result = await _orderController.UpdateOrder(nepostojeciId, updatedMealIds);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;

            Assert.IsNotNull(badRequestResult.Value);

            Assert.AreEqual($"Order with ID {nepostojeciId} not found.", badRequestResult.Value);
        }

        [Test]
        public async Task PreuzmiOrders_UspesnoPreuzimanje()
        {

            Restaurant newRestaurant = new Restaurant
            {
                Name = "Cinema",
                Address = "Beocina",
                Meals = new List<Meal>
                {
                    new Meal { Name = "Sufle", Description = "Cokolada", Price = 700 },
                    new Meal { Name = "Sah torta", Description = "Dodatak: sladoled", Price = 600 },
                    new Meal { Name = "Plazma torta", Description = "Dodatak: sladoled", Price = 600 }
                }
            };

            User newUser = new User
            {
                Name = "Brankica",
                Surname = "Nikolic",
                Username = "Brankica",
                Email = "brankica@gmail.com", //pri ponovnom pokretanju testa, promeniti email
                Password = "brankica",
                PhoneNumber = "0663393767",
                Role = "user"
            };

            var restaurantActionResult = await _restaurantController.CreateRestaurant(newRestaurant);
            var restaurantObjectResult = restaurantActionResult as ObjectResult;
            var restaurant = restaurantObjectResult.Value as Restaurant;

            var userActionResult = await _userController.Register(newUser);
            var userObjectResult = userActionResult as ObjectResult;
            var user = userObjectResult.Value as User;


            for (int i = 0; i < 3; i++)
            {
                var selectedMeal = restaurant.Meals[i];

                var mealIds = new List<string> { selectedMeal.Id.ToString() };

                var orderActionResult = await _orderController.CreateOrder(restaurant.Id, user.Id, mealIds);
                var orderObjectResult = orderActionResult as ObjectResult;
                var order = orderObjectResult.Value as Order;
            }

            var actionResult = await _orderController.GetAllOrders();
            var result = actionResult.Result as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            var getOrders = result.Value as List<Order>;
            Assert.That(getOrders, Is.Not.Null);

        }

        [Test]
        public async Task PreuzmiOrdersForUser_NepostojeciIddUser()
        {
            var nepostojeciId = "65ed984c454619ba306c8c63";

            var result = await _orderController.GetOrdersForUser(nepostojeciId);

            Assert.IsInstanceOf<BadRequestObjectResult>(result.Result); 
            var badRequestResult = result.Result as BadRequestObjectResult;

            Assert.IsNotNull(badRequestResult.Value);

            Assert.AreEqual($"User with ID {nepostojeciId} does not exist.", badRequestResult.Value);
        }

        [Test]
        public async Task PreuzmiOrdersForUser_UspesnoPreuzimanje()
        {
            User newUser = new User
            {
                Name = "Luka",
                Surname = "Lukic",
                Username = "luka",
                Email = "luka@example.com", //pri ponovnom pokretanju testa, promeniti email
                Password = "luka",
                PhoneNumber = "0652334567",
                Role = "user"
            };

            var userActionResult = await _userController.Register(newUser);
            var userObjectResult = userActionResult as ObjectResult;
            var user = userObjectResult.Value as User;

            // Kreiranje restorana 1
            Restaurant restaurant1 = new Restaurant
            {
                Name = "Na zemlji",
                Address = "Adresa restorana 1",
                Meals = new List<Meal>
                {
                    new Meal { Name = "Neko Jelo 1", Description = "Opis jela 1", Price = 1000 },
                    new Meal { Name = "Neko Jelo 2", Description = "Opis jela 2", Price = 1200 }
                }
            };

            var restaurant1ActionResult = await _restaurantController.CreateRestaurant(restaurant1);
            var restaurant1ObjectResult = restaurant1ActionResult as ObjectResult;
            var restaurant1Result = restaurant1ObjectResult.Value as Restaurant;

            // Kreiranje restorana 2
            Restaurant restaurant2 = new Restaurant
            {
                Name = "Na nebu",
                Address = "Adresa restorana 2",
                Meals = new List<Meal>
                {
                    new Meal { Name = "Neko Jelo 3", Description = "Opis jela 3", Price = 800 },
                    new Meal { Name = "Neko Jelo 4", Description = "Opis jela 4", Price = 900 }
                }
            };

            var restaurant2ActionResult = await _restaurantController.CreateRestaurant(restaurant2);
            var restaurant2ObjectResult = restaurant2ActionResult as ObjectResult;
            var restaurant2Result = restaurant2ObjectResult.Value as Restaurant;

            // Kreiranje narudžbine 1
            var selectedMeals1 = restaurant1Result.Meals.Take(1).ToList();
            var mealIds1 = selectedMeals1.Select(meal => meal.Id).ToList();
            var orderActionResult1 = await _orderController.CreateOrder(restaurant1Result.Id, user.Id, mealIds1);
            var orderObjectResult1 = orderActionResult1 as ObjectResult;
            var order1 = orderObjectResult1.Value as Order;

            // Kreiranje narudžbine 2
            var selectedMeals2 = restaurant2Result.Meals.Take(1).ToList();
            var mealIds2 = selectedMeals2.Select(meal => meal.Id).ToList();
            var orderActionResult2 = await _orderController.CreateOrder(restaurant2Result.Id, user.Id, mealIds2);
            var orderObjectResult2 = orderActionResult2 as ObjectResult;
            var order2 = orderObjectResult2.Value as Order;

            var result = await _orderController.GetOrdersForUser(user.Id);

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var orders = okResult.Value as List<Order>;
            Assert.IsNotNull(orders);
            Assert.AreEqual(2, orders.Count);
            Assert.IsTrue(orders.Any(order => order.Id == order1.Id));
            Assert.IsTrue(orders.Any(order => order.Id == order2.Id));
        }

    }
}
