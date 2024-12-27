using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ChatWYData.SearchEntities;
using ChatWYData.ApiServices;
using OpenAI.Chat;
using ChatWYData.ChatApp.Model;

namespace ChatWYData.ChatApp.Components.Chat;

public partial class Chat
{
    [Inject]
    internal VectorStoreApiService? vectorStoreApiService { get; init; }    
    List<ChatMessageWithDocuments> conversationMessages = new();    
    ElementReference writeMessageElement;
    string? userMessageText;
    internal bool isSearching = false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                await using var module = await JS.InvokeAsync<IJSObjectReference>("import", "./Components/Chat/Chat.razor.js");
                await module.InvokeVoidAsync("submitOnEnter", writeMessageElement);
            }
            catch (JSDisconnectedException)
            {
                // Not an error
            }
        }
    }

    async void SendMessage()
    {
        if (vectorStoreApiService is null) { return; }

        if (!string.IsNullOrWhiteSpace(userMessageText))
        {
            var currentQuestion = userMessageText;
            userMessageText = string.Empty;
            StateHasChanged();

            var chatMessage = new ChatMessageWithDocuments { ChatMessage = new UserChatMessage(currentQuestion) };
            conversationMessages.Add(chatMessage);

            isSearching = true;
            StateHasChanged();

            // run the query
            var searchRequest = new SearchRequest
            {
                Query = currentQuestion,
                Messages = GetSearchMessagesFromCurrentConversationMessages()
            };
            var searchResponse = await vectorStoreApiService.SearchAsync(searchRequest);

            chatMessage = new ChatMessageWithDocuments { 
                ChatMessage = new AssistantChatMessage(searchResponse.ResponseMessage),
                Documents = searchResponse.Documents
            };
            conversationMessages.Add(chatMessage);

            isSearching = false;

            StateHasChanged();
        }
    }

    public List<SearchChatMessage> GetSearchMessagesFromCurrentConversationMessages()
    {
        List<SearchChatMessage> searchMessages = new();
        foreach (var conversationMessage in conversationMessages)
        {
            var searchMessage = new SearchChatMessage
            {
                Content = conversationMessage.ChatMessage.Content[0].Text
            };
            if (conversationMessage.ChatMessage is AssistantChatMessage)
            {
                searchMessage.Role = ChatMessageRole.Assistant;
            }
            else if (conversationMessage.ChatMessage is UserChatMessage)
            {
                searchMessage.Role = ChatMessageRole.User;
            }
            else if (conversationMessage.ChatMessage is SystemChatMessage)
            {
                searchMessage.Role = ChatMessageRole.System;
            }
            searchMessages.Add(searchMessage);
        }
        return searchMessages;
    }
}