using LChat.GUI.Data;
using Microsoft.AspNetCore.SignalR.Client;

namespace LChat.GUI.ChatService;

public class ChatService : IChatService
{
    private HubConnection? _hubConnection;
    private readonly ILogger<ChatService>? _logger;
    private bool _disposed;

    public bool IsConnected => _hubConnection != null &&
                               _hubConnection.State == HubConnectionState.Connected;

    public event Action<Message>? MessageReceived;

    public ChatService(ILogger<ChatService>? logger = null)
    {
        _logger = logger;
    }

    private void EnsureConnectionInitialized()
    {
        if (_hubConnection == null)
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:32768/Chat")
                .WithAutomaticReconnect([
                    TimeSpan.Zero,
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(30) ])
                .Build();

            ConfigureHubConnection();
        }
    }

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
            _logger?.LogError(exception, "Chat hub connection closed");
            await Task.CompletedTask;
        };

        _hubConnection.On<Message>("Receive", (message) =>
        {
            MessageReceived?.Invoke(message);
        });
    }

    public async Task StartAsync()
    {
        EnsureConnectionInitialized();

        if (_hubConnection!.State == HubConnectionState.Disconnected)
        {
            try
            {
                await _hubConnection.StartAsync();
                _logger?.LogInformation("Chat hub connection started");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error starting chat hub connection");
                throw;
            }
        }
    }

    public Task SendMessageAsync(string message)
    {
        throw new NotImplementedException();
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
