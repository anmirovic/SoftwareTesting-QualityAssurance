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
    public class UserTests
    {
        private DeliverEase.Controllers.UserController _userController;

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

             _userController = new DeliverEase.Controllers.UserController(userService);
        }

        
        [Test]
        public async Task DodajKorisnika_User()
        {
            User user = new User
            {
                Name = "Marko",
                Surname = "Markovic",
                Username = "Marko",
                Email = "marko@gmail.com",
                Password = "marko",
                PhoneNumber = "0621234567",
                Role = "user" 
            };

            var result = await _userController.Register(user);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;

            Assert.IsNotNull(okResult, "Povratna vrednost nije instanca OkObjectResult-a.");

            var addedUser = okResult.Value as User;
            Assert.IsNotNull(addedUser, "Dodati korisnik nije instanciran.");
            Assert.IsNotNull(addedUser.Id, "ID nije postavljen nakon dodavanja korisnika.");
            Assert.AreEqual(user.Id, addedUser.Id, "ID dodatog korisnika se ne podudara sa očekivanim ID-em.");

        }

        [Test]
        public async Task DodajKorisnika_Admin()
        {
            User user = new User
            {
                Name = "Petar",
                Surname = "Peric",
                Username = "Petar",
                Email = "petar@gmail.com",
                Password = "petar",
                PhoneNumber = "0625434567",
                Role = "admin"
            };

            var result = await _userController.Register(user);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;

            Assert.IsNotNull(okResult, "Povratna vrednost nije instanca OkObjectResult-a.");

            var addedUser = okResult.Value as User;
            Assert.IsNotNull(addedUser, "Dodati korisnik nije instanciran.");
            Assert.IsNotNull(addedUser.Id, "ID nije postavljen nakon dodavanja korisnika.");
            Assert.AreEqual(user.Id, addedUser.Id, "ID dodatog korisnika se ne podudara sa očekivanim ID-em.");

        }

        [Test]
        public async Task DodajKorisnika_NeuspesnoDodavanje()
        {
            User UserSaGreskom = new User
            {
                Name = "Olja",
                Surname = "Peric",
                Username = "Olja"
            };

            var result = await _userController.Register(UserSaGreskom);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        //BadRequest za Duplikate email
        [Test]
        public async Task DodajKorisnika_DuplikatEmail()
        {
            var postojeciKorisnik = new User
            {
                Name = "Mina",
                Surname = "Petrovic",
                Username = "Mina",
                Email = "mina@gmail.com",
                Password = "mina",
                PhoneNumber = "0625459567",
                Role = "admin"
            };

            await _userController.Register(postojeciKorisnik);

            var noviKorisnik = new User
            {
                Name = "Mina",
                Surname = "Stevic",
                Username = "MinaS",
                Email = "mina@gmail.com",
                Password = "mina",
                PhoneNumber = "0625459567",
                Role = "admin"
            };

            var result = await _userController.Register(noviKorisnik);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.AreEqual("Email is already taken.", badRequestResult.Value);
        }

        [Test]
        public async Task ObrisiKorisnika_UspesnoBrisanje()
        {
            var korisnik = new User
            {
                Name = "Mina",
                Surname = "Stevic",
                Username = "MinaS",
                Email = "m@gmail.com",
                Password = "mina",
                PhoneNumber = "0625459567",
                Role = "admin"
            };

            await _userController.Register(korisnik);

            var result = await _userController.DeleteUser(korisnik.Id);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.AreEqual($"Izbrisan je korisnik: {korisnik.Id}", okResult.Value);
        }

        [Test]
        public async Task ObrisiKorisnika_NepostojeciKorisnik()
        {
            string nepostojeciId = "65ed984c454619ba306c8c63";

            var result = await _userController.DeleteUser(nepostojeciId);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;

            // Check if the Value is not null
            Assert.IsNotNull(badRequestResult.Value);

            Assert.AreEqual($"User with ID {nepostojeciId} does not exist.", badRequestResult.Value);
        }

        //da li se obrisao iz baze
        [Test]
        public async Task ObrisiKorisnika_ProveraBrisanjaIzBaze()
        {
            var korisnik = new User
            {
                Name = "Mina",
                Surname = "Stevic",
                Username = "MinaS",
                Email = "mmm@gmail.com",
                Password = "mina",
                PhoneNumber = "0625459567",
                Role = "user"
            };

            await _userController.Register(korisnik);

            await _userController.DeleteUser(korisnik.Id);
            var korisnikIzBaze = await _userController.GetUserById(korisnik.Id);

            Assert.IsNotNull(korisnikIzBaze, "Korisnik nije obrisana iz baze.");
        }


        [Test]
        public async Task PreuzmiKorisnike_UspesnoPreuzimanje()
        {

            var users = new List<User>
            {
                new User { Name = "Jovan", Surname = "Jovic", Username = "Jovan", Email = "jovan@gmail.com", Password = "jovan", PhoneNumber = "0624597896", Role = "admin" },
                new User { Name = "Isidora", Surname = "Radosavljevic", Username = "Isidora", Email = "isidora@gmail.com", Password = "isidora", PhoneNumber = "0632456987", Role = "user" }
            };

            foreach (var user in users)
            {
                await _userController.Register(user);
            }

            var actionResult = await _userController.GetAllUsers();
            var result = actionResult.Result as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            var getUsers = result.Value as List<User>;
            Assert.That(getUsers, Is.Not.Null);
            Assert.That(getUsers.Count+ users.Count, Is.EqualTo(users.Count+getUsers.Count));

        }

        [Test]
        public async Task PreuzmiKorisnike_PraznaLista()
        {
            // Kreiramo MongoDB klienta
            var mongoClient = new MongoClient("mongodb://localhost:27017");

            // Dobavljamo referencu na bazu podataka
            var database = mongoClient.GetDatabase("TestDatabase");

            // Brišemo sve postojeće korisnike iz kolekcije
            var korisniciCollection = database.GetCollection<User>("Korisnici");
            await korisniciCollection.DeleteManyAsync(Builders<User>.Filter.Empty);

            var userService = new UserService(database);
            var userController = new DeliverEase.Controllers.UserController(userService);

            // Kreiramo listu korisnika
            var users = new List<User>();

            // Registrovanje korisnika
            foreach (var user in users)
            {
                await userController.Register(user);
            }

            // Pozivamo akciju GetAllUsers() na kontroleru
            var actionResult = await userController.GetAllUsers();

            // Proveravamo rezultat
            Assert.IsInstanceOf<OkObjectResult>(actionResult.Result);
            var result = (OkObjectResult)actionResult.Result;

            // Proveravamo da li je rezultat pravilan
            Assert.IsNotNull(result);

            // Izvlačimo listu korisnika iz rezultata
            var getUsers = result.Value as List<User>;

            // Proveravamo da li je lista korisnika prazna
            Assert.IsNotNull(getUsers);
            Assert.AreEqual(users.Count, getUsers.Count);
        }

        [Test]
        public async Task PreuzmiKorisnika_UspesnoPreuzimanje()
        {
            // Kreiramo korisnika za test
            var user = new User
            {
                Name = "Ognjen",
                Surname = "Savic",
                Username = "Ognjen",
                Email = "ognjen@gmail.com",
                Password = "ognjen",
                PhoneNumber = "0624597875",
                Role = "admin"
            };

            // Registrujemo korisnika
            await _userController.Register(user);

            // Pozivamo akciju GetUserById() na kontroleru
            var actionResult = await _userController.GetUserById(user.Id);

            // Proveravamo rezultat
            Assert.IsInstanceOf<OkObjectResult>(actionResult.Result);
            var result = (OkObjectResult)actionResult.Result;

            // Proveravamo da li je rezultat pravilan
            Assert.IsNotNull(result);

            // Izvlačimo korisnika iz rezultata
            var getUser = result.Value as User;

            // Proveravamo da li je korisnik pronađen
            Assert.IsNotNull(getUser);
            Assert.AreEqual(user.Id, getUser.Id);
        }


        [Test]
        public async Task PreuzmiNepostojecegKorisnika()
        {
            string nepostojeciId = "65ed984c454619ba306c8c10";

            var rezultat = await _userController.GetUserById(nepostojeciId);


            Assert.IsInstanceOf<BadRequestObjectResult>(rezultat.Result);
            var result = (BadRequestObjectResult)rezultat.Result;

            // Proveravamo da li je rezultat pravilan
            Assert.IsNotNull(result);


        }















    }
}






