using Zenject;
using IPA.Loader;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Actions
{
    public struct HttpResponse
    {
        public bool Successful { get; set; }
        public string? Content { get; set; }

        public HttpResponse(bool success, string? content)
        {
            Successful = success;
            Content = content;
        }
    }

    internal sealed class Http
    {
        internal Dictionary<string, string> PersistentRequestHeaders { get; private set; }

        internal Http([Inject(Id = nameof(Actions))] PluginMetadata metadata)
        {
            PersistentRequestHeaders = new Dictionary<string, string>();
            string userAgent = $"{metadata.Name}/{metadata.Version}";
            PersistentRequestHeaders.Add("User-Agent", userAgent);
        }

        internal async Task SendHttpAsyncRequest(UnityWebRequest request)
        {
            foreach (var header in PersistentRequestHeaders)
                request.SetRequestHeader(header.Key, header.Value);
            AsyncOperation asyncOperation = request.SendWebRequest();
            while (!asyncOperation.isDone)
                await Task.Delay(100);
        }

        internal async Task<HttpResponse> GetAsync(string url, string? authBearerKey = null, string? clientID = null!)
        {
            using UnityWebRequest request = UnityWebRequest.Get(url);
            if (authBearerKey != null)
                request.SetRequestHeader("Authorization", $"Bearer {authBearerKey}");
            if (clientID != null)
                request.SetRequestHeader("Client-ID", clientID);
            request.timeout = 15;
            await SendHttpAsyncRequest(request);
            return new HttpResponse(!(request.isNetworkError || request.isHttpError), Encoding.UTF8.GetString(request.downloadHandler.data));
        }

        internal async Task<HttpResponse> PostAsync(string url, string body, string? authBearerKey = null)
        {
            using UnityWebRequest request = body == null ? UnityWebRequest.Post(url, body) : UnityWebRequest.Put(url, body);
            if (authBearerKey != null)
                request.SetRequestHeader("Authorization", $"Bearer {authBearerKey}");
            if (body != null)
            {
                request.SetRequestHeader("Content-Type", "application/json");
                request.method = "POST"; // WHAT THE FUCK, UNITY?!
            }
            request.timeout = 15;
            await SendHttpAsyncRequest(request);
            return new HttpResponse(!(request.isNetworkError || request.isHttpError), Encoding.UTF8.GetString(request.downloadHandler.data));
        }
    }
}