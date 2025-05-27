using ManagementPortal.Server.Context;
using ManagementPortal.Shared.Constants;
using ManagementPortal.Shared.Dtos;
using ManagementPortal.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace ManagementPortal.Server.Services
{
    /// <summary>
    /// Servicio para agrupar todas las funcionalidades de mensajería.
    /// </summary>
    public class MessagingService
    {
        private readonly ApplicationContext _applicationContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingService"/> class.
        /// </summary>
        /// <param name="applicationContext">Context de la aplicación para persistir mensajería en base de datos.</param>
        public MessagingService(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        /// <summary>
        /// Obtiene los canales a los que pertenece un usuario.
        /// </summary>
        /// <param name="userId">Id del usuario.</param>
        /// <returns>Retorna la lista de canales a los que pertenece el usuario.</returns>
        public async Task<List<MessagingChannelDto>> GetUserChannels(string userId)
        {
            var userChannels = await _applicationContext.Users
                .Where(u => u.Id == userId)
                .Include(u => u.Channels)
                    .ThenInclude(ch => ch.Users)
                .SelectMany(u => u.Channels.Select(ch => new MessagingChannelDto
                {
                    Id = ch.Id.ToString(),
                    Name = ch.IsPrivate
                        ? ch.Users.First(u => u.Id != userId).UserName ?? string.Empty
                        : ch.Name,
                    IsPrivate = ch.IsPrivate,
                    Users = ch.Users.Select(u => new ApplicationUserDto
                    {
                        Id = u.Id,
                        UserName = u.UserName ?? string.Empty,
                        IsDeleted = u.IsDeleted,
                        Email = u.Email ?? string.Empty,
                    }).ToList(),
                }))
                .ToListAsync();

            foreach (var channel in userChannels)
            {
                var lastMessage = await _applicationContext.Channels
                    .Where(ch => ch.Id.ToString().Equals(channel.Id))
                    .Include(ch => ch.Messages)
                    .Select(ch => ch.Messages
                        .OrderByDescending(m => m.Timestamp)
                        .Select(m => new MessageDto
                        {
                            Id = m.Id,
                            State = MessageState.Read,
                            Text = m.Text,
                            Timestamp = m.Timestamp,
                            User = new ApplicationUserDto
                            {
                                Id = m.User.Id,
                                UserName = m.User.UserName ?? string.Empty,
                                Name = m.User.Name,
                                IsDeleted = m.User.IsDeleted,
                                Email = m.User.Email ?? string.Empty,
                            },
                            MessagingChannel = new MessagingChannelDto
                            {
                                Id = ch.Id.ToString(),
                                Name = ch.IsPrivate ? m.User.UserName ?? string.Empty : ch.Name,
                                IsPrivate = ch.IsPrivate,
                            },
                            IsAction = m.IsAction,
                        })
                    .FirstOrDefault())
                .FirstOrDefaultAsync();

                if (lastMessage != null)
                {
                    channel.Messages.Add(lastMessage);
                }
            }

            return userChannels;
        }

        /// <summary>
        /// Retorna las notificaciones del usuario dado.
        /// </summary>
        /// <param name="userId">Id del usuario.</param>
        /// <returns>Notificaciones del usuario.</returns>
        public async Task<List<MessageDto>> GetUnreadMessagesAsync(string userId)
        {
            var user = await _applicationContext.Users
                .Where(u => u.Id.Equals(userId))
                .Include(u => u.ReceivedMessages)
                    .ThenInclude(m => m.Message)
                    .ThenInclude(m => m.User)
                .FirstAsync();

            List<MessageDto> messages = new();

            foreach (var message in user.ReceivedMessages.Where(m => !m.IsRead))
            {
                string channelName = string.Empty;

                var messageChannel = await GetChannelByMessage(message.Message.Id);

                if (messageChannel != null)
                {
                    messages.Add(new MessageDto
                    {
                        Id = message.Message.Id,
                        State = message.IsRead ? MessageState.Read : MessageState.Send,
                        Text = message.Message.Text,
                        Timestamp = message.Message.Timestamp,
                        User = new ApplicationUserDto
                        {
                            Id = message.Message.User.Id,
                            UserName = message.Message.User.UserName ?? string.Empty,
                            Name = message.Message.User.Name,
                            IsDeleted = message.User.IsDeleted,
                            Email = message.Message.User.Email ?? string.Empty,
                        },
                        MessagingChannel = new MessagingChannelDto
                        {
                            Id = messageChannel.Id.ToString(),
                            Name = messageChannel.IsPrivate
                            ? message.Message.User.UserName ?? string.Empty
                            : messageChannel.Name,
                            IsPrivate = messageChannel.IsPrivate,
                        },
                        IsAction = message.Message.IsAction,
                    });
                }
            }

            return messages;
        }

        /// <summary>
        /// Retorna los mensajes de un canal.
        /// </summary>
        /// <param name="channelId">Id del canal.</param>
        /// <param name="userId">Id del usuario.</param>
        /// <returns>Mensajes que fueron enviados en el canal.</returns>
        public async Task<List<MessageDto>> GetMessagesForChannel(Guid channelId, string userId)
        {
            List<MessageDto> messagesResult = new();

            var channel = await _applicationContext.Channels
                .Include(c => c.Users)
                .Include(c => c.Messages)
                    .ThenInclude(m => m.User)
                .AsSplitQuery()
                .FirstOrDefaultAsync(c => c.Id.Equals(channelId));

            if (channel != null)
            {
                foreach (var message in channel.Messages)
                {
                    var msg = await _applicationContext.Messages
                        .Include(m => m.User)
                        .FirstOrDefaultAsync(m => m.Id.Equals(message.Id));

                    MessageState mState = MessageState.Read;

                    if (msg != null && msg.User.Id.Equals(userId))
                    {
                        mState = await _applicationContext.MessageReceivers
                            .AnyAsync(mr => mr.Message.Id == msg.Id && !mr.IsRead) ? MessageState.Received : MessageState.Read;
                    }
                    else if (msg != null && !msg.User.Id.Equals(userId))
                    {
                        var messageReceiver = await _applicationContext.MessageReceivers
                            .FirstOrDefaultAsync(mr => mr.Message.Id == msg.Id && mr.User.Id.Equals(userId));
                        mState = messageReceiver != null && messageReceiver.IsRead ? MessageState.Read : MessageState.Received;
                    }

                    messagesResult.Add(new MessageDto
                    {
                        Id = message.Id,
                        Text = message.Text,
                        State = mState,
                        Timestamp = message.Timestamp,
                        User = new ApplicationUserDto
                        {
                            Id = message.User.Id,
                            UserName = message.User.UserName ?? string.Empty,
                            Name = message.User.Name,
                            IsDeleted = message.User.IsDeleted,
                            LastName = message.User.LastName,
                        },
                        MessagingChannel = new MessagingChannelDto
                        {
                            Id = channel.Id.ToString(),
                            IsPrivate = channel.IsPrivate,
                            Name = channel.IsPrivate
                        ? channel.Users.First(u => u.Id != userId).UserName ?? string.Empty
                        : channel.Name,
                        },
                        IsAction = message.IsAction,
                    });
                }
            }

            return messagesResult;
        }

        /// <summary>
        /// Marcar un mensaje como leído dado el usuario.
        /// </summary>
        /// <param name="messages">Id del mensaje.</param>
        /// <param name="userId">Id del usuario.</param>
        /// <returns>No retorna nada, se implementa como Task.</returns>
        public async Task MarkMessageAsRead(List<Guid> messages, string userId)
        {
            foreach (var messageId in messages)
            {
                var messageReceiver = await _applicationContext.MessageReceivers
                    .Where(m => m.Message.Id.Equals(messageId) && m.User.Id.Equals(userId))
                    .FirstOrDefaultAsync();

                if (messageReceiver != null)
                {
                    messageReceiver.IsRead = true;
                    await _applicationContext.SaveChangesAsync();
                }
            }
        }

        /// <summary>
        /// Retorna un message dado su Id.
        /// </summary>
        /// <param name="messageId">Id del mensaje.</param>
        /// <param name="userId">Usuario que lee el mensaje..</param>
        /// <returns>Retorna el mensaje dado el Id.</returns>
        public async Task<(Message? Message, bool IsRead)> GetMessageById(Guid messageId, string userId)
        {
            var message = await _applicationContext.Messages
                .Include(m => m.User)
                .Include(m => m.MessageReceivers)
                .FirstOrDefaultAsync(m => m.Id.Equals(messageId));

            if (message == null)
            {
                return (null, false);
            }

            var isNotRead = await _applicationContext.MessageReceivers
                .AnyAsync(mr => mr.Message.Id == messageId && !mr.IsRead && !mr.User.Id.Equals(userId));

            return (message, !isNotRead);
        }

        /// <summary>
        /// Regresa el canal al que pertenece el mensaje.
        /// </summary>
        /// <param name="messageId">Id del messaje.</param>
        /// <returns>Retorna un canal.</returns>
        public async Task<MessagingChannel?> GetChannelByMessage(Guid messageId)
        {
            return await _applicationContext.Channels
                .FirstOrDefaultAsync(ch => ch.Messages.Any(m => m.Id.Equals(messageId)));
        }

        /// <summary>
        /// Metodo para regresar todos los usuarios de un canal.
        /// </summary>
        /// <param name="channelId">Id del canal.</param>
        /// <returns>Usuarios dentro del canal.</returns>
        public async Task<List<ApplicationUser>> GetUsersInChannelAsync(Guid channelId)
        {
            return await _applicationContext.Channels
                .Where(ch => ch.Id.Equals(channelId))
                .SelectMany(ch => ch.Users)
                .ToListAsync();
        }

        /// <summary>
        /// Retorna el canal dado el id.
        /// </summary>
        /// <param name="channelId">ID del canal.</param>
        /// <returns>Retorna el canal si existe o null.</returns>
        public async Task<MessagingChannel?> GetChannelByIdAsync(Guid channelId)
        {
            return await _applicationContext.Channels.FindAsync(channelId);
        }

        /// <summary>
        /// Retorna un canal privado dado dos usuarios.
        /// </summary>
        /// <param name="userIdA">Usuario que pertenece al canal privado.</param>
        /// <param name="userIdB">Usuario que pertence al canal privado.</param>
        /// <returns>Regresa el canal si existe null.</returns>
        public async Task<MessagingChannel?> GetPrivateChannel(string userIdA, string userIdB)
        {
            var channel = await _applicationContext.Channels
                .Where(c => c.IsPrivate)
                .Where(c => c.Users.Any(uc => uc.Id == userIdA) && c.Users.Any(uc => uc.Id == userIdB))
                .FirstOrDefaultAsync();

            return channel;
        }

        /// <summary>
        /// Agrega un usuario a un canal.
        /// </summary>
        /// <param name="channelId">Id del canal.</param>
        /// <param name="userId">Id del usuario.</param>
        /// <returns>No retorna nada, se implementa como Task.</returns>
        public async Task AddUserChannel(Guid channelId, string userId)
        {
            var channel = _applicationContext.Channels.FirstOrDefault(ch => ch.Id == channelId);
            var user = _applicationContext.Users.FirstOrDefault(u => u.Id == userId);

            if (channel != null && user != null)
            {
                channel.Users.Add(user);
                await _applicationContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Agrega un nuevo mensaje.
        /// </summary>
        /// <param name="channelId">Id del canal del mensaje.</param>
        /// <param name="userId">Id del usuario que emite el mensaje.</param>
        /// <param name="message">Mensaje que genera el usuario.</param>
        /// <returns>Retorna el mensaje creado.</returns>
        public async Task<Message?> AddMessage(Guid channelId, string userId, MessageDto message)
        {
            var channel = _applicationContext.Channels
                .Include(ch => ch.Users)
                .FirstOrDefault(ch => ch.Id == channelId);

            var user = _applicationContext.Users.FirstOrDefault(u => u.Id == userId);

            if (channel != null && user != null)
            {
                var msg = new Message
                {
                    Timestamp = message.Timestamp,
                    Channel = channel,
                    Text = message.Text,
                    User = user,
                    IsAction = message.IsAction,
                };

                _applicationContext.Messages.Add(msg);

                foreach (var userChannel in channel.Users.Where(u => !u.Id.Equals(userId)))
                {
                    userChannel.ReceivedMessages.Add(new MessageReceiver
                    {
                        User = userChannel,
                        Message = msg,
                        IsRead = false,
                    });
                }

                await _applicationContext.SaveChangesAsync();

                return msg;
            }

            return null;
        }

        /// <summary>
        /// Crear un canal dado el nombre y los usuarios.
        /// </summary>
        /// <param name="channelName">Nombre del canal.</param>
        /// <param name="users">Usuarios del canal.</param>
        /// <returns>Retorna el canal creado.</returns>
        public async Task<MessagingChannel> CreateChannel(string channelName, List<ApplicationUser> users)
        {
            var newChannel = new MessagingChannel
            {
                IsPrivate = false,
                Name = $"{channelName}",
                Users = users,
            };

            _applicationContext.Channels.Add(newChannel);

            await _applicationContext.SaveChangesAsync();

            return newChannel;
        }

        /// <summary>
        /// Crear un canal privado entre dos usuarios.
        /// </summary>
        /// <param name="userIdA">Usuario A del canal privado.</param>
        /// <param name="userIdB">Usuario B del canal privado.</param>
        /// <returns>Retorna el canal creado.</returns>
        public async Task<MessagingChannel?> CreatePrivateChannel(string userIdA, string userIdB)
        {
            var existsChannel = await GetPrivateChannel(userIdA, userIdB);

            if (existsChannel != null)
            {
                return existsChannel;
            }
            else
            {
                var userA = _applicationContext.Users.FirstOrDefault(u => u.Id == userIdA);
                var userB = _applicationContext.Users.FirstOrDefault(u => u.Id == userIdB);

                if (userA != null && userB != null)
                {
                    var newChannel = new MessagingChannel
                    {
                        IsPrivate = true,
                        Name = $"{userIdA}-{userIdB}",
                        Users = new List<ApplicationUser> { userA, userB },
                    };

                    _applicationContext.Channels.Add(newChannel);

                    await _applicationContext.SaveChangesAsync();

                    return newChannel;
                }
            }

            return null;
        }
    }
}