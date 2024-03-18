using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
namespace PlaywrightTests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class AddRestaurantTests : PageTest
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
    public async Task DodajRestoran_Uspesno()
    {
        await page.GotoAsync($"http://localhost:5173/addrestaurant");
        Assert.That(page.Url, Is.EqualTo("http://localhost:5173/addrestaurant"));

        await page.ScreenshotAsync(new() { Path = "../../../Slike/AddRestaurantUspesno1.png" });

        await page.GetByPlaceholder("Name").FillAsync("Uspesni Restoran");
        await page.GetByPlaceholder("Address").FillAsync("Obrenoviceva");

        await page.GetByPlaceholder("Ime jela").FillAsync("Pizza");
        await page.GetByPlaceholder("Opis").FillAsync("Capricoza");
        await page.GetByPlaceholder("Cena").FillAsync(1500.ToString());

        var formFilled1 = await page.EvaluateAsync<bool>(@"() => {
                const inputs = Array.from(document.getElementsByTagName('input'));
                return inputs.every(input => input.value !== '');
        }");
        Assert.That(formFilled1, Is.True);

        await page.GetByPlaceholder("Add meal").ClickAsync();

        await page.ScreenshotAsync(new() { Path = "../../../Slike/AddRestaurantUspesno2.png" });

        await page.GetByPlaceholder("Ime jela").FillAsync("Pasta");
        await page.GetByPlaceholder("Opis").FillAsync("Carbonara");
        await page.GetByPlaceholder("Cena").FillAsync(600.ToString());

        var formFilled2 = await page.EvaluateAsync<bool>(@"() => {
                const inputs = Array.from(document.getElementsByTagName('input'));
                return inputs.every(input => input.value !== '');
        }");
        Assert.That(formFilled2, Is.True);

        await page.GetByPlaceholder("Add meal").ClickAsync();

        await page.ScreenshotAsync(new() { Path = "../../../Slike/AddRestaurantUspesno3.png" });

        await page.GetByPlaceholder("Create Restaurant").ClickAsync();

        await page.GotoAsync($"http://localhost:5173/");
        Assert.That(page.Url, Is.EqualTo("http://localhost:5173/"));

        await page.EvaluateAsync(@"() => {
            window.scrollTo(0, document.body.scrollHeight);
        }");

        await page.WaitForTimeoutAsync(2000);

        await page.ScreenshotAsync(new() { Path = "../../../Slike/AddRestaurantUspesno4.png" });

    }

    [Test]
    public async Task DodajRestoran_Neuspesno()
    {
        await page.GotoAsync($"http://localhost:5173/addrestaurant");
        Assert.That(page.Url, Is.EqualTo("http://localhost:5173/addrestaurant"));

        await page.ScreenshotAsync(new() { Path = "../../../Slike/AddRestaurantNeuspesno1.png" });

        await page.GetByPlaceholder("Address").FillAsync("Obrenoviceva");

        await page.GetByPlaceholder("Ime jela").FillAsync("Pizza");
        await page.GetByPlaceholder("Opis").FillAsync("Capricoza");
        await page.GetByPlaceholder("Cena").FillAsync(1500.ToString());

        await page.GetByPlaceholder("Add meal").ClickAsync();
        await page.ScreenshotAsync(new() { Path = "../../../Slike/AddRestaurantNeuspesno2.png" });

        await page.GetByPlaceholder("Ime jela").FillAsync("Pasta");
        await page.GetByPlaceholder("Opis").FillAsync("Carbonara");
        await page.GetByPlaceholder("Cena").FillAsync(600.ToString());

        await page.GetByPlaceholder("Add meal").ClickAsync();
        await page.ScreenshotAsync(new() { Path = "../../../Slike/AddRestaurantNeuspesno3.png" });

        await page.GetByPlaceholder("Create Restaurant").ClickAsync();

        var nameErrorHandle = await page.QuerySelectorAsync(".error");
        var nameErrorMessage = await nameErrorHandle.EvaluateAsync<string>("element => element.textContent");
        Assert.AreEqual("Please fill Restaurant name.", nameErrorMessage);

        await page.ScreenshotAsync(new() { Path = "../../../Slike/AddRestaurantNeuspesno4.png" });

    }

    //Korisnik mora biti admin da bi mogao da dodaje restorane, to ćemo pokazati kroz sledeće testove
    [Test]
    public async Task DodajRestoran_Admin()
    {
        await page.GotoAsync($"http://localhost:5173/register");
        Assert.That(page.Url, Is.EqualTo("http://localhost:5173/register"));

        await page.ScreenshotAsync(new() { Path = "../../../Slike/DodajRestoranAdmin1.png" });

        await page.GetByPlaceholder("Name").FillAsync("Jelena");
        await page.GetByPlaceholder("Prezime").FillAsync("Jankovic");
        await page.GetByPlaceholder("KorisnickoIme").FillAsync("Jelena");
        await page.GetByPlaceholder("n@example.com").FillAsync("jelenajanko@gmail.com");
        await page.GetByPlaceholder("Lozinka").FillAsync("jelena");
        await page.GetByPlaceholder("Telefon").FillAsync("0652859632");
        await page.GetByPlaceholder("Admin").CheckAsync();

        var formFilled = await page.EvaluateAsync<bool>(@"() => {
                const inputs = Array.from(document.getElementsByTagName('input'));
                return inputs.every(input => input.value !== '');
        }");
        Assert.That(formFilled, Is.True);

        await page.ScreenshotAsync(new() { Path = "../../../Slike/DodajRestoranAdmin2.png" });

        await page.GetByPlaceholder("SignUp").ClickAsync();

        await page.GotoAsync($"http://localhost:5173/login");
        Assert.That(page.Url, Is.EqualTo("http://localhost:5173/login"));

        await page.ScreenshotAsync(new() { Path = "../../../Slike/DodajRestoranAdmin3.png" });

        await page.GetByPlaceholder("name@example.com").FillAsync("jelenajanko@gmail.com");
        await page.GetByPlaceholder("Password").FillAsync("jelena");

        var formFilled2 = await page.EvaluateAsync<bool>(@"() => {
                const inputs = Array.from(document.getElementsByTagName('input'));
                return inputs.every(input => input.value !== '');
        }");
        Assert.That(formFilled2, Is.True);

        await page.GetByPlaceholder("SignIn").ClickAsync();

        await page.GotoAsync($"http://localhost:5173/");
        Assert.That(page.Url, Is.EqualTo("http://localhost:5173/"));

        await page.ScreenshotAsync(new() { Path = "../../../Slike/DodajRestoranAdmin4.png" });

        await page.GetByPlaceholder("Account").ClickAsync();
        await page.GotoAsync($"http://localhost:5173/account");  //ispod Orders i Reviews stoji opcija Add Restaurant
        Assert.That(page.Url, Is.EqualTo("http://localhost:5173/account"));

        var addRestaurantLink = await page.QuerySelectorAsync("a[placeholder='AddRestaurant']");
        Assert.NotNull(addRestaurantLink);
        await page.ScreenshotAsync(new() { Path = "../../../Slike/DodajRestoranAdmin5.png" });
    }

    [Test]
    public async Task DodajRestoran_User()
    {
        await page.GotoAsync($"http://localhost:5173/register");
        Assert.That(page.Url, Is.EqualTo("http://localhost:5173/register"));

        await page.ScreenshotAsync(new() { Path = "../../../Slike/DodajRestoranUser1.png" });

        await page.GetByPlaceholder("Name").FillAsync("Petko");
        await page.GetByPlaceholder("Prezime").FillAsync("Petkovic");
        await page.GetByPlaceholder("KorisnickoIme").FillAsync("Petko");
        await page.GetByPlaceholder("n@example.com").FillAsync("petko55@gmail.com");
        await page.GetByPlaceholder("Lozinka").FillAsync("petko");
        await page.GetByPlaceholder("Telefon").FillAsync("0652859632");

        var formFilled = await page.EvaluateAsync<bool>(@"() => {
                const inputs = Array.from(document.getElementsByTagName('input'));
                return inputs.every(input => input.value !== '');
        }");
        Assert.That(formFilled, Is.True);

        await page.ScreenshotAsync(new() { Path = "../../../Slike/DodajRestoranUser2.png" });

        await page.GetByPlaceholder("SignUp").ClickAsync();

        await page.GotoAsync($"http://localhost:5173/login");
        Assert.That(page.Url, Is.EqualTo("http://localhost:5173/login"));

        await page.GetByPlaceholder("name@example.com").FillAsync("petko55@gmail.com");
        await page.GetByPlaceholder("Password").FillAsync("petko");

        var formFilled2 = await page.EvaluateAsync<bool>(@"() => {
                const inputs = Array.from(document.getElementsByTagName('input'));
                return inputs.every(input => input.value !== '');
        }");
        Assert.That(formFilled2, Is.True);

        await page.ScreenshotAsync(new() { Path = "../../../Slike/DodajRestoranUser3.png" });

        await page.GetByPlaceholder("SignIn").ClickAsync();
        await page.GotoAsync($"http://localhost:5173/");
        Assert.That(page.Url, Is.EqualTo("http://localhost:5173/"));

        await page.ScreenshotAsync(new() { Path = "../../../Slike/DodajRestoranUser4.png" });

        await page.GetByPlaceholder("Account").ClickAsync();
        await page.GotoAsync($"http://localhost:5173/account");  //ispod Orders i Reviews nema opcije AddRestaurant
        Assert.That(page.Url, Is.EqualTo("http://localhost:5173/account"));

        var addRestaurantLink = await page.QuerySelectorAsync("a[placeholder='AddRestaurant']");
        Assert.Null(addRestaurantLink);
        await page.ScreenshotAsync(new() { Path = "../../../Slike/DodajRestoranUser5.png" });
    }

    [TearDown]
    public async Task Teardown()
    {
        await page.CloseAsync();
        await browser.DisposeAsync();
    }
}

