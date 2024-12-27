using OpenAI.Chat;
using System.Text.Json.Serialization;

namespace ChatWYData.SearchEntities;

// NOTE: we currently do not have a way to serialize the OpenAI.Chat.ChatMessage
// so this one is the one that we are using, but we should change it to the OpenAI.Chat.ChatMessage
// once we have a way to serialize it.

public class SearchChatMessage
{
    [JsonConstructor]
    public SearchChatMessage()
    {
        Role = ChatMessageRole.User;
        Content = string.Empty;
    }

    [JsonPropertyName("Role")]
    public ChatMessageRole Role { get; set; }

    [JsonPropertyName("Content")]
    public string Content { get; set; }
    //public ChatMessageContent Content { get; set; }
}

