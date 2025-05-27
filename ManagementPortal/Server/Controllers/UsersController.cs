using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ManagementPortal.Server.Configurations.Parsers;
using ManagementPortal.Server.Context;
using ManagementPortal.Server.Models;
using ManagementPortal.Server.Services;
using ManagementPortal.Shared.Constants;
using ManagementPortal.Shared.Dtos;
using ManagementPortal.Shared.Dtos.Apis;
using ManagementPortal.Shared.Dtos.ResponseMessages;
using ManagementPortal.Shared.Models;
using ManagementPortal.Shared.Resources;
using ManagementPortal.Shared.Resources.Server;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;

namespace ManagementPortal.Server.Controllers
{
    /// <summary>
    /// Controller que expone endpoints para las funcionalidades de Usuarios.
    /// Justifacion: Se decide agrupar en un controller las funcionalidades de Usuarios.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TwoFactorAuthService _twoFactorAuthService;
        private readonly RoleManager<ApplicationRole> _rolemanager;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsersController"/> class.
        /// </summary>
        /// /// <param name="dbContext">EF para manejar la base de datos de la aplicacion.</param>
        /// <param name="userManager">Manejador de Identity para usuarios.</param>
        /// <param name="twoFactorAuthService">Servicio que implementa funcionalidades de 2FA.</param>
        /// <param name="roleManager">Manejador de Identity para Roles.</param>
        /// <param name="httpClientFactory">Factory para crear los httpClient.</param>
        /// <param name="configuration">Configuración de la aplicación.</param>
        public UsersController(
            ApplicationContext dbContext,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            TwoFactorAuthService twoFactorAuthService,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            this._dbContext = dbContext;
            this._userManager = userManager;
            this._twoFactorAuthService = twoFactorAuthService;
            this._rolemanager = roleManager;
            this._httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        /// <summary>
        /// Crear un nuevo usuario usando Identity.
        /// </summary>
        /// <param name="applicationUser">Se incluyen los atributos necesarios: Name, LastName, Username, Email, Password, y los opcionales: Image, Roles, Products.</param>
        /// <returns>Retorna 400 si el usuario existe y 200 si el usuario se creo con exito.</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateUser([FromBody] RegisterUserRequest applicationUser)
        {
            var userPermission = User.Claims
                            .Where(c => c.Type == ClaimTypes.Role)
                            .Select(c => c.Value)
                            .ToList();

            if (!userPermission.Contains(ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.CreateUser]))
            {
                Log.Error(string.Format(LogErrorResources.InvalidRole, ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.CreateUser]));
                return Unauthorized(UsersResources.InvalidRole);
            }

