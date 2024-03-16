/*using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
namespace PlaywrightTests;

public class RestaurantTests : PageTest
{
    IPage page;
    IBrowser browser;
    private string createdRestaurantId;

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
    public async Task DodajRestoran_Uspesno()
    {
        await page.GotoAsync($"http://localhost:5173/addrestaurant");

        await page.GetByPlaceholder("Name").FillAsync("Night&Day");
        await page.GetByPlaceholder("Address").FillAsync("Obrenoviceva");

        await page.GetByPlaceholder("Ime jela").FillAsync("Pizza");
        await page.GetByPlaceholder("Opis").FillAsync("Capricoza");
        await page.GetByPlaceholder("Cena").FillAsync(1500.ToString());

        await page.GetByPlaceholder("Add meal").ClickAsync();

        await page.GetByPlaceholder("Ime jela").FillAsync("Pasta");
        await page.GetByPlaceholder("Opis").FillAsync("Carbonara");
        await page.GetByPlaceholder("Cena").FillAsync(600.ToString());

        await page.GetByPlaceholder("Add meal").ClickAsync();

        await page.GetByPlaceholder("Create Restaurant").ClickAsync();

        await page.GotoAsync($"http://localhost:5173/account");

        createdRestaurantId = page.Url;
        createdRestaurantId = createdRestaurantId.Substring(createdRestaurantId.LastIndexOf('/') + 1);

    }

    [Test]
    public async Task KreirajNarudzbinu_Uspesno()
    {
        await page.GotoAsync($"http://localhost:5173/restaurant/{createdRestaurantId}");

        await page.ScreenshotAsync(new() { Path = "../../../Slike/KreirajNarudzbinuUspesno1.png" });

        var increaseButton = await page.QuerySelectorAsync($"button[data-meal-name='Pizza'][onclick*='handleIncrease']");

        await increaseButton.ClickAsync();

        var pizzaOrderButton = await page.QuerySelectorAsync($"button[data-meal-id='Pizza']");

        await pizzaOrderButton.ClickAsync();

    }


    [TearDown]
    public async Task Teardown()
    {
        await page.CloseAsync();
        await browser.DisposeAsync();
    }
}

*/