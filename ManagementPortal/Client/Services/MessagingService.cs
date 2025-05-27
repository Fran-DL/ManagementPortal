using ManagementPortal.Client.Services;
using ManagementPortal.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

/// <summary>
/// Servicio para implementar mensajería por SignalR.
/// </summary>
public class MessagingService
{
    private readonly CustomAuthStateProvider _customAuthStateProvider;
    private HubConnection? _connection;
    private string _hubUrl;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessagingService"/> class.
    /// Inicializa una nueva instancia de la clase <see cref="MessagingService"/>.
    /// </summary>
    /// <param name="navigationManager">NavigationManager para obtener la url base.</param>
    /// <param name="customAuthStateProvider">Servicio para manejar la expiración de la sesión.</param>
    public MessagingService(NavigationManager navigationManager, CustomAuthStateProvider customAuthStateProvider)
    {
        _customAuthStateProvider = customAuthStateProvider;
        var baseUri = navigationManager.BaseUri;
        var hubUrl = $"{baseUri}chathub";
        _hubUrl = hubUrl;
        UnreadMessages = new();
    }

    /// <summary>
    /// Evento para notificar de mensajes leidos.
    /// </summary>
    public event Action? MessagesChanged;

    /// <summary>
    /// Identifica el canal activo en el chat.
    /// </summary>
    public MessagingChannelDto ActiveChannel { get; set; } = new();

    /// <summary>
    /// Lista de mensajes sin leer de la mensajería.
    /// </summary>
    public List<MessageDto> UnreadMessages { get; set; }

    /// <summary>
    /// Crear la conexión al hub del backend.
    /// </summary>
    /// <param name="token">Token del usuario logueado.</param>
    public void CreateHubConnection(string token)
    {
        var connectionBuilder = new HubConnectionBuilder()
            .WithUrl(_hubUrl, options =>
            {
                if (!string.IsNullOrEmpty(token))
                {
                    options.AccessTokenProvider = () => Task.FromResult(token ?? null);
                }
            });

        _connection = connectionBuilder.Build();
    }

    /// <summary>
    /// Metodo para enviar un mensaje a un canal.
    /// </summary>
    /// <param name="channelId">Id del canal.</param>
    /// <param name="message">Dto del mensaje.</param>
    /// <returns>No regresa nada, se implementa como Task.</returns>
    public async Task SendMessageToChannel(string channelId, MessageDto message)
    {
        if (_connection != null)
        {
            await _connection.InvokeAsync("SendMessageToChannel", channelId, message);
        }
    }

    /// <summary>
    /// Iniciar la conexión con el hub.
    /// </summary>
    /// <returns>No retorna nada, se implementa como Task.</returns>
    public async Task StartAsync()
    {
        if (_connection != null && _connection.State != HubConnectionState.Connected)
        {
            _connection.Closed += async (exception) =>
            {
                if (exception != null && exception.Message.Contains("Unauthorized"))
                {
                    _customAuthStateProvider.NotifyTokenExpired(false);
                }
                else
                {
                    await _connection.StartAsync();
                }
            };

            await _connection.StartAsync();
        }
    }

    /// <summary>
    /// Metodo para cerrar la conexion del Hub.
    /// </summary>
    /// <returns>Cierra la conexion del Hub.</returns>
    public async Task DisposeHubConnection()
    {
        if (_connection != null)
        {
            await _connection.StopAsync();
            await _connection.DisposeAsync();
        }
    }

    /// <summary>
    /// Carga mensajes del usuario.
    /// </summary>
    /// <param name="handler">Acción que ejecuta cuando los mensajes fueron cargados.</param>
    public void LoadMessages(Action<List<MessageDto>> handler)
    {
        if (_connection != null)
        {
            _connection.On<List<MessageDto>>("LoadMessages", handler);
        }
    }

    /// <summary>
    /// Recibe la notificación de que el mensaje fue leído.
    /// </summary>
    /// <param name="handler">Acción que ejecuta cuando los mensajes fueron cargados.</param>
    public void LoadIsReadMessage(Action<Guid> handler)
    {
        if (_connection != null)
        {
            _connection.On<Guid>("LoadIsReadMessage", handler);
        }
    }

    /// <summary>
    /// Carga notificaciones del usuario.
    /// </summary>
    /// <param name="handler">Acción que ejecuta cuando los mensajes fueron cargados.</param>
    public void LoadUnreadMessages(Action<List<MessageDto>> handler)
    {
        if (_connection != null)
        {
            _connection.On<List<MessageDto>>("LoadUnreadMessages", handler);
        }
    }

    /// <summary>
    /// Carga notificaciones del usuario.
    /// </summary>
    /// <param name="handler">Acción que ejecuta cuando los mensajes fueron cargados.</param>
    public void LoadNotification(Action<List<MessageDto>> handler)
    {
        if (_connection != null)
        {
            _connection.On<List<MessageDto>>("LoadNotification", handler);
        }
    }

    /// <summary>
    /// Carga notificaciones del usuario (primera vez).
    /// </summary>
    /// <param name="handler">Acción que ejecuta cuando los mensajes fueron cargados.</param>
    public void LoadNotifications(Action<List<MessageDto>> handler)
    {
        if (_connection != null)
        {
            _connection.On<List<MessageDto>>("LoadNotifications", handler);
        }
    }

