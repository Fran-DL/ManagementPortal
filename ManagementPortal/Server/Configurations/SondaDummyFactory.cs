using ManagementPortal.Shared.Constants;
using ManagementPortal.Shared.Dtos.Apis;
using ManagementPortal.Shared.Models;
using ManagementPortal.Shared.Resources.ApiSONDA;

namespace ManagementPortal.Server.Configurations
{
    /// <summary>
    /// Clase que se utiliza para generar los datos de prueba de las API's de SONDA.
    /// </summary>
    public class SondaDummyFactory
    {
        private static SondaDummyFactory? _instance;

        private static List<UserDummy> _usersList = new();

        private static List<RoleDummy> _rolesList = new();

        private static int _rolesID;

        private static ILogger? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SondaDummyFactory"/> class.
        /// Clase que se utiliza para generar los datos de prueba de las API's de SONDA.
        /// </summary>
        /// <param name="users">Usuarios registrados.</param>
        private SondaDummyFactory(List<ApplicationUser> users)
        {
            if (_logger == null)
            {
                throw new ArgumentNullException(nameof(ILogger));
            }

            _logger.LogInformation("Inicializando ...");
            _rolesID = 10;
            CreateRoles();
            _logger.LogInformation("Roles OK ...");
            CreateUsers(users);
            _logger.LogInformation("Users OK ...");
            _logger.LogInformation("FIN");
        }

        /// <summary>
        /// Inicializar la instancia.
        /// </summary>
        /// <param name="users">Usuarios registrados.</param>
        /// <param name="logger">Logger para información.</param>
        /// <returns> Insancia.</returns>
        public static SondaDummyFactory Singleton(List<ApplicationUser> users, ILogger logger)
        {
            if (_instance == null)
            {
                _logger = logger;
                _instance = new SondaDummyFactory(users);
            }

            return _instance;
        }

        /// <summary>
        /// Listado de usuarios.
        /// </summary>
        /// <param name="prod"> Producto.</param>
        /// <param name="ord">Orden (Ascendente|Descendente).</param>
        /// <param name="query">Filtro.</param>
        /// <param name="sortField">Campo para filtrar.</param>
        /// <param name="pageSize">Tamaño de página.</param>
        /// <param name="currentPage">Página actual.</param>"
        /// <returns>Lista.</returns>
        public static UserApiPagination ListUser(Products prod, Order ord, string query, SortFieldUser sortField, int pageSize, int currentPage)
        {
            var users = _usersList.Where(x => x.Producto == prod).ToList();
            var results = new List<UserBasicInfo>();

            foreach (var user in users)
            {
                results.Add(new UserBasicInfo
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Name = user.Name,
                    LastName = user.LastName,
                    Roles = user.Roles.Select(r => new RoleBasicInfo { Id = r.Id, Name = r.Name }).ToList(),
                });
            }

            if (!string.IsNullOrEmpty(query))
            {
                results = results.Where(x => x.UserName.ToLower().Contains(query.ToLower()) || x.Email.ToLower().Contains(query.ToLower()) || x.Name.ToLower().Contains(query.ToLower())).ToList();
            }

            switch (sortField)
            {
                case SortFieldUser.Name:
                    results = ord == Order.Ascending ? results.OrderBy(x => x.UserName).ToList() : results.OrderByDescending(x => x.UserName).ToList();
                    break;
                case SortFieldUser.Email:
                    results = ord == Order.Ascending ? results.OrderBy(x => x.Email).ToList() : results.OrderByDescending(x => x.Email).ToList();
                    break;
                case SortFieldUser.Username:
                    results = ord == Order.Ascending ? results.OrderBy(x => x.Name).ToList() : results.OrderByDescending(x => x.Name).ToList();
                    break;
                default:
                    results = ord == Order.Ascending ? results.OrderBy(x => x.Id).ToList() : results.OrderByDescending(x => x.Id).ToList();
                    break;
            }

            return new UserApiPagination
            {
                CurrentPage = currentPage,
                TotalPages = (int)Math.Ceiling(results.Count / (double)pageSize),
                TotalItems = results.Count,
                ErrorMessage = string.Empty,
                Results = results.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList(),
            };
        }

