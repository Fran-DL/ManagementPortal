using System.IdentityModel.Tokens.Jwt;
using ManagementPortal.Shared.Constants;
using ManagementPortal.Shared.Dtos;
using ManagementPortal.Shared.Models;
using ManagementPortal.Shared.Resources.Server;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Serilog;
using Serilog.Context;

namespace ManagementPortal.Server.Hubs
{
    /// <summary>
    /// Hub para implementar mensajería.
    /// </summary>
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly Services.MessagingService _messagingService;
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatHub"/> class.
        /// </summary>
        /// <param name="messagingService">Servicio de mensajería.</param>
        /// <param name="userManager">Manager de usuarios de Identity.</param>
        public ChatHub(Services.MessagingService messagingService, UserManager<ApplicationUser> userManager)
        {
            _messagingService = messagingService;
            _userManager = userManager;
        }

        /// <summary>
        /// Carga los canales del usuario.
        /// </summary>
        /// <returns>Retorna los canales del usuario.</returns>
        public async Task GetChannels()
        {
            var user = await ValidateAuthUser();
            var userChannels = await _messagingService.GetUserChannels(user.Id);
            await Clients.Caller.SendAsync("LoadChannels", userChannels);

            await ConfigureLogContext("GetChannels", string.Empty);
        }

        /// <summary>
        /// Obtiene los mensajes no leídos del usuario.
        /// </summary>
        /// <returns>Retorna los mensajes sin leer del usuario.</returns>
        public async Task GetUnreadMessages()
        {
            var user = await ValidateAuthUser();
            var unreadMessages = await _messagingService.GetUnreadMessagesAsync(user.Id);
            await Clients.Caller.SendAsync("LoadUnreadMessages", unreadMessages);
        }

        /// <summary>
        /// Obtiene los mensajes no leídos del usuario (primera vez).
        /// </summary>
        /// <returns>Retorna los mensajes sin leer del usuario.</returns>
        public async Task GetNotifications()
        {
            var user = await ValidateAuthUser();
            var unreadMessages = await _messagingService.GetUnreadMessagesAsync(user.Id);
            await Clients.Caller.SendAsync("LoadNotifications", unreadMessages);
        }

        /// <summary>
        /// Metodo para agregar usuario al canal y enviarle los mensajes del mismo.
        /// </summary>
        /// <param name="channelId">Id del canal.</param>
        /// <returns>Retorna los mensajes del canal.</returns>
        public async Task JoinChannel(Guid channelId)
        {
            var user = await ValidateAuthUser();

            await Groups.AddToGroupAsync(Context.ConnectionId, channelId.ToString());

            var messages = await _messagingService.GetMessagesForChannel(channelId, user.Id);

            await Clients.Caller.SendAsync("LoadMessages", messages);

            await ConfigureLogContext($"JoinChannel - ChannelId: {channelId}", string.Empty);
        }

        /// <summary>
        /// Metodo para crear canales (grupos) entre usuarios.
        /// </summary>
        /// <param name="channelName">Nombre del nuevo canal.</param>
        /// <param name="users">Listado de usuario del canal.</param>
        /// <returns>Retorna el canal creado.</returns>
        public async Task CreateChannel(string channelName, List<ApplicationUserDto> users)
        {
            var sendUser = await ValidateAuthUser();

            List<ApplicationUser> applicationUsers = new();

            foreach (var user in users)
            {
                var existsUser = await _userManager.FindByNameAsync(user.UserName);

                if (existsUser != null)
                {
                    applicationUsers.Add(existsUser);
                }
            }

            applicationUsers.Add(sendUser);

            var newChanel = await _messagingService.CreateChannel(channelName, applicationUsers);

            if (newChanel != null)
            {
                await Clients.Caller.SendAsync("LoadChannel", new MessagingChannelDto
                {
                    Id = newChanel.Id.ToString(),
                    IsPrivate = false,
                    Name = newChanel.Name,
                    Users = newChanel.Users.Select(u => new ApplicationUserDto
                    {
                        Id = u.Id,
                        UserName = u.UserName ?? string.Empty,
                        Email = u.Email ?? string.Empty,
                    }).ToList(),
                });

                foreach (var user in newChanel.Users)
                {
                    if (!user.Id.Equals(sendUser.Id))
                    {
                        var newmessage = new MessageDto
                        {
                            Id = Guid.NewGuid(),
                            Text = $"Agregó a {user.UserName}",
                            Timestamp = DateTime.UtcNow,
                            State = MessageState.Send,
                            User = new ApplicationUserDto
                            {
                                Name = sendUser.Name,
                                LastName = sendUser.LastName,
                                Email = sendUser.Email ?? string.Empty,
                                UserName = sendUser.UserName ?? string.Empty,
                            },
                            IsAction = true,
                        };

                        var newMessage = await _messagingService.AddMessage(newChanel.Id, sendUser.Id, newmessage);

                        if (newMessage != null)
                        {
                            var message = new MessageDto
                            {
                                Id = newMessage.Id,
                                Text = newMessage.Text,
                                Timestamp = newMessage.Timestamp,
                                State = MessageState.Received,
                                User = new ApplicationUserDto
                                {
                                    Name = newMessage.User.Name,
                                    UserName = newMessage.User.UserName ?? string.Empty,
                                    Id = newMessage.User.Id,
                                },
                                MessagingChannel = new MessagingChannelDto
                                {
                                    Id = newChanel.Id.ToString(),
                                    IsPrivate = newChanel.IsPrivate,
                                    Name = newChanel.IsPrivate ? user.UserName ?? string.Empty : newChanel.Name,
                                    Users = newChanel.Users.Select(u => new ApplicationUserDto
                                    {
                                        Id = u.Id,
                                        UserName = u.UserName ?? string.Empty,
                                        Email = u.Email ?? string.Empty,
                                    }).ToList(),
                                },
                                IsAction = true,
                            };

                            await Clients
                                .Users(newChanel.Users.Select(p => p.Id))
                                .SendAsync("LoadUnreadMessages", new List<MessageDto> { message });

                            await Clients.Users(newChanel.Users.Select(p => p.Id)).SendAsync("LoadNotification", new List<MessageDto> { message });
                        }
                    }
                }
            }

            await ConfigureLogContext($"CreateChannel - channelName: {channelName}", string.Empty);
        }

