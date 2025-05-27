using System.Security.Cryptography;
using System.Text;
using ManagementPortal.Server.Configurations;
using ManagementPortal.Server.Configurations.Parsers;
using ManagementPortal.Server.Services;
using ManagementPortal.Shared.Constants;
using ManagementPortal.Shared.Dtos;
using ManagementPortal.Shared.Dtos.Apis;
using Microsoft.AspNetCore.Mvc;

namespace ManagementPortal.Server.Controllers
{
    /// <summary>
    /// Controller que simula api del producto AssetManager.
    /// Justifacion: Para poder implementar historias avanzadas.
    /// </summary>
    [Route("[controller]/api")]
    [ApiController]
    public class AssetManagerController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly OtpDummyService _dummyService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetManagerController"/> class.
        /// </summary>
        /// <param name="configuration"> Configuración de la aplicación.</param>
        /// <param name="dummyService">Servicio dummy para implementar otp.</param>
        public AssetManagerController(IConfiguration configuration, OtpDummyService dummyService)
            : base()
        {
            _configuration = configuration;
            _dummyService = dummyService;
        }

        // METODOS PARA USUARIOS.

        /// <summary>
        /// Listado de usuarios.
        /// </summary>
        /// <param name="apiKey">apiKey.</param>
        /// <param name="page">pagina.</param>
        /// <param name="pageSize">tamaño de pagina.</param>
        /// <param name="query">filtrado.</param>
        /// <param name="sort">ordenacion.</param>
        /// <param name="columna">columna.</param>
        /// <returns>Retorna el listado de usuarios segun los filtros de buscado y paginado.</returns>
        [HttpGet("User")]
        public async Task<IActionResult> GetUsers([FromHeader(Name = "X-API-KEY")] string apiKey, int page = 1, int pageSize = 10, string query = "", string sort = "Ascending", string columna = "Name")
        {
            if (string.IsNullOrEmpty(apiKey) || apiKey != "key")
            {
                return Unauthorized("API Key inválido");
            }

            if (query == "ERROR")
            {
                return BadRequest("Error en la consulta");
            }

            Order order = sort == "Ascending" ? Order.Ascending : Order.Descending;

            SortFieldUser field;
            if (columna == "Name")
            {
                field = SortFieldUser.Name;
            }
            else if (columna == "Email")
            {
                field = SortFieldUser.Email;
            }
            else if (columna == "Username")
            {
                field = SortFieldUser.Username;
            }
            else
            {
                field = SortFieldUser.Status;
            }

            await Task.Delay(0);

            return Ok(SondaDummyFactory.ListUser(Products.AssetManager, order, query, field, pageSize, page));
        }

        /// <summary>
        /// Consultar usuario.
        /// </summary>
        /// <param name="user_id">ID del usuario a consultar.</param>
        /// <param name="apiKey">apiKey.</param>
        /// <returns>Retorna el usuario o error 404 si no lo encuentra.</returns>
        [HttpGet("User/{user_id}")]
        public async Task<IActionResult> GetUser(string user_id, [FromHeader(Name = "X-API-KEY")] string apiKey)
        {
            if (apiKey != "key")
            {
                return Unauthorized("API Key inválido");
            }

            if (user_id == "ERROR" || user_id == "0")
            {
                return NotFound("Usuario no encontrado");
            }

            await Task.Delay(0);

            var response = SondaDummyFactory.GetUser(Products.AssetManager, user_id);

            if (response != null)
            {
                return Ok(response);
            }
            else
            {
                return NotFound("Usuario no encontrado");
            }
        }

        /// <summary>
        /// Alta de usuario.
        /// </summary>
        /// <param name="applicationUser">Dto con los datos de usuario a registrar.</param>
        /// <param name="apiKey">apiKey.</param>
        /// <returns>Retorna 200 si el usuario se creó exitosamente y 400 en caso de error.</returns>
        [HttpPost("User")]
        public async Task<IActionResult> CreateUser([FromBody] UserPostDto applicationUser, [FromHeader(Name = "X-API-KEY")] string apiKey)
        {
            if (apiKey != "key")
            {
                return Unauthorized("API Key inválido");
            }

            if (applicationUser == null || applicationUser.Name == "FIODOR")
            {
                return BadRequest("Datos de usuario inválidos");
            }

            await Task.Delay(0);

            var response = SondaDummyFactory.AddUser(Products.AssetManager, applicationUser);

            if (response != null)
            {
                return Ok(response);
            }
            else
            {
                return NotFound("No se pudo crear");
            }
        }