        /// <summary>
        /// Crear imagen de perfil.
        /// </summary>
        /// <returns>imagen en string.</returns>
        public static byte[]? PictureGenerator()
        {
            int rand = new Random().Next(0, 4);

            if (rand == 0)
            {
                return null;
            }
            else if (rand == 1)
            {
                return UserDummyPictures.IMG_1;
            }
            else if (rand == 2)
            {
                return UserDummyPictures.IMG_2;
            }
            else if (rand == 3)
            {
                return UserDummyPictures.IMG_3;
            }
            else
            {
                return UserDummyPictures.IMG_4;
            }
        }

        /// <summary>
        /// Obtener usuario.
        /// </summary>
        /// <param name="prod">Producto.</param>
        /// <param name="id">Id.</param>
        /// <returns> Usuario.</returns>
        public static UserGet? GetUser(Products prod, string id)
        {
            var user = _usersList.FirstOrDefault(x => x.Producto == prod && x.Id.Equals(id));

            if (user == null)
            {
                return null;
            }

            var rtn = new UserGet()
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Name = user.Name,
                LastName = user.LastName,
                Roles = user.Roles.Select(r => new RoleBasicInfo { Id = r.Id, Name = r.Name }).ToList(),
                Picture = user.Image ?? string.Empty,
                Signature = user.Signature == null ? null : Convert.ToBase64String(user.Signature),
                Active = user.Active,
                LastVersion = user.LastVersion,
                TenantId = new Random().Next(100, int.MaxValue),
                ExternalIds = user.ExternalIds,
                Status = GetStatus(GenerateStatus()),
            };

