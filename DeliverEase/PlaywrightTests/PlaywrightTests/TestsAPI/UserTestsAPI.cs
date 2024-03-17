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
        public async Task DodajKorisnika_User_Uspesno()
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

        [Test]
        public async Task DodajKorisnika_Admin_Uspesno()
        {
            var noviKorisnik = new //obavezno promeni email
            {

                name = "admin10",
                surname = "admin10",
                username = "admin10",
                email = "admin10@gmail.com",
                password = "admin10",
                phoneNumber = "062546787",
                role = "admin"
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

        [Test]
        public async Task DodajAdmina_Neuspesno()
        {
            var noviKorisnik = new //obavezno promeni email
            {

                name = "Ignjat",
                surname = "Milosevic",
                email = "ignjat@gmail.com",
                password = "ignjat",
                role = "admin"
            };

            await using var response = await Request.PostAsync("/api/User/Register", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = noviKorisnik
            }); ;

            Assert.That(response.Status, Is.EqualTo(400));
            var textResponse = await response.TextAsync();
            Assert.IsNotNull(textResponse);
            Assert.That(textResponse, Does.Contain("The Username field is required."));
            Assert.That(textResponse, Does.Contain("The PhoneNumber field is required."));

        }

        [Test]
        public async Task DodajUsera_Neuspesno()
        {
            var noviKorisnik = new //obavezno promeni email
            {

                name = "Miroslav",
                surname = "Milosevic",
                username = "Miroslav",
                email = "miroslav@gmail.com",
                password = "miroslav",
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

            Assert.That(response.Status, Is.EqualTo(400));
            var textResponse = await response.TextAsync();
            Assert.IsNotNull(textResponse);
            Assert.That(textResponse, Does.Contain("The PhoneNumber field is required."));

        }

        [Test]
        public async Task DodajKorisnika_DuplikatEmail()
        {
            var noviKorisnik = new //obavezno promeni email
            {

                name = "Mitar",
                surname = "Peric",
                username = "Mitar",
                email = "mitar@gmail.com",
                password = "mitar",
                phoneNumber = "0627895467",
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


            var drugiNoviKorisnik = new //obavezno promeni email
            {

                name = "Mitar",
                surname = "Kovacevic",
                username = "Mitar",
                email = "mitar@gmail.com",
                password = "mitar",
                phoneNumber = "0627635467",
                role = "user"
            };

            await using var response2 = await Request.PostAsync("/api/User/Register", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = drugiNoviKorisnik
            }); ;


            Assert.That(response2.Status, Is.EqualTo(400));
            var textResponse = await response2.TextAsync();
            Assert.IsNotNull(textResponse);
            Assert.That(textResponse, Does.Contain("Email is already taken."));
           

        }

        [Test]
        public async Task PreuzmiUsera_Uspesno()
        {
            var korisnik = new //obavezno promeni email
            {

                name = "Ilija",
                surname = "Ilic",
                username = "Ilija",
                email = "ilija@gmail.com",
                password = "ilija",
                phoneNumber = "062546787",
                role = "user"
            };

            await using var createResponse = await Request.PostAsync("/api/User/Register", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = korisnik
            });

            if (createResponse.Status != 200)
            {
                Assert.Fail($"Code: {createResponse.Status} - {createResponse.StatusText}");
            }

            Assert.That(createResponse.Status, Is.EqualTo(200));
            var createTextResponse = await createResponse.TextAsync();
            var createJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createTextResponse);
            var userId = createJsonResponse["id"].ToString();

            await using var response = await Request.GetAsync($"/api/User/GetUserById?id={userId}");

            Assert.That(response.Status, Is.EqualTo(200));
            var textResponse = await createResponse.TextAsync();
            Assert.IsNotNull(textResponse);

            var jsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(textResponse);

            Assert.That(jsonResponse["id"].ToString(), Is.EqualTo(userId));
            Assert.That(jsonResponse["email"].ToString(), Is.EqualTo("ilija@gmail.com"));
            Assert.That(jsonResponse["role"].ToString(), Is.EqualTo("user"));


        }

        [Test]
        public async Task PreuzmiAdmina_Uspesno()
        {
            var korisnik = new //obavezno promeni email
            {

                name = "Iskra",
                surname = "Milic",
                username = "Iskra",
                email = "iskra@gmail.com",
                password = "iskra",
                phoneNumber = "062546787",
                role = "admin"
            };

            await using var createResponse = await Request.PostAsync("/api/User/Register", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = korisnik
            });

            if (createResponse.Status != 200)
            {
                Assert.Fail($"Code: {createResponse.Status} - {createResponse.StatusText}");
            }

            Assert.That(createResponse.Status, Is.EqualTo(200));
            var createTextResponse = await createResponse.TextAsync();
            var createJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createTextResponse);
            var userId = createJsonResponse["id"].ToString();

            await using var response = await Request.GetAsync($"/api/User/GetUserById?id={userId}");

            Assert.That(response.Status, Is.EqualTo(200));
            var textResponse = await createResponse.TextAsync();
            Assert.IsNotNull(textResponse);

            var jsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(textResponse);

            Assert.That(jsonResponse["id"].ToString(), Is.EqualTo(userId));
            Assert.That(jsonResponse["email"].ToString(), Is.EqualTo("iskra@gmail.com"));
            Assert.That(jsonResponse["role"].ToString(), Is.EqualTo("admin"));

        }


        [Test]
        public async Task PreuzmiUsera_Neuspesno()
        {
            string nepostojeciId = "65ed984c454619ba306c8c63";

            await using var response = await Request.GetAsync($"/api/User/GetUserById?id={nepostojeciId}");

            Assert.That(response.Status, Is.EqualTo(400));
            
        }

        [Test]
        public async Task PreuzmiAdmina_Neuspesno()
        {
            string nepostojeciId = "65ed984c454619ba306c8c63";

            await using var response = await Request.GetAsync($"/api/User/GetUserById?id={nepostojeciId}");

            Assert.That(response.Status, Is.EqualTo(400));

        }

        [Test]
        public async Task AzurirajKorisnika_Uspesno()
        {
            var korisnik = new //obavezno promeni email
            {
                name = "Vera",
                surname = "Matic",
                username = "Vera",
                email = "vera@gmail.com",
                password = "vera",
                phoneNumber = "062546787",
                role = "admin"
            };

            await using var createResponse = await Request.PostAsync("/api/User/Register", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = korisnik
            });

            if (createResponse.Status != 200)
            {
                Assert.Fail($"Code: {createResponse.Status} - {createResponse.StatusText}");
            }

            Assert.That(createResponse.Status, Is.EqualTo(200));
            var createTextResponse = await createResponse.TextAsync();
            var createJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createTextResponse);
            var userId = createJsonResponse["id"].ToString();

            var updatedUser = new
            {
                name = "Vera",
                surname = "Matic",
                username = "noviUsername",
                email = "noviemail@gmail.com",
                password = "novalozinka",
                phoneNumber = "0638968542",
                role = "admin"
                
            };

            await using var response = await Request.PutAsync($"/api/User/UpdateUser?id={userId}", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = updatedUser
            });

            if (response.Status != 200)
            {
                Assert.Fail($"Code: {response.Status} - {response.StatusText}");
            }

            Assert.That(response.Status, Is.EqualTo(200));
            var textResponse = await response.TextAsync();
            Assert.IsNotNull(textResponse);
            Assert.That(textResponse, Does.Contain("User successfully updated."));

        }

        [Test]
        public async Task AzuirajKorisnika_Neuspesno()
        {
            string nepostojeciId = "65ed984c454619ba306c8c63";

            var updatedUser = new
            {
                name = "NoviUser",
                surname = "NoviUser",
                username = "noviUsername",
                email = "noviuser@gmail.com",
                password = "novalozinka",
                phoneNumber = "0638968542",
                role = "user"

            };

            await using var response = await Request.PutAsync($"/api/User/UpdateUser?id={nepostojeciId}", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = updatedUser
            });

            Assert.That(response.Status, Is.EqualTo(400));
            var textResponse = await response.TextAsync();
            Assert.IsNotNull(textResponse);

            Assert.That(textResponse, Does.Contain($"User with ID {nepostojeciId} does not exist."));
        }

        [Test]
        public async Task ObrisiKorisnika_Uspesno()
        {
            var newUser = new //obavezno promeni email
            {
                name = "Sava",
                surname = "Savic",
                username = "Sava",
                email = "sava@gmail.com",
                password = "sava",
                phoneNumber = "0638968542",
                role = "user"

            };
            await using var createResponse = await Request.PostAsync("/api/User/Register", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = newUser
            });

            if (createResponse.Status != 200)
            {
                Assert.Fail($"Code: {createResponse.Status} - {createResponse.StatusText}");
            }

            Assert.That(createResponse.Status, Is.EqualTo(200));
            var createTextResponse = await createResponse.TextAsync();
            var createJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createTextResponse);
            var userId = createJsonResponse["id"].ToString();


            await using var response = await Request.DeleteAsync($"/api/User/DeleteUser?id={userId}");

            Assert.That(response.Status, Is.EqualTo(200));
            var textResponse = await response.TextAsync();
            Assert.IsNotNull(textResponse);
            Assert.That(textResponse, Does.Contain($"Izbrisan je korisnik: {userId}"));
        }

        [Test]
        public async Task ObrisiKorisnika_Neuspesno()
        {
            string nepostojeciId = "65ed984c454619ba306c8c63";

            await using var response = await Request.DeleteAsync($"/api/User/DeleteUser?id={nepostojeciId}");

            Assert.That(response.Status, Is.EqualTo(400));
            var textResponse = await response.TextAsync();
            Assert.IsNotNull(textResponse);
            Assert.That(textResponse, Does.Contain($"User with ID {nepostojeciId} does not exist."));
        }


        [Test]
        public async Task LogIn_ValidniPodaci()
        {
            var newUser = new   //obavezno promeni email
            {
                name = "Tadej",
                surname = "Popovic",
                username = "Tadej",
                email = "tadej@gmail.com",
                password = "tadej",
                phoneNumber = "0638234542",
                role = "user"

            };
            await using var createResponse = await Request.PostAsync("/api/User/Register", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = newUser
            });

            if (createResponse.Status != 200)
            {
                Assert.Fail($"Code: {createResponse.Status} - {createResponse.StatusText}");
            }

            Assert.That(createResponse.Status, Is.EqualTo(200));
            var createTextResponse = await createResponse.TextAsync();
            var createJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createTextResponse);
            var email = newUser.email;
            var password = newUser.password;

            await using var response2 = await Request.PostAsync($"/api/User/Login?email={email}&password={password}", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            });

            Assert.That(response2.Status, Is.EqualTo(200));
            var textResponse = await response2.TextAsync();
            Assert.IsNotNull(textResponse);

        }

        [Test]
        public async Task LogIn_NevalidniEmail()
        {
            var newUser = new //obavezno promeni email
            {
                name = "Aca",
                surname = "Petrovic",
                username = "Aca",
                email = "aca@gmail.com",
                password = "aca",
                phoneNumber = "0638695542",
                role = "admin"

            };
            await using var createResponse = await Request.PostAsync("/api/User/Register", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = newUser
            });

            if (createResponse.Status != 200)
            {
                Assert.Fail($"Code: {createResponse.Status} - {createResponse.StatusText}");
            }

            Assert.That(createResponse.Status, Is.EqualTo(200));
            var createTextResponse = await createResponse.TextAsync();
            var createJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createTextResponse);
            var email = "aca10@gmail.com";
            var password = newUser.password;

            await using var response2 = await Request.PostAsync($"/api/User/Login?email={email}&password={password}", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            });

            Assert.That(response2.Status, Is.EqualTo(400));
            var textResponse = await response2.TextAsync();
            Assert.IsNotNull(textResponse);
            Assert.That(textResponse, Does.Contain("Invalid email"));

        }

        [Test]
        public async Task LogIn_NevalidniPassword()
        { 
            var newUser = new  //obavezno promeni email
            {
                name = "Luka",
                surname = "Petrovic",
                username = "Luka",
                email = "luka@gmail.com",
                password = "luka",
                phoneNumber = "0638612542",
                role = "user"

            };
            await using var createResponse = await Request.PostAsync("/api/User/Register", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = newUser
            });

            if (createResponse.Status != 200)
            {
                Assert.Fail($"Code: {createResponse.Status} - {createResponse.StatusText}");
            }

            Assert.That(createResponse.Status, Is.EqualTo(200));
            var createTextResponse = await createResponse.TextAsync();
            var createJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createTextResponse);
            var email = newUser.email;
            var password = "luka10";

            await using var response2 = await Request.PostAsync($"/api/User/Login?email={email}&password={password}", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            });

            Assert.That(response2.Status, Is.EqualTo(400));
            var textResponse = await response2.TextAsync();
            Assert.IsNotNull(textResponse);
            Assert.That(textResponse, Does.Contain("Invalid password"));

        }

        [Test]
        public async Task PreuzmiUlogovanogKorisnika()
        {
            var newUser = new  //obavezno promeni email
            {
                name = "Jana",
                surname = "Nedeljkovic",
                username = "Jana",
                email = "jana@gmail.com",
                password = "jana",
                phoneNumber = "0638612542",
                role = "user"

            };
            await using var createResponse = await Request.PostAsync("/api/User/Register", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = newUser
            });

            if (createResponse.Status != 200)
            {
                Assert.Fail($"Code: {createResponse.Status} - {createResponse.StatusText}");
            }

            Assert.That(createResponse.Status, Is.EqualTo(200));
            var createTextResponse = await createResponse.TextAsync();
            var createJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createTextResponse);
            var email = newUser.email;
            var password = newUser.password;

            await using var response2 = await Request.PostAsync($"/api/User/Login?email={email}&password={password}", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            });

            var textResponse = await response2.TextAsync();
            Assert.IsNotNull(textResponse);

            await using var response3 = await Request.GetAsync($"/api/User/GetUser", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            });


            Assert.That(response3.Status, Is.EqualTo(200));
            var createTextResponse3 = await response3.TextAsync();
            var jsonResponse3 = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createTextResponse3);

            Assert.That(jsonResponse3["name"].ToString(), Is.EqualTo("Jana"));
            Assert.That(jsonResponse3["surname"].ToString(), Is.EqualTo("Nedeljkovic"));
            Assert.That(jsonResponse3["email"].ToString(), Is.EqualTo("jana@gmail.com"));
        }


        [Test]
        public async Task PreuzmiKorisnika_NijeUlogovan()
        {
            var newUser = new  //obavezno promeni email
            {
                name = "Anja",
                surname = "Nedeljkovic",
                username = "Anja",
                email = "anja@gmail.com",
                password = "anja",
                phoneNumber = "0638612542",
                role = "user"

            };
            await using var createResponse = await Request.PostAsync("/api/User/Register", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = newUser
            });

            if (createResponse.Status != 200)
            {
                Assert.Fail($"Code: {createResponse.Status} - {createResponse.StatusText}");
            }

            Assert.That(createResponse.Status, Is.EqualTo(200));
            var createTextResponse = await createResponse.TextAsync();
            var createJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createTextResponse);
            var email = newUser.email;
            var password = newUser.password;

            await using var response2 = await Request.GetAsync($"/api/User/GetUser", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            });


            Assert.That(response2.Status, Is.EqualTo(401));
            var textResponse = await response2.TextAsync();
            Assert.IsNotNull(textResponse);
            Assert.That(textResponse, Does.Contain("Unauthorized"));

        }

        [Test]
        public async Task LogOut()
        {
            var newUser = new  //obavezno promeni email
            {
                name = "Andrea",
                surname = "Mitic",
                username = "Andrea",
                email = "andrea@gmail.com",
                password = "andrea",
                phoneNumber = "0638612542",
                role = "user"

            };
            await using var createResponse = await Request.PostAsync("/api/User/Register", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                DataObject = newUser
            });

            if (createResponse.Status != 200)
            {
                Assert.Fail($"Code: {createResponse.Status} - {createResponse.StatusText}");
            }

            Assert.That(createResponse.Status, Is.EqualTo(200));
            var createTextResponse = await createResponse.TextAsync();
            var createJsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createTextResponse);
            var email = newUser.email;
            var password = newUser.password;

            await using var response2 = await Request.PostAsync($"/api/User/Login?email={email}&password={password}", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            });

            var textResponse = await response2.TextAsync();
            Assert.IsNotNull(textResponse);

            await using var response3 = await Request.PostAsync($"/api/User/logOut", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            });


            Assert.That(response3.Status, Is.EqualTo(200));
            var textResponse3 = await response3.TextAsync();
            Assert.IsNotNull(textResponse3);
            Assert.That(textResponse3, Does.Contain("success"));
        }


    }

}