    /// <summary>
    /// Escuchar por canales del usuario.
    /// </summary>
    /// <param name="handler">Acción que ejecuta cuando los mensajes fueron cargados.</param>
    public void LoadChannels(Action<List<MessagingChannelDto>> handler)
    {
        if (_connection != null)
        {
            _connection.On<List<MessagingChannelDto>>("LoadChannels", handler);
        }
    }

    /// <summary>
    /// Escuchar por nuevo canal del usuario.
    /// </summary>
    /// <param name="handler">Acción que ejecuta cuando los mensajes fueron cargados.</param>
    public void LoadChannel(Action<MessagingChannelDto> handler)
    {
        if (_connection != null)
        {
            _connection.On<MessagingChannelDto>("LoadChannel", handler);
        }
    }

    /// <summary>
    /// Obtiene canales del usuario.
    /// </summary>
    /// <returns>No retorna nada, se implementa como Task.</returns>
    public async Task GetChannels()
    {
        if (_connection != null)
        {
            await _connection.InvokeAsync("GetChannels");
        }
    }

    /// <summary>
    /// Ingresar a un canal.
    /// </summary>
    /// <param name="channelId">Id del canal.</param>
    /// <returns>No retorna nada, se implementa como Task.</returns>
    public async Task JoinChannel(string channelId)
    {
        if (_connection != null)
        {
            await _connection.InvokeAsync("JoinChannel", channelId);
        }
    }

    /// <summary>
    /// Ingresar a un canal privado.
    /// </summary>
    /// <param name="userId">Id del otro usuario del canal privado.</param>
    /// <returns>No retorna nada, se implementa como Task.</returns>
    public async Task JoinPrivateChannel(string userId)
    {
        if (_connection != null)
        {
            await _connection.InvokeAsync("JoinPrivateChannel", userId);
        }
    }

    /// <summary>
    /// Crear un grupo para varios usuarios.
    /// </summary>
    /// <param name="channelName">Nombre del grupo.</param>
    /// <param name="users">Usuarios del grupo.</param>
    /// <returns>Retorna el grupo creado.</returns>
    public async Task CreateChannel(string channelName, List<ApplicationUserDto> users)
    {
        if (_connection != null)
        {
            await _connection.InvokeAsync("CreateChannel", channelName, users);
        }
    }

    /// <summary>
    /// Agregaga un listado de usuarios a un canal.
    /// </summary>
    /// <param name="channelId">Id del canal.</param>
    /// <param name="users">Listado de usuarios.</param>
    /// <returns>Agrega los usuarios al grupo.</returns>
    public async Task AddUsersToChannel(Guid channelId, List<ApplicationUserDto> users)
    {
        if (_connection != null)
        {
            await _connection.InvokeAsync("AddUsersToChannel", channelId, users);
        }
    }

    /// <summary>
    /// Cargar canales privados del usuario.
    /// </summary>
    /// <param name="handler">Acción que ejecuta cuando los mensajes fueron cargados.</param>
    public void LoadPrivateChannel(Action<string> handler)
    {
        if (_connection != null)
        {
            _connection.On<string>("LoadPrivateChannel", handler);
        }
    }

    /// <summary>
    /// Abandonar el canal.
    /// </summary>
    /// <param name="channelId">Id del canal a abandonar por el usuario.</param>
    /// <returns>No retorna nada, se implementa como Task.</returns>
    public async Task LeaveChannel(string channelId)
    {
        if (_connection != null)
        {
            await _connection.InvokeAsync("LeaveChannel", channelId);
        }
    }

    /// <summary>
    /// Marcar un mensaje como leido.
    /// </summary>
    /// <param name="messages">Id del mensaje.</param>
    /// <param name="channelid">Id del canal.</param>
    /// <returns>No retorna nada, se implementa como Task.</returns>
    public async Task MarkMessagesAsRead(List<Guid> messages, string channelid)
    {
        if (_connection != null)
        {
            await _connection.InvokeAsync("MarkMessagesAsRead", messages, channelid);
        }
    }

    /// <summary>
    /// Obtiene notificaciones del usuario (primera vez).
    /// </summary>
    /// <returns>No retorna nada, se implementa como Task.</returns>
    public async Task GetNotifications()
    {
        if (_connection != null)
        {
            await _connection.InvokeAsync("GetNotifications");
        }
    }

    /// <summary>
    /// Metodo para agregar un mensaje a las notificaciones.
    /// </summary>
    /// <param name="message">Message para agregar a la colección de notificaciones.</param>
    public void AddUnreadMessage(MessageDto message)
    {
        UnreadMessages.Add(message);
        MessagesChanged?.Invoke();
    }

    /// <summary>
    /// Metodo para remover un mensaje de las notificaciones dado su Id.
    /// </summary>
    /// <param name="messageId">Id del usuario a eliminar de la colección.</param>
    public void RemoveUnreadMessage(Guid messageId)
    {
        UnreadMessages.RemoveAll(m => m.Id.Equals(messageId));
        MessagesChanged?.Invoke();
    }

    /// <summary>
    /// Metodo para desuscribirse de todos los handlers.
    /// </summary>
    public void UnsubscribeAll()
    {
        if (_connection != null)
        {
            _connection.Remove("LoadChannels");
            _connection.Remove("LoadChannel");
            _connection.Remove("LoadUnreadMessages");
            _connection.Remove("LoadMessages");
            _connection.Remove("LoadIsReadMessage");
        }
    }
}