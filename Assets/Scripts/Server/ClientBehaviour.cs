using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;

public class ClientBehaviour : MonoBehaviour
{
    NetworkDriver networkDriver;
    NetworkConnection connection;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        networkDriver = NetworkDriver.Create();

        var endpoint = NetworkEndpoint.LoopbackIpv4.WithPort(7777);
        connection = networkDriver.Connect(endpoint);
    }

    // Update is called once per frame
    void Update()
    {
        networkDriver.ScheduleUpdate().Complete();
        if(!connection.IsCreated)
        {
            return;
        }
        DataStreamReader _stream;
        NetworkEvent.Type _cmd;
        while((_cmd = connection.PopEvent(networkDriver, out _stream)) != NetworkEvent.Type.Empty)
        {
            if(_cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("Now connected to server");
                uint _value = 1;
                networkDriver.BeginSend(connection, out DataStreamWriter _writer);
                _writer.WriteUInt(_value);
                networkDriver.EndSend(_writer);
            }
            else if(_cmd == NetworkEvent.Type.Data)
            {
                uint _value = _stream.ReadUInt();
                Debug.Log($"Got value {_value} back from server");
                connection.Disconnect(networkDriver);
                connection = default;
            }
            else if(_cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client got disconnected from server");
                connection = default;
            }
        }
    }

    void OnDestroy()
    {
        networkDriver.Dispose();
    }
}
