namespace LChat.GUI.Data;

public record Message(MessageType Type, string Content, DateTime Timestamp);

public enum MessageType 
{
    None,
    LLM,
    Client,
    Meta,
    Error,
    Log
}
