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




namespace PlaywrightTests.TestsAPI
{
    [TestFixture]
    internal class UserTestsAPI : PlaywrightTest
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
        public async Task PreuzmiKorisnike()
        {
            await using var response = await Request.GetAsync("/api/User/GetAllUsers");
            
            Assert.That(response.Status, Is.EqualTo(200));
            var jsonResponse = await response.JsonAsync();
            Assert.That(jsonResponse, Is.Not.Null);
        }

        [Test]
        public async Task DodajKorisnika_User()
        {
            var noviKorisnik = new //obavezno promeni email
            {

                name = "user10",
                surname = "user10",
                username = "user10",
                email = "user10@gmail.com",
                password = "user10",
                phoneNumber = "0625467",
                role = "user"
            };
            
            await using var response = await Request.PostAsync("/api/User/Register", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = noviKorisnik
            }); ;

            if (response.Status != 200)
            {
                Assert.Fail($"Code: {response.Status} - {response.StatusText}");
            }

            Assert.That(response.Status, Is.EqualTo(200));
            var textResponse = await response.TextAsync();
            Assert.IsNotNull(textResponse);
        }


        













    }

}


