# Management Portal
Este proyecto tiene como objetivo desarrollar una plataforma web progresiva (PWA) denominada **Smart Platform**, que unificará la administración y acceso de usuarios a múltiples productos dentro del ecosistema de Smart Cities. La solución proporcionará un **portal de acceso único** con soporte para diferentes métodos de autenticación, incluyendo autenticación multifactor (MFA), y permitirá a los administradores gestionar usuarios, roles, permisos, y configuraciones personalizadas tanto para usuarios como para tenants. Además, la plataforma incluirá funcionalidades de mensajería, auditoría del 
sistema y APIs para realizar login, logout y gestión de mensajes, facilitando la integración con otros productos conectados.

# Requisitos
Asegúrate de tener los siguientes componentes instalados antes de comenzar:

- **.NET Core SDK 8 (necesario)**
- Microsoft SQL Server (Desarrollo: Opcional, Producción: necesario)
- Docker (opcional)

# Instalación
## Batch Script
Con la intención de poder publicar y ejecutar la aplicación de forma automática se provee un script para ejecutar en Windows. En la raíz del proyecto y desde la consola de Windows ejecutar:

`RunBlazorApp.cmd $ENVIROMENT $PORT`

Por ejemplo, para publicar y ejecutar la aplicación en ambiente de desarrollo y en el puerto 8085:

`RunBlazorApp.cmd Development 8085`

## Docker (opcional)
Con la intención de poder validar el manejo de la base de datos en SQL Server por parte de la aplicación se provee un archivo **docker-compose.yml** encargado de levantar un contenedor Docker para SQL Server y otro para la aplicación. Desde la raíz del proyecto ejecutar:

`docker-compose up`

# Configuración
## Ambientes
La aplicación cuenta con tres configuraciones de ambientes:
- Development
- Staging
- Production

## Base de datos
### Gestores de base de datos:
Se implementa EF particular por ambientes:
| Enviroment | RDBMS |
|--|--|
| Development | Sqlite |
| Staging | SQL Server |
| Production | SQL Server |

### Conexion a la base de datos (Sql Server):
La aplicación tiene configura la siguiente conexión a base de datos (ambientes Staging y Production):
- Server Name: **localhost, 1433**
- Authentication: **SQL Server Authentication**
- Login: **sa**
- Password: **MiP@ssw0rd2024!**

**Se deberá contar con un servidor para gestionar la base de datos en SQL Server.**

### Inicialización de base de datos:
La clase definida en `SeedData.cs` implementa la inicialización de la base de datos en el ambiente de Development y Staging. Se debe parametrizar `SeedData: true` en el archivo **appsettings.json** correspondiente.

## Wiki
La documentación de la aplicación se realiza de forma automática gracias al soporte que brinda DocFx. El archivo **docfx.json** contiene la Configuración para DocFx. Se configura CI para generar y publicar documentación en archivo **.gitlab-ci.yaml**.

[IR A LA WIKI](https://management-portal-management-portal-pis-2024-gr--72a3abf090b307.pages.fing.edu.uy/)