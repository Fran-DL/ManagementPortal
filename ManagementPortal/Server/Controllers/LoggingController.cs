using System.Security.Claims;
using System.Text.RegularExpressions;
using ManagementPortal.Server.Context;
using ManagementPortal.Shared.Constants;
using ManagementPortal.Shared.Dtos;
using ManagementPortal.Shared.Models;
using ManagementPortal.Shared.Resources.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManagementPortal.Server.Controllers
{
    /// <summary>
    /// Controller que expone endpoints para las funcionalidades de Logs.
    /// Justifacion: Se decide agrupar en un controller las funcionalidades de Logs.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LoggingController : ControllerBase
    {
        private readonly ApplicationContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingController"/> class.
        /// </summary>
        /// <param name="dbContext">EF para manejar la base de datos de la aplicacion.</param>
        /// <param name="userManager">Manejador de Identity para usuarios.</param>
        public LoggingController(ApplicationContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        /// <summary>
        /// Devuelve los logs en sistema.
        /// </summary>
        /// <param name="logFilter">Filtros a aplicar.</param>
        /// <returns>Retorna 200 si se listan correctamente los logs.</returns>
        [HttpGet]
        public async Task<IActionResult> GetLogs([FromQuery] FilterLogDto logFilter)
        {
            // Verificar permisos del usuario
            var userPermission = User.Claims
                                .Where(c => c.Type == ClaimTypes.Role)
                                .Select(c => c.Value)
                                .ToList();

            if (!userPermission.Contains(ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.ListLogs]))
            {
                return Unauthorized(LoggingResources.InvalidRole);
            }

            List<ApplicationAuditLogDto> logsDto = new();

            DateTime? startDate = logFilter.StartDate;
            DateTime? endDate = logFilter.EndDate;

            string busqueda = logFilter.SearchText.ToLower();
            var logs = await _dbContext.Logs
                .Where(log =>
                    ((logFilter.StartDate == null || log.TimeStamp >= startDate) &&
                    (logFilter.EndDate == null || log.TimeStamp <= endDate)) &&
                    (string.IsNullOrEmpty(busqueda) ||
                    ((log.Application != null && log.Application.ToLower().Contains(busqueda)) ||
                    (log.Level != null && log.Level.ToLower().Contains(busqueda)) ||
                    (log.UserId != null && log.UserId.ToLower().Contains(busqueda)) ||
                    (log.Action != null && log.Action.ToLower().Contains(busqueda)) ||
                    (log.IpAddress != null && log.IpAddress.ToLower().Contains(busqueda)) ||
                    (log.Message != null && log.Message.ToLower().Contains(busqueda)))))
                .ToListAsync();

            var totalLogsCount = logs.Count();

            if (logFilter.SortField == SortFieldLog.Level)
            {
                logs = logFilter.SortOrder == Order.Ascending
                ? logs.OrderBy(l => l.Level).ToList()
                : logs.OrderByDescending(l => l.Level).ToList();
            }
            else if (logFilter.SortField == SortFieldLog.TimeStamp)
            {
                logs = logFilter.SortOrder == Order.Ascending
                ? logs.OrderBy(l => l.TimeStamp).ToList()
                : logs.OrderByDescending(l => l.TimeStamp).ToList();
            }
            else if (logFilter.SortField == SortFieldLog.ProductId)
            {
                logs = logFilter.SortOrder == Order.Ascending
                ? logs.OrderBy(l => l.Application).ToList()
                : logs.OrderByDescending(l => l.Application).ToList();
            }
            else if (logFilter.SortField == SortFieldLog.UserId)
            {
                logs = logFilter.SortOrder == Order.Ascending
                ? logs.OrderBy(l => l.UserId).ToList()
                : logs.OrderByDescending(l => l.UserId).ToList();
            }
            else if (logFilter.SortField == SortFieldLog.ActionId)
            {
                logs = logFilter.SortOrder == Order.Ascending
                ? logs.OrderBy(l => l.Action).ToList()
                : logs.OrderByDescending(l => l.Action).ToList();
            }
            else if (logFilter.SortField == SortFieldLog.FromIp)
            {
                logs = logFilter.SortOrder == Order.Ascending
                ? logs.OrderBy(l => l.IpAddress).ToList()
                : logs.OrderByDescending(l => l.IpAddress).ToList();
            }
            else if (logFilter.SortField == SortFieldLog.Message)
            {
                logs = logFilter.SortOrder == Order.Ascending
                ? logs.OrderBy(l => l.Message).ToList()
                : logs.OrderByDescending(l => l.Message).ToList();
            }
            else
            {
                logs = logs.OrderByDescending(l => l.TimeStamp).ToList();
            }

            // Aplicar la paginacion
            logs = logs.Skip((logFilter.CurrentPage - 1) * logFilter.PageSize).Take(logFilter.PageSize).ToList();

            logsDto = new List<ApplicationAuditLogDto>();
            foreach (ApplicationAuditLog log in logs)
            {
                if (log.Message.Contains("Sending HTTP request"))
                {
                    log.Action = log.Message;
                    log.Action = Regex.Replace(log.Action, @"(Sending HTTP request ""[A-Z]+"").*", "$1");
                    log.Application = "ManagementPortal";
                }

                if (log.Message.Contains("Received HTTP response"))
                {
                    log.Action = "Received HTTP response";
                    log.Application = "ManagementPortal";
                }

                logsDto.Add(new ApplicationAuditLogDto
                {
                    TimeStamp = log.TimeStamp,
                    Level = log.Level,
                    Action = log.Action,
                    UserId = log.UserId,
                    IpAddress = log.IpAddress,
                    Application = log.Application,
                    Message = log.Message,
                });
            }

            return Ok(new LogPagination
            {
                Logs = logsDto,
                CurrentPage = logFilter.CurrentPage,
                PageSize = logFilter.PageSize,
                TotalItems = totalLogsCount,
                SearchText = logFilter.SearchText,
                SortOrder = logFilter.SortOrder,
                Product = logFilter.Product,
                SortField = logFilter.SortField,
            });
        }
    }
}