            try
            {
                ApplicationUser newUser = new()
                {
                    UserName = applicationUser.UserName,
                    Email = applicationUser.Email,
                    Name = applicationUser.Name,
                    LastName = applicationUser.LastName,
                };

                var userEmail = await _userManager.FindByEmailAsync(applicationUser.Email);
                var userName = await _userManager.FindByNameAsync(applicationUser.UserName);
                var password = applicationUser.Password;
                var productsUser = new List<ApplicationUserProduct>();

                if (userEmail != null)
                {
                    Log.Error(string.Format(LogErrorResources.UserEmailAlreadyExists, applicationUser.Email));
                    return BadRequest(string.Format(UsersResources.UserEmailAlreadyExists, applicationUser.Email));
                }

                if (userName != null)
                {
                    Log.Error(string.Format(LogErrorResources.UserNameAlreadyExists, applicationUser.UserName));
                    return BadRequest(string.Format(UsersResources.UserNameAlreadyExists, applicationUser.UserName));
                }

                if (applicationUser.Image != null)
                {
                    newUser.ProfilePhoto = applicationUser.Image;
                }

                if (applicationUser.Products != null)
                {
                    foreach (var product in applicationUser.Products)
                    {
                        UserPostDto newUserDto = new()
                        {
                            UserName = applicationUser.UserName,
                            Email = applicationUser.Email,
                            Name = applicationUser.Name,
                            LastName = applicationUser.LastName,
                            Password = applicationUser.Password,
                            ConfirmPassword = applicationUser.Password,
                            Picture = applicationUser.Image?.ToString() ?? string.Empty,
                            Signature = string.Empty,
                            LastVersion = string.Empty,
                            ExternalIds = product.ExternalIds,
                            RoleIds = product.Roles,
                        };

                        if (product?.Product != null)
                        {
                            string productName = product.Product.ToString();
                            var httpClient = _httpClientFactory.CreateClient(productName);

                            var response = await httpClient.PostAsJsonAsync("api/User", newUserDto);

                            if (!response.IsSuccessStatusCode)
                            {
                                string errorData = await response.Content.ReadAsStringAsync();
                                string errorMessage = string.Format(RolesResources.ProductError, productName, errorData);
                                Log.Error(string.Format(LogErrorResources.ProductError, productName, errorData));
                                return StatusCode((int)response.StatusCode, errorMessage);
                            }

                            var idResponse = await response.Content.ReadAsStringAsync();
                            var productUser = new ApplicationUserProduct() { Product = product.Product, UserProductId = idResponse, };
                            productsUser.Add(productUser);
                        }
                    }
                }

                newUser.Products = productsUser;
                var resultUser = await _userManager.CreateAsync(newUser, password);

                if (resultUser.Succeeded)
                {
                    if (applicationUser.MPRoles != null && applicationUser.MPRoles.Any())
                    {
                        if (newUser.Name == null)
                        {
                            Log.Error(string.Format(LogErrorResources.UserNotFound, JsonConvert.SerializeObject(newUser)));
                            return NotFound(UsersResources.UserNotFound);
                        }

                        var existingRoles = _dbContext.ApplicationRoles
                            .Where(applicationRole =>
                                applicationUser.MPRoles != null &&
                                applicationRole.Name != null &&
                                applicationUser.MPRoles.Contains(applicationRole.Name))
                            .ToList();

                        var roleNames = existingRoles
                        .Select(role => role.Name)
                        .Where(roleName => roleName != null)
                        .Cast<string>()
                        .ToList();

                        var result = await _userManager.AddToRolesAsync(newUser, roleNames);
                    }

                    return Ok(new
                    {
                        UserId = newUser.Id,
                    });
                }
                else
                {
                    var userError = JsonConvert.SerializeObject(applicationUser);
                    string pattern = @"ssword\"":\""[^\""]*\""";
                    string replacement = "ssword\":\"[REDACTED]\"";
                    userError = Regex.Replace(userError, pattern, replacement);
                    Log.Error(string.Format(LogErrorResources.CreateUserError, userError));
                    return BadRequest(UsersResources.CreateUserError);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Permite dar de baja un usuario en MP o en un producto en específico.
        /// </summary>
        /// <param name="product"> Producto del que se desea dar de baja al usuario.</param>
        /// <param name="username">ID del usuario que se desea eliminar.</param>
        /// <returns>Retorna 200 si el usuario se eliminó correctamente y 400 en caso de error.</returns>
        [HttpDelete("{product}/{username}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(Products product, string username)
        {
            var userPermission = User.Claims
                            .Where(c => c.Type == ClaimTypes.Role)
                            .Select(c => c.Value)
                            .ToList();

            if (!userPermission.Contains(ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.DeleteUser]))
            {
                Log.Error(string.Format(LogErrorResources.InvalidRole, ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.DeleteUser]));
                return Unauthorized(UsersResources.InvalidRole);
            }

            if (string.IsNullOrEmpty(username))
            {
                Log.Error(LogErrorResources.UsernameRequired);
                return BadRequest(UsersResources.UsernameRequired);
            }

            var user = await _userManager.Users.Include(u => u.Products).FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null)
            {
                Log.Error(string.Format(LogErrorResources.UserNotFound, username));
                return NotFound(UsersResources.UserNotFound);
            }

            var message = new List<AssignProductMessage>();

            if (product == Products.ManagementPortal)
            {
                // Eliminar al usuario de MP (y por lo tanto de todos los productos)
                var userLogged = await _userManager.GetUserAsync(User);

                // Si el usuario se quiere eliminar a si mismo retornar error.
                if (userLogged?.UserName == username)
                {
                    return BadRequest(new { message = UsersResources.DeleteYourselfInvalid });
                }

                user.IsDeleted = true;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    try
                    {
                        message = await AsignProducts(username, new List<Products>()); // Para eliminarlo de todos los productos le mando la lista vacía
                    }
                    catch (InvalidOperationException ex)
                    {
                        Log.Error(ex.Message);
                        return BadRequest(ex.Message);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message);
                        return StatusCode(500, ex.Message);
                    }

                    return Ok(message);
                }
                else
                {
                    Log.Error(result.ToString());
                    return BadRequest();
                }
            }
            else
            {
                // Eliminar al usuario de un producto en concreto.
                var newUserProducts = user.Products?
                .Where(up => up.Product != product) // Obtener productos del usuario excluyendo el producto a eliminar
                .Select(up => up.Product)
                .ToList() ?? new List<Products>();

                try
                {
                    message = await AsignProducts(username, newUserProducts);
                }
                catch (InvalidOperationException ex)
                {
                    Log.Error(ex.Message);
                    return BadRequest(ex.Message);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    return StatusCode(500, ex.Message);
                }

                return Ok(message);
            }
        }

        /// <summary>
        /// Permite a un usuario con permiso de edición editar todos los datos de un usuario.
        /// </summary>
        /// <param name="applicationUser"> Datos del usuario a modificar (ver UserCompleteEditDto).</param>
        /// <returns>Codigo de error en caso de error externo a los métodos, en otro caso ok(EditUserMessage) dentro del DTO se incluyen el resultado de cada operación.</returns>
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateUser([FromBody] UserCompleteEditDto applicationUser)
        {
            var userPermission = User.Claims
                           .Where(c => c.Type == ClaimTypes.Role)
                           .Select(c => c.Value)
                           .ToList();

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                Log.Error(LogErrorResources.UserLoginNotFound);
                return NotFound(UsersResources.UserNotFound);
            }

            if (!userPermission.Contains(ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.EditUser]))
            {
                Log.Error(string.Format(LogErrorResources.InvalidRole, ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.EditUser]));
                return Unauthorized(UsersResources.InvalidRole);
            }

            var message = new EditUserMessage
            {
                ProductAssignMessages = new List<AssignProductMessage>(),
                UserProfileMessage = string.Empty,
                UserProfileResult = false,
            };

            // Obtener la lista de Productos del usuario
            var userProducts = applicationUser.Products.Select(up => up.Product).ToList();

            try
            {
                message.ProductAssignMessages = await AsignProducts(applicationUser.UserName, userProducts);
            }
            catch (InvalidOperationException ex)
            {
                Log.Error(ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return StatusCode(500, ex.Message);
            }

            try
            {
                await EditUserProfile(applicationUser);
            }
            catch (InvalidOperationException ex)
            {
                message.UserProfileMessage = ex.Message;
                Log.Error(ex.Message);
                return Ok(message);
            }
            catch (Exception ex)
            {
                message.UserProfileMessage = ex.Message;
                Log.Error(ex.Message);
                return Ok(message);
            }

            message.UserProfileMessage = UsersResources.modifySuccess;
            message.UserProfileResult = true;
            return Ok(message);
        }

        /// <summary>
        /// Le permite a un usuario editar su propio perfil.
        /// </summary>
        /// <param name="applicationUser"> Datos del usuario a modificar (ver UserProfileEditDto). </param>
        /// <returns> codigo 200 si la operación fue exitosa, 400 si hay un error en los parametros o 500 si hubo otro error. </returns>
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UserProfileEditDto applicationUser)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                Log.Error(LogErrorResources.UserLoginNotFound);
                return NotFound(UsersResources.UserNotFound);
            }

            if (user.UserName != applicationUser.UserName)
            {
                Log.Error(string.Format(LogErrorResources.DifferentUser, user.UserName, applicationUser.UserName));
                return Unauthorized(UsersResources.InvalidRole);
            }

            try
            {
                await EditUserProfile(applicationUser);
            }
            catch (InvalidOperationException ex)
            {
                Log.Error(ex.Message);
                return StatusCode(500, ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Ok(ex.Message);
            }

            return Ok();
        }

        /// <summary>
        /// Lista todos los usuarios en la base de datos con filtros y paginación.
        /// </summary>
        /// <param name="userFilter"> Dto con los filtros a aplicar.</param>
        /// <returns>Retorna el listado de usuarios según los filtros de búsqueda y paginación.</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ListUsers([FromQuery] FilterUserDto userFilter)
        {
            // Verificar permisos del usuario
            var userPermission = User.Claims
                                .Where(c => c.Type == ClaimTypes.Role)
                                .Select(c => c.Value)
                                .ToList();

            if (!userPermission.Contains(ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.ListUsers]))
            {
                Log.Error(string.Format(LogErrorResources.InvalidRole, ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.ListUsers]));
                return Unauthorized(UsersResources.InvalidRole);
            }

            try
            {
                var usersQuery = _userManager.Users.Include(u => u.Products).AsQueryable();

                if (!string.IsNullOrEmpty(userFilter.SearchText))
                {
                    userFilter.SearchText = userFilter.SearchText.ToLower();
                    usersQuery = usersQuery.Where(a => (a.Name != null && a.Name.ToLower().Contains(userFilter.SearchText)) ||
                        (a.LastName != null && a.LastName.ToLower().Contains(userFilter.SearchText)) ||
                        (a.UserName != null && a.UserName.ToLower().Contains(userFilter.SearchText)) ||
                        (a.Email != null && a.Email.ToLower().Contains(userFilter.SearchText)));
                }

                if (userFilter.State == UserState.Active)
                {
                    usersQuery = usersQuery.Where(u => u.IsDeleted == false);
                }
                else if (userFilter.State == UserState.Deleted)
                {
                    usersQuery = usersQuery.Where(u => u.IsDeleted == true);
                }

                var users = await usersQuery.ToListAsync();

                if (userFilter.Product != Products.ManagementPortal)
                {
                    users = users.Where(u => u.Products != null && u.Products.Any(p => p.Product == userFilter.Product)).ToList();
                }

                foreach (var user in users)
                {
                    if (user.Products != null)
                    {
                        foreach (var product in user.Products)
                        {
                            if (product.Product == userFilter.Product)
                            {
                                user.IsDeleted = product.ProductDelete || user.IsDeleted;
                            }
                        }
                    }
                }

                var totalUsersCount = users.Count();

                if (userFilter.SortField == SortFieldUser.Name)
                {
                    users = userFilter.SortOrder == Order.Ascending
                    ? users.OrderBy(r => r.Name).ToList()
                    : users.OrderByDescending(r => r.Name).ToList();
                }
                else if (userFilter.SortField == SortFieldUser.LastName)
                {
                    users = userFilter.SortOrder == Order.Ascending
                    ? users.OrderBy(r => r.LastName).ToList()
                    : users.OrderByDescending(r => r.LastName).ToList();
                }
                else if (userFilter.SortField == SortFieldUser.LastLogin)
                {
                    users = userFilter.SortOrder == Order.Ascending
                    ? users.OrderBy(r => r.LastLoginDate).ToList()
                    : users.OrderByDescending(r => r.LastLoginDate).ToList();
                }
                else if (userFilter.SortField == SortFieldUser.Email)
                {
                    users = userFilter.SortOrder == Order.Ascending
                    ? users.OrderBy(r => r.Email).ToList()
                    : users.OrderByDescending(r => r.Email).ToList();
                }
                else if (userFilter.SortField == SortFieldUser.Status)
                {
                    users = userFilter.SortOrder == Order.Ascending
                    ? users.OrderBy(r => r.IsDeleted).ToList()
                    : users.OrderByDescending(r => r.IsDeleted).ToList();
                }
                else if (userFilter.SortField == SortFieldUser.Username)
                {
                    users = userFilter.SortOrder == Order.Ascending
                    ? users.OrderBy(r => r.UserName).ToList()
                    : users.OrderByDescending(r => r.UserName).ToList();
                }

                // Aplicar la paginación
                users = users.Skip((userFilter.CurrentPage - 1) * userFilter.PageSize).Take(userFilter.PageSize).ToList();

                // Crear el DTO con los usuarios filtrados
                List<ApplicationUserDto> usersList = new();
                foreach (var user in users)
                {
                    List<ApplicationRoleDto> roles = new();
                    var listRoles = new List<string>(await _userManager.GetRolesAsync(user));

                    foreach (var role in listRoles)
                    {
                        var currentRole = await _rolemanager.FindByNameAsync(role);
                        if (currentRole != null)
                        {
                            roles.Add(new ApplicationRoleDto
                            {
                                Id = currentRole.Id,
                                Name = role,
                                ApplicationManagment = Products.ManagementPortal,
                            });
                        }
                    }

                    List<ApplicationUserProductDto> productDtos = new();

                    if (user.Products != null)
                    {
                        foreach (var product in user.Products)
                        {
                            ApplicationUserProductDto productDto = new()
                            {
                                Product = product.Product,
                                ProductDelete = product.ProductDelete,
                            };

                            productDtos.Add(productDto);
                        }

                        ApplicationUserProductDto productDtoMp = new()
                        {
                            Product = Products.ManagementPortal,
                        };

                        productDtos.Add(productDtoMp);
                    }

                    if (!string.IsNullOrEmpty(userFilter.SearchRole))
                    {
                        if (roles.Any(role => role.Name.Contains(userFilter.SearchRole, StringComparison.OrdinalIgnoreCase)))
                        {
                            usersList.Add(new ApplicationUserDto
                            {
                                Id = user.Id,
                                UserName = user.UserName ?? string.Empty,
                                Email = user.Email ?? string.Empty,
                                Name = user.Name,
                                LastName = user.LastName,
                                IsDeleted = user.IsDeleted,
                                LastLoginDate = user.LastLoginDate,
                                Roles = roles,
                                Products = productDtos,
                            });
                        }
                    }
                    else
                    {
                        usersList.Add(new ApplicationUserDto
                        {
                            Id = user.Id,
                            UserName = user.UserName ?? string.Empty,
                            Email = user.Email ?? string.Empty,
                            Name = user.Name,
                            LastName = user.LastName,
                            IsDeleted = user.IsDeleted,
                            LastLoginDate = user.LastLoginDate,
                            Roles = roles,
                            Products = productDtos,
                        });
                    }
                }

                // Retornar los usuarios junto con la información de la paginación
                return Ok(new UserPagination
                {
                    Users = usersList,
                    CurrentPage = userFilter.CurrentPage,
                    PageSize = userFilter.PageSize,
                    TotalItems = totalUsersCount,
                    SearchText = userFilter.SearchText,
                    SearchRole = userFilter.SearchRole ?? string.Empty,
                    State = userFilter.State,
                    Product = userFilter.Product,
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene un usuario dado su username.
        /// </summary>
        /// <param name="username">Username del usuario en Identity.</param>
        /// <returns>Retorna el usuario con el username dado o 404 si no lo encuentra.</returns>
        [HttpGet("username/{username}")]
        [Authorize]
        public async Task<IActionResult> GetUser(string username)
        {
            var userPermission = User.Claims
                           .Where(c => c.Type == ClaimTypes.Role)
                           .Select(c => c.Value)
                           .ToList();

            var userLogin = await _userManager.GetUserAsync(User);

            if (userLogin == null || string.IsNullOrEmpty(userLogin.UserName))
            {
                Log.Error(LogErrorResources.UserLoginNotFound);
                return NotFound(UsersResources.UserNotFound);
            }

            if (!userPermission.Contains(ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.ListUsers]) && !userLogin.UserName.Equals(username))
            {
                Log.Error(string.Format(LogErrorResources.InvalidRole, ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.ListUsers]));
                return Unauthorized(UsersResources.InvalidRole);
            }

            if (string.IsNullOrEmpty(username))
            {
                Log.Error(LogErrorResources.UsernameRequired);
                return BadRequest(UsersResources.UsernameRequired);
            }

            var user = await _userManager.Users.Include(u => u.Products).FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null)
            {
                Log.Error(string.Format(LogErrorResources.UserNotFound, username));
                return NotFound(UsersResources.UserNotFound);
            }

            try
            {
                var userMethod = await _twoFactorAuthService.GetTwoFactorMethod(user.Id);

                var rolesDto = new List<ApplicationRoleDto>();

                var listRoles = await _userManager.GetRolesAsync(user);
                foreach (var role in listRoles)
                {
                    var currentRole = await _rolemanager.FindByNameAsync(role);
                    if (currentRole != null)
                    {
                        rolesDto.Add(new ApplicationRoleDto
                        {
                            Id = currentRole.Id,
                            Name = role,
                            ApplicationManagment = Products.ManagementPortal,
                        });
                    }
                }

                ApplicationUserDto userDto = new()
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    Name = user.Name,
                    LastName = user.LastName,
                    TwoFactorMethod = userMethod
                    .Where(um => um != null)
                    .Select(um => um!.Method)
                    .ToList(),
                    Image = user.ProfilePhoto,
                    IsDeleted = user.IsDeleted,
                };

                if (user.Products != null)
                {
                    foreach (var product in user.Products)
                    {
                        if (product?.Product != null)
                        {
                            string productName = product.Product.ToString();
                            var httpClient = _httpClientFactory.CreateClient(productName);

                            var response = await httpClient.GetAsync($"api/User/{product.UserProductId}");

                            if (response.IsSuccessStatusCode)
                            {
                                var userResponse = await response.Content.ReadFromJsonAsync<ManagementPortal.Shared.Dtos.Apis.UserGet>();

                                if (userResponse?.Roles != null)
                                {
                                    foreach (var role in userResponse.Roles)
                                    {
                                        rolesDto.Add(new()
                                        {
                                            ApplicationManagment = product.Product,
                                            Id = role.Id,
                                            Name = role.Name,
                                        });
                                    }
                                }

                                // Se crea un nuevo ApplicationUserProductDto para cada producto
                                ApplicationUserProductDto productUser = new()
                                {
                                    Product = product.Product,
                                    ProductDelete = product.ProductDelete,
                                    ExternalIds = userResponse?.ExternalIds,
                                    Active = !product.ProductDelete,
                                    TenantId = userResponse?.TenantId,
                                    Status = userResponse?.Status ?? new StatusDto(),
                                    LastVersion = userResponse?.LastVersion,
                                    Signature = userResponse?.Signature,
                                };

                                userDto.Products.Add(productUser);
                            }
                            else
                            { // TODO: ESTO ESTA MAL HAY QUE CORREGIRLO UNA PETICIÓN FALLA PERO LAS OTRAS OK ENTONCES MUESTRA TODO MENOS LA FALLA Y MSG DE ERROR.
                                string errorData = await response.Content.ReadAsStringAsync();
                                string errorMessage = string.Format(RolesResources.ProductError, productName, errorData);
                                Log.Error(errorMessage);
                                return StatusCode((int)response.StatusCode, errorMessage);
                            }
                        }
                    }
                }

                userDto.Roles = rolesDto;

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Permite al usuario logueado subir una foto de perfil.
        /// </summary>
        /// <param name="imagen">Archivo de imagen que el usuario desea subir.</param>
        /// <returns>Retorna 200 si la imagen fue subida y asignada correctamente, de lo contrario 400.</returns>
        [HttpPost("UploadProfilePhoto")]
        [Consumes("multipart/form-data")]
        [Authorize]
        public async Task<IActionResult> UploadProfilePhoto([FromForm] Imagen imagen)
        {
            var userPermission = User.Claims
                           .Where(c => c.Type == ClaimTypes.Role)
                           .Select(c => c.Value)
                           .ToList();
            if (!userPermission.Contains(ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.EditUser]))
            {
                Log.Error(string.Format(LogErrorResources.InvalidRole, ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.EditUser]));
                return Unauthorized(UsersResources.InvalidRole);
            }

            // Obtener el usuario logueado
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                Log.Error(LogErrorResources.UserLoginNotFound);
                return NotFound(UsersResources.UserNotFound);
            }

            // Validar el archivo
            if (imagen.File == null)
            {
                Log.Error(string.Format(LogErrorResources.FileNotValid, user.UserName));
                return BadRequest(UsersResources.FileNotValid);
            }

            // Leer el archivo en un array de bytes
            using (var memoryStream = new MemoryStream())
            {
                await imagen.File.CopyToAsync(memoryStream);
                user.ProfilePhoto = memoryStream.ToArray(); // Asignar la imagen como byte[]
            }

            // Actualizar el usuario con la nueva imagen de perfil
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                Log.Error(string.Format(LogErrorResources.ImageNotUpdated, user.UserName));
                return BadRequest(UsersResources.ImageNotUpdated);
            }

            // Retornar un mensaje de óxito
            return Ok(UsersResources.ImageUpdateSuccess);
        }

        /// <summary>
        /// Obtiene la foto de perfil de un usuario dado su username.
        /// </summary>
        /// <param name="username">El username del usuario.</param>
        /// <returns>La imagen de perfil como archivo, o un error si no se encuentra.</returns>
        [HttpGet("GetProfilePhoto/{username}")]
        [Authorize]
        public async Task<IActionResult> GetProfilePhoto(string username)
        {
            var userPermission = User.Claims
                           .Where(c => c.Type == ClaimTypes.Role)
                           .Select(c => c.Value)
                           .ToList();

            var userLogin = await _userManager.GetUserAsync(User);

            if (userLogin == null || string.IsNullOrEmpty(userLogin.UserName))
            {
                Log.Error(LogErrorResources.UserLoginNotFound);
                return NotFound(UsersResources.UserNotFound);
            }

            if (!userPermission.Contains(ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.ListUsers]) && !userLogin.UserName.Equals(username))
            {
                Log.Error(string.Format(LogErrorResources.InvalidRole, ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.ListUsers]));
                return Unauthorized(UsersResources.InvalidRole);
            }

            // Buscar al usuario por username
            var user = await _userManager.FindByNameAsync(username);
            if (user == null || user.ProfilePhoto == null)
            {
                Log.Error(string.Format(LogErrorResources.ImageNotUpdated, username));
                return NotFound(UsersResources.ImageNotUpdated);
            }

            // Retornar la imagen como un archivo (asume que la imagen es JPEG, ajusta el tipo MIME si es necesario)
            return File(user.ProfilePhoto, "image/jpeg"); // Cambia "image/jpeg" si es de otro formato como "image/png"
        }

        /// <summary>
        /// Permite al usuario logueado cambiar su contraseña.
        /// </summary>
        /// <param name="model">Se incluyen los atributos necesarios CurrentPassword, NewPassword y ConfrimNewPasswrod.</param>
        /// <returns>Retorna 200 si la nueva contraseña fue actualizada correctamente, de lo contrario 400.</returns>
        [HttpPut("changePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Obtener el usuario actualmente autenticado
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                Log.Error(string.Format(LogErrorResources.UserNotFound, model.Username));
                return NotFound(UsersResources.UserNotFound);
            }

            // Cambiar la contraseña
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                // Si se cambia la contraseña para un usuario nuevo se le registra como primer login
                if (user.LastLoginDate == null)
                {
                    user.LastLoginDate = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);
                }

                return Ok(UsersResources.PasswordChanged);
            }

            string errors = string.Empty;

            // Devolver los errores si algo sale mal
            foreach (var error in result.Errors)
            {
                errors = errors + " " + error.Description;
                ModelState.AddModelError(string.Empty, error.Description);
            }

            Log.Error($"Error: {errors}");
            return BadRequest(ModelState);
        }

        /// <summary>
        /// Permite obtener el acceso a un producto desde el MP.
        /// </summary>
        /// <param name="product">Producto al que el usuario desea logearse.</param>
        /// <returns>Mensaje de OK en caso de que sea correcto.</returns>
        [HttpGet("access-product/{product}")]
        [Authorize]
        public async Task<IActionResult> GetProductAccess(string product)
        {
            var username = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var applicationUser = _userManager.Users
                                .Include(u => u.Products)
                                .FirstOrDefault(u => u.UserName == username);

            // Si el usuario no existe
            if (applicationUser == null)
            {
                Log.Error(LogErrorResources.UserLoginNotFound);
                return NotFound(UsersResources.UserNotFound);
            }

            // Si el usuario no tiene productos, entonces no esta autorizado a este en particular
            if (applicationUser.Products == null)
            {
                Log.Error(string.Format(LogErrorResources.NoAccessProduct, applicationUser.UserName, product));
                return Unauthorized(UsersResources.NoAccessProduct);
            }

            var hasAccess = applicationUser.Products.Any(p =>
                             p.Product.ToString().Equals(product, StringComparison.OrdinalIgnoreCase) &&
                             !p.ProductDelete); // Chequeo que tenga acceso al producto

            // Si no tiene el producto, no esta autorizado
            if (!hasAccess)
            {
                Log.Error(string.Format(LogErrorResources.NoAccessProduct, applicationUser.UserName, product));
                return Unauthorized(UsersResources.NoAccessProduct);
            }

            var userProdId = string.Empty;

            foreach (var prodUser in applicationUser.Products)
            {
                if (string.Equals(prodUser.Product.ToString(), product))
                {
                    userProdId = prodUser.UserProductId;
                }
            }

            var reqHMAC = string.Empty;

            try
            {
                var sharedKey = string.Empty;
                var products = _configuration.GetSection("Products").Get<List<ProductSettings>>();
                if (products != null)
                {
                    foreach (var prod in products)
                    {
                        if (string.Equals(prod.Name, product))
                        {
                             sharedKey = prod.SharedKey;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(sharedKey))
                {
                    using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(sharedKey)))
                    {
                        if (!string.IsNullOrEmpty(userProdId))
                        {
                            var messageBytes = Encoding.UTF8.GetBytes(userProdId);
                            var hash = hmac.ComputeHash(messageBytes);

                            reqHMAC = Convert.ToBase64String(hash);
                        }
                    }
                }
            }
            catch
            {
                Log.Error("Invalid request signature");
                return Unauthorized("Invalid request signature");
            }

            var httpClient = _httpClientFactory.CreateClient(product);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("HMAC", reqHMAC);

            var responseOtp = await httpClient.PostAsync($"api/access?message={userProdId}", null);
            if (!responseOtp.IsSuccessStatusCode)
            {
                Log.Error("Failed to communicate with product backend");
                return StatusCode((int)responseOtp.StatusCode, "Failed to communicate with product backend");
            }

            var otpString = await responseOtp.Content.ReadAsStringAsync();

            return Ok(new { Otp = otpString, userProdudctId = userProdId });
        }

        /// <summary>
        /// Función auxiliar para editar los campos básicos y roles de un usuario, tanto en el portal como en los productos en que esté asignado.
        /// </summary>
        /// <param name="userDTO"> Datos del usuario a modificar (ver UserCompleteEditDto). </param>
        /// <exception cref="Exception"> Exception genérica, devolver en consulta http el error 500. </exception>
        /// <exception cref="InvalidOperationException"> Error por campos no validos, devolver un badRequest. </exception>
        private async Task EditUserProfile(UserProfileEditDto userDTO)
        {
            var user = await _userManager.Users
                .Include(u => u.Products)
                .FirstOrDefaultAsync(u => u.UserName == userDTO.UserName)
                ?? throw new InvalidOperationException(UsersResources.UserNotFound);

            if (user.IsDeleted)
            {
                Log.Error(string.Format(LogErrorResources.UserIsDeleted, user.UserName));
                throw new InvalidOperationException(UsersResources.UserIsDeleted);
            }

            var userEmail = await _userManager.FindByEmailAsync(userDTO.Email);
            if (userEmail != null && userEmail != user)
            {
                Log.Error(string.Format(LogErrorResources.UserEmailAlreadyExists, user.Email));
                throw new InvalidOperationException(UsersResources.EmailExists);
            }

            user.Name = userDTO.Name;
            user.LastName = userDTO.LastName;
            user.Email = userDTO.Email;

            byte[]? profilePhoto = null;
            if (userDTO.Image != null)
            {
                profilePhoto = userDTO.Image;

                // si image es un array vacio quiere decir que hay que borrar la imagen.
                if (userDTO.Image.Length > 0)
                {
                    user.ProfilePhoto = userDTO.Image;
                }
                else
                {
                    user.ProfilePhoto = null;
                }
            }

            if (userDTO is UserCompleteEditDto completeEditDto)
            {
                // Modificar los roles de MP
                var assignToUser = completeEditDto.RolesMP;
                if (assignToUser != null)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);

                    var removeResult = await _userManager.RemoveFromRolesAsync(user, userRoles);
                    if (!removeResult.Succeeded)
                    {
                        throw new Exception(UsersResources.RemoveRolesFromUserError);
                    }

                    var addResult = await _userManager.AddToRolesAsync(user, assignToUser);
                    if (!addResult.Succeeded)
                    {
                        throw new Exception(UsersResources.AddRolesUserError);
                    }
                }

                // Modificar los datos en otros productos
                var userProducts = completeEditDto.Products;
                foreach (var product in userProducts)
                {
                    // Obtener el id del usuario en este producto
                    var userProductId = user.Products?.FirstOrDefault(prod => prod.Product == product.Product)?.UserProductId ?? string.Empty;

                    UserPutDto updateUserDto = new()
                    {
                        Id = userProductId,
                        Name = completeEditDto.Name,
                        LastName = completeEditDto.LastName,
                        UserName = completeEditDto.UserName,
                        Email = completeEditDto.Email,
                        NewPassword = null,
                        ConfirmPassword = null,
                        OldPassword = null,
                        Picture = completeEditDto.Image != null ? Convert.ToBase64String(completeEditDto.Image) : string.Empty,
                        ExternalIds = product.ExternalIds ?? string.Empty,
                        Signature = string.Empty,
                        StatusDto = new(),
                        Roles = new(),
                        RoleIds = product.RolesId,
                    };

                    string productName = product.Product.ToString();
                    var httpClient = _httpClientFactory.CreateClient(productName);

                    var response = await httpClient.PutAsJsonAsync("api/User", updateUserDto);

                    if (!response.IsSuccessStatusCode)
                    {
                        string errorData = await response.Content.ReadAsStringAsync();
                        string errorMessage = string.Format(RolesResources.ProductError, productName, errorData);
                        throw new Exception(errorMessage);
                    }
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new Exception(UsersResources.ModifyFailed);
            }
        }

        /// <summary>
        /// Función auxiliar para asignar o desasignar productos a un usuario.
        /// </summary>
        /// <param name="username"> Nombre de usuario del usuario a modificar. </param>
        /// <param name="newProductsOfUser"> Es la nueva lista de productos asignados al usuario. </param>
        /// <exception cref="Exception"> Exception genérica, devolver en consulta http el error 500. </exception>
        /// <exception cref="InvalidOperationException"> Error por campos no validos, devolver un badRequest. </exception>
        private async Task<List<AssignProductMessage>> AsignProducts(string username, List<Products> newProductsOfUser)
        {
            var userPermission = User.Claims
    .Where(c => c.Type == ClaimTypes.Role)
    .Select(c => c.Value)
    .ToList();

            var applicationUser = await _userManager.Users
                    .Include(u => u.Products)
                    .FirstOrDefaultAsync(u => u.UserName == username)
                    ?? throw new InvalidOperationException(UsersResources.UserNotFound);

            if (applicationUser.Products == null)
            {
                applicationUser.Products = new List<ApplicationUserProduct>() { new() { Product = Products.ManagementPortal, UserProductId = username }, };
            }

            // Luego aplico el filtro de productos eliminados
            var userProducts = applicationUser.Products
                .Where(up => !up.ProductDelete) // Solo toma los productos que no fueron eliminados
                .Select(up => up.Product)
                .ToList();

            IEnumerable<Products> products = Enum.GetValues(typeof(Products))
                .Cast<Products>()
                .Where(p => p != Products.OthersProducts && p != Products.ManagementPortal)
                .ToList();

            List<Products> productsToDelete = userProducts.Except(newProductsOfUser).ToList();
            List<Products> productsToAdd = newProductsOfUser.Except(userProducts).ToList();

            UpdateUserProductsResponse updateUserProductsResponse = new();
            updateUserProductsResponse.SuccessProducts = userProducts
                .Concat(products)
                .Distinct()
                .Except(productsToDelete)
                .Except(productsToAdd)
                .ToList();

            // Listas para acumular los resultados
            var messagePost = new List<AssignProductMessage>();

            // Agrego los productos nuevos
            foreach (var product in productsToAdd)
            {
                if (product == Products.ManagementPortal)
                {
                    continue;
                }

                var existingProduct = applicationUser.Products.FirstOrDefault(up => up.Product == product);

                if (existingProduct != null && existingProduct.ProductDelete)
                {
                    // El producto fue previamente eliminado (ProductDelete == true)
                    messagePost.Add(new AssignProductMessage
                    {
                        Product = product.ToString(),
                        Message = $"{UsersResources.ProductDeleteBefore}.",
                        Result = false,
                    });
                    continue;
                }

                UserPostDto newUserDto = new()
                {
                    UserName = applicationUser.UserName ?? string.Empty,
                    Email = applicationUser.Email ?? string.Empty,
                    Name = applicationUser.Name,
                    LastName = applicationUser.LastName,
                    Password = applicationUser.PasswordHash ?? string.Empty,
                    ConfirmPassword = applicationUser.PasswordHash ?? string.Empty,
                    Picture = (applicationUser.ProfilePhoto == null) ? string.Empty : Encoding.ASCII.GetString(applicationUser.ProfilePhoto),
                    RoleIds = new(),
                    ExternalIds = string.Empty,
                };

                try
                {
                    string productName = product.ToString();
                    var httpClient = _httpClientFactory.CreateClient(productName);

                    var response = await httpClient.PostAsJsonAsync("api/User", newUserDto);

                    if (response.IsSuccessStatusCode)
                    {
                        var idResponse = await response.Content.ReadAsStringAsync();
                        var newProduct = new ApplicationUserProduct
                        {
                            Product = product,
                            UserProductId = idResponse,
                        };
                        applicationUser.Products.Add(newProduct);

                        var result = await _userManager.UpdateAsync(applicationUser);

                        if (result.Succeeded)
                        {
                            messagePost.Add(new AssignProductMessage
                            {
                                Product = product.ToString(),
                                Message = $"{UsersResources.AddSuccess}.",
                                Result = true,
                            });
                        }
                        else
                        {
                            messagePost.Add(new AssignProductMessage
                            {
                                Product = product.ToString(),
                                Message = $"{UsersResources.NotUpdateDB}.",
                                Result = false,
                            });
                        }
                    }
                    else
                    {
                        messagePost.Add(new AssignProductMessage
                        {
                            Product = product.ToString(),
                            Message = $"{UsersResources.FailAPIrequest}: {response.StatusCode}",
                            Result = false,
                        });
                    }
                }
                catch (Exception ex)
                {
                    messagePost.Add(new AssignProductMessage
                    {
                        Product = product.ToString(),
                        Message = $"{UsersResources.Produced_error}: {ex.Message}",
                        Result = false,
                    });
                }
            }

            // Elimino productos que fueron dados de baja
            foreach (var product in productsToDelete)
            {
                if (product == Products.ManagementPortal)
                {
                    continue;
                }

                try
                {
                    string productName = product.ToString();
                    var httpClient = _httpClientFactory.CreateClient(productName);

                    var userProduct = applicationUser.Products.FirstOrDefault(up => up.Product == product);
                    if (userProduct != null && !userProduct.ProductDelete)
                    {
                        var response = await httpClient.DeleteAsync($"api/User/{userProduct.UserProductId}");

                        if (response.IsSuccessStatusCode)
                        {
                            userProduct.ProductDelete = true;
                            var result = await _userManager.UpdateAsync(applicationUser);
                            if (result.Succeeded)
                            {
                                messagePost.Add(new AssignProductMessage
                                {
                                    Product = product.ToString(),
                                    Message = $"{UsersResources.DeleteSucces}.",
                                    Result = true,
                                });
                            }
                            else
                            {
                                messagePost.Add(new AssignProductMessage
                                {
                                    Product = product.ToString(),
                                    Message = $"{UsersResources.NotUpdateDB}.",
                                    Result = false,
                                });
                            }
                        }
                        else
                        {
                            messagePost.Add(new AssignProductMessage
                            {
                                Product = product.ToString(),
                                Message = $"{UsersResources.FailAPIrequest}: {response.StatusCode}",
                                Result = false,
                            });
                        }
                    }
                    else
                    {
                        messagePost.Add(new AssignProductMessage
                        {
                            Product = product.ToString(),
                            Message = $"{UsersResources.ProductToDeleteNotFound}.",
                            Result = false,
                        });
                    }
                }
                catch (Exception ex)
                {
                    messagePost.Add(new AssignProductMessage
                    {
                        Product = product.ToString(),
                        Message = $"{UsersResources.Produced_error}: {ex.Message}",
                        Result = false,
                    });
                }
            }

            // Devuelvo los resultados
            return messagePost;
        }
    }
}