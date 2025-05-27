using System.Data;
using Bogus;
using ManagementPortal.Server.Context;
using ManagementPortal.Shared.Constants;
using ManagementPortal.Shared.Dtos;
using ManagementPortal.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ManagementPortal.Server.Configurations
{
    /// <summary>
    /// Clase que se implementa para inicializar la base de datos con datos dummy.
    /// </summary>
    public static class SeedData
    {
        /// <summary>
        /// Metodo que se encarga de hacer el seed data en la base de datos (roles, aplicaciones, usuarios, permisos).
        /// </summary>
        /// <param name="serviceProvider">Interfaz para obtener servicios necesarios.</param>
        /// <returns>No retorna nada ya que se implementa como Task.</returns>
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var dbContext = serviceProvider.GetRequiredService<ApplicationContext>();

            await dbContext.Database.EnsureDeletedAsync(); // Comentar para hacer deploy con SeedData=true
            await dbContext.Database.EnsureCreatedAsync(); // Comentar para hacer deploy con SeedData=true

            await dbContext.Database.ExecuteSqlRawAsync("DROP TABLE Log");

            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            // Definir permisos
            var listarusuariosPermission = new ApplicationPermission { Name = ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.ListUsers] };
            var editarUsuarioPermission = new ApplicationPermission { Name = ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.EditUser] };
            var borrarUsuarioPermission = new ApplicationPermission { Name = ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.DeleteUser] };
            var crearUsuarioPermission = new ApplicationPermission { Name = ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.CreateUser] };
            var listarRolesPermission = new ApplicationPermission { Name = ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.ListRoles] };
            var editarRolPermission = new ApplicationPermission { Name = ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.EditRole] };
            var borrrarRolPermission = new ApplicationPermission { Name = ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.DeleteRole] };
            var crearRolPermission = new ApplicationPermission { Name = ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.CreateRole] };
            var listarLogsPermission = new ApplicationPermission { Name = ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.ListLogs] };
            var messagingPermission = new ApplicationPermission { Name = ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.Messaging] };
            var groupMessagingPermission = new ApplicationPermission { Name = ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.GroupMessaging] };
            var update2fa = new ApplicationPermission { Name = ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.Update2fa] };

            var permissions = new List<ApplicationPermission>
            {
                    listarusuariosPermission,
                    editarUsuarioPermission,
                    borrarUsuarioPermission,
                    crearUsuarioPermission,
                    listarRolesPermission,
                    editarRolPermission,
                    borrrarRolPermission,
                    crearRolPermission,
                    listarLogsPermission,
                    messagingPermission,
                    groupMessagingPermission,
                    update2fa,
            };

            foreach (var permission in permissions)
            {
                dbContext.ApplicationPermissions.Add(permission);
            }

            await dbContext.SaveChangesAsync();

            // Definir roles
            var administradorRole = new ApplicationRole { Name = "administrador" };
            var lecturaUsuariosRole = new ApplicationRole { Name = "Lectura usuarios" };
            var escrituraUsuariosRole = new ApplicationRole { Name = "Escritura usuarios" };
            var lecturaRolesRole = new ApplicationRole { Name = "Lectura roles" };
            var escrituraRolesRole = new ApplicationRole { Name = "Escritura roles" };
            var listarLogsRole = new ApplicationRole { Name = "Lectura logs" };
            var mensajeriaSimpleRole = new ApplicationRole { Name = "Mensajeria simple" };
            var mensajeriaAvanzadaRole = new ApplicationRole { Name = "Mensajeria avanzada" };
            var roles = new List<ApplicationRole>
            {
                administradorRole,
                lecturaUsuariosRole,
                escrituraUsuariosRole,
                lecturaRolesRole,
                escrituraRolesRole,
                listarLogsRole,
                mensajeriaSimpleRole,
                mensajeriaAvanzadaRole,
            };

            foreach (var role in roles)
            {
                var rolePermissions = new List<ApplicationPermission>();
                if (role.Name == "administrador")
                {
                    foreach (var permission in permissions)
                    {
                        rolePermissions.Add(permission);
                    }
                }
                else if (role.Name == "Lectura usuarios")
                {
                    rolePermissions.Add(listarusuariosPermission);
                }
                else if (role.Name == "Escritura usuarios")
                {
                    rolePermissions.Add(editarUsuarioPermission);
                    rolePermissions.Add(borrarUsuarioPermission);
                    rolePermissions.Add(crearUsuarioPermission);
                    rolePermissions.Add(listarusuariosPermission);
                    rolePermissions.Add(update2fa);
                }
                else if (role.Name == "Lectura roles")
                {
                    rolePermissions.Add(listarRolesPermission);
                }
                else if (role.Name == "Escritura roles")
                {
                    rolePermissions.Add(editarRolPermission);
                    rolePermissions.Add(borrrarRolPermission);
                    rolePermissions.Add(crearRolPermission);
                    rolePermissions.Add(listarRolesPermission);
                }
                else if (role.Name == "Lectura logs")
                {
                    rolePermissions.Add(listarLogsPermission);
                }
                else if (role.Name == "Mensajeria simple")
                {
                    rolePermissions.Add(messagingPermission);
                }
                else if (role.Name == "Mensajeria avanzada")
                {
                    rolePermissions.Add(messagingPermission);
                    rolePermissions.Add(groupMessagingPermission);
                }

                role.Permissions = rolePermissions;
                await roleManager.CreateAsync(role);
            }

            // genero 50 roles genericos mas
            for (int i = 1; i <= 50; i++)
            {
                var role = new ApplicationRole
                {
                    Name = $"Role{i:00}",
                };

                // Selecciona 4 permisos aleatorios para cada rol
                var rolePermissions = new Faker().PickRandom(permissions, 4).ToList();
                role.Permissions = rolePermissions;
                roles.Add(role);
                await roleManager.CreateAsync(role);
            }

            await dbContext.SaveChangesAsync();

            // Usuarios
            var productsAdmin = new List<ApplicationUserProduct>()
            {
                new() { Product = Products.EventManager, UserProductId = System.Guid.NewGuid().ToString() },
                new() { Product = Products.AssetManager, UserProductId = System.Guid.NewGuid().ToString() },
                new() { Product = Products.IoTMonitor, UserProductId = System.Guid.NewGuid().ToString() },
            };
            var admin = new ApplicationUser
            {
                Email = "squiel.dutra@gmail.com",
                Name = "Admin",
                LastName = "Admin",
                UserName = "admin",
                LastLoginDate = DateTime.UtcNow,
                Products = productsAdmin,
                ProfilePhoto = SondaDummyFactory.PictureGenerator(),
            };
            var userRoles = new List<ApplicationRole>()
            {
                administradorRole,
                lecturaUsuariosRole,
                escrituraUsuariosRole,
                lecturaRolesRole,
                escrituraRolesRole,
                listarLogsRole,
                mensajeriaAvanzadaRole,
            };
            var rolesNames = userRoles.Select(ur => ur.Name!).ToList();
            var result = await userManager.CreateAsync(admin, "Password.123");
            await userManager.AddToRolesAsync(admin, rolesNames);

            var productsColab = new List<ApplicationUserProduct>()
            {
                new() { Product = Products.EventManager, UserProductId = System.Guid.NewGuid().ToString() },
                new() { Product = Products.IoTMonitor, UserProductId = System.Guid.NewGuid().ToString() },
            };
            var colaborador = new ApplicationUser
            {
                Email = "felipemiranda20023@gmail.com",
                Name = "Colaborador",
                LastName = "Colaborador",
                UserName = "colaborador",
                LastLoginDate = DateTime.UtcNow,
                Products = productsColab,
                ProfilePhoto = SondaDummyFactory.PictureGenerator(),
            };
            userRoles = new List<ApplicationRole>()
            {
                lecturaUsuariosRole,
                lecturaRolesRole,
                mensajeriaAvanzadaRole,
            };
            result = await userManager.CreateAsync(colaborador, "Password.123");
            await userManager.AddToRolesAsync(colaborador, rolesNames);

            dbContext.TwoFactorMethods.AddRange(
                new TwoFactorMethod
                {
                    Method = "App",
                },
                new TwoFactorMethod
                {
                    Method = "Email",
                });

            await dbContext.SaveChangesAsync();

            var roleList = await roleManager.Roles.ToListAsync();

            var faker = new Faker<ApplicationUser>()
                .RuleFor(u => u.UserName, (f, u) => f.Internet.UserName())
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email())
                .RuleFor(u => u.Name, (f, u) => f.Person.FullName)
                .RuleFor(u => u.LastName, (f, u) => f.Person.LastName);

            // Usuarios de IoT (no tienen rol de mensajeria)
            for (int i = 0; i < 30; i++)
            {
                var userFaker = faker.Generate();
                if ((i + 1) % 5 == 0)
                {
                    userFaker.IsDeleted = true;
                }

                userFaker.ProfilePhoto = SondaDummyFactory.PictureGenerator();

                userFaker.Products = new List<ApplicationUserProduct>()
                {
                    new () { Product = Products.IoTMonitor, UserProductId = System.Guid.NewGuid().ToString() },
                };

                var resultFaker = await userManager.CreateAsync(userFaker, "Password.123");

                if (resultFaker.Succeeded)
                {
                    // Generar entre 1 y 10 roles al azar para el usuario
                    var roleCount = new Random().Next(1, 10); // Número aleatorio entre 1 y 10
                    var randomRoles = roles.OrderBy(r => Guid.NewGuid()).Take(roleCount).ToList(); // Seleccionar roles al azar

                    foreach (var role in randomRoles)
                    {
                        if (role == null || string.IsNullOrEmpty(role.Name))
                        {
                            continue;
                        }

                        // Asignar cada rol al usuario
                        var addRoleResult = await userManager.AddToRoleAsync(userFaker, role.Name);
                        if (!addRoleResult.Succeeded)
                        {
                            // Manejar el error al agregar rol
                            Console.WriteLine($"Error al agregar rol {role} al usuario {userFaker.UserName}: {string.Join(", ", addRoleResult.Errors.Select(e => e.Description))}");
                        }
                    }
                }
                else
                {
                    // Manejar el error al crear el usuario
                    Console.WriteLine($"Error al crear el usuario: {string.Join(", ", resultFaker.Errors.Select(e => e.Description))}");
                }
            }

            // Usuarios de EventManager
            for (int i = 0; i < 30; i++)
            {
                var userFaker = faker.Generate();
                if ((i + 1) % 5 == 0)
                {
                    userFaker.IsDeleted = true;
                }

                userFaker.ProfilePhoto = SondaDummyFactory.PictureGenerator();

                userFaker.Products = new List<ApplicationUserProduct>()
                {
                    new () { Product = Products.EventManager, UserProductId = System.Guid.NewGuid().ToString() },
                };

                var resultFaker = await userManager.CreateAsync(userFaker, "Password.123");

                if (resultFaker.Succeeded)
                {
                    var roleCount = new Random().Next(1, 10);
                    var randomRoles = roles.OrderBy(r => Guid.NewGuid()).Take(roleCount).ToList(); // Seleccionar roles al azar

                    foreach (var role in randomRoles)
                    {
                        if (role == null || string.IsNullOrEmpty(role.Name))
                        {
                            continue;
                        }

                        // Asignar cada rol al usuario
                        var addRoleResult = await userManager.AddToRoleAsync(userFaker, role.Name);
                        if (!addRoleResult.Succeeded)
                        {
                            // Manejar el error al agregar rol
                            Console.WriteLine($"Error al agregar rol {role} al usuario {userFaker.UserName}: {string.Join(", ", addRoleResult.Errors.Select(e => e.Description))}");
                        }
                    }

                    await userManager.AddToRoleAsync(userFaker, "Mensajeria avanzada");
                }
                else
                {
                    // Manejar el error al crear el usuario
                    Console.WriteLine($"Error al crear el usuario: {string.Join(", ", resultFaker.Errors.Select(e => e.Description))}");
                }
            }

            // Usuarios de AssetManager
            for (int i = 0; i < 30; i++)
            {
                var userFaker = faker.Generate();
                if ((i + 1) % 5 == 0)
                {
                    userFaker.IsDeleted = true;
                }

                userFaker.ProfilePhoto = SondaDummyFactory.PictureGenerator();

                userFaker.Products = new List<ApplicationUserProduct>()
                {
                    new () { Product = Products.AssetManager, UserProductId = System.Guid.NewGuid().ToString() },
                };

                var resultFaker = await userManager.CreateAsync(userFaker, "Password.123");

                if (resultFaker.Succeeded)
                {
                    // Generar entre 1 y 5 roles al azar para el usuario
                    var roleCount = new Random().Next(1, 6); // Número aleatorio entre 1 y 5
                    var randomRoles = roles.OrderBy(r => Guid.NewGuid()).Take(roleCount).ToList(); // Seleccionar roles al azar

                    foreach (var role in randomRoles)
                    {
                        if (role == null || string.IsNullOrEmpty(role.Name))
                        {
                            continue;
                        }

                        // Asignar cada rol al usuario
                        var addRoleResult = await userManager.AddToRoleAsync(userFaker, role.Name);
                        if (!addRoleResult.Succeeded)
                        {
                            // Manejar el error al agregar rol
                            Console.WriteLine($"Error al agregar rol {role} al usuario {userFaker.UserName}: {string.Join(", ", addRoleResult.Errors.Select(e => e.Description))}");
                        }
                    }

                    await userManager.AddToRoleAsync(userFaker, "Mensajeria avanzada");
                }
                else
                {
                    // Manejar el error al crear el usuario
                    Console.WriteLine($"Error al crear el usuario: {string.Join(", ", resultFaker.Errors.Select(e => e.Description))}");
                }
            }

            // Usuarios de AssetManager y IoT
            for (int i = 0; i < 30; i++)
            {
                var userFaker = faker.Generate();
                if ((i + 1) % 5 == 0)
                {
                    userFaker.IsDeleted = true;
                }

                userFaker.ProfilePhoto = SondaDummyFactory.PictureGenerator();

                userFaker.Products = new List<ApplicationUserProduct>()
                {
                    new () { Product = Products.AssetManager, UserProductId = System.Guid.NewGuid().ToString() },
                    new () { Product = Products.IoTMonitor, UserProductId = System.Guid.NewGuid().ToString() },
                };

                var resultFaker = await userManager.CreateAsync(userFaker, "Password.123");

                if (resultFaker.Succeeded)
                {
                    // Generar entre 1 y 5 roles al azar para el usuario
                    var roleCount = new Random().Next(1, 6); // Número aleatorio entre 1 y 5
                    var randomRoles = roles.OrderBy(r => Guid.NewGuid()).Take(roleCount).ToList(); // Seleccionar roles al azar

                    foreach (var role in randomRoles)
                    {
                        if (role == null || string.IsNullOrEmpty(role.Name))
                        {
                            continue;
                        }

                        // Asignar cada rol al usuario
                        var addRoleResult = await userManager.AddToRoleAsync(userFaker, role.Name);
                        if (!addRoleResult.Succeeded)
                        {
                            // Manejar el error al agregar rol
                            Console.WriteLine($"Error al agregar rol {role} al usuario {userFaker.UserName}: {string.Join(", ", addRoleResult.Errors.Select(e => e.Description))}");
                        }
                    }

                    await userManager.AddToRoleAsync(userFaker, "Mensajeria avanzada");
                }
                else
                {
                    // Manejar el error al crear el usuario
                    Console.WriteLine($"Error al crear el usuario: {string.Join(", ", resultFaker.Errors.Select(e => e.Description))}");
                }
            }

            // Usuarios de AssetManager, IoT, EventManager y AssetManager
            for (int i = 0; i < 30; i++)
            {
                var userFaker = faker.Generate();
                if ((i + 1) % 5 == 0)
                {
                    userFaker.IsDeleted = true;
                }

                userFaker.ProfilePhoto = SondaDummyFactory.PictureGenerator();

                userFaker.Products = new List<ApplicationUserProduct>()
                {
                    new () { Product = Products.AssetManager, UserProductId = System.Guid.NewGuid().ToString() },
                    new () { Product = Products.IoTMonitor, UserProductId = System.Guid.NewGuid().ToString() },
                    new () { Product = Products.EventManager, UserProductId = System.Guid.NewGuid().ToString() },
                };

                var resultFaker = await userManager.CreateAsync(userFaker, "Password.123");

                if (resultFaker.Succeeded)
                {
                    // Generar entre 1 y 5 roles al azar para el usuario
                    var roleCount = new Random().Next(1, 6); // Número aleatorio entre 1 y 5
                    var randomRoles = roles.OrderBy(r => Guid.NewGuid()).Take(roleCount).ToList(); // Seleccionar roles al azar

                    foreach (var role in randomRoles)
                    {
                        if (role == null || string.IsNullOrEmpty(role.Name))
                        {
                            continue;
                        }

                        // Asignar cada rol al usuario
                        var addRoleResult = await userManager.AddToRoleAsync(userFaker, role.Name);
                        if (!addRoleResult.Succeeded)
                        {
                            // Manejar el error al agregar rol
                            Console.WriteLine($"Error al agregar rol {role} al usuario {userFaker.UserName}: {string.Join(", ", addRoleResult.Errors.Select(e => e.Description))}");
                        }
                    }

                    await userManager.AddToRoleAsync(userFaker, "Mensajeria avanzada");
                }
                else
                {
                    // Manejar el error al crear el usuario
                    Console.WriteLine($"Error al crear el usuario: {string.Join(", ", resultFaker.Errors.Select(e => e.Description))}");
                }
            }

            // DATOS DE PRUEBA DE MENSAJERIA
            var messagingService = serviceProvider.GetRequiredService<Services.MessagingService>();
            var users = await dbContext.Users.OrderByDescending(u => u.UserName).Take(50).ToListAsync();

            // Lista de mensajes aleatorios
            var mensajesAleatorios = new List<string>
            {
                "¡Hola! ¿Cómo estás?",
                "¿Has tenido un buen día?",
                "¿Qué te inspiró a unirte a este proyecto?",
                "¿Tienes alguna meta personal que quieras compartir?",
                "¿Qué libro o película recomendarías a cualquiera?",
                "¡Qué gusto conocerte! ¿Qué haces en tu tiempo libre?",
                "¿Cómo te describirías en tres palabras?",
                "¿Tienes alguna habilidad o talento oculto?",
                "¿Te gusta aprender cosas nuevas?",
                "¿Cuál es tu hobby favorito?",
                "¿Hay algo nuevo que hayas aprendido recientemente?",
                "¡Espero que tengamos una buena conversación!",
                "¿Tienes alguna serie o película favorita?",
                "¿Prefieres la playa o la montaña?",
                "¿Cuál es el último lugar interesante que has visitado?",
                "Si pudieras viajar a cualquier parte del mundo, ¿a dónde irías?",
                "¿Qué canción escuchas siempre para animarte?",
                "¿Cómo te ves dentro de cinco años?",
                "¿Cuál es tu comida favorita?",
                "¿Tienes algún deporte favorito o practicas alguno?",
                "¿Eres más de café o té?",
                "¿Tienes algún objetivo profesional para este año?",
                "¿Qué fue lo último que te hizo reír mucho?",
                "¿Hay alguna actividad que te relaje al final del día?",
                "¿Cuál es tu red social favorita?",
                "¿Qué te gusta hacer para desestresarte?",
                "¿Tienes algún talento que pocos conozcan?",
                "Si tuvieras que describir tu trabajo en una palabra, ¿cuál sería?",
                "¿Cuál es la habilidad que más te gustaría mejorar?",
                "¿Prefieres trabajar en equipo o de manera independiente?",
                "¿Qué es lo más importante que has aprendido últimamente?",
            };

            if (users != null)
            {
                var newChannel = await messagingService.CreateChannel("Management Portal", users);
                var random = new Random();

                // Crear un canal privado entre el colaborador y el administrador
                var privateChannelColabAdmin = await messagingService.CreatePrivateChannel(colaborador.Id, admin.Id);

                if (privateChannelColabAdmin != null)
                {
                    for (int i = 0; i < 30; i++)
                    {
                        // Mensaje aleatoria del usuario
                        var randomMessage = mensajesAleatorios[random.Next(mensajesAleatorios.Count)];
                        var userMessage = await messagingService.AddMessage(privateChannelColabAdmin.Id, colaborador.Id, new MessageDto
                        {
                            Timestamp = DateTime.UtcNow,
                            Text = randomMessage,
                        });

                        // Mensaje aleatoria del admin
                        randomMessage = mensajesAleatorios[random.Next(mensajesAleatorios.Count)];
                        var adminMessage = await messagingService.AddMessage(privateChannelColabAdmin.Id, admin.Id, new MessageDto
                        {
                            Timestamp = DateTime.UtcNow,
                            Text = randomMessage,
                        });

                        // Se marca como leido el mensaje
                        if (userMessage != null)
                        {
                            await messagingService.MarkMessageAsRead(new List<Guid> { userMessage.Id }, admin.Id);
                        }
                    }
                }

                foreach (var user in users)
                {
                    // Evitar crear un canal privado si el usuario actual es el administrador
                    if (user.Id != admin.Id && user.Id != colaborador.Id)
                    {
                        // Crear un canal privado entre cada usuario y el administrador
                        var privateChannel = await messagingService.CreatePrivateChannel(user.Id, admin.Id);

                        if (privateChannel != null)
                        {
                            for (int i = 0; i < 6; i++)
                            {
                                // Mensaje aleatoria del usuario
                                var randomMessage = mensajesAleatorios[random.Next(mensajesAleatorios.Count)];
                                var userMessage = await messagingService.AddMessage(privateChannel.Id, user.Id, new MessageDto
                                {
                                    Timestamp = DateTime.UtcNow,
                                    Text = randomMessage,
                                });

                                // Mensaje aleatoria del admin
                                randomMessage = mensajesAleatorios[random.Next(mensajesAleatorios.Count)];
                                var adminMessage = await messagingService.AddMessage(privateChannel.Id, admin.Id, new MessageDto
                                {
                                    Timestamp = DateTime.UtcNow,
                                    Text = randomMessage,
                                });

                                // Se marca como leido el mensaje
                                if (userMessage != null)
                                {
                                    await messagingService.MarkMessageAsRead(new List<Guid> { userMessage.Id }, admin.Id);
                                }
                            }
                        }
                    }
                }

                if (newChannel != null)
                {
                    foreach (var user in users)
                    {
                        await messagingService
                            .AddMessage(newChannel.Id, user.Id, new MessageDto
                            {
                                Timestamp = DateTime.UtcNow,
                                Text = $"Hola, me llamo {user.Name ?? string.Empty}",
                            });
                    }
                }
            }
        }
    }
}