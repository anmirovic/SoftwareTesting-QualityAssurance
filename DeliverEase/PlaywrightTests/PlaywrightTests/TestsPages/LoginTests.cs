﻿using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
namespace PlaywrightTests
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class LoginTests : PageTest
    {
        IPage page;
        IBrowser browser;

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
            await using var response2 = await Request.PostAsync("/api/User/Register", new APIRequestContextOptions()
            {

                Headers = headers2,
                DataObject = new
                {
                    name = "admin2",
                    surname = "admin2",
                    username = "admin2",
                    email = "admin2@gmail.com",
                    password = "admin2",
                    phoneNumber = "0625467",
                    role = "admin"
                }
            });

            await page.GotoAsync("http://localhost:5173/login");

        }

        [Test]
        [Order(1)]
        public async Task LogIn_User_Uspesno()
        {
            //await page.GotoAsync($"http://localhost:5173/login");
            //Assert.That(page.Url, Is.EqualTo("http://localhost:5173/login"));

            //await page.ScreenshotAsync(new() { Path = "../../../Slike/RegisterUser1.png" });

            await page.GetByPlaceholder("name@example.com").FillAsync("admin2@gmail.com");
            await page.GetByPlaceholder("Password").FillAsync("admin2");

           // await page.ScreenshotAsync(new() { Path = "../../../Slike/RegisterUser2.png" });

            var formFilled = await page.EvaluateAsync<bool>(@"() => {
                const inputs = Array.from(document.getElementsByTagName('input'));
                return inputs.every(input => input.value !== '');
                }");
            Assert.That(formFilled, Is.True);

            await page.GetByPlaceholder("SignIn").ClickAsync();

            //await page.GotoAsync($"http://localhost:5173/");

            //Assert.That(page.Url, Is.EqualTo("http://localhost:5173/"));
            //await Expect(page.InnerTextAsync("Welcome")).ToBeVisibleAsync();
            //await page.ScreenshotAsync(new() { Path = "../../../Slike/RegisterUser3.png" });

        }

        //[Test]
        //public async Task LogIn_User_Neuspesno()
        //{
        //    await page.GotoAsync($"http://localhost:5173/login");
        //    Assert.That(page.Url, Is.EqualTo("http://localhost:5173/login"));

        //    await page.ScreenshotAsync(new() { Path = "../../../Slike/RegisterUser1.png" });

        //    await page.GetByPlaceholder("name@example.com").FillAsync("user@gmail.com");

        //    await page.ScreenshotAsync(new() { Path = "../../../Slike/RegisterUser2.png" });

        //    var formFilled = await page.EvaluateAsync<bool>(@"() => {
        //            const inputs = Array.from(document.getElementsByTagName('input'));
        //            return inputs.every(input => input.value !== '');
        //    }");
        //    Assert.That(formFilled, Is.False);

        //    await page.GetByPlaceholder("SignIn").ClickAsync();

        //    await page.ScreenshotAsync(new() { Path = "../../../Slike/RegisterUser3.png" });

        //}

        //[Test]
        //public async Task LogIn_Admin_Uspesno()
        //{
        //    await page.GotoAsync($"http://localhost:5173/login");
        //    Assert.That(page.Url, Is.EqualTo("http://localhost:5173/login"));

        //    await page.ScreenshotAsync(new() { Path = "../../../Slike/RegisterUser1.png" });

        //    await page.GetByPlaceholder("name@example.com").FillAsync("admin@gmail.com");
        //    await page.GetByPlaceholder("Password").FillAsync("admin");

        //    await page.ScreenshotAsync(new() { Path = "../../../Slike/RegisterUser2.png" });

        //    var formFilled = await page.EvaluateAsync<bool>(@"() => {
        //            const inputs = Array.from(document.getElementsByTagName('input'));
        //            return inputs.every(input => input.value !== '');
        //    }");
        //    Assert.That(formFilled, Is.True);

        //    await page.GetByPlaceholder("SignIn").ClickAsync();
        //    await page.GotoAsync($"http://localhost:5173/");

        //    Assert.That(page.Url, Is.EqualTo("http://localhost:5173/"));

        //    await page.GetByPlaceholder("Account").AllInnerTextsAsync();

        //    await page.ScreenshotAsync(new() { Path = "../../../Slike/RegisterUser3.png" });

        //}

        //[Test]
        //public async Task LogIn_Admin_Neuspesno()
        //{
        //    await page.GotoAsync($"http://localhost:5173/login");
        //    Assert.That(page.Url, Is.EqualTo("http://localhost:5173/login"));

        //    await page.ScreenshotAsync(new() { Path = "../../../Slike/RegisterUser1.png" });

        //    await page.GetByPlaceholder("name@example.com").FillAsync("admin@gmail.com");

        //    await page.ScreenshotAsync(new() { Path = "../../../Slike/RegisterUser2.png" });

        //    var formFilled = await page.EvaluateAsync<bool>(@"() => {
        //            const inputs = Array.from(document.getElementsByTagName('input'));
        //            return inputs.every(input => input.value !== '');
        //    }");
        //    Assert.That(formFilled, Is.False);

        //    await page.GetByPlaceholder("SignIn").ClickAsync();

        //    await page.ScreenshotAsync(new() { Path = "../../../Slike/RegisterUser3.png" });

        //}


        [TearDown]
        public async Task Teardown()
        {
            await page.CloseAsync();
            await browser.DisposeAsync();
        }
    }
}

