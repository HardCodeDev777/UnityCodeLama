using UnityEngine;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HardCodeDev.UnityCodeLama
{
    public static class OllamaBase
    {
        public async static Task<string> SendMessage(string modelName, string userPrompt, bool clearThinking)
        {
            var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
            { DisableDefaults = true });

            builder.Services.AddChatClient(new OllamaChatClient(new System.Uri("http://localhost:11434"), modelName));

            var chatClient = builder.Build().Services.GetRequiredService<IChatClient>();
            var chatHistory = new List<ChatMessage>();

            chatHistory.Add(new(ChatRole.System, "Your answer MUSTN'T be more than 290 words!"));

            chatHistory.Add(new(ChatRole.User, userPrompt));

            var chatResponse = "";
            await foreach (var item in chatClient.GetStreamingResponseAsync(chatHistory))
            {
                chatResponse += item.Text;
            }
            chatHistory.Add(new(ChatRole.Assistant, chatResponse));

            if (clearThinking == true)
            {
                var newChatResponse = ClearThinking(chatResponse);
                return newChatResponse;
            }
            else return chatResponse;
        }

        private static string ClearThinking(string input)
        {
            var start = input.IndexOf("<think>");
            var end = input.IndexOf("</think>");
            return input.Remove(start, (end + "</think>".Length) - start); 
        }
    }
}