            return rtn;
        }

        /// <summary>
        /// Obtener rol.
        /// </summary>
        /// <param name="prod">Producto.</param>
        /// <param name="name"> Nombre.</param>
        /// <returns>Rol DTO.</returns>
        public static RoleGetDto? GetRol(Products prod, string name)
        {
            var rol = _rolesList.FirstOrDefault(x => x.Producto == prod && x.Name == name);

            if (rol == null)
            {
                return null;
            }

            return new RoleGetDto
            {
                Id = rol.Id.ToString(),
                Name = rol.Name,
                Permissions = rol.Permissions.Select(p => new PermissionGetDto { Id = p.Id, Name = p.Name }).ToList(),
            };
        }

        /// <summary>
        /// Listado de roles.
        /// </summary>
        /// <param name="prod">Producto.</param>
        /// <param name="ord">Campo para ordenar.</param>
        /// <param name="query">Filtro.</param>
        /// <param name="sortField"> Campo para filtrar.</param>
        /// <param name="pageSize"> Tamaño de página.</param>
        /// <param name="currentPage"> Página actual.</param>
        /// <returns> Lista de roles.</returns>
        public static RoleApiPagination ListRol(Products prod, Order ord, string query, SortFieldRole sortField, int pageSize, int currentPage)
        {
            var roles = _rolesList.Where(x => x.Producto == prod).ToList();
            var results = new List<RoleGetDto>();

            foreach (var rol in roles)
            {
                results.Add(new RoleGetDto
                {
                    Id = rol.Id.ToString(),
                    Name = rol.Name,
                    Permissions = rol.Permissions.Select(p => new PermissionGetDto { Id = p.Id, Name = p.Name }).ToList(),
                });
            }

            if (!string.IsNullOrEmpty(query))
            {
                results = results.Where(x => x.Name.ToLower().Contains(query.ToLower()) || x.Id.ToString().Contains(query)).ToList();
            }

            switch (sortField)
            {
                case SortFieldRole.Name:
                    results = ord == Order.Ascending ? results.OrderBy(x => x.Name).ToList() : results.OrderByDescending(x => x.Name).ToList();
                    break;
                default:
                    results = ord == Order.Ascending ? results.OrderBy(x => x.Id).ToList() : results.OrderByDescending(x => x.Id).ToList();
                    break;
            }

            return new RoleApiPagination
            {
                CurrentPage = currentPage,
                TotalPages = (int)Math.Ceiling(results.Count / (double)pageSize),
                TotalItems = results.Count,
                ErrorMessage = "OK",
                Results = results.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList(),
            };
        }

        /// <summary>
        /// Listado de permisos.
        /// </summary>
        /// <param name="prod">Producto.</param>
        /// <param name="ord">Campo para ordenar.</param>
        /// <param name="query">Filtro.</param>
        /// <param name="sortField"> Campo para filtrar.</param>
        /// <param name="pageSize"> Tamaño de página.</param>
        /// <param name="currentPage"> Página actual.</param>
        /// <returns> Lista de permisos.</returns>
        public static List<PermissionGetDto> ListPermission(Products prod, Order ord, string query, SortFieldPermission sortField, int pageSize, int currentPage)
        {
            var permissions = GetPermissions(prod);

            switch (sortField)
            {
                case SortFieldPermission.Name:
                    permissions = ord == Order.Ascending ? permissions.OrderBy(x => x.Name).ToList() : permissions.OrderByDescending(x => x.Name).ToList();
                    break;
                default:
                    permissions = ord == Order.Ascending ? permissions.OrderBy(x => x.Id).ToList() : permissions.OrderByDescending(x => x.Id).ToList();
                    break;
            }

            if (!string.IsNullOrEmpty(query))
            {
                permissions = permissions.Where(x => x.Name.ToLower().Contains(query.ToLower()) || x.Id.ToString().Contains(query)).ToList();
            }

            List<PermissionGetDto> rtn = permissions.Skip((currentPage - 1) * pageSize).Take(pageSize)
                .Select(p => new PermissionGetDto { Id = p.Id, Name = p.Name }).ToList();

            return rtn;
        }

        /// <summary>
        /// Remover rol de las API's.
        /// </summary>
        /// <param name="producto"> Producto.</param>
        /// <param name="id"> Id del rol.</param>
        /// <returns> True si se elimino, false si no se encontro.</returns>
        public static bool RemoveRol(Products producto, string id)
        {
            var rol = _rolesList.FirstOrDefault(x => x.Producto == producto && x.Id.ToString() == id);

            if (rol == null)
            {
                return false;
            }

            foreach (var user in _usersList)
            {
                user.Roles.RemoveAll(x => x.Id.ToString() == id && x.Producto == producto);
            }

            _rolesList.Remove(rol);

            return true;
        }

        /// <summary>
        /// Remover usuario de las API's.
        /// </summary>
        /// <param name="producto">Producto.</param>
        /// <param name="id">ID.</param>
        /// <returns>Resultado.</returns>
        public static bool RemoveUser(Products producto, string id)
        {
            var user = _usersList.FirstOrDefault(x => x.Producto == producto && x.Id == id);

            if (user == null || user.Active == false)
            {
                return false;
            }

            user.Active = false;

            return true;
        }

        /// <summary>
        /// Agregar usuario a las API's.
        /// </summary>
        /// <param name="prod"> Producto.</param>
        /// <param name="userDTO"> DTO del usuario.</param>
        /// <returns> ID del nuevo usuario.</returns>
        public static string? AddUser(Products prod, UserPostDto userDTO)
        {
            List<RoleDummy> existRoles = new();

            var exist = _usersList.FirstOrDefault(user => user.UserName.ToUpper().Equals(userDTO.UserName.ToUpper()) && user.Producto == prod);

            if (exist != null)
            {
                return null;
            }

            if (userDTO.RoleIds != null && userDTO.RoleIds.Count != 0)
            {
                existRoles = _rolesList.Where(rol => userDTO.RoleIds.Contains(rol.Id.ToString()) && rol.Producto == prod).ToList();
            }

            UserDummy user = new()
            {
                Id = System.Guid.NewGuid().ToString(),
                Producto = prod,
                UserName = userDTO.UserName,
                Email = userDTO.Email,
                Status = GenerateStatus(),
                Name = userDTO.Name,
                LastName = userDTO.LastName,
                Password = userDTO.Password,
                Roles = existRoles,
                Image = userDTO.Picture,
                Signature = SignatureGenerator(),
                Active = true,
                TenantId = new Random().Next(200, 5000),
                LastVersion = VersionGenerator(),
                ExternalIds = userDTO.ExternalIds ?? string.Empty,
            };

            _usersList.Add(user);

            return user.Id;
        }

        /// <summary>
        /// Agregar rol a las API's.
        /// </summary>
        /// <param name="prod">Producto.</param>
        /// <param name="rolDTO">DTO del Rol.</param>
        /// <returns>True si se agrego, false si no.</returns>
        public static bool AddRol(Products prod, RoleGetDto rolDTO)
        {
            _rolesID++;

            List<PermissionDummy> existPerms = new();

            var exist = _rolesList.FirstOrDefault(rol => rol.Name.ToUpper().Equals(rolDTO.Name.ToUpper()) && rol.Producto == prod);

            if (exist != null)
            {
                return false;
            }

            if (rolDTO.Permissions != null && rolDTO.Permissions.Count != 0)
            {
                var permissionIds = rolDTO.Permissions.Select(p => p.Id).ToList();
                existPerms = GetPermissions(prod).Where(permission => permissionIds.Contains(permission.Id)).ToList();

                if (existPerms.Count == 0)
                {
                    return false;
                }
            }

            RoleDummy rol = new()
            {
                Id = _rolesID.ToString(),
                Producto = prod,
                Name = rolDTO.Name,
                Permissions = existPerms,
            };

            _rolesList.Add(rol);

            return true;
        }

        /// <summary>
        /// Actualizar usuario de las API's.
        /// </summary>
        /// <param name="producto"> Producto.</param>
        /// <param name="userDTO"> DTO del usuario.</param>
        /// <returns> True si se actualizo, false si no.</returns>
        public static bool UpdateUser(Products producto, UserPutDto userDTO)
        {
            var user = _usersList.FirstOrDefault(x => x.Producto == producto && x.Id == userDTO.Id);

            if (user == null || user.Active == false)
            {
                return false;
            }

            if (userDTO.Roles != null)
            {
                List<RoleDummy> existRoles = _rolesList.Where(rol => userDTO.RoleIds.Contains(rol.Id.ToString()) && rol.Producto == producto).ToList();
                user.Roles = existRoles;
            }

            user.UserName = userDTO.UserName;
            user.Email = userDTO.Email;
            user.Name = userDTO.Name;
            user.LastName = userDTO.LastName;
            user.Image = userDTO.Picture;
            user.ExternalIds = userDTO.ExternalIds;

            return true;
        }

        /// <summary>
        /// Actualizar usuario pswd de las API's.
        /// </summary>
        /// <param name="producto"> Producto.</param>
        /// <param name="userDTO"> DTO del usuario.</param>
        /// <returns> True si se actualizo, false si no.</returns>
        public static bool UpdatePasswordUser(Products producto, UserPasswordPutDto userDTO)
        {
            var user = _usersList.FirstOrDefault(x => x.Producto == producto && x.Id == userDTO.Id);

            if (user == null || user.Active == false)
            {
                return false;
            }

            user.Password = userDTO.NewPassword;

            return true;
        }

        /// <summary>
        /// Actualizar rol de las API's.
        /// </summary>
        /// <param name="producto">Producto.</param>
        /// <param name="rolDTO">DTO del rol.</param>
        /// <returns>True si se actualizo, false si no.</returns>
        public static bool UpdateRol(Products producto, RoleGetDto rolDTO)
        {
            var rol = _rolesList.FirstOrDefault(x => x.Producto == producto && x.Id.ToString() == rolDTO.Id);

            if (rol == null)
            {
                return false;
            }

            var exist = _rolesList.FirstOrDefault(rol => rol.Name.ToUpper().Equals(rolDTO.Name.ToUpper()) && rol.Producto == producto);

            if (exist != null && exist.Id != rolDTO.Id)
            {
                return false;
            }

            List<PermissionDummy> existPerms = new();

            if (rolDTO.Permissions != null && rolDTO.Permissions.Count != 0)
            {
                var permissionIds = rolDTO.Permissions.Select(p => p.Id).ToList();
                existPerms = GetPermissions(producto).Where(permission => permissionIds.Contains(permission.Id)).ToList();

                if (existPerms.Count == 0)
                {
                    return false;
                }
            }

            rol.Name = rolDTO.Name;
            rol.Permissions = existPerms;

            return true;
        }

        /// <summary>
        /// Constructor de los roles los permisos.
        /// </summary>
        /// <param name="prod">Determina los permisos a obtener.</param>
        /// <returns>Lista de permisos.</returns>
        /// <exception cref="Exception">Enumerado incorrecto.</exception>
        public static List<PermissionDummy> GetPermissions(Products prod)
        {
            switch (prod)
            {
                case Products.AssetManager:
                    return GeneratePermissionAsset();
                case Products.EventManager:
                    return GeneratePermissionEvent();
                case Products.IoTMonitor:
                    return GeneratePermissionIoT();
                default:
                    throw new Exception("OOoops eso no se esperaba");
            }
        }

        /// <summary>
        /// Constructor de los permisos del AssetManager.
        /// </summary>
        /// <returns>Lista de permisos del Asset.</returns>
        private static List<PermissionDummy> GeneratePermissionAsset()
        {
            return new List<PermissionDummy>
            {
                    new() { Id = 1, Name = "RegisterUser" },
                    new() { Id = 2, Name = "ReadUser" },
                    new() { Id = 3, Name = "UpdateUser" },
                    new() { Id = 4, Name = "DeleteUser" },
                    new() { Id = 5, Name = "CreateRole" },
                    new() { Id = 6, Name = "ReadRole" },
                    new() { Id = 7, Name = "UpdateRole" },
                    new() { Id = 8, Name = "DeleteRole" },
                    new() { Id = 9, Name = "reateGroup" },
                    new() { Id = 10, Name = "ReadGroup" },
                    new() { Id = 11, Name = "UpdateGroup" },
                    new() { Id = 12, Name = "DeleteGroup" },
                    new() { Id = 13, Name = "CreateBundle" },
                    new() { Id = 14, Name = "ReadBundle" },
                    new() { Id = 15, Name = "UpdateBundle" },
                    new() { Id = 16, Name = "DeleteBundle" },
                    new() { Id = 17, Name = "CreateAssetType" },
                    new() { Id = 18, Name = "ReadAssetType" },
                    new() { Id = 19, Name = "UpdateAssetType" },
                    new() { Id = 20, Name = "DeleteAssetType" },
                    new() { Id = 21, Name = "CreateAsset" },
                    new() { Id = 22, Name = "ReadAsset" },
                    new() { Id = 23, Name = "UpdateAsset" },
                    new() { Id = 24, Name = "DeleteAsset" },
                    new() { Id = 25, Name = "ReadAudit" },
                    new() { Id = 26, Name = "ExportAudit" },
                    new() { Id = 27, Name = "ReadReport" },
                    new() { Id = 28, Name = "ExportReports" },
                    new() { Id = 29, Name = "CreateState" },
                    new() { Id = 30, Name = "ReadState" },
                    new() { Id = 31, Name = "UpdateState" },
                    new() { Id = 32, Name = "DeleteState" },
                    new() { Id = 33, Name = "CreateBrand" },
                    new() { Id = 34, Name = "ReadBrand" },
                    new() { Id = 35, Name = "UpdateBrand" },
                    new() { Id = 36, Name = "DeleteBrand" },
                    new() { Id = 37, Name = "CreateResponsible" },
                    new() { Id = 38, Name = "ReadResponsible" },
                    new() { Id = 39, Name = "UpdateResponsible" },
                    new() { Id = 40, Name = "DeleteResponsible" },
                    new() { Id = 41, Name = "CreateProvider" },
                    new() { Id = 42, Name = "ReadProvider" },
                    new() { Id = 43, Name = "UpdateProvider" },
                    new() { Id = 44, Name = "DeleteProvider" },
                    new() { Id = 45, Name = "CreateModel" },
                    new() { Id = 46, Name = "ReadModel" },
                    new() { Id = 47, Name = "UpdateModel" },
                    new() { Id = 48, Name = "DeleteModel" },
                    new() { Id = 49, Name = "CreateEventTask" },
                    new() { Id = 50, Name = "ReadEventTask" },
                    new() { Id = 51, Name = "UpdateEventTask" },
                    new() { Id = 52, Name = "DeleteEventTask" },
                    new() { Id = 53, Name = "ReadEventTaskInstance" },
                    new() { Id = 54, Name = "ReadMyEventTaskInstance" },
                    new() { Id = 55, Name = "UpdateEventTaskInstance" },
                    new() { Id = 56, Name = "DeleteEventTaskInstance" },
                    new() { Id = 57, Name = "CreateTaskType" },
                    new() { Id = 58, Name = "ReadTaskType" },
                    new() { Id = 59, Name = "UpdateTaskType" },
                    new() { Id = 60, Name = "DeleteTaskType" },
                    new() { Id = 61, Name = "CreateActionType" },
                    new() { Id = 62, Name = "ReadActionType" },
                    new() { Id = 63, Name = "UpdateActionType" },
                    new() { Id = 64, Name = "DeleteActionType" },
                    new() { Id = 65, Name = "WorkInTask" },
                    new() { Id = 66, Name = "SendTaskByMail" },
                    new() { Id = 67, Name = "TakeOverTask" },
                    new() { Id = 68, Name = "BlockTask" },
                    new() { Id = 69, Name = "FreeTask" },
                    new() { Id = 70, Name = "CreateStock" },
                    new() { Id = 71, Name = "ReadStock" },
                    new() { Id = 72, Name = "UpdateStock" },
                    new() { Id = 73, Name = "DeleteStock" },
                    new() { Id = 74, Name = "ManageStock" },
                    new() { Id = 75, Name = "ReadIncidentTask" },
                    new() { Id = 76, Name = "ReadPlannedTask" },
            };
        }

        /// <summary>
        /// Constructor de los permisos del EventManager.
        /// </summary>
        /// <returns>Lista de permisos del Event.</returns>
        private static List<PermissionDummy> GeneratePermissionEvent()
        {
            return new List<PermissionDummy>
            {
                  new() { Id = 1, Name = "ListAlert" },
                  new() { Id = 2, Name = "CreateEventFromAlert" },
                  new() { Id = 3, Name = "CreateEventManually" },
                  new() { Id = 4, Name = "ListExtension" },
                  new() { Id = 5, Name = "CreateExtension" },
                  new() { Id = 6, Name = "AddTextToExtension" },
                  new() { Id = 7, Name = "AddMediaFileToExtension" },
                  new() { Id = 8, Name = "AssignExtension" },
                  new() { Id = 9, Name = "UnassignExtension" },
                  new() { Id = 10, Name = "ForceUnassignExtension" },
                  new() { Id = 11, Name = "CloseExtension" },
                  new() { Id = 12, Name = "PrintExtension" },
                  new() { Id = 13, Name = "ListClosedExtension" },
                  new() { Id = 14, Name = "ManageClosedExtension" },
                  new() { Id = 15, Name = "AssignResource" },
                  new() { Id = 16, Name = "ViewMap" },
                  new() { Id = 17, Name = "ViewDevice" },
                  new() { Id = 18, Name = "ListWorkZone" },
                  new() { Id = 19, Name = "ManageWorkZone" },
                  new() { Id = 20, Name = "ListPermission" },
                  new() { Id = 21, Name = "AbmPermission" },
                  new() { Id = 22, Name = "UserResource" },
                  new() { Id = 23, Name = "ViewPlane" },
                  new() { Id = 24, Name = "Audit" },
                  new() { Id = 25, Name = "RequireReason" },
                  new() { Id = 26, Name = "ManageLayers" },
                  new() { Id = 27, Name = "Monitor" },
                  new() { Id = 28, Name = "ManageUsers" },
                  new() { Id = 29, Name = "ManageRoles" },
                  new() { Id = 30, Name = "ManageCategories" },
                  new() { Id = 31, Name = "ManageActions" },
                  new() { Id = 32, Name = "ViewCameraStream" },
                  new() { Id = 33, Name = "ViewObservationalZones" },
                  new() { Id = 34, Name = "ManageObservationalZones" },
                  new() { Id = 35, Name = "UseTelephony" },
                  new() { Id = 36, Name = "ManageResources" },
                  new() { Id = 37, Name = "ManageList" },
                  new() { Id = 38, Name = "DiscardAlert" },
                  new() { Id = 39, Name = "ListEvent" },
                  new() { Id = 40, Name = "ManageEventType" },
                  new() { Id = 41, Name = "Report" },
                  new() { Id = 42, Name = "AuditEvents" },
                  new() { Id = 43, Name = "AuditEventTimeRestriction" },
                  new() { Id = 44, Name = "RecoverEvent" },
                  new() { Id = 45, Name = "AddDescriptionCloseExtension" },
                  new() { Id = 46, Name = "AddCategoryCreatedEvent" },
                  new() { Id = 47, Name = "WorkZoneData" },
                  new() { Id = 48, Name = "ExportData" },
                  new() { Id = 49, Name = "CreateEventWithDateTime" },
                  new() { Id = 50, Name = "ManageResourceType" },
                  new() { Id = 51, Name = "ManageIcons" },
                  new() { Id = 52, Name = "SendEventToExternal" },
            };
        }

        /// <summary>
        /// Constructor de los permisos del IoTMonitor.
        /// </summary>
        /// <returns>Lista de permisos del IoT.</returns>
        private static List<PermissionDummy> GeneratePermissionIoT()
        {
            return new List<PermissionDummy>
            {
            new () { Id = 1, Name = "Alta de dispositivos" },
            new () { Id = 2, Name = "Alta de fuentes" },
            new() { Id = 3, Name = "Alta de grupos" },
            new() { Id = 4, Name = "Baja de dispositivos" },
            new() { Id = 5, Name = "Baja de fuentes" },
            new() { Id = 6, Name = "Baja de grupos" },
            new() { Id = 7, Name = "Modificación de dispositivos" },
            new() { Id = 8, Name = "Modificación de fuentes" },
            new() { Id = 9, Name = "Modificación de grupos" },
            new() { Id = 10, Name = "Listado de dispositivos" },
            new() { Id = 11, Name = "Listado de fuentes" },
            new() { Id = 12, Name = "Listado de grupos" },
            new() { Id = 13, Name = "Visualización mapa" },
            new() { Id = 14, Name = "Alta de usuarios " },
            new() { Id = 15, Name = "Baja de usuarios " },
            new() { Id = 16, Name = "Modificación de usuarios" },
            new() { Id = 17, Name = "Listado de usuarios" },
            new() { Id = 18, Name = "Acceso a reportes" },
            new() { Id = 19, Name = "Acceso a gráficas" },
            new() { Id = 20, Name = "Acceso a dashboards" },
            new() { Id = 21, Name = "Acceso a dashboards personalizados" },
            new() { Id = 22, Name = "Alta de dahboards personalizados" },
            new() { Id = 23, Name = "Baja de dahboards personalizados" },
            new() { Id = 24, Name = "Modificación de dahboards personalizados" },
            new() { Id = 25, Name = "Listado de dahboards personalizados" },
            new() { Id = 26, Name = "Visualizar analíticas" },
            new() { Id = 27, Name = "Alta de analíticas" },
            new() { Id = 28, Name = "Baja de analíticas" },
            new() { Id = 29, Name = "Modificación de analíticas" },
            new() { Id = 30, Name = "Listado de analíticas" },
            new() { Id = 31, Name = "Acceso a estado de los análisis" },
            new() { Id = 32, Name = "Acceso a alertas" },
            new() { Id = 33, Name = "Acceso a simular" },
            new() { Id = 34, Name = "Acceso a auditoria " },
            new() { Id = 35, Name = "Alta de salidas" },
            new() { Id = 36, Name = "Baja de salidas" },
            new() { Id = 37, Name = "Modificación de salidas" },
            new() { Id = 38, Name = "Listado de salidas" },
            new() { Id = 39, Name = "Alta de configuración" },
            new() { Id = 40, Name = "Baja de configuración" },
            new() { Id = 41, Name = "Modificación de configuración" },
            new() { Id = 42, Name = "Listado de configuración" },
            };
        }

        /// <summary>
        /// Constructor de los roles.
        /// </summary>
        private static void CreateRoles()
        {
            foreach (Products prod in Enum.GetValues(typeof(Products)))
            {
                List<RoleDummy> roles = new();

                if (prod == Products.ManagementPortal || prod == Products.OthersProducts)
                {
                    continue;
                }

                var perm = GetPermissions(prod);

                for (int i = 1; i <= 60; i++)
                {
                    int cantPerm = new Random().Next(1, perm.Count);
                    _rolesID++;

                    var role = new RoleDummy
                    {
                        Id = _rolesID.ToString(),
                        Name = $"Role {i} en " + prod,
                        Producto = prod,
                        Permissions = perm.OrderBy(_ => new Random().Next()).Take(cantPerm).ToList(),
                    };
                    roles.Add(role);
                }

                _rolesList.AddRange(roles);
            }
        }

        /// <summary>
        /// Crea la lista de los Usuarios.
        /// </summary>
        /// <param name="registredUsers">Usuarios ya registrados.</param>
        private static void CreateUsers(List<ApplicationUser> registredUsers)
        {
            foreach (ApplicationUser appUser in registredUsers)
            {
                if (appUser.Products == null || appUser.Products.Count == 0)
                {
                    continue;
                }

                foreach (ApplicationUserProduct prodReg in appUser.Products)
                {
                    // Defino la lista de roles para el usuario x.
                    var roles = _rolesList.Where(x => x.Producto == prodReg.Product).ToList();
                    if (roles.Count == 0)
                    {
                        roles = new List<RoleDummy>();
                    }
                    else
                    {
                        int cantRoles = new Random().Next(1, roles.Count);
                        roles = roles.OrderBy(_ => new Random().Next()).Take(cantRoles).ToList();
                    }

                    UserDummy? user = null;

                    try
                    {
                        user = new()
                        {
                            Id = prodReg.UserProductId,
                            Producto = prodReg.Product,
                            UserName = appUser.UserName ?? string.Empty,
                            Email = appUser.Email ?? string.Empty,
                            Name = appUser.Name,
                            Status = GenerateStatus(),
                            LastName = appUser.LastName ?? string.Empty,
                            Password = appUser.PasswordHash ?? string.Empty,
                            Roles = roles,
                            Image = appUser.ProfilePhoto == null ? null : Convert.ToBase64String(appUser.ProfilePhoto),
                            Signature = SignatureGenerator(),
                            Active = !(appUser.IsDeleted || prodReg.ProductDelete),
                            TenantId = new Random().Next(200, 5000),
                            LastVersion = VersionGenerator(),
                            ExternalIds = System.Guid.NewGuid().ToString(),
                        };

                        _usersList.Add(user);
                    }
                    catch (Exception ex)
                    {
                        string info = appUser.ToString();
                        _logger?.LogCritical(ex, "Error en dummies user \n" + info + "\n");
                    }
                }
            }
        }

        /// <summary>
        /// Generar firma.
        /// </summary>
        /// <returns>Firma en string.</returns>
        private static byte[]? SignatureGenerator()
        {
            int rand = new Random().Next(4, 7);

            if (rand == 4)
            {
                return null;
            }
            else if (rand == 5)
            {
                return UserDummyPictures.IMG_5;
            }
            else if (rand == 6)
            {
                return UserDummyPictures.IMG_6;
            }
            else
            {
                return UserDummyPictures.IMG_7;
            }
        }

        /// <summary>
        /// generador de status.
        /// </summary>
        /// <returns>Status del usuario.</returns>
        private static Status GenerateStatus()
        {
            return new Random().Next(1, 6) switch
            {
                1 => Status.Disponible,
                2 => Status.Ocupado,
                3 => Status.Desconectado,
                4 => Status.EnServicio,
                5 => Status.EnMantenimiento,
                _ => Status.Desconocido,
            };
        }

        /// <summary>
        /// Paso de el enum al DTO de status.
        /// </summary>
        /// <param name="status"> status.</param>
        /// <returns>DTO de status.</returns>
        private static StatusDto GetStatus(Status status)
        {
            switch (status)
            {
                case Status.Disponible:
                    return new StatusDto { Id = 1, Name = "Disponible", Color = "#00FF00" };
                case Status.Ocupado:
                    return new StatusDto { Id = 2, Name = "Ocupado", Color = "#FF0000" };
                case Status.Desconectado:
                    return new StatusDto { Id = 3, Name = "Desconectado", Color = "#0000FF" };
                case Status.EnServicio:
                    return new StatusDto { Id = 4, Name = "En Servicio", Color = "#FF00FF" };
                case Status.EnMantenimiento:
                    return new StatusDto { Id = 5, Name = "En Mantenimiento", Color = "#FFFF00" };
                default:
                    return new StatusDto { Id = 0, Name = "Desconocido", Color = "#000000" };
            }
        }

        /// <summary>
        /// Generador de versiones.
        /// </summary>
        /// <returns>version del producto.</returns>
        private static string VersionGenerator()
        {
            int rand_Max = new Random().Next(0, 9);
            int rand_Middle = new Random().Next(0, 9);
            int rand_Min = new Random().Next(0, 9);

            return $"{rand_Max}.{rand_Middle}.{rand_Min}";
        }
    }
}