using System.Security.Claims;
using ManagementPortal.Server.Context;
using ManagementPortal.Shared.Constants;
using ManagementPortal.Shared.Dtos;
using ManagementPortal.Shared.Models;
using ManagementPortal.Shared.Resources.Server;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManagementPortal.Server.Controllers
{
    /// <summary>
    /// Controller que expone endpoints para las funcionalidades de Mensajeria.
    /// Justificación: Se decide agrupar en un controlador las funcionalidades de Mensajeria.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MessagingController : ControllerBase
    {
        private readonly ApplicationContext _dbContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingController"/> class.
        /// </summary>
        /// <param name="dbContext">EF para manejar la base de datos de la aplicacion.</param>
        /// <param name="httpClientFactory">Factory para crear los httpClient.</param>
        /// <param name="userManager">Manejador de Identity para usuarios.</param>
        public MessagingController(ApplicationContext dbContext, IHttpClientFactory httpClientFactory, UserManager<ApplicationUser> userManager)
        {
            this._dbContext = dbContext;
            this._httpClientFactory = httpClientFactory;
            this._userManager = userManager;
        }

        /// <summary>
        /// Lista todos los usuarios a los que se les puede enviar un mensaje.
        /// </summary>
        /// <param name="searchText">Buscador por userName.</param>
        /// <param name="pageSize">Cantidad de usuarios a listar.</param>
        /// <returns>Retorna el listado de usuarios a los que se les puede enviar un mensaje.</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ListUsers(string searchText, int pageSize = 5)
        {
            var userPermission = User.Claims
                                .Where(c => c.Type == ClaimTypes.Role)
                                .Select(c => c.Value)
                                .ToList();

            if (!userPermission.Contains(ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.Messaging]))
            {
                return Unauthorized(UsersResources.InvalidRole);
            }

            var loginUser = await _userManager.GetUserAsync(User);

            if (loginUser == null)
            {
                return NotFound(UsersResources.UserNotFound);
            }

            try
            {
                var usersQuery = _userManager.Users.Where(u => !u.IsDeleted && u.UserName != loginUser.UserName).AsQueryable();

                if (!string.IsNullOrEmpty(searchText))
                {
                    searchText = searchText.ToLower();
                    usersQuery = usersQuery.Where(a => (a.UserName != null && a.UserName.ToLower().Contains(searchText)) ||
                        (a.Email != null && a.Email.ToLower().Contains(searchText)));
                }

                var users = await usersQuery.ToListAsync();

                List<ApplicationUserDto> usersList = new();
                foreach (var user in users)
                {
                    var listRoles = new List<string>(await _userManager.GetRolesAsync(user));
                    bool tienePermisosMensajeria = false;
                    if (listRoles != null)
                    {
                        foreach (var role in listRoles)
                        {
                            var currentRole = await _dbContext.Roles
                                .Include(r => r.Permissions)
                                .FirstOrDefaultAsync(r => r.Name == role);

                            if (currentRole != null && currentRole.Permissions != null)
                            {
                                tienePermisosMensajeria = currentRole.Permissions.Any(p => p.Name == ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.Messaging]);
                                if (tienePermisosMensajeria)
                                {
                                    break;
                                }
                            }
                        }
                    }

                    if (tienePermisosMensajeria)
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
                            Roles = new List<ApplicationRoleDto>(),
                            Products = new List<ApplicationUserProductDto>(),
                        });
                    }
                }

                var totalUsersCount = users.Count();

                usersList = usersList.Take(pageSize).ToList();

                return Ok(new UserPagination
                {
                    Users = usersList,
                    CurrentPage = 1,
                    PageSize = pageSize,
                    TotalItems = totalUsersCount,
                    SearchText = searchText,
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Lista todos los usuarios a los que se les puede añadir a un grupo en mensajeria.
        /// </summary>
        /// <param name="searchText">Buscador por userName.</param>
        /// <param name="pageSize">Cantidad de usuarios a listar.</param>
        /// <returns>Retorna el listado de usuarios a los que se les puede añadir a un grupo en mensajeria.</returns>
        [HttpGet("Groups")]
        [Authorize]
        public async Task<IActionResult> ListUsersForGroups(string searchText, int pageSize = 5)
        {
            var userPermission = User.Claims
                                .Where(c => c.Type == ClaimTypes.Role)
                                .Select(c => c.Value)
                                .ToList();

            if (!userPermission.Contains(ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.Messaging]))
            {
                return Unauthorized(UsersResources.InvalidRole);
            }

            var loginUser = await _userManager.GetUserAsync(User);

            if (loginUser == null)
            {
                return NotFound(UsersResources.UserNotFound);
            }

            try
            {
                var usersQuery = _userManager.Users.Where(u => !u.IsDeleted && u.UserName != loginUser.UserName).AsQueryable();

                if (!string.IsNullOrEmpty(searchText))
                {
                    searchText = searchText.ToLower();
                    usersQuery = usersQuery.Where(a => (a.UserName != null && a.UserName.ToLower().Contains(searchText)) ||
                        (a.Email != null && a.Email.ToLower().Contains(searchText)));
                }

                var users = await usersQuery.ToListAsync();

                List<ApplicationUserDto> usersList = new();
                foreach (var user in users)
                {
                    var listRoles = new List<string>(await _userManager.GetRolesAsync(user));
                    bool tienePermisosMensajeria = false;
                    bool tienePermisosGrupos = false;
                    if (listRoles != null)
                    {
                        foreach (var role in listRoles)
                        {
                            var currentRole = await _dbContext.Roles
                                .Include(r => r.Permissions)
                                .FirstOrDefaultAsync(r => r.Name == role);

                            if (currentRole != null && currentRole.Permissions != null)
                            {
                                if (!tienePermisosMensajeria)
                                {
                                    tienePermisosMensajeria = currentRole.Permissions.Any(p => p.Name == ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.Messaging]);
                                }

                                if (!tienePermisosGrupos)
                                {
                                    tienePermisosGrupos = currentRole.Permissions.Any(p => p.Name == ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.GroupMessaging]);
                                }

                                if (tienePermisosMensajeria && tienePermisosGrupos)
                                {
                                    break;
                                }
                            }
                        }
                    }

                    if (tienePermisosMensajeria && tienePermisosGrupos)
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
                            Roles = new List<ApplicationRoleDto>(),
                            Products = new List<ApplicationUserProductDto>(),
                        });
                    }
                }

                var totalUsersCount = users.Count();

                usersList = usersList.Take(pageSize).ToList();

                return Ok(new UserPagination
                {
                    Users = usersList,
                    CurrentPage = 1,
                    PageSize = pageSize,
                    TotalItems = totalUsersCount,
                    SearchText = searchText,
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}