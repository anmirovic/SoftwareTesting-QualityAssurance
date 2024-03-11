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
using Microsoft.AspNetCore.Identity.Data;
using DeliverEase.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace NUnitTests
{
    public class ReviewTests
    {
        private DeliverEase.Controllers.ReviewController _reviewController;
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

            var restaurantService = new RestaurantService(database);
            var reviewService = new ReviewService(database);

            _reviewController = new DeliverEase.Controllers.ReviewController(reviewService, restaurantService);

            var userService = new UserService(database);

            _userController = new DeliverEase.Controllers.UserController(userService);

            _restaurantController = new DeliverEase.Controllers.RestaurantController(restaurantService, reviewService);

        }

        [Test]
        public async Task PreuzmiReviews_UspesnoPreuzimanje()
        {

            var restoran = new Restaurant
            {
                Name = "Cezar",
                Address = "Kralja Aleksandra",
                Meals = new List<Meal>()

            };

            var resultRestaurant = await _restaurantController.CreateRestaurant(restoran);
            Assert.IsInstanceOf<OkObjectResult>(resultRestaurant);

            var korisnik = new User
            {
                Name = "Mila",
                Surname = "Petrovic",
                Username = "Mila",
                Email = "mila@gmail.com",  //pri ponovnom pokretanju testa, promeniti email
                Password = "mila",
                PhoneNumber = "0625459567",
                Role = "admin"
            };

            var resultUser = await _userController.Register(korisnik);
            Assert.IsInstanceOf<OkObjectResult>(resultUser);
           

            var reviews = new List<Review>
            {
                new Review
                {
                    Rating = 3,
                    UserId = korisnik.Id,
                    RestaurantId = restoran.Id
                },

                new Review
                {
                    Rating = 2,
                    UserId = korisnik.Id,
                    RestaurantId = restoran.Id
                },

                new Review
                {
                    Rating = 5,
                    UserId = korisnik.Id,
                    RestaurantId = restoran.Id
                },

            };

            foreach (var review in reviews)
            {
                await _reviewController.AddReview(review);
            }

            var actionResult = await _reviewController.GetAllReviews();
            var result = actionResult.Result as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            var getReview = result.Value as List<Review>;
            Assert.That(getReview, Is.Not.Null);
            Assert.That(getReview.Count + reviews.Count, Is.EqualTo(reviews.Count + getReview.Count));

        }

        [Test]
        public async Task DodajReview_Uspesno()
        {
            User user = new User
            {
                Name = "P",
                Surname = "P",
                Username = "P",
                Email = "p@gmail.com",  //pri ponovnom pokretanju testa, promeniti email
                Password = "p",
                PhoneNumber = "0625434567",
                Role = "admin"
            };

            var resultUser = await _userController.Register(user);

            Assert.IsInstanceOf<OkObjectResult>(resultUser);
            var okResultUser = resultUser as OkObjectResult;

            Assert.IsNotNull(okResultUser, "Povratna vrednost nije instanca OkObjectResult-a.");

            var addedUser = okResultUser.Value as User;
            Assert.IsNotNull(addedUser, "Dodati korisnik nije instanciran.");
            Assert.IsNotNull(addedUser.Id, "ID nije postavljen nakon dodavanja korisnika.");
            Assert.AreEqual(user.Id, addedUser.Id, "ID dodatog korisnika se ne podudara sa očekivanim ID-em.");

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

            var resultRestaurant = await _restaurantController.CreateRestaurant(restaurant);

            Assert.IsInstanceOf<OkObjectResult>(resultRestaurant);
            var okResultRestaurant = resultRestaurant as OkObjectResult;

            Assert.IsNotNull(okResultRestaurant, "Povratna vrednost nije instanca okObjectResult-a.");

            var addedRestaurant = okResultRestaurant.Value as Restaurant;
            Assert.IsNotNull(addedRestaurant, "Dodati restoran nije instanciran.");
            Assert.IsNotNull(addedRestaurant.Id, "ID nije postavljen nakon dodavanja restorana.");
            Assert.AreEqual(restaurant.Id, addedRestaurant.Id, "ID dodatog restorana se ne podudara sa ocekivanim ID-em.");

            var review = new Review
            {
                Rating = 3,
                UserId = user.Id,
                RestaurantId = restaurant.Id
            };

            var resultReview = await _reviewController.AddReview(review);

            Assert.IsInstanceOf<OkObjectResult>(resultReview);
            var okResultReview = resultReview as OkObjectResult;

            Assert.IsNotNull(okResultReview, "Povratna vrednost nije instanca okObjectResult-a.");

            var addedReview = okResultReview.Value as Review;

            Assert.IsNotNull(addedReview, "Dodati review nije instanciran.");
            Assert.IsNotNull(addedReview.Id, "ID nije postavljen nakon dodavanja review-a.");
            Assert.AreEqual(review.Id, addedReview.Id, "ID dodatog review-a se ne podudara sa ocekivanim ID-em.");

        }

        [Test]
        public async Task PreuzmiReview_UspesnoPreuzimanje()
        {
            var restoran = new Restaurant
            {
                Name = "Cezar",
                Address = "Kralja Aleksandra",
                Meals = new List<Meal>()

            };

            var resultRestaurant = await _restaurantController.CreateRestaurant(restoran);
            Assert.IsInstanceOf<OkObjectResult>(resultRestaurant);

            var korisnik = new User
            {
                Name = "user",
                Surname = "user",
                Username = "user",
                Email = "user@gmail.com",  //pri ponovnom pokretanju testa, promeniti email
                Password = "user",
                PhoneNumber = "0625459567",
                Role = "user"
            };

            var resultUser = await _userController.Register(korisnik);
            Assert.IsInstanceOf<OkObjectResult>(resultUser);
            

            var review = new Review
            {
                Rating = 3,
                UserId = korisnik.Id,
                RestaurantId = restoran.Id
            };

            await _reviewController.AddReview(review);

            var actionResult = await _reviewController.GetReviewById(review.Id);

            Assert.IsInstanceOf<OkObjectResult>(actionResult.Result);
            var result = (OkObjectResult)actionResult.Result;

            Assert.IsNotNull(result);

            var getReview = result.Value as Review;

            Assert.IsNotNull(getReview);
            Assert.AreEqual(review.Id, getReview.Id);
        }

        [Test]
        public async Task PreuzmiNepostojeciReview()
        {
            string nepostojeciId = "65ed984c454619ba306c8c10";

            var rezultat = await _reviewController.GetReviewById(nepostojeciId);

            Assert.IsInstanceOf<BadRequestObjectResult>(rezultat.Result);
            var result = (BadRequestObjectResult)rezultat.Result;

            Assert.IsNotNull(result);

        }

        [Test]
        public async Task AzurirajReview_UspesnoAzuriranje()
        {

            var restoran = new Restaurant
            {
                Name = "Cezar",
                Address = "Kralja Aleksandra",
                Meals = new List<Meal>()

            };

            var resultRestaurant = await _restaurantController.CreateRestaurant(restoran);
            Assert.IsInstanceOf<OkObjectResult>(resultRestaurant);

            var korisnik = new User
            {
                Name = "user1",
                Surname = "user1",
                Username = "user1",
                Email = "user1@gmail.com",  //pri ponovnom pokretanju testa, promeniti email
                Password = "user1",
                PhoneNumber = "0625459567",
                Role = "user"
            };

            var resultUser = await _userController.Register(korisnik);
            Assert.IsInstanceOf<OkObjectResult>(resultUser);
            
            var review = new Review
            {
                Rating = 3,
                UserId = korisnik.Id,
                RestaurantId = restoran.Id
            };

            await _reviewController.AddReview(review);

            review.Rating = 5;

            var result = await _reviewController.UpdateReview(review.Id, review);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.AreEqual("Review updated successfully.", okResult.Value);

        }

        [Test]
        public async Task AzurirajReview_NepostojeciId()
        {
            var nepostojeciId = "65ed984c454619ba306c8c63";

            var restoran = new Restaurant
            {
                Name = "Cezar",
                Address = "Kralja Aleksandra",
                Meals = new List<Meal>()

            };

            var resultRestaurant = await _restaurantController.CreateRestaurant(restoran);
            Assert.IsInstanceOf<OkObjectResult>(resultRestaurant);

            var korisnik = new User
            {
                Name = "user2",
                Surname = "user2",
                Username = "user2",
                Email = "user2@gmail.com",  //pri ponovnom pokretanju testa, promeniti email
                Password = "user2",
                PhoneNumber = "0625459567",
                Role = "user"
            };

            var resultUser = await _userController.Register(korisnik);
            Assert.IsInstanceOf<OkObjectResult>(resultUser);


            var review = new Review
            {
                Rating = 3,
                UserId = korisnik.Id,
                RestaurantId = restoran.Id
            };

            var result = await _reviewController.UpdateReview(nepostojeciId, review);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;

            Assert.IsNotNull(badRequestResult.Value);

            Assert.AreEqual($"Review with ID {nepostojeciId} not found.", badRequestResult.Value);
        }

        [Test]
        public async Task ObrisiReview_UspesnoBrisanje()
        {
            var restoran = new Restaurant
            {
                Name = "Cezar",
                Address = "Kralja Aleksandra",
                Meals = new List<Meal>()

            };

            var resultRestaurant = await _restaurantController.CreateRestaurant(restoran);
            Assert.IsInstanceOf<OkObjectResult>(resultRestaurant);

            var korisnik = new User
            {
                Name = "user3",
                Surname = "user3",
                Username = "user3",
                Email = "user3@gmail.com",  //pri ponovnom pokretanju testa, promeniti email
                Password = "user3",
                PhoneNumber = "0625459567",
                Role = "user"
            };

            var resultUser = await _userController.Register(korisnik);
            Assert.IsInstanceOf<OkObjectResult>(resultUser);
         
            var review = new Review
            {
                Rating = 3,
                UserId = korisnik.Id,
                RestaurantId = restoran.Id
            };

            await _reviewController.AddReview(review);

            var result = await _reviewController.DeleteReview(review.Id);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.AreEqual($"Review deleted successfully.", okResult.Value);
        }


        [Test]
        public async Task ObrisiReview_NepostojeciReview()
        {
            string nepostojeciId = "65ed984c454619ba306c8c63";

            var result = await _reviewController.DeleteReview(nepostojeciId);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;

            Assert.IsNotNull(badRequestResult.Value);

            Assert.AreEqual($"Review with ID {nepostojeciId} not found.", badRequestResult.Value);
        }


        [Test]
        public async Task ObrisiReview_ProveraBrisanjaIzBaze()
        {
            var restoran = new Restaurant
            {
                Name = "Cezar",
                Address = "Kralja Aleksandra",
                Meals = new List<Meal>()

            };

            var resultRestaurant = await _restaurantController.CreateRestaurant(restoran);
            Assert.IsInstanceOf<OkObjectResult>(resultRestaurant);

            var korisnik = new User
            {
                Name = "user4",
                Surname = "user4",
                Username = "user4",
                Email = "user4@gmail.com",  //pri ponovnom pokretanju testa, promeniti email
                Password = "user4",
                PhoneNumber = "0625459567",
                Role = "user"
            };

            var resultUser = await _userController.Register(korisnik);
            Assert.IsInstanceOf<OkObjectResult>(resultUser);

            var review = new Review
            {
                Rating = 3,
                UserId = korisnik.Id,
                RestaurantId = restoran.Id
            };

            await _reviewController.AddReview(review);

            await _reviewController.DeleteReview(review.Id);
            var reviewIzBaze = await _reviewController.GetReviewById(review.Id);

            Assert.IsNotNull(reviewIzBaze, "Review nije obrisan iz baze.");
        }

        [Test]
        public async Task PreuzmiReviewsNekogRestorana_UspesnoPreuzimanje()
        {
            var restoran = new Restaurant
            {
                Name = "Cezar",
                Address = "Kralja Aleksandra",
                Meals = new List<Meal>()

            };

            var resultRestaurant = await _restaurantController.CreateRestaurant(restoran);
            Assert.IsInstanceOf<OkObjectResult>(resultRestaurant);

            var korisnik = new User
            {
                Name = "user5",
                Surname = "user5",
                Username = "user5",
                Email = "user5@gmail.com",  //pri ponovnom pokretanju testa, promeniti email
                Password = "user5",
                PhoneNumber = "0625459567",
                Role = "user"
            };

            var resultUser = await _userController.Register(korisnik);
            Assert.IsInstanceOf<OkObjectResult>(resultUser);
     

            var reviews = new List<Review>
            {
                new Review
                {
                    Rating = 3,
                    UserId = korisnik.Id,
                    RestaurantId = restoran.Id
                },

                new Review
                {
                    Rating = 2,
                    UserId = korisnik.Id,
                    RestaurantId = restoran.Id
                },

                new Review
                {
                    Rating = 5,
                    UserId = korisnik.Id,
                    RestaurantId = restoran.Id
                },

            };

            foreach (var review in reviews)
            {
                await _reviewController.AddReview(review);
            }


            var actionResult = await _reviewController.GetReviewsForRestaurant(restoran.Id);
            var result = actionResult.Result as OkObjectResult;

            Assert.IsNotNull(result);

            var rev = result.Value as List<Review>;
            Assert.IsNotNull(rev);


        }

        [Test]
        public async Task PreuzmiReviewsNekogKorisnika_UspesnoPreuzimanje()
        {
            var restoran = new Restaurant
            {
                Name = "Cezar",
                Address = "Kralja Aleksandra",
                Meals = new List<Meal>()

            };

            var resultRestaurant = await _restaurantController.CreateRestaurant(restoran);
            Assert.IsInstanceOf<OkObjectResult>(resultRestaurant);

            var korisnik = new User
            {
                Name = "user6",
                Surname = "user6",
                Username = "user6",
                Email = "user6@gmail.com",  //pri ponovnom pokretanju testa, promeniti email
                Password = "user6",
                PhoneNumber = "0625459567",
                Role = "user"
            };

            var resultUser = await _userController.Register(korisnik);
            Assert.IsInstanceOf<OkObjectResult>(resultUser);
           

            var reviews = new List<Review>
            {
                new Review
                {
                    Rating = 3,
                    UserId = korisnik.Id,
                    RestaurantId = restoran.Id
                },

                new Review
                {
                    Rating = 2,
                    UserId = korisnik.Id,
                    RestaurantId = restoran.Id
                },

                new Review
                {
                    Rating = 5,
                    UserId = korisnik.Id,
                    RestaurantId = restoran.Id
                },

            };

            foreach (var review in reviews)
            {
                await _reviewController.AddReview(review);
            }


            var actionResult = await _reviewController.GetReviewsForUser(korisnik.Id);
            var result = actionResult.Result as OkObjectResult;

            Assert.IsNotNull(result);

            var rev = result.Value as List<Review>;
            Assert.IsNotNull(rev);


        }


    }
}
