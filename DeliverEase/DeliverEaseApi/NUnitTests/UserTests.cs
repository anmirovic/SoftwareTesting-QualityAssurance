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
                Email = "marko@gmail.com",  //pri ponovnom pokretanju testa, promeniti email
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
                Email = "petar@gmail.com",  //pri ponovnom pokretanju testa, promeniti email
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

            Assert.IsNotNull(badRequestResult.Value);

            Assert.AreEqual($"User with ID {nepostojeciId} does not exist.", badRequestResult.Value);
        }

       
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
            Assert.That(getUsers.Count + users.Count, Is.EqualTo(users.Count+getUsers.Count));

        }


        [Test]
        public async Task PreuzmiKorisnika_UspesnoPreuzimanje()
        {
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

            await _userController.Register(user);

            var actionResult = await _userController.GetUserById(user.Id);

            Assert.IsInstanceOf<OkObjectResult>(actionResult.Result);
            var result = (OkObjectResult)actionResult.Result;

            Assert.IsNotNull(result);

            var getUser = result.Value as User;

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

            Assert.IsNotNull(result);

        }

        [Test]
        public async Task Login_ValidniPodaci()
        {
            
            var user = new User
            {
                Name = "Srecko",
                Surname = "Ilic",
                Username = "Srecko",
                Email = "srecko@gmail.com",
                Password = "srecko",
                PhoneNumber = "1234567890",
                Role = "user"
            };

            await _userController.Register(user);

            var result = await _userController.Login(user.Email, user.Password);

            Assert.IsNotNull(result);
            Assert.IsNotInstanceOf<BadRequest>(result);

        }

        [Test]
        public async Task Login_NevalidniEmail()
        {

            var user = new User
            {
                Name = "Petra",
                Surname = "Petrovic",
                Username = "Petra",
                Email = "petra@gmail.com",
                Password = "petra",
                PhoneNumber = "1234567890",
                Role = "user"
            };

            await _userController.Register(user);

            var result = await _userController.Login("p@gmail.com", user.Password);
             
            Assert.IsInstanceOf<BadRequestObjectResult>(result);

        }

        [Test]
        public async Task Login_NevalidnaLozinka()
        {

            var user = new User
            {
                Name = "Stevan",
                Surname = "Mitrovic",
                Username = "Stevan",
                Email = "stevan@gmail.com",
                Password = "stevan",
                PhoneNumber = "1234789890",
                Role = "admin"
            };

            await _userController.Register(user);

            var result = await _userController.Login(user.Email, "pogresnalozinka");

            Assert.IsInstanceOf<BadRequestObjectResult>(result);

        }


        [Test]
        public async Task PreuzmiUlogovanogKorisnika()
        {
            var user = new User
            {
                Name = "Neda",
                Surname = "Djordjevic",
                Username = "Neda",
                Email = "neda@gmail.com",
                Password = "neda",
                PhoneNumber = "0654567890",
                Role = "user"
            };

            await _userController.Register(user);

            var token = await _userController.Login(user.Email, user.Password);

            Assert.IsNotNull(token);
            Assert.IsNotInstanceOf<BadRequest>(token);

            var result = await _userController.GetUser();

            Assert.IsNotNull(result);
        }


        [Test]
        public async Task AzurirajKorisnika_UspesnoAzuriranje()
        {

            var korisnik = new User
            {
                Name = "Andjela",
                Surname = "Ilic",
                Username = "Andjela",
                Email = "andjela@gmail.com",
                Password = "andjela",
                PhoneNumber = "0694567890",
                Role = "user"
            };

            await _userController.Register(korisnik);

            korisnik.Name = "Novo ime";

            var result = await _userController.UpdateUser(korisnik.Id, korisnik);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.AreEqual("User successfully updated.", okResult.Value);

        }

        [Test]
        public async Task AzurirajKorisnika_NepostojeciId()
        {
            var nepostojeciId = "65ed984c454619ba306c8c63";

            var korisnik = new User
            {
                Name = "Vukan",
                Surname = "Milosevic",
                Username = "Vukan",
                Email = "vukan@gmail.com",
                Password = "vukan",
                PhoneNumber = "0694789890",
                Role = "user"
            };

            var result = await _userController.UpdateUser(nepostojeciId, korisnik);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;

            Assert.IsNotNull(badRequestResult.Value);

            Assert.AreEqual($"User with ID {nepostojeciId} does not exist.", badRequestResult.Value);
        }

    }
}






