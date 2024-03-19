using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
namespace PlaywrightTests
{ 

    public class RestaurantTests : PageTest
    {
        IPage page;
        IBrowser browser;
        private string createdRestaurantId;
        private IAPIRequestContext Request;

        [SetUp]
        public async Task Setup()
        {
            browser = await Playwright.Chromium.LaunchAsync(new()
            {
                Headless = false,
                SlowMo = 2000
            });

            page = await browser.NewPageAsync(new()
            {
                ViewportSize = new()
                {
                    Width = 1280,
                    Height = 720
                },
                ScreenSize = new()
                {
                    Width = 1280,
                    Height = 720
                },
                RecordVideoSize = new()
                {
                    Width = 1280,
                    Height = 720
                },
                RecordVideoDir = "../../../Videos"
            });

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
            Dictionary<string, string> headers2 = new()
                {
                    { "Content-Type", "application/json" }
                };

            await using var response1 = await Request.PostAsync("/api/Restaurant/CreateRestaurant", new APIRequestContextOptions()
            {

                Headers = headers2,
                DataObject = new
                {
                    name = "NightandDay",
                    address = "Obrenoviceva",
                    meals = new List<object>
                    {
                        new { Name = "Pizza", Description = "Capricoza", Price = 1700 },
                        new { Name = "Pasta", Description = "Bolonjeze", Price = 1000 }
                    }
                }
            });
            var responseData = await response1.JsonAsync<Dictionary<string, object>>();
            createdRestaurantId = responseData["id"].ToString();


            var headers1 = new Dictionary<string, string>
            {
                { "Accept", "application/json" }
            };
            Request = await Playwright.APIRequest.NewContextAsync(new()
            {
                BaseURL = "https://localhost:7050",
                ExtraHTTPHeaders = headers,
                IgnoreHTTPSErrors = true
            });
            Dictionary<string, string> headers3 = new()
            {
                { "Content-Type", "application/json" }
            };

            await using var response2 = await Request.PostAsync("/api/User/Register", new APIRequestContextOptions()
            {

                Headers = headers3,
                DataObject = new
                {
                    name = "user245",
                    surname = "user245",
                    username = "user245",
                    email = "user245@gmail.com",
                    password = "user245",
                    phoneNumber = "0625467",
                    role = "user"
                }
            });

        }


        [Test]
        public async Task KreirajNarudzbinu_Uspesno()
        {
            await page.GotoAsync($"http://localhost:5173/login");
            Assert.That(page.Url, Is.EqualTo("http://localhost:5173/login"));

            await page.GetByPlaceholder("name@example.com").FillAsync("user245@gmail.com");
            await page.GetByPlaceholder("Password").FillAsync("user245");

            var formFilled2 = await page.EvaluateAsync<bool>(@"() => {
                const inputs = Array.from(document.getElementsByTagName('input'));
                return inputs.every(input => input.value !== '');
            }");
            Assert.That(formFilled2, Is.True);

            await page.GetByPlaceholder("SignIn").ClickAsync();

            await page.GotoAsync($"http://localhost:5173/");
            Assert.That(page.Url, Is.EqualTo("http://localhost:5173/"));

            await page.GotoAsync($"http://localhost:5173/restaurant/{createdRestaurantId}");
            Assert.That(page.Url, Is.EqualTo($"http://localhost:5173/restaurant/{createdRestaurantId}"));

            await Task.Delay(3000);

            await page.ScreenshotAsync(new() { Path = "../../../Slike/KreirajNarudzbinu1.png" });

            var pizzaOrderButton = await page.QuerySelectorAsync($"button[data-meal-id='Pizza']");

            await pizzaOrderButton.ClickAsync();

            await page.GetByPlaceholder("Account").ClickAsync();
            await page.GotoAsync($"http://localhost:5173/account"); 
            Assert.That(page.Url, Is.EqualTo("http://localhost:5173/account"));

            await Task.Delay(2000);

            await page.ScreenshotAsync(new() { Path = "../../../Slike/KreirajNarudzbinu2.png" });       
 
            var orderReviewExists = await page.QuerySelectorAsync($"input[placeholder='Rate the restaurant']");
            Assert.IsNotNull(orderReviewExists, "Ocenjivanje restorana nije dostupno");

        }


        [TearDown]
        public async Task Teardown()
        {
            await page.CloseAsync();
            await browser.DisposeAsync();
        }
    }
}


