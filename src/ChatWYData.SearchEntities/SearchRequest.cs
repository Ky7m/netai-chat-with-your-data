using OpenAI.Chat;
using System.Text.Json.Serialization;

namespace ChatWYData.SearchEntities;

public class SearchRequest(string query = "", int maxResults = 2, int minScore = 80, List<SearchChatMessage> messages = null)
{
    public string Query { get; set; } = query;
    public int MaxResults { get; set; } = maxResults;
    public int MinScore { get; set; } = minScore;

    [JsonPropertyName("messages")]
    public List<SearchChatMessage> Messages { get; set; } = messages ?? [];

    public static List<ChatMessage> GetChatMessagesFromSearchMessages(List<SearchChatMessage> searchMessages)
    {
        List<ChatMessage> messages = [];
        foreach (SearchChatMessage searchMessage in searchMessages)
        {
            if (searchMessage.Role == ChatMessageRole.User)
            {
                messages.Add(new UserChatMessage(searchMessage.Content));
            }
            else if (searchMessage.Role == ChatMessageRole.Assistant)
            {
                messages.Add(new AssistantChatMessage(searchMessage.Content));
            }
            else if (searchMessage.Role == ChatMessageRole.System)
            {
                messages.Add(new SystemChatMessage(searchMessage.Content));
            }
        }
        return messages;
    }

    public static List<SearchChatMessage> GetSearchChatMessagesFromMessages(List<ChatMessage> messages)
    {
        List<SearchChatMessage> searchMessages = [];
        foreach (ChatMessage message in messages)
        {
            var searchMessage = new SearchChatMessage
            {
                Content = message.Content[0].Text
            };
            if (message is AssistantChatMessage)
            {
                searchMessage.Role = ChatMessageRole.Assistant; 
            }
            else if (message is UserChatMessage)
            {
                searchMessage.Role = ChatMessageRole.User;
            }
            else if (message is SystemChatMessage)
            {
                searchMessage.Role = ChatMessageRole.System;
            }
            searchMessages.Add(searchMessage);
        }
        return searchMessages;
    }
}

