using System.Security.Claims;
using ManagementPortal.Server.Context;
using ManagementPortal.Shared.Constants;
using ManagementPortal.Shared.Dtos;
using ManagementPortal.Shared.Dtos.Apis;
using ManagementPortal.Shared.Models;
using ManagementPortal.Shared.Resources.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace ManagementPortal.Server.Controllers
{
    /// <summary>
    /// Controller que expone endpoints para las funcionalidades de Permisos.
    /// Justificación: Se decide agrupar en un controlador las funcionalidades de Permisos.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionsController : ControllerBase
    {
        private readonly ApplicationContext _dbContext;
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionsController"/> class.
        /// </summary>
        /// <param name="dbContext">EF para manejar la base de datos de la aplicacion.</param>
        /// <param name="httpClientFactory">Factory para crear los httpClient.</param>
        public PermissionsController(ApplicationContext dbContext, IHttpClientFactory httpClientFactory)
        {
            this._dbContext = dbContext;
            this._httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Listado de permisos.
        /// </summary>
        /// <param name="permissionsFilter">Dto con los filtros solicitados.</param>
        /// <returns>Retorna el listado de permisos según los filtros de buscado y paginado.</returns>
        [HttpGet]
        public async Task<IActionResult> GetPermissions([FromQuery] FilterPermissionDto permissionsFilter)
        {
            var userRoles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            if (!(userRoles.Contains(ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.EditRole]) || userRoles.Contains(ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.CreateRole])))
            {
                Log.Error(RolesResources.InvalidRole);
                return Unauthorized(RolesResources.InvalidRole);
            }

            try
            {
                var permissions = new List<ApplicationPermissionDto>();

                decimal permissionCount = 0;
                int totalPages = 0;

                if (permissionsFilter.Product == Products.ManagementPortal)
                {
                    var db_permissions = await _dbContext.ApplicationPermissions
                        .Where(p => p.Name.Contains(permissionsFilter.SearchText) || p.Name.Contains(permissionsFilter.SearchText))
                        .ToListAsync();

                    foreach (ApplicationPermission db_premission in db_permissions)
                    {
                        permissions.Add(new ApplicationPermissionDto
                        {
                            Id = db_premission.Id,
                            Name = db_premission.Name,
                            ApplicationManagment = Products.ManagementPortal,
                        });
                    }

                    // Filtrado
                    permissions = permissions.Where(r => (r.Name != null ? r.Name : string.Empty).ToLower().Contains(permissionsFilter.SearchText.ToLower())).ToList();

                    permissionCount = permissions.Count();

                    // Ordenar por 'columna' y 'sort'
                    if (permissionsFilter.SortField == SortFieldPermission.Name)
                    {
                        permissions = permissionsFilter.SortOrder == Order.Ascending
                        ? permissions.OrderBy(r => r.Name).ToList()
                        : permissions.OrderByDescending(r => r.Name).ToList();
                    }
                    else if (permissionsFilter.SortField == SortFieldPermission.Id)
                    {
                        permissions = permissionsFilter.SortOrder == Order.Ascending
                        ? permissions.OrderBy(r => r.Id).ToList()
                        : permissions.OrderByDescending(r => r.Id).ToList();
                    }

                    // Paginación
                    permissions = permissions.Skip((permissionsFilter.CurrentPage - 1) * permissionsFilter.PageSize).Take(permissionsFilter.PageSize).ToList();
                }
                else
                {
                    string productName = permissionsFilter.Product.ToString();
                    var httpClient = _httpClientFactory.CreateClient(productName);
                    try
                    {
                        var requestUri = $"api/Permissions?page={permissionsFilter.CurrentPage}&pageSize={permissionsFilter.PageSize}&query={permissionsFilter.SearchText}&sort={permissionsFilter.SortOrder.ToString()}&columna={permissionsFilter.SortField.ToString()}";
                        HttpResponseMessage response = await httpClient.GetAsync(requestUri);
                        if (response.IsSuccessStatusCode)
                        {
                            var responseContent = await response.Content.ReadFromJsonAsync<List<PermissionGetDto>>();

                            if (responseContent?.Count > 0)
                            {
                                permissions = responseContent.Select(p => new ApplicationPermissionDto
                                {
                                    Id = p.Id,
                                    Name = p.Name,
                                }).ToList() ?? new List<ApplicationPermissionDto>();
                                permissionCount = responseContent.Count;
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

                return Ok(new PermissionPagination
                {
                    Permissions = permissions,
                    CurrentPage = permissionsFilter.CurrentPage,
                    PageSize = permissionsFilter.PageSize,
                    TotalItems = (int)permissionCount,
                    SearchText = permissionsFilter.SearchText,
                    TotalPages = (int)totalPages,
                    Product = permissionsFilter.Product,
                    SortField = permissionsFilter.SortField,
                    SortOrder = permissionsFilter.SortOrder,
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return BadRequest(ex.Message);
            }
        }
    }
}