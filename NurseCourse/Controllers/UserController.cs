using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using NurseCourse.Models;
using NurseCourse.ViewModels;
using System.Configuration;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Authorization;
using NurseCourse.Services;

namespace NurseCourse.Controllers
{
    [Authorize(Roles = "admin")]
    public class UserController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Auth0Service _auth0Service;
        private readonly IConfiguration _configuration;
        private readonly string _auth0ApiUrl = "https://dev-r3d1ami1jr1ck5ez.us.auth0.com/api/v2/users";

        public UserController(IHttpClientFactory httpClientFactory, IConfiguration configuration, Auth0Service auth0Service)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _auth0Service = auth0Service;
        }



        // GET: UsuariosController
        public async Task<IActionResult> Index()
        {
            try
            {
                var usuarios = await ObtenerUsuariosAsync();
                return View(usuarios);
            }
            catch (Exception ex)
            {
                // Log del error
                Console.Write(ex.Message);
                // Puedes mostrar un mensaje de error en la vista
                ViewBag.ErrorMessage = "No se pudieron cargar los usuarios. Por favor, intenta nuevamente más tarde.";
                return View(new List<VUser>());
            }
        }

        private async Task<List<VUser>> ObtenerUsuariosAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var token = await _auth0Service.GetAuth0TokenAsync();

            var connection = _configuration["Auth0:Connection"];

            var requestUrl = $"{_auth0ApiUrl}?search_engine=v3";

            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var usuarios = JsonSerializer.Deserialize<List<VUser>>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return usuarios;
        }


        // GET: UsuariosController/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: UsuariosController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel model)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var connection_ = _configuration["Auth0:Connection"];
                var token = await _auth0Service.GetAuth0TokenAsync();
                var encodedEmail = Uri.EscapeDataString(model.Email);
                var api = "https://dev-r3d1ami1jr1ck5ez.us.auth0.com/api/v2";
                // 1. Verificar si el correo ya existe en la conexión
                var emailCheckRequest = new HttpRequestMessage(HttpMethod.Get, $"{api}/users-by-email?email={encodedEmail}");
                emailCheckRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

                var emailCheckResponse = await client.SendAsync(emailCheckRequest);
                emailCheckResponse.EnsureSuccessStatusCode();

                var emailCheckResponseContent = await emailCheckResponse.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<List<VUser>>(emailCheckResponseContent);

                var existingUser = users?.FirstOrDefault(user =>
                    user.identities.Any(identity => identity.connection == connection_));

                if (existingUser != null)
                {
                    // El usuario ya existe en la misma conexión
                    ViewData["email"] = "El correo electrónico ya está registrado";

                    return View(model);
                }

                // 2. Si el usuario no existe, proceder con la creación
                var user = new
                {
                    email = model.Email,
                    blocked = model.Blocked,
                    name = model.Name,
                    nickname = model.Nickname,
                    connection = connection_,
                    password = model.Password,
                    username = model.UserName
                };

                var jsonUser = JsonSerializer.Serialize(user);
                var createUserRequest = new HttpRequestMessage(HttpMethod.Post, _auth0ApiUrl)
                {
                    Content = new StringContent(jsonUser, Encoding.UTF8, "application/json")
                };
                createUserRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

                var createUserResponse = await client.SendAsync(createUserRequest);
                createUserResponse.EnsureSuccessStatusCode();

                // Redirigir al índice y actualizar los datos
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                ModelState.AddModelError(string.Empty, "No se pudo crear el usuario. Por favor, intenta nuevamente.");
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(string userId)
        {
            var client = _httpClientFactory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, $"{_auth0ApiUrl}/{userId}");
            var token = await _auth0Service.GetAuth0TokenAsync();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var userJson = JsonDocument.Parse(jsonResponse).RootElement;

            string email = userJson.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null;
            bool blocked = userJson.TryGetProperty("blocked", out var blockedProp) && blockedProp.GetBoolean();
            string name = userJson.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;
            string nickname = userJson.TryGetProperty("nickname", out var nicknameProp) ? nicknameProp.GetString() : null;
            string username = userJson.TryGetProperty("username", out var usernameprop) ? usernameprop.GetString() : null;

            string connection = null;

            // Verificar si el campo "identities" existe y tiene al menos un elemento
            if (userJson.TryGetProperty("identities", out var identities) && identities.GetArrayLength() > 0)
            {
                // Verificar si el campo "connection" existe en el primer elemento de identities
                if (identities[0].TryGetProperty("connection", out var connectionProp))
                {
                    connection = connectionProp.GetString();
                }
            }

            var userViewModel = new UserViewModel
            {
                UserId = userId,
                Email = email,
                Blocked = blocked,
                Name = name,
                Nickname = nickname,
                UserName = username,
                Connection = connection, // Puede ser null si no existe

            };

            return View(userViewModel);
        }



        // POST: UsuariosController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string userId, UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var client = _httpClientFactory.CreateClient();

                var userUpdate = new
                {

                    blocked = model.Blocked,
                    email = model.Email,
                    name = model.Name,
                    nickname = model.Nickname,
                    connection = model.Connection,
                    //user_name = model.UserName,

                };

                var jsonUserUpdate = JsonSerializer.Serialize(userUpdate);
                var token = await _auth0Service.GetAuth0TokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Patch, $"{_auth0ApiUrl}/{userId}")
                {
                    Content = new StringContent(jsonUserUpdate, Encoding.UTF8, "application/json")
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                ModelState.AddModelError(string.Empty, "No se pudo actualizar el usuario. Por favor, intenta nuevamente.");
                return View(model);
            }
        }

        // GET: UsuariosController/Delete/5
        public IActionResult Delete(int id)
        {
            return View();
        }

        // POST: UsuariosController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // Implementar la lógica de eliminación de usuario en Auth0
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View();
            }
        }
    }
}
