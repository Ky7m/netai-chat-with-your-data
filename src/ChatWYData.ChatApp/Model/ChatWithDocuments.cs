using ChatWYData.DataEntities;
using OpenAI.Chat;

namespace ChatWYData.ChatApp.Model;

public class ChatMessageWithDocuments
{
    public ChatMessage ChatMessage { get; set; } = null;
    public DateTime ChatMessageDateTime { get; set; } = DateTime.Now;
    public List<Document> Documents { get; set; } = new();

    public void Add(UserChatMessage incomingUserMessage)
    { 
        ChatMessage = incomingUserMessage;
    }

    public void Add(AssistantChatMessage incomingAssistantMessage)
    {
        ChatMessage = incomingAssistantMessage;
    }
}