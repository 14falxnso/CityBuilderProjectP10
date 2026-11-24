using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class ServerConnection : MonoBehaviour
{
    private readonly byte[] dhPrime = Convert.FromBase64String("//////////+t+FRYortKmq/cViAnPTzx2LnFg84tNpWp4TZBFGQz+8yTnc4kmz75fS/jY2MMddj2gbICrsRhetPfHtXV/WVhJDP1H18GbtCFY2VVPe0a87VXE15/V8k1mE8McODmi3fipona8+/och3xWKE2rec1MKzKT0g6eXq8CrGCsyT7YdEIqUuyyOP7uWrat2DX9GgdT0Kj3jlN9K5W7edjcrsZCwenyO4KbXCeAvzhzffi7MA0BM0oNC9hkXL+nOmFg/+OTxIy7vKBg8P+OxtMb61zO7X8vC7CIAXFjvGDfRaDssbzSibBsu/6iGtCOGEoXJf//////////w==");
    private readonly byte[] dhGenerator = { 0x02 };
    
    
    [SerializeField] private string path = "/city_builder";
    [SerializeField] private string testIp = "localhost";
    [SerializeField] private bool debugConnect = false;
    
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
        if (debugConnect)
        {
            bool ok = await Connect(testIp + ":25565", 5000);
            Debug.Log(ok ? "Connected to: " + url : "Failed to connect.", this);
        
            if (ok) Debug.Log(await SendRequest<test, test>(new test()));
        }
    }
    
    
    [Serializable]
    private class test {}
    
 
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


    public async Task<T> SendRequest<T, U>(U data)
    {
        RequestPayload<U> payload = new RequestPayload<U>
        {
            type = typeof(T).Name, // event type is name of requested class
            data = data
        };
        
        int key_id = 0;
        Key key = new Key{success = false};
        
        byte[] private_key = new byte[32];
        RandomNumberGenerator.Fill(private_key);

        while (!key.success)
        {
            key_id = Random.Range(int.MinValue, int.MaxValue);
            
            key = await RequestKey(key_id, private_key);
        }

        byte[] shared_secret = ModPow(Convert.FromBase64String(key.public_key), private_key, dhPrime, 256);

        using SHA256 sha = SHA256.Create();
        
        var encryption_key = Convert.ToBase64String(sha.ComputeHash(shared_secret));
        

        RequestPayload<SecureData> secure_payload = new RequestPayload<SecureData>
        {
            type = "secure",
            data = new SecureData
            {
                key_id = key_id,
                content = EncodeString(JsonUtility.ToJson(payload), encryption_key)
            }
        };
        
        using UnityWebRequest request = new UnityWebRequest(url + "/requests", UnityWebRequest.kHttpVerbPOST);
        
        byte[] body = Encoding.UTF8.GetBytes(JsonUtility.ToJson(secure_payload));
        
        request.uploadHandler = new UploadHandlerRaw(body);
        
        request.downloadHandler = new DownloadHandlerBuffer();
        
        request.SetRequestHeader("Content-Type", "application/json");
        
        await request.SendWebRequest();
        
        return JsonUtility.FromJson<T>(request.downloadHandler.text);
    }

    private async Task<Key> RequestKey(int key_id, byte[] private_key)
    {
        byte[] public_key = ModPow(dhGenerator, private_key, dhPrime, 256);
        
        RequestPayload<KeyId> payload = new RequestPayload<KeyId>
        {
            type = "getKey",
            data = new KeyId
            {
                key_id = key_id,
                public_key = Convert.ToBase64String(public_key)
            }
        };

        using UnityWebRequest request = new UnityWebRequest(url + "/requests", UnityWebRequest.kHttpVerbPOST);
        
        byte[] body = Encoding.UTF8.GetBytes(JsonUtility.ToJson(payload));
        
        request.uploadHandler = new UploadHandlerRaw(body);
        
        request.downloadHandler = new DownloadHandlerBuffer();
        
        request.SetRequestHeader("Content-Type", "application/json");
        
        await request.SendWebRequest();
        
        return JsonUtility.FromJson<Key>(request.downloadHandler.text);
    }


    private string EncodeString(string str, string base64Key)
    {
        byte[] key = Convert.FromBase64String(base64Key);
        byte[] hmac_key;

        using (SHA256 sha = SHA256.Create())
            hmac_key = sha.ComputeHash(Combine(Encoding.UTF8.GetBytes("HMAC"), key));

        byte[] iv = new byte[16];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            rng.GetBytes(iv);

        byte[] ciphertext;

        using (Aes aes = Aes.Create())
        {
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key;
            aes.IV = iv;

            using (ICryptoTransform encryptor = aes.CreateEncryptor())
                ciphertext = encryptor.TransformFinalBlock(
                    Encoding.UTF8.GetBytes(str), 0, 
                    Encoding.UTF8.GetByteCount(str));
        }
        
        byte[] data = Combine(iv, ciphertext);

        byte[] mac;
        using (HMACSHA256 hmac = new HMACSHA256(hmac_key))
            mac = hmac.ComputeHash(data);
        
        return Convert.ToBase64String(Combine(data, mac));
    }

    private byte[] Combine(byte[] a, byte[] b)
    {
        byte[] result = new byte[a.Length + b.Length];
        Buffer.BlockCopy(a, 0, result, 0, a.Length);
        Buffer.BlockCopy(b, 0, result, a.Length, b.Length);
        return result;
    }

    private byte[] ModPow(byte[] gen, byte[] power, byte[] mod, int outputBytes)
    {
        BigInteger result = BigInteger.ModPow(
            new BigInteger(gen, true, true),
            new BigInteger(power, true, true),
            new BigInteger(mod, true, true));
        
        byte[] raw = result.ToByteArray(true, true);
        
        byte[] output = new byte[outputBytes];
        
        Buffer.BlockCopy(raw, 0, output, outputBytes - raw.Length, raw.Length);
        
        return output;
    }


    [Serializable]
    private class RequestPayload<T>
    {
        public string type;
        public T data;
    }

    [Serializable]
    private class SecureData
    {
        public int key_id;
        public string content;
    }

    [Serializable]
    private class Key
    {
        public bool success;
        public string public_key;
    }

    [Serializable]
    private class KeyId
    {
        public int key_id;
        public string public_key;
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
