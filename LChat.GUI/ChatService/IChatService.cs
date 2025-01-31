using LChat.GUI.Data;

namespace LChat.GUI.ChatService;

public interface IChatService : IAsyncDisposable
{
    event Action<Message>? MessageReceived;
    Task StartAsync();
    Task SendMessageAsync(string message);
    bool IsConnected { get; }
}