        /// <summary>
        /// Agregaga un listado de usuarios a un canal.
        /// </summary>
        /// <param name="channelId">Id del canal.</param>
        /// <param name="users">Listado de usuarios.</param>
        /// <returns>Agrega los usuarios al grupo.</returns>
        public async Task AddUsersToChannel(Guid channelId, List<ApplicationUserDto> users)
        {
            var sendUser = await ValidateAuthUser();

            var usersInChannel = await _messagingService.GetUsersInChannelAsync(channelId);
            var channel = await _messagingService.GetChannelByIdAsync(channelId);
            if (channel != null)
            {
                foreach (var user in users)
                {
                    var newmessage = new MessageDto
                    {
                        Id = Guid.NewGuid(),
                        Text = $"Agregó a {user.UserName}",
                        Timestamp = DateTime.UtcNow,
                        State = MessageState.Send,
                        User = new ApplicationUserDto
                        {
                            Name = sendUser.Name,
                            LastName = sendUser.LastName,
                            Email = sendUser.Email ?? string.Empty,
                            UserName = sendUser.UserName ?? string.Empty,
                        },
                        IsAction = true,
                    };

                    await _messagingService.AddUserChannel(channelId, user.Id);
                    var newMessage = await _messagingService.AddMessage(channelId, sendUser.Id, newmessage);

                    if (newMessage != null)
                    {
                        var message = new MessageDto
                        {
                            Id = newMessage.Id,
                            Text = newMessage.Text,
                            Timestamp = newMessage.Timestamp,
                            State = MessageState.Received,
                            User = new ApplicationUserDto
                            {
                                Name = newMessage.User.Name,
                                UserName = newMessage.User.UserName ?? string.Empty,
                                Id = newMessage.User.Id,
                            },
                            MessagingChannel = new MessagingChannelDto
                            {
                                Id = channel.Id.ToString(),
                                IsPrivate = channel.IsPrivate,
                                Name = channel.IsPrivate ? user.UserName ?? string.Empty : channel.Name,
                                Users = channel.Users.Select(u => new ApplicationUserDto
                                {
                                    Id = u.Id,
                                    UserName = u.UserName ?? string.Empty,
                                    Email = u.Email ?? string.Empty,
                                }).ToList(),
                            },
                            IsAction = true,
                        };

                        await Clients
                            .Users(usersInChannel.Select(p => p.Id))
                            .SendAsync("LoadUnreadMessages", new List<MessageDto> { message });
                    }
                }
            }

            await ConfigureLogContext($"AddUsersToChannel - channelId: {channelId}", string.Empty);
        }

        /// <summary>
        /// Metodo para ingesar a un canal privado entre usuarios (lo crea si no existe).
        /// </summary>
        /// <param name="userId">Id del usuario destinatario.</param>
        /// <returns>Crea el canal o retorna los mensajes si el canal ya existe.</returns>
        public async Task JoinPrivateChannel(string userId)
        {
            var user = await ValidateAuthUser();

            var privateChannel = await _messagingService.CreatePrivateChannel(user.Id, userId);

            if (privateChannel != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, privateChannel.Id.ToString());

                var messages = await _messagingService.GetMessagesForChannel(privateChannel.Id, user.Id);

                var recipientUser = await _userManager.FindByIdAsync(userId);

                if (recipientUser != null)
                {
                    await Clients.Caller.SendAsync("LoadChannel", new MessagingChannelDto
                    {
                        Id = privateChannel.Id.ToString(),
                        IsPrivate = true,
                        Name = recipientUser.UserName ?? string.Empty,
                    });
                    await Clients.Caller.SendAsync("LoadMessages", messages);
                }
            }

