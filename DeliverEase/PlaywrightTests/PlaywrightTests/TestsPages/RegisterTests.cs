using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
namespace PlaywrightTests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class RegisterTests : PageTest
{
    IPage page;
    IBrowser browser;

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
    }

    [Test]
    public async Task DodajKorisnika_User()
    {
        await page.GotoAsync($"http://localhost:5173/register");

        await page.ScreenshotAsync(new() { Path = "../../../Slike/RegisterUser1.png" });

        await page.GetByPlaceholder("Name").FillAsync("Janko");
        await page.GetByPlaceholder("Prezime").FillAsync("Jankovic");
        await page.GetByPlaceholder("KorisnickoIme").FillAsync("Janko");
        await page.GetByPlaceholder("n@example.com").FillAsync("janko@gmail.com");
        await page.GetByPlaceholder("Lozinka").FillAsync("janko");
        await page.GetByPlaceholder("Telefon").FillAsync("0652859632");

       
        await page.ScreenshotAsync(new() { Path = "../../../Slike/RegisterUser2.png" });

        await page.GetByPlaceholder("SignUp").ClickAsync();
        await page.GotoAsync($"http://localhost:5173/login");

        await page.ScreenshotAsync(new() { Path = "../../../Slike/RegisterUser3.png" });

        
    }

    [Test]
    public async Task DodajKorisnika_Admin()
    {
        await page.GotoAsync($"http://localhost:5173/register");

        await page.ScreenshotAsync(new() { Path = "../../../Slike/RegisterAdmin1.png" });

        await page.GetByPlaceholder("Name").FillAsync("Janko");
        await page.GetByPlaceholder("Prezime").FillAsync("Jankovic");
        await page.GetByPlaceholder("KorisnickoIme").FillAsync("Janko");
        await page.GetByPlaceholder("n@example.com").FillAsync("janko@gmail.com");
        await page.GetByPlaceholder("Lozinka").FillAsync("janko");
        await page.GetByPlaceholder("Telefon").FillAsync("0652859632");
        await page.GetByPlaceholder("Admin").CheckAsync();

        await page.ScreenshotAsync(new() { Path = "../../../Slike/RegisterAdmin2.png" });

        await page.GetByPlaceholder("SignUp").ClickAsync();
        await page.GotoAsync($"http://localhost:5173/login");

        await page.ScreenshotAsync(new() { Path = "../../../Slike/RegisterAdmin3.png" });

    }


    [TearDown]
    public async Task Teardown()
    {
        await page.CloseAsync();
        await browser.DisposeAsync();
    }
}

