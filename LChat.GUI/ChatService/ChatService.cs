using LChat.GUI.Data;
using Microsoft.AspNetCore.SignalR.Client;

namespace LChat.GUI.ChatService;

public class ChatService(ILogger<ChatService>? logger = null) : IChatService
{
    private HubConnection? _hubConnection;
    private readonly ILogger<ChatService>? _logger = logger;
    private bool _disposed;

    private HubConnection Connection
    {
        get
        {
            if (_hubConnection == null)
            {
                _hubConnection = new HubConnectionBuilder()
                                     .WithUrl("https://localhost:32768/Chat")
                                     .WithAutomaticReconnect([
                                         TimeSpan.Zero,
                                         TimeSpan.FromSeconds(2),
                                         TimeSpan.FromSeconds(10),
                                         TimeSpan.FromSeconds(30)])
                                     .Build();

                ConfigureHubConnection();
            }

            return _hubConnection;
        }
    }

    public bool IsConnected => _hubConnection != null &&
                               _hubConnection.State == HubConnectionState.Connected;

    public event Action<Message>? MessageReceived;

    private void ConfigureHubConnection()
    {
        if (_hubConnection == null)
        {
            return;
        }

        _hubConnection.Reconnected += async _ =>
        {
            _logger?.LogInformation("Reconnected to chat hub");
            await Task.CompletedTask;
        };

        _hubConnection.Reconnecting += async exception =>
        {
            _logger?.LogWarning(exception, "Chat hub connection lost, attempting to reconnect...");
            await Task.CompletedTask;
        };

        _hubConnection.Closed += async exception =>
        {
            var content = $"Disconnected! {exception?.Message}";
            _logger?.LogError(content);
            MessageReceived?.Invoke(ConstructErrorMessage(content));
            await Task.CompletedTask;
        };

        _hubConnection.On<Message>("Receive", (message) =>
        {
            MessageReceived?.Invoke(message);
        });
    }

    public async Task StartAsync()
    {
        try
        {
            await Connection.StartAsync();
            _logger?.LogInformation("Chat hub connection started");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error starting chat hub connection");
            MessageReceived?.Invoke(ConstructErrorMessage(ex.Message));
        }
    }

    public async Task SendMessageAsync(string content)
    {
        if (!IsConnected)
        {
            _logger?.LogWarning("No connection, send ignored.");

            return;
        }

        try
        {
            var message = new Message(MessageType.Client, content, DateTime.Now);
            await Connection.SendAsync("SendChat", message);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error sending message to the hub");
            MessageReceived?.Invoke(ConstructErrorMessage(ex.Message));
        }
    }

    private static Message ConstructErrorMessage(string content)
    {
        var message = new Message(MessageType.Error, content, DateTime.Now);

        return message;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            if(_hubConnection == null)
            {
                return;
            }

            await _hubConnection.DisposeAsync();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error disposing chat service");
        }
    }
}