            await ConfigureLogContext($"JoinPrivateChannel - userId: {userId}", string.Empty);
        }

        /// <summary>
        /// Marcar un mensaje como leído.
        /// </summary>
        /// <param name="messages">Ids de los mensajes a marcar como leidos.</param>
        /// <param name="channelId">ID del canal.</param>
        /// <returns>No retorna nada.</returns>
        public async Task MarkMessagesAsRead(List<Guid> messages, string channelId)
        {
            var user = await ValidateAuthUser();

            var channel = await _messagingService.GetChannelByIdAsync(Guid.Parse(channelId));

            var usersInChannel = (await _messagingService.GetUsersInChannelAsync(Guid.Parse(channelId)))
                .Where(u => !u.Id.Equals(user.Id));

            if (channel != null)
            {
                foreach (var messageId in messages)
                {
                    (var message, var isRead) = await _messagingService.GetMessageById(messageId, user.Id);

                    if (message != null && isRead)
                    {
                        await Clients
                            .Users(usersInChannel.Where(u => u.Id.Equals(message.User.Id)).Select(u => u.Id))
                            .SendAsync("LoadIsReadMessage", messageId);
                    }
                }
            }

            await _messagingService.MarkMessageAsRead(messages, user.Id);
        }

        /// <summary>
        /// Metodo para retirar el usaurio del canal en línea.
        /// </summary>
        /// <param name="channelId">Id del canal.</param>
        /// <returns>Retira el usuario del canal en línea.</returns>
        public async Task LeaveChannel(string channelId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, channelId);
        }

        /// <summary>
        /// Envía un mensaje del usuario al canal.
        /// </summary>
        /// <param name="channelId">ID del canal.</param>
        /// <param name="inputMesage">Mensaje que genera el usuario.</param>
        /// <returns>Envía el mensaje a todos los usuarios del canal.</returns>
        public async Task SendMessageToChannel(Guid channelId, MessageDto inputMesage)
        {
            var user = await ValidateAuthUser();

            List<MessageDto> messages = new();

            var newMessage = await _messagingService.AddMessage(channelId, user.Id, inputMesage);
            var channel = await _messagingService.GetChannelByIdAsync(channelId);

            if (newMessage != null && channel != null)
            {
                var message = new MessageDto
                {
                    Id = newMessage.Id,
                    Text = newMessage.Text,
                    Timestamp = inputMesage.Timestamp,
                    State = MessageState.Received,
                    User = new ApplicationUserDto
                    {
                        Name = newMessage.User.Name,
                        UserName = newMessage.User.UserName ?? string.Empty,
                        Id = newMessage.User.Id,
                    },
                    MessagingChannel = new MessagingChannelDto
                    {
                        Id = channel.Id.ToString(),
                        IsPrivate = channel.IsPrivate,
                        Name = channel.IsPrivate ? user.UserName ?? string.Empty : channel.Name,
                        Users = channel.Users.Select(u => new ApplicationUserDto
                        {
                            Id = u.Id,
                            UserName = u.UserName ?? string.Empty,
                            Email = u.Email ?? string.Empty,
                        }).ToList(),
                    },
                };

                messages.Add(message);

                var usersInChannel = await _messagingService.GetUsersInChannelAsync(channel.Id);

                await Clients.Users(usersInChannel.Select(p => p.Id)).SendAsync("LoadUnreadMessages", messages);
                await Clients.Users(usersInChannel.Select(p => p.Id)).SendAsync("LoadNotification", messages);
            }

            await ConfigureLogContext($"SendMessageToChannel - ChannelId: {channelId}", inputMesage.Text);
        }

        /// <summary>
        /// Valida autenticacion del usuario en el chat.
        /// </summary>
        /// <returns>Retorna el usuario si se encuentra logueado.</returns>
        private async Task<ApplicationUser> ValidateAuthUser()
        {
            var user = Context?.User != null
                ? await _userManager.GetUserAsync(Context.User)
                : null;
            if (user == null)
            {
                throw new Exception(UsersResources.UserNotAuth);
            }

            return user;
        }

        /// <summary>
        /// Configuracion de los logs para mensajeria.
        /// </summary>
        /// <param name="action">Accion a registrar.</param>
        /// <param name="text">Texto informativo del log.</param>
        /// <returns>Se registra el log.</returns>
        private Task ConfigureLogContext(string action, string text)
        {
            string product = string.Empty;
            var ipAddress = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            if (Context.GetHttpContext()?.Request.Headers.TryGetValue("Authorization", out var extractedToken) == true)
            {
                var token = extractedToken.ToString().Replace("Bearer ", string.Empty);
                var handler = new JwtSecurityTokenHandler();

                if (handler.CanReadToken(token))
                {
                    var jwtToken = handler.ReadJwtToken(token);
                    product = jwtToken.Audiences.FirstOrDefault() ?? "Unknown";
                }
            }

            var user = Context.User?.Identity?.Name ?? "Anonymous";

            using (LogContext.PushProperty("Application", product))
            using (LogContext.PushProperty("IpAddress", ipAddress))
            using (LogContext.PushProperty("UserId", user))
            using (LogContext.PushProperty("Action", action))
            {
                Log.Information("{Action} {text}", action, text);
            }

            return Task.CompletedTask;
        }
    }
}