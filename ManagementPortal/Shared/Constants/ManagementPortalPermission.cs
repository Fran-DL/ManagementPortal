namespace ManagementPortal.Shared.Constants
{
    /// <summary>
    /// Clase dedicada para manejar todos los permisos del ManagementPortal.
    /// </summary>
    public class ManagementPortalPermission
    {
        /// <summary>
        /// Diccionario que tiene todos los permisos del ManagementPortal.
        /// </summary>
        public static readonly Dictionary<Permission, string> PermissionDictionary = new()
                    {
                        { Permission.CreateUser, "CreateUser" },
                        { Permission.DeleteUser, "DeleteUser" },
                        { Permission.EditUser, "EditUser" },
                        { Permission.ListUsers, "ListUsers" },
                        { Permission.CreateRole, "CreateRole" },
                        { Permission.DeleteRole, "DeleteRole" },
                        { Permission.EditRole, "EditRole" },
                        { Permission.ListRoles, "ListRoles" },
                        { Permission.ListLogs, "ListLogs" },
                        { Permission.Messaging, "Messaging" },
                        { Permission.GroupMessaging, "GroupMessaging" },
                        { Permission.Update2fa, "Update2fa" },
                    };

        /// <summary>
        /// Enumerado que tiene todos los permisos del ManagementPortal.
        /// </summary>
        public enum Permission
        {
            /// <summary>
            /// Permiso que permite al usuario crear usuarios.
            /// </summary>
            CreateUser,

            /// <summary>
            /// Permiso que permite al usuario borrar usuarios.
            /// </summary>
            DeleteUser,

            /// <summary>
            /// Permiso que permite al usuario editar usuarios.
            /// </summary>
            EditUser,

            /// <summary>
            /// Permiso que permite al usuario listar usuarios.
            /// </summary>
            ListUsers,

            /// <summary>
            /// Permiso que permite al usuario crear roles.
            /// </summary>
            CreateRole,

            /// <summary>
            /// Permiso que permite al usuario borrar roles.
            /// </summary>
            DeleteRole,

            /// <summary>
            /// Permiso que permite al usuario editar roles.
            /// </summary>
            EditRole,

            /// <summary>
            /// Rol que permite al usuario editar el 2fa de los demas usuarios.
            /// </summary>
            Update2fa,

            /// <summary>
            /// Permiso que permite al usuario listar roles.
            /// </summary>
            ListRoles,

            /// <summary>
            /// Permiso que permite al usuario listar logs.
            /// </summary>
            ListLogs,

            /// <summary>
            /// Permiso que permite al usuario acceder a las funcionalidades de mensajeria.
            /// </summary>
            Messaging,

            /// <summary>
            /// Permiso que permite al usuario acceder a las funcionalidades de mensajeria en grupos.
            /// </summary>
            GroupMessaging,
        }
    }
}