        /// <summary>
        /// Modificar usuario.
        /// </summary>
        /// <param name="newUserData">Dto con los datos de usuario a modificar.</param>
        /// <param name="apiKey">apiKey.</param>
        /// <returns>Retorna 200 si los datos del usuario se actualizaron correctamente y 400 en caso de error.</returns>
        [HttpPut("User")]
        public async Task<IActionResult> UpdateUser([FromBody] UserPutDto newUserData, [FromHeader(Name = "X-API-KEY")] string apiKey)
        {
            if (apiKey != "key")
            {
                return Unauthorized("API Key inválido");
            }

            if (newUserData == null || newUserData.Name == "FIODOR")
            {
                return BadRequest("Datos de usuario inválidos");
            }

            await Task.Delay(0);

            if (SondaDummyFactory.UpdateUser(Products.AssetManager, newUserData))
            {
                return Ok();
            }
            else
            {
                return BadRequest("Error al actualizar el usuario");
            }
        }

        /// <summary>
        /// Baja de usuario.
        /// </summary>
        /// <param name="userid">ID del usuario que se desea eliminar.</param>
        /// <param name="apiKey">apiKey.</param>
        /// <returns>Retorna 200 si el usuario se eliminó correctamente y 400 en caso de error.</returns>
        [HttpDelete("User/{userid}")]
        public async Task<IActionResult> DeleteUser(string userid, [FromHeader(Name = "X-API-KEY")] string apiKey)
        {
            if (apiKey != "key" || userid == "ERROR")
            {
                return Unauthorized("API Key inválido");
            }

            await Task.Delay(0);

            if (SondaDummyFactory.RemoveUser(Products.AssetManager, userid))
            {
                return Ok();
            }
            else
            {
                return BadRequest("Error al eliminar el usuario");
            }
        }

        /// <summary>
        /// Permite al usuario logueado cambiar su contraseña.
        /// </summary>
        /// <param name="model">Se incluyen los atributos necesarios CurrentPassword, NewPassword y ConfrimNewPasswrod.</param>
        /// <param name="apiKey">apiKey.</param>
        /// <returns>Retorna 200 si la nueva contraseña fue actualizada correctamente, de lo contrario 400.</returns>
        [HttpPut("User/ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] UserPasswordPutDto model, [FromHeader(Name = "X-API-KEY")] string apiKey)
        {
            if (apiKey != "key")
            {
                return Unauthorized("API Key inválido");
            }

            if (model.NewPassword == "Mi.Pass12345")
            {
                return BadRequest("La nueva contraseña no puede ser igual a la anterior");
            }

            await Task.Delay(0);

            if (SondaDummyFactory.UpdatePasswordUser(Products.AssetManager, model))
            {
                return Ok();
            }
            else
            {
                return BadRequest("Error al actualizar la contraseña");
            }
        }

        // METODOS PARA ROLES.

        /// <summary>
        /// Alta de rol.
        /// </summary>
        /// <param name="role">Dto con los datos del rol a crear.</param>
        /// <param name="apiKey">apiKey.</param>
        /// <returns>Retorna 200 si el rol se creó exitosamente y 400 en caso de error.</returns>
        [HttpPost("Roles")]
        public async Task<IActionResult> CreateRole([FromBody] RoleGetDto role, [FromHeader(Name = "X-API-KEY")] string apiKey)
        {
            if (apiKey != "key")
            {
                return Unauthorized("API Key inválido");
            }

            if (role == null || role.Name == "Divina Commedia" || role.Name == "prestupleniye i nakazaniye")
            {
                return BadRequest("Datos de rol inválidos");
            }

            await Task.Delay(0);

            if (SondaDummyFactory.AddRol(Products.AssetManager, role))
            {
                return Ok();
            }
            else
            {
                return BadRequest("Error al crear el rol");
            }
        }

        /// <summary>
        /// Baja de rol.
        /// </summary>
        /// <param name="roleId">ID del rol que se desea eliminar.</param>
        /// <param name="apiKey">apiKey.</param>
        /// <returns>Retorna 200 si el rol se eliminó correctamente y 400 en caso de error.</returns>
        [HttpDelete("Roles/{roleId}")]
        public async Task<IActionResult> DeleteRole(string roleId, [FromHeader(Name = "X-API-KEY")] string apiKey)
        {
            if (apiKey != "key")
            {
                return Unauthorized("API Key inválido");
            }

            if (roleId == "ERROR" || roleId == "0")
            {
                return NotFound("Rol no encontrado");
            }

            await Task.Delay(0);

            if (SondaDummyFactory.RemoveRol(Products.AssetManager, roleId))
            {
                return Ok();
            }
            else
            {
                return BadRequest("Error al eliminar el rol");
            }
        }

        /// <summary>
        /// Retorna un Rol, inventa uno ya que los dummies no tienen persistencia.
        /// </summary>
        /// <param name="roleName"> Nombre del rol.</param>
        /// <param name="apiKey">apiKey.</param>
        /// <returns>Datos de un rol.</returns>
        [HttpGet("Roles/{roleName}")]
        public async Task<IActionResult> GetRole(string roleName, [FromHeader(Name = "X-API-KEY")] string apiKey)
        {
            if (!string.Equals(apiKey, "key"))
            {
                return Unauthorized("API Key inválido");
            }

            if (string.IsNullOrEmpty(roleName) || roleName == "Core Dump")
            {
                return BadRequest("Nombre de rol inválido");
            }

            await Task.Delay(0);

            var response = SondaDummyFactory.GetRol(Products.AssetManager, roleName);

            if (response == null)
            {
                return NotFound("Rol no encontrado");
            }
            else
            {
                return Ok(response);
            }
        }

        /// <summary>
        /// Listado de roles.
        /// </summary>
        /// <param name="apiKey">apiKey.</param>
        /// <param name="page">pagina.</param>
        /// <param name="pageSize">tamaño de pagina.</param>
        /// <param name="query">filtrado.</param>
        /// <param name="sort">ordenacion.</param>
        /// <param name="columna">columna.</param>
        /// <returns>Retorna el listado de roles según los filtros de buscado y paginado.</returns>
        [HttpGet("Roles")]
        public async Task<IActionResult> GetRoles([FromHeader(Name = "X-API-KEY")] string apiKey, int page = 1, int pageSize = 10, string query = "", string sort = "Ascending", string columna = "Name")
        {
            if (apiKey != "key" || query == "NARNIA")
            {
                return Unauthorized("API Key inválido");
            }

            if (query == "ERROR")
            {
                return BadRequest("Error en la consulta");
            }

            Order order = sort == "Ascending" ? Order.Ascending : Order.Descending;
            SortFieldRole field = columna == "Name" ? SortFieldRole.Name : SortFieldRole.Id;

            await Task.Delay(0);

            return Ok(SondaDummyFactory.ListRol(Products.AssetManager, order, query, field, pageSize, page));
        }

        /// <summary>
        /// Actualizar un rol en el AssetManager.
        /// </summary>
        /// <param name="id">El id del rol a actualizar.</param>
        /// <param name="roleEdit">Los detalles del rol a actualizar.</param>
        /// <param name="apiKey">La API key para la autenticación.</param>
        /// <returns>Retorna el id del rol actualizado.</returns>
        [HttpPut("Roles/{id}")]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] RoleGetDto roleEdit, [FromHeader(Name = "X-API-KEY")] string apiKey)
        {
            if (apiKey != "key")
            {
                return Unauthorized("API Key inválido");
            }

            if (roleEdit == null || roleEdit.Name == "ERROR")
            {
                return BadRequest("Datos de rol inválidos");
            }

            await Task.Delay(0);

            roleEdit.Id = id;

            if (SondaDummyFactory.UpdateRol(Products.AssetManager, roleEdit))
            {
                return Ok();
            }
            else
            {
                return BadRequest("Error al actualizar el rol");
            }
        }

