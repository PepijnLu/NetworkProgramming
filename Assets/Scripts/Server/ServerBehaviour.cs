using System.Collections;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;


public class ServerBehaviour : MonoBehaviour
{
    NetworkDriver networkDriver;
    NativeList<NetworkConnection> connections;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        networkDriver = NetworkDriver.Create();

        connections = new(16, Allocator.Persistent);
        NetworkEndpoint endpoint = NetworkEndpoint.AnyIpv4.WithPort(7777);  
        if(networkDriver.Bind(endpoint) != 0)
        {
            Debug.LogError("Failed to bind to port 7777");
            return;
        }
        networkDriver.Listen();

        StartCoroutine(GetRequest());
    }

    // Update is called once per frame
    void Update()
    {
        networkDriver.ScheduleUpdate().Complete();
        for (int i = 0; i < connections.Length; i++)
        {
            if(!connections[i].IsCreated)
            {
                connections.RemoveAtSwapBack(i);
                i--;
            }
        }

        NetworkConnection _connection;
        while((_connection = networkDriver.Accept()) != default)
        {
            connections.Add(_connection);
            Debug.Log("Accepted a connection");
        }
        for (int i = 0; i < connections.Length; i++)
        {
            DataStreamReader _stream;
            NetworkEvent.Type _cmd;
            while((_cmd = networkDriver.PopEventForConnection(connections[i], out _stream)) != NetworkEvent.Type.Empty)
            {
                if(_cmd == NetworkEvent.Type.Data)
                {
                    uint _number = _stream.ReadUInt();
                    Debug.Log($"Got {_number} from a client, adding 2 to it");
                    _number += 2;
                    networkDriver.BeginSend(NetworkPipeline.Null, connections[i], out DataStreamWriter _writer);
                    _writer.WriteUInt(_number);
                    networkDriver.EndSend(_writer);
                }
                else if(_cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected from server");
                    connections[i] = default;
                    break;
                }
            }
        }
    }

    void OnDestroy()
    {
        if(networkDriver.IsCreated)
        {
            networkDriver.Dispose();
            connections.Dispose();
        }
    }

    IEnumerator GetRequest()
    {
        using(UnityWebRequest _www = UnityWebRequest.Get("https://studenthome.hku.nl/~pepijn.luchtmeijer"))
        {
            yield return _www.SendWebRequest();
            if(_www.result == UnityWebRequest.Result.Success)
            {
                ProcessData(_www.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"Error: {_www.error}");
            }
        }

    }

    void ProcessData(string _data)
    {
        Debug.Log($"Recieved: {_data}");
        RootData processedData = JsonConvert.DeserializeObject<RootData>(_data);

        foreach(ScoreEntry _se in processedData.Last5Scores)
        {
            Debug.Log($"Username: {_se.Username} - Score: {_se.Score} - Date: {_se.ScoredAt}");
        }
    }
}
