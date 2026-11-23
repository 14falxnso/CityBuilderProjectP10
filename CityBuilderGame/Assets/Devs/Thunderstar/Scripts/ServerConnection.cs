using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ServerConnection : MonoBehaviour
{
    [SerializeField] private string path = "/city_builder";
    
    private string url;
    
    private HttpClient client;
    private HttpResponseMessage response;
    private StreamReader reader;
    
    
    private static ServerConnection instance;

    public static ServerConnection Instance
    {
        get
        {
            if (instance) return instance;
            
            GameObject go = new GameObject("[Server Connection]");
            instance = go.AddComponent<ServerConnection>();
            return instance;
        }
    }


    private class SseEvent
    {
        public string Type;
        public string Data;
    }


    [Serializable]
    private class HelloData
    {
        public bool ok;
    }
    

    private void Awake()
    {
        if (instance != this)
        {
            if (instance)
            {
                Destroy(this);
            }
            else
            {
                instance = this;
            }
        }
        
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        Disconnect();
    }


    private async void Start()
    {
        bool ok = await Connect("localhost:25565", 5000);
        Debug.Log(ok ? "Connected to: " + url : "Failed to connect.", this);
    }
    
 
    public async Task<bool> Connect(string ip, int timeout)
    {
        Task<bool> connect_task = ConnectAsync(ip);
        Task completed = await Task.WhenAny(connect_task, Task.Delay(timeout));
 
        if (completed == connect_task)
            return await connect_task;
 
        Debug.Log("Connection timed out.", this);
        Disconnect();
        return false;
    }

    private async Task<bool> ConnectAsync(string ip)
    {
        try
        {
            url = "http://" + ip + path;

            client = new HttpClient();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url + "/events");

            request.Headers.Accept.ParseAdd("text/event-stream");

            response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            Stream stream = await response.Content.ReadAsStreamAsync();
            reader = new StreamReader(stream);

            SseEvent message = await ReadSseEvent();

            if (message == null)
            {
                Disconnect();
                return false;
            }

            if (message.Type == "hello")
            {

                HelloData hello = JsonUtility.FromJson<HelloData>(message.Data);

                if (hello is { ok: true })
                {
                    _ = ListenForEvents();

                    return true;
                }
            }

            Disconnect();
            return false;
        }
        catch (Exception e)
        {
            Debug.LogWarning(e.Message);
            
            Disconnect();
            return false;
        }
    }

    public void Disconnect()
    {
        url = null;
        
        reader?.Dispose();
        reader = null;
        
        response?.Dispose();
        response = null;
        
        client?.Dispose();
        client = null;
        
        Debug.Log("Disconnected.", this);
    }


    private async Task<SseEvent> ReadSseEvent()
    {
        string type = "message";
        StringBuilder data = new StringBuilder();
        bool has_data = false;

        
        for (;;)
        {
            string line = await reader.ReadLineAsync();
            
            if (line == null) return null;

            if (line.Length == 0)
            {
                if (has_data)
                {
                    return new SseEvent
                    {
                        Type = type,
                        Data = data.ToString()
                    };
                }
                
                type = "message";
                continue;
            }
            
            if (line.StartsWith(":")) continue;
            
            int separator = line.IndexOf(':');

            string field = separator >= 0 ? line.Substring(0, separator) : "";
            
            string value = separator >= 0 ? line.Substring(separator + 1) : "";
            
            if (value.StartsWith(" ")) value = value.Substring(1);

            switch (field)
            {
                case "event":
                    type = value;
                    break;
                    
                case "data":
                    if (has_data) data.Append('\n');
                    data.Append(value);
                    has_data = true;
                    break;
            }
        }
    }

    private async Task ListenForEvents()
    {
        while (url != null)
        {
            SseEvent message = await ReadSseEvent();
            
            if (message == null) break;
            
            HandleMessage(message.Type, message.Data);
        }
        
        Disconnect();
    }

    private void HandleMessage(string type, string data)
    {
        Debug.Log(type + ": " + data);

        switch (type)
        {
            case "bye":
                Disconnect();
                break;
        }
    }
}
