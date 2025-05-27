using System.Security.Claims;
using ManagementPortal.Server.Context;
using ManagementPortal.Shared.Constants;
using ManagementPortal.Shared.Dtos;
using ManagementPortal.Shared.Dtos.Apis;
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
    /// Controller que expone endpoints para las funcionalidades de roles y permisos.
    /// Justifacion: Se decide agrupar en un controller las funcionalidades de roles y permisos.
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly ApplicationContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="RolesController"/> class.
        /// </summary>
        /// <param name="dbContext">EF para manejar la base de datos de la aplicacion.</param>
        /// <param name="userManager">Manejador de Identity para Usuarios.</param>
        /// <param name="roleManager">Manejador de Identity para Roles.</param>
        /// <param name="httpClientFactory">Factory para crear los httpClient.</param>
        public RolesController(
            ApplicationContext dbContext,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IHttpClientFactory httpClientFactory)
        {
            this._dbContext = dbContext;
            this._userManager = userManager;
            this._roleManager = roleManager;
            this._httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Metodo para dar agregar un rol.
        /// </summary>
        /// <param name="product">El producto asociado al rol.</param>
        /// <param name="role">Se incluyen los atributos necesarios para creear un rol (ver ApplicationRoleDto).</param>
        /// <returns>retorna codigo 200 en caso de que se agregue el rol correctamente.</returns>
        [HttpPost("{product}")]
        public async Task<IActionResult> CreateRole(Products product, [FromBody] ApplicationRoleDto role)
        {
            var userRoles = User.Claims
                            .Where(c => c.Type == ClaimTypes.Role)
                            .Select(c => c.Value)
                            .ToList();

            if (!userRoles.Contains(ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.CreateRole]))
            {
                Log.Error(string.Format(LogErrorResources.InvalidRole, ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.CreateRole]));
                return Unauthorized(RolesResources.InvalidRole);
            }

            if (string.IsNullOrEmpty(role.Name))
            {
                Log.Error(string.Format(LogErrorResources.RoleNameRequired, JsonConvert.SerializeObject(role)));
                return BadRequest(RolesResources.RoleNameRequired);
            }

            if (!AtLeastOnePermissionInRole(product, role))
            {
                Log.Error(string.Format(LogErrorResources.RolePermissionRequired, JsonConvert.SerializeObject(role)));
                return BadRequest(RolesResources.RolePermissionRequired);
            }

            if (product == Products.ManagementPortal)
            {
                var existingRole = await _roleManager.FindByNameAsync(role.Name);
                if (existingRole != null)
                {
                    Log.Error(string.Format(LogErrorResources.RoleNameExists, existingRole));
                    return Conflict(RolesResources.RoleNameExists);
                }

                try
                {
                    var existingPermissions = _dbContext.ApplicationPermissions
                        .Where(
                                aplicationPermission => role.Permissions != null &&
                                role.Permissions.Select(rp => rp.Id).Contains(aplicationPermission.Id))
                        .ToList();

                    var newRole = new ApplicationRole
                    {
                        Name = role.Name,
                        Permissions = existingPermissions,
                    };
                    await _roleManager.CreateAsync(newRole);

                    return Ok();
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    return BadRequest(ex.Message);
                }
            }
            else
            {
                List<PermissionGetDto> apiPermissions = new();
                if (role.Permissions != null)
                {
                    foreach (var apiPermission in role.Permissions)
                    {
                        apiPermissions.Add(new PermissionGetDto
                        {
                            Id = apiPermission.Id,
                            Name = apiPermission.Name,
                        });
                    }
                }

                RoleGetDto apiRole = new()
                {
                    Id = role.Id,
                    Name = role.Name,
                    Permissions = apiPermissions,
                };

                string productName = product.ToString();
                var httpClient = _httpClientFactory.CreateClient(productName);
                try
                {
                    HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/Roles", apiRole);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseData = await response.Content.ReadAsStringAsync();
                        return Ok();
                    }
                    else
                    {
                        string errorData = await response.Content.ReadAsStringAsync();
                        string errorMessage = string.Format(RolesResources.ProductError, productName, errorData);
                        Log.Error(errorMessage);
                        return StatusCode((int)response.StatusCode, errorMessage);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    return StatusCode(500, ex.Message);
                }
            }
        }

        /// <summary>
        /// Obtiene un rol específico basado en el nombre del rol y el producto.
        /// </summary>
        /// <param name="roleName">El nombre del rol a buscar.</param>
        /// <param name="product">El producto asociado al rol.</param>
        /// <returns>Retorna el rol encontrado o un código 404 si no se encuentra.</returns>
        [HttpGet("name/{product}/{roleName}")]
        public async Task<IActionResult> GetRole(string roleName, Products product)
        {
            ApplicationRoleDto roleDto = new();
            if (product == Products.ManagementPortal)
            {
                var role = await _dbContext.ApplicationRoles
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Name == roleName);

                if (role == null)
                {
                    Log.Error(string.Format(LogErrorResources.RoleNotFound, roleName));
                    return NotFound();
                }
                else
                {
                    roleDto = new ApplicationRoleDto
                    {
                        Name = role.Name ?? string.Empty,
                        Id = role.Id,
                        Permissions = role?.Permissions?.Select(p => new ApplicationPermissionDto
                        {
                            Name = p.Name,
                            Id = p.Id,
                        }).ToList(),
                    };
                }

                return Ok(roleDto);
            }
            else
            {
                string productName = product.ToString();
                var httpClient = _httpClientFactory.CreateClient(productName);
                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync($"api/Roles/{roleName}");
                    if (response.IsSuccessStatusCode)
                    {
                        var apiRole = await response.Content.ReadFromJsonAsync<RoleGetDto>();
                        if (apiRole != null)
                        {
                            roleDto = new ApplicationRoleDto
                            {
                                Name = apiRole.Name,
                                Id = apiRole.Id,
                                ApplicationManagment = product,
                                Permissions = apiRole.Permissions?.Select(p => new ApplicationPermissionDto
                                {
                                    Name = p.Name,
                                    Id = p.Id,
                                }).ToList(),
                            };
                            return Ok(roleDto);
                        }
                        else
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            Log.Error(content);
                            return NotFound();
                        }
                    }
                    else
                    {
                        string errorData = await response.Content.ReadAsStringAsync();
                        string errorMessage = string.Format(RolesResources.ProductError, productName, errorData);
                        Log.Error(errorMessage);
                        return StatusCode((int)response.StatusCode, errorMessage);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    return StatusCode(500, ex.Message);
                }
            }
        }

        /// <summary>
        /// Listado de roles de todos los productos con filtros y paginación.
        /// </summary>
        /// <param name="roleFilter"> Filtros a aplicar.</param>
        /// <returns>Retorna el listado de roles según los filtros de buscado y paginado.</returns>
        [HttpGet]
        public async Task<IActionResult> GetRoles([FromQuery] FilterRoleDto roleFilter)
        {
            var userRoles = User.Claims
                            .Where(c => c.Type == ClaimTypes.Role)
                            .Select(c => c.Value)
                            .ToList();

            if (!userRoles.Contains(ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.ListRoles]) && !userRoles.Contains(ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.EditUser]))
            {
                Log.Error(string.Format(LogErrorResources.InvalidRole, ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.ListRoles]));
                return Unauthorized(RolesResources.InvalidRole);
            }

            try
            {
                List<ApplicationRoleDto> rolesDto = new();
                decimal rolesCount = 0;

                if (roleFilter.Product == Products.ManagementPortal)
                {
                    var rolesQuery = _dbContext.ApplicationRoles
                    .Include(e => e.Permissions)
                    .Where(r => (r.Name != null ? r.Name : string.Empty).ToLower().Contains(roleFilter.SearchText.ToLower()));

                    var roles = await rolesQuery.ToListAsync();

                    if (roleFilter.SearchPermission != 0)
                    {
                        roles = roles
                            .Where(r => r.Permissions != null && r.Permissions.Select(p => p.Id).ToList().Contains(roleFilter.SearchPermission))
                            .ToList();
                    }

                    rolesCount = roles.Count();

                    // Ordenar por 'columna' y 'sort'
                    if (roleFilter.SortField == SortFieldRole.Name)
                    {
                        roles = roleFilter.SortOrder == Order.Ascending
                        ? roles.OrderBy(r => r.Name).ToList()
                        : roles.OrderByDescending(r => r.Name).ToList();
                    }
                    else if (roleFilter.SortField == SortFieldRole.Id)
                    {
                        roles = roleFilter.SortOrder == Order.Ascending
                        ? roles.OrderBy(r => r.Id).ToList()
                        : roles.OrderByDescending(r => r.Id).ToList();
                    }

                    roles = roles.Skip((roleFilter.CurrentPage - 1) * roleFilter.PageSize).Take(roleFilter.PageSize).ToList();

                    rolesDto = new List<ApplicationRoleDto>();
                    foreach (ApplicationRole role in roles)
                    {
                        rolesDto.Add(new ApplicationRoleDto
                        {
                            Id = role.Id,
                            Name = role.Name ?? string.Empty,
                            Permissions = role.Permissions?.Select(permission => new ApplicationPermissionDto
                            {
                                Id = permission.Id,
                                Name = permission.Name,
                                ApplicationManagment = 0,
                            }).ToList() ?? new List<ApplicationPermissionDto>(),
                        });
                    }
                }
                else
                {
                    string productName = roleFilter.Product.ToString();
                    var httpClient = _httpClientFactory.CreateClient(productName);
                    try
                    {
                        var requestUri = $"api/Roles?page={roleFilter.CurrentPage}&pageSize={roleFilter.PageSize}&query={roleFilter.SearchText}&sort={roleFilter.SortOrder.ToString()}&columna={roleFilter.SortField.ToString()}";
                        HttpResponseMessage response = await httpClient.GetAsync(requestUri);
                        if (response.IsSuccessStatusCode)
                        {
                            var responseContent = await response.Content.ReadFromJsonAsync<RoleApiPagination>();

                            if (responseContent?.Results != null)
                            {
                                rolesDto = responseContent.Results.Select(r => new ApplicationRoleDto
                                {
                                    Id = r.Id,
                                    Name = r.Name,
                                    ApplicationManagment = roleFilter.Product,
                                    Permissions = r.Permissions?.Select(p => new ApplicationPermissionDto
                                    {
                                        Id = p.Id,
                                        Name = p.Name,
                                        ApplicationManagment = 0,
                                    }).ToList() ?? new List<ApplicationPermissionDto>(),
                                }).ToList();
                                rolesCount = responseContent.TotalItems;
                            }
                        }
                        else
                        {
                            string errorData = await response.Content.ReadAsStringAsync();
                            string errorMessage = string.Format(RolesResources.ProductError, productName, errorData);
                            Log.Error(errorMessage);
                            return StatusCode((int)response.StatusCode, errorMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message);
                        return StatusCode(500, ex.Message);
                    }
                }

                return Ok(new Shared.Dtos.RolePagination
                {
                    Roles = rolesDto,
                    CurrentPage = roleFilter.CurrentPage,
                    PageSize = roleFilter.PageSize,
                    TotalItems = (int)rolesCount,
                    SearchText = roleFilter.SearchText,
                    SearchPermission = roleFilter.SearchPermission,
                    SortOrder = roleFilter.SortOrder,
                    Product = roleFilter.Product,
                    SortField = roleFilter.SortField,
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Operación para editar un rol.
        /// </summary>
        /// <param name="idRole">Id del rol que se desea actualizar.</param>
        /// <param name="roleEdit">Atributos para la edición.</param>
        /// <param name="idProduct">El id del producto al que pertenece el rol.</param>
        /// <returns>Se retorna el id del rol actualizado.</returns>
        [HttpPut("{idProduct}/{idRole}")]
        public async Task<IActionResult> UpdateRole(string idRole, [FromBody] ApplicationRoleDto roleEdit, Products idProduct)
        {
            var product = idProduct;
            var id = idRole;
            var userRoles = User.Claims
                               .Where(c => c.Type == ClaimTypes.Role)
                               .Select(c => c.Value)
                               .ToList();

            if (!userRoles.Contains(ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.EditRole]))
            {
                Log.Error(string.Format(LogErrorResources.InvalidRole, ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.EditRole]));
                return Unauthorized(RolesResources.InvalidRole);
            }

            if (!AtLeastOnePermissionInRole(product, roleEdit))
            {
                Log.Error(string.Format(LogErrorResources.RolePermissionRequired, JsonConvert.SerializeObject(roleEdit)));
                return BadRequest(RolesResources.RolePermissionRequired);
            }

            try
            {
                List<ApplicationRole> roles = new();

                if (product == Products.ManagementPortal)
                {
                    var existingRole = await _roleManager.FindByNameAsync(roleEdit.Name);
                    if (existingRole != null && existingRole.Id != roleEdit.Id)
                    {
                        Log.Error(LogErrorResources.RoleNameExists);
                        return Conflict(RolesResources.RoleNameExists);
                    }

                    var role = await _roleManager.Roles.Include(r => r.Permissions).FirstOrDefaultAsync(r => r.Id == id);
                    if (role == null)
                    {
                        Log.Error(string.Format(LogErrorResources.RoleNotFound, id));
                        return NotFound(RolesResources.RoleNotFound);
                    }

                    var existingPermissions = _dbContext.ApplicationPermissions
                        .Where(p => roleEdit.Permissions != null && roleEdit.Permissions.Select(rp => rp.Id).Contains(p.Id))
                        .ToList();

                    role.Name = roleEdit.Name;
                    role.Permissions = existingPermissions;

                    await _roleManager.UpdateAsync(role);
                    await _dbContext.SaveChangesAsync();

                    return Ok(id);
                }
                else
                {
                    List<PermissionGetDto> apiPermissions = new();
                    if (roleEdit.Permissions != null)
                    {
                        foreach (var apiPermission in roleEdit.Permissions)
                        {
                            apiPermissions.Add(new PermissionGetDto
                            {
                                Id = apiPermission.Id,
                                Name = apiPermission.Name,
                            });
                        }
                    }

                    RoleGetDto apiRole = new()
                    {
                        Id = roleEdit.Id,
                        Name = roleEdit.Name,
                        Permissions = apiPermissions,
                    };

                    string productName = product.ToString();
                    var httpClient = _httpClientFactory.CreateClient(productName);
                    try
                    {
                        var requestUri = $"api/Roles/{id}";
                        HttpResponseMessage response = await httpClient.PutAsJsonAsync(requestUri, apiRole);
                        if (response.IsSuccessStatusCode)
                        {
                            return Ok(id);
                        }
                        else
                        {
                            string errorData = await response.Content.ReadAsStringAsync();
                            string errorMessage = string.Format(RolesResources.ProductError, productName, errorData);
                            return StatusCode((int)response.StatusCode, errorMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message);
                        return StatusCode(500, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Operacion para eliminar un rol (a nivel fisico).
        /// </summary>
        /// <param name="product">Producto al que pertenece el rol.</param>
        /// <param name="idRole">Id del rol a eliminar.</param>
        /// <returns>Retorna código 200 en caso de que se elimine el rol correctamente.</returns>
        [HttpDelete("{product}/{idRole}")]
        public async Task<IActionResult> DeleteRole(Products product, string idRole)
        {
            var userRoles = User.Claims
                             .Where(c => c.Type == ClaimTypes.Role)
                             .Select(c => c.Value)
                             .ToList();

            if (!userRoles.Contains(ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.DeleteRole]))
            {
                Log.Error(string.Format(LogErrorResources.InvalidRole, ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.DeleteRole]));
                return Unauthorized(RolesResources.InvalidRole);
            }

            try
            {
                if (product == Products.ManagementPortal)
                {
                    var role = await _roleManager.Roles.Include(r => r.Permissions).FirstOrDefaultAsync(r => r.Id == idRole);
                    if (role == null || role.Name == null)
                    {
                        Log.Error(string.Format(LogErrorResources.RoleNotFound, idRole));
                        return NotFound(RolesResources.RoleNotFound);
                    }

                    var usersWithRole = await _userManager.GetUsersInRoleAsync(role.Name);

                    foreach (var user in usersWithRole)
                    {
                        var result = await _userManager.RemoveFromRoleAsync(user, role.Name);
                        if (!result.Succeeded)
                        {
                            string errorMessage = string.Format(RolesResources.UserRoleNotDeleted, user.UserName);
                            Log.Error(errorMessage);
                            return BadRequest(errorMessage);
                        }
                    }

                    var resultDelete = await _roleManager.DeleteAsync(role);
                    if (!resultDelete.Succeeded)
                    {
                        Log.Error(string.Format(LogErrorResources.DeleteRoleDbError, role.Name));
                        return BadRequest(RolesResources.DeleteRoleDbError);
                    }

                    await _dbContext.SaveChangesAsync();

                    return Ok(RolesResources.RoleDeleted);
                }
                else
                {
                    string productName = product.ToString();
                    var httpClient = _httpClientFactory.CreateClient(productName);
                    try
                    {
                        var requestUri = $"api/Roles/{idRole}";
                        HttpResponseMessage response = await httpClient.DeleteAsync(requestUri);
                        if (response.IsSuccessStatusCode)
                        {
                            string responseData = await response.Content.ReadAsStringAsync();
                            return Ok(responseData);
                        }
                        else
                        {
                            string errorData = await response.Content.ReadAsStringAsync();
                            string errorMessage = string.Format(RolesResources.ProductError, productName, errorData);
                            Log.Error(errorMessage);
                            return StatusCode((int)response.StatusCode, errorMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message);
                        return StatusCode(500, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Operacion que devuelve true si el roleDTO tiene al menos un permiso.
        /// Para productos devuelve siempre true si el campo role.Permissions existe y Count > 0 true.
        /// Para el portal ademas chequea si al menos uno de los permisos existe en base de datos.
        /// </summary>
        /// <param name="product">producto del rol.</param>
        /// <param name="roleDto"> DTO a verificar.</param>
        /// <returns>True si el rol tiene al menos un permiso, false si no.</returns>
        private bool AtLeastOnePermissionInRole(Products product, ApplicationRoleDto roleDto)
        {
            var permissionNames = roleDto.Permissions?.Select(p => p.Name).ToList();
            if (permissionNames == null || (permissionNames.Count == 0))
            {
                return false;
            }
            else
            {
                if (product == Products.ManagementPortal)
                {
                    var permissionsInDb = _dbContext.Set<ApplicationPermission>()
                                                  .Where(p => permissionNames.Contains(p.Name) && !p.IsDeleted)
                                                  .Select(p => p.Name)
                                                  .ToList();
                    return permissionsInDb.Count > 0;
                }
                else
                {
                    return true;
                }
            }
        }
    }
}