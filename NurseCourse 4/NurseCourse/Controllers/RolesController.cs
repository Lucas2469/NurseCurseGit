using Microsoft.AspNetCore.Mvc;
using NurseCourse.Models;
using NurseCourse.Services;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using NurseCourse.ViewModels;

namespace NurseCourse.Controllers
{
    [Authorize(Roles = "admin")]
    public class RolesController : Controller
    {
        private readonly ModularCourseDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Auth0Service _auth0Service;
        private readonly string _auth0ApiUrl = $"https://dev-vkqv0tzyahvotuzs.us.auth0.com/api/v2/roles";

        public RolesController(IHttpClientFactory httpClientFactory, Auth0Service auth0Service, ModularCourseDbContext context)
        {
            _httpClientFactory = httpClientFactory;
            _auth0Service = auth0Service;
            _context = context;
        }


        private async Task<Role> ObtenerRolPorIdAsync(string id)
        {
            var client = _httpClientFactory.CreateClient();
            var token = await _auth0Service.GetAuth0TokenAsync();
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_auth0ApiUrl}/{id}");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var role = JsonSerializer.Deserialize<Role>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return role;
        }
        private async Task<List<Role>> ObtenerRolesAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var token = await _auth0Service.GetAuth0TokenAsync();
            var request = new HttpRequestMessage(HttpMethod.Get, _auth0ApiUrl);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var roles = JsonSerializer.Deserialize<List<Role>>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var rolesFiltrados = roles.Where(r => r.Name.Equals("admin", StringComparison.OrdinalIgnoreCase)).ToList();
            return rolesFiltrados;
        }

        [HttpGet]
        // GET: RolesController/AssignRole/5
        public async Task<IActionResult> AssignRole(string userId)
        {
            try
            {
                // Obtener los datos del usuario desde Auth0
                var roles = await ObtenerRolesAsync() ?? new List<Role>();
                var usuarioAuth0 = await ObtenerUsuarioPorIdAsync(userId);

                if (usuarioAuth0 == null)
                {
                    ViewBag.ErrorMessage = "No se pudo encontrar el usuario en Auth0. Por favor, intenta nuevamente.";
                    return View(new UserDetailsViewModel()); // Devolver una vista vacía si no se encuentra el usuario en Auth0
                }

                // Intentar obtener la información adicional del usuario en la base de datos local
                var usuarioDb = _context.Users.FirstOrDefault(u => u.Email == usuarioAuth0.email);
                var enrolledCourses = usuarioDb != null
                    ? _context.Certificates
                              .Where(c => c.UserId == usuarioDb.Id)
                              .Select(c => c.Course.Name)
                              .ToList()
                    : new List<string>();

                // Crear el ViewModel combinando datos de Auth0 y de la base de datos (si están disponibles)
                var userDetailsViewModel = new UserDetailsViewModel
                {
                    UserId = usuarioAuth0.user_id,
                    Email = usuarioAuth0.email,
                    Name = usuarioAuth0.name,
                    Nickname = usuarioAuth0.nickname,
                    Picture = usuarioAuth0.picture,
                    Roles = roles, // Lista de roles disponibles en el sistema
                    AssignedRoleIds = await ObtenerAssignedRoleIdsAsync(userId) ?? new List<string>(), // Lista de roles asignados al usuario en Auth0

                    // Información adicional de la base de datos, si está disponible
                    Age = usuarioDb?.Age,
                    Gender = usuarioDb?.Gender,
                    IdNumber = usuarioDb?.IdNumber,
                    CountryOfOrigin = usuarioDb?.CountryOfOrigin,
                    StateOfOrigin = usuarioDb?.StateOfOrigin,
                    CityOfOrigin = usuarioDb?.CityOfOrigin,
                    BirthDate = usuarioDb?.BirthDate.ToDateTime(TimeOnly.MinValue),
                    Occupation = usuarioDb?.Occupation,
                    SpecifyOccupation = usuarioDb?.SpecifyOccupation,
                    HealthProfession = usuarioDb?.HealthProfession,
                    EducationLevel = usuarioDb?.EducationLevel,
                    Institution = usuarioDb?.Institution,
                    Workplace = usuarioDb?.Workplace,
                    EnrolledCourses = enrolledCourses, // Cursos en los que el usuario está inscrito
                };

                return View(userDetailsViewModel);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                ViewBag.ErrorMessage = "No se pudieron cargar los roles o los detalles del usuario. Por favor, intenta nuevamente más tarde.";
                return View(new UserDetailsViewModel
                {
                    Roles = new List<Role>(), // Asegurarse de que siempre haya una lista de roles para evitar errores en la vista
                    EnrolledCourses = new List<string>() // Lista vacía de cursos en caso de error
                });
            }
        }


        private async Task<ICollection<string>> ObtenerAssignedRoleIdsAsync(string userId)
        {
            var client = _httpClientFactory.CreateClient();
            var token = await _auth0Service.GetAuth0TokenAsync();
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://dev-vkqv0tzyahvotuzs.us.auth0.com/api/v2/users/{userId}/roles");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var roles = JsonSerializer.Deserialize<List<Role>>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return roles.Select(role => role.Id).ToList();
        }
        private async Task<VUser> ObtenerUsuarioPorIdAsync(string userId)
        {
            var client = _httpClientFactory.CreateClient();
            var token = await _auth0Service.GetAuth0TokenAsync();
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://dev-vkqv0tzyahvotuzs.us.auth0.com/api/v2/users/{userId}");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var usuario = JsonSerializer.Deserialize<VUser>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return usuario;
        }

        // POST: RolesController/AssignRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(string userId, string[] selectedRoleIds, string action)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || selectedRoleIds == null || selectedRoleIds.Length == 0)
                {
                    ModelState.AddModelError(string.Empty, "Usuario o rol inválido.");
                    // Recargar los roles y detalles del usuario para mostrar en la vista de error
                    var roles = await ObtenerRolesAsync() ?? new List<Role>();
                    var usuarioAuth0 = await ObtenerUsuarioPorIdAsync(userId) ?? new VUser();
                    var usuarioDb = _context.Users.FirstOrDefault(u => u.Email == usuarioAuth0.email);

                    var userDetailsViewModel = new UserDetailsViewModel
                    {
                        UserId = usuarioAuth0.user_id,
                        Email = usuarioAuth0.email,
                        Name = usuarioAuth0.name,
                        Nickname = usuarioAuth0.nickname,
                        Picture = usuarioAuth0.picture,
                        Roles = roles,
                        AssignedRoleIds = await ObtenerAssignedRoleIdsAsync(userId) ?? new List<string>(),
                        Age = usuarioDb?.Age,
                        Gender = usuarioDb?.Gender,
                        IdNumber = usuarioDb?.IdNumber,
                        CountryOfOrigin = usuarioDb?.CountryOfOrigin,
                        StateOfOrigin = usuarioDb?.StateOfOrigin,
                        CityOfOrigin = usuarioDb?.CityOfOrigin,
                        BirthDate = usuarioDb?.BirthDate.ToDateTime(TimeOnly.MinValue),
                        Occupation = usuarioDb?.Occupation,
                        SpecifyOccupation = usuarioDb?.SpecifyOccupation,
                        HealthProfession = usuarioDb?.HealthProfession,
                        EducationLevel = usuarioDb?.EducationLevel,
                        Institution = usuarioDb?.Institution,
                        Workplace = usuarioDb?.Workplace,
                        EnrolledCourses = usuarioDb != null
                            ? _context.Certificates.Where(c => c.UserId == usuarioDb.Id).Select(c => c.Course.Name).ToList()
                            : new List<string>()
                    };

                    ViewData["error"] = "Selecciona al menos 1 rol";
                    return View(userDetailsViewModel);
                }

                // Lógica para asignar o eliminar roles
                var client = _httpClientFactory.CreateClient();
                var token = await _auth0Service.GetAuth0TokenAsync();
                var baseUrl = "https://dev-vkqv0tzyahvotuzs.us.auth0.com/api/v2/users";

                if (action == "assign")
                {
                    foreach (var roleId in selectedRoleIds)
                    {
                        var data = new { roles = new[] { roleId } };
                        var jsonContent = JsonSerializer.Serialize(data);
                        var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/{userId}/roles")
                        {
                            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
                        };
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
                        var response = await client.SendAsync(request);
                        response.EnsureSuccessStatusCode();
                    }
                }
                else if (action == "remove")
                {
                    var data = new { roles = selectedRoleIds };
                    var jsonContent = JsonSerializer.Serialize(data);
                    var request = new HttpRequestMessage(HttpMethod.Delete, $"{baseUrl}/{userId}/roles")
                    {
                        Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
                    };
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                }

                return RedirectToAction("Index", "User");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                ModelState.AddModelError(string.Empty, "No se pudo actualizar los roles. Por favor, intenta nuevamente.");
                // Recargar los roles y datos del usuario para la vista en caso de error
                var roles = await ObtenerRolesAsync();
                var usuario = await ObtenerUsuarioPorIdAsync(userId);
                var usuarioDb = _context.Users.FirstOrDefault(u => u.Email == usuario.email);

                var userDetailsViewModel = new UserDetailsViewModel
                {
                    UserId = usuario.user_id,
                    Email = usuario.email,
                    Name = usuario.name,
                    Nickname = usuario.nickname,
                    Picture = usuario.picture,
                    Roles = roles ?? new List<Role>(),
                    AssignedRoleIds = await ObtenerAssignedRoleIdsAsync(userId) ?? new List<string>(),
                    Age = usuarioDb?.Age,
                    Gender = usuarioDb?.Gender,
                    IdNumber = usuarioDb?.IdNumber,
                    CountryOfOrigin = usuarioDb?.CountryOfOrigin,
                    StateOfOrigin = usuarioDb?.StateOfOrigin,
                    CityOfOrigin = usuarioDb?.CityOfOrigin,
                    BirthDate = usuarioDb?.BirthDate.ToDateTime(TimeOnly.MinValue),
                    Occupation = usuarioDb?.Occupation,
                    SpecifyOccupation = usuarioDb?.SpecifyOccupation,
                    HealthProfession = usuarioDb?.HealthProfession,
                    EducationLevel = usuarioDb?.EducationLevel,
                    Institution = usuarioDb?.Institution,
                    Workplace = usuarioDb?.Workplace,
                    EnrolledCourses = usuarioDb != null
                        ? _context.Certificates.Where(c => c.UserId == usuarioDb.Id).Select(c => c.Course.Name).ToList()
                        : new List<string>()
                };

                return View(userDetailsViewModel);
            }
        }

        // POST: RolesController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var token = await _auth0Service.GetAuth0TokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{_auth0ApiUrl}/{id}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                ModelState.AddModelError(string.Empty, "No se pudo eliminar el rol. Por favor, intenta nuevamente.");
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