        /// <summary>
        /// Genera Otp para acceso al producto.
        /// </summary>
        /// <param name="message">Mensaje a validar para luego generar Otp.</param>
        /// <returns>200 si se genera el Otp exitosamente, 401 en caso contrario.</returns>
        [HttpPost("access")]
        public async Task<IActionResult> GenerateOtp(string message)
        {
            // Generamos el HMAC basado en el mensaje recibido y la clave compartida
            var sharedKey = string.Empty;
            var products = _configuration.GetSection("Products").Get<List<ProductSettings>>();
            if (products != null)
            {
                foreach (var prod in products)
                {
                    if (string.Equals(prod.Name, ControllerContext.ActionDescriptor.ControllerName))
                    {
                        sharedKey = prod.SharedKey;
                    }
                }
            }

            // Extrae el HMAC del encabezado
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            var hmacParts = authHeader.Split(' ');

            if (hmacParts.Length != 2 || hmacParts[0] != "HMAC")
            {
                return Unauthorized("Missing or invalid HMAC.");
            }

            var reqHMAC = hmacParts[1];

            var messageBytes = Encoding.UTF8.GetBytes(message);

            if (!string.IsNullOrEmpty(sharedKey))
            {
                using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(sharedKey));
                var hashBytes = hmac.ComputeHash(messageBytes);

                var expectedHmac = Convert.ToBase64String(hashBytes);

                // Comparamos el HMAC recibido con el generado
                if (string.Equals(reqHMAC, expectedHmac))
                {
                    var otp = _dummyService.GenerateOtp(message, ControllerContext.ActionDescriptor.ControllerName);

                    return Ok(otp);
                }
                else
                {
                    return Unauthorized("HMAC inválido");
                }
            }

            await Task.CompletedTask; // Añadido para cumplir el requerimiento de async
            return Unauthorized("Error procesando HMAC");
        }

        /// <summary>
        /// Función que valida el otp enviado desde el front del MP, para logearse en el producto.
        /// </summary>
        /// <param name="otpdto">Dto de Otp enviado desde el MP, que deberia de ser el generado por el back en generateOtp, junto con el Id del usuario en el producto.</param>
        /// <returns>Retorna Ok si el otp es valido, o no autorizado en caso de que no.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> ValidateOtp([FromBody] OtpRequestDto otpdto)
        {
            if (otpdto == null || string.IsNullOrEmpty(otpdto.Otp) || string.IsNullOrEmpty(otpdto.UserId))
            {
                return BadRequest("OTP inválido");
            }

            var validOtp = _dummyService.ValidateOtp(otpdto.Otp, otpdto.UserId, ControllerContext.ActionDescriptor.ControllerName);
            if (validOtp)
            {
                return Ok("Login exitoso");
            }

            await Task.CompletedTask; // Añadido para cumplir el requerimiento de async
            return Unauthorized("OTP inválido");
        }

            // METODOS PARA PERMISOS.

            /// <summary>
            /// Listado de permisos.
            /// </summary>
            /// <param name="page">pagina.</param>
            /// <param name="pageSize">tamaño de pagina.</param>
            /// <param name="query">filtrado.</param>
            /// <param name="sort">ordenacion.</param>
            /// <param name="columna">columna.</param>
            /// <param name="apiKey">apiKey.</param>
            /// <returns>Lista de roles.</returns>
        [HttpGet("Permissions")]
        public async Task<IActionResult> GetPermissions(int page = 1, int pageSize = 10, string query = "", string sort = "Ascending", string columna = "Name", [FromHeader(Name = "X-API-KEY")] string apiKey = "")
        {
            if (string.IsNullOrEmpty(apiKey) || apiKey != "key")
            {
                return Unauthorized("API Key inválido");
            }

            Order order = sort == "Ascending" ? Order.Ascending : Order.Descending;
            SortFieldPermission field = columna == "Name" ? SortFieldPermission.Name : SortFieldPermission.Id;

            await Task.Delay(0);

            return Ok(SondaDummyFactory.ListPermission(Products.AssetManager, order, query, field, pageSize, page));
        }
    }
}