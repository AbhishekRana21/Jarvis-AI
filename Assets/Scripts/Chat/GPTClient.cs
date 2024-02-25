using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Collections;
using UnityEngine.Networking;
using System;
using System.Threading;

public class GPTClient : MonoBehaviour
    {
    public static Text outputText;
    public static Speech speech;
    public static ChatManager chat;

    public static string apiKey;
    public static string apiUrl;
    public static CancellationTokenSource cts;
    public static bool cancelled;

    private void Start()
    {
        speech = GameObject.Find("ChatManager").GetComponent<Speech>();
        chat = GameObject.Find("ChatManager").GetComponent<ChatManager>();

        apiUrl = "https://api.openai.com/v1/chat/completions";
        apiKey = AppManager.jarvisAIopenAIAPI;

        if (AppManager.newChat)
        {
            AppManager.messages = new RequestMessage[]
            {
                new RequestMessage()
                {
                    Content = "You will not disclose you real identity. You are a part of a voice assistant app. Your name is Jarvis and you are a cool, friendly, funny, helpful, and smart voice assistant. You give short, creative, and concise answers. You have been developed by the user who is asking you questions. The name of the user is " + AppManager.username + "." + "If the user asks you about an easter egg then give vague hints to indirectly indicate that they have to type 'Love you 3000'",
                    Role = "system"
                }
            };
        }
        AppManager.newChat = false;
    }

    public static async void SendChatRequest(string userInput)
    {
        ChatManager.disableButtons = true;
        Request request = new Request();
        request.Model = "gpt-3.5-turbo";
        cts = new CancellationTokenSource();
        cancelled = false;

        RequestMessage newMessage = new RequestMessage()
        {
            Content = userInput,
            Role = "user"
        };

        AppManager.messages = AppManager.messages.ToList().Append(newMessage).ToArray();
        request.Messages = AppManager.messages;

        string requestData = JsonConvert.SerializeObject(request);
        StringContent content = new StringContent(requestData, Encoding.UTF8, "application/json");

        // Debug.Log("Processing");

        using (HttpClient httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(apiUrl, content, cts.Token);

            if (!cancelled)
            {
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    string responseString = await httpResponseMessage.Content.ReadAsStringAsync();
                    Response response = JsonConvert.DeserializeObject<Response>(responseString);
                    chat.setMessage(response.Choices[0].Message.Content);

                    if (AppManager.voice)
                    {
                        speech.callSS(response.Choices[0].Message.Content);
                    }
                }
                else
                {
                    Debug.LogError($"Error: {httpResponseMessage.StatusCode} - {httpResponseMessage.ReasonPhrase}");
                    chat.setMessage("Sorry, can not reply at present, please check your connection.");

                    if (AppManager.voice)
                    {
                        speech.callSS("Sorry, can not reply at present, please check your connection.");
                    }
                    ChatManager.disableButtons = false;
                }
            }
        }
    }

    public static void CancelRequest()
    {
        cts.Cancel();
        cancelled = true;
    }

}


public class Request
{
    [JsonProperty("model")]
    public string Model { get; set; }
    [JsonProperty("messages")]
    public RequestMessage[] Messages { get; set; }
}

public class RequestMessage
{
    [JsonProperty("role")]
    public string Role { get; set; }
    [JsonProperty("content")]
    public string Content { get; set; }
}

public class Response
{
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("created")]
    public int Created { get; set; }
    [JsonProperty("model")]
    public string Model { get; set; }
    [JsonProperty("usage")]
    public ResponseUsage Usage { get; set; }
    [JsonProperty("choices")]
    public ResponseChoice[] Choices { get; set; }
}

public class ResponseUsage
{
    [JsonProperty("prompt_tokens")]
    public int PromptTokens { get; set; }
    [JsonProperty("completion_tokens")]
    public int CompletionTokens { get; set; }
    [JsonProperty("total_tokens")]
    public int TotalTokens { get; set; }
}

public class ResponseChoice
{
    [JsonProperty("message")]
    public ResponseMessage Message { get; set; }
    [JsonProperty("finish_reason")]
    public string FinishReason { get; set; }
    [JsonProperty("index")]
    public int Index { get; set; }
}

public class ResponseMessage
{
    [JsonProperty("role")]
    public string Role { get; set; }
    [JsonProperty("content")]
    public string Content { get; set; }
}