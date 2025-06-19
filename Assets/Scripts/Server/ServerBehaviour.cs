using System.Collections;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Threading.Tasks;


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
                    ReadDataFromClient(_stream, i);
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

    public void SendDataToClient(int _connectionIndex, string _task, int success, uint[] _intData = null, string[] _stringData = null)
    {
        int dataType = 0;
        if(_intData != null) dataType = 1; 
        else if(_stringData != null) dataType = 2;
        else Debug.LogWarning($"No Datatype found for: {_task}");

        networkDriver.BeginSend(NetworkPipeline.Null, connections[_connectionIndex], out DataStreamWriter _writer);

        //Message Type ID
        _writer.WriteUInt((uint)dataType);
        //Behaviour
        _writer.WriteFixedString32(_task);
        //Success
        _writer.WriteUInt((uint)success);
        //Connection Index

        //Data
        //Int
        if(dataType == 1)
        {
            //Number of Fields
            _writer.WriteUInt((uint)_intData.Length);
            foreach(uint _uint in _intData)
            {
                _writer.WriteUInt(_uint);
            }
        }

        //String
        else if(dataType == 2)
        {
            //Number of Fields
            _writer.WriteUInt((uint)_stringData.Length);
            //Data
            foreach(string _str in _stringData)
            {
                _writer.WriteFixedString32(_str);
            }
        }

        networkDriver.EndSend(_writer);
    }

    void ReadDataFromClient(DataStreamReader _stream, int index)
    {
        uint dataType = _stream.ReadUInt();
        uint numberOfFields = _stream.ReadUInt();
        string behaviour = _stream.ReadFixedString32().ToString();

        //Int
        if(dataType == 1)
        {
            uint[] data = new uint[numberOfFields];

            for (int i = 0; i < numberOfFields; i++)
            {
                data[i] = _stream.ReadUInt();
            }
            GetRequests.instance.RunTask(index, behaviour, _intData: data);
        }

        //String
        if(dataType == 2)
        {
            string[] data = new string[numberOfFields];

            for (int i = 0; i < numberOfFields; i++)
            {
                data[i] = _stream.ReadFixedString32().ToString();
            }
            GetRequests.instance.RunTask(index, behaviour, _stringData: data);
        }
    }
    async void OnDestroy()
    {
        await GetRequests.instance.GetRequest<LoginResponse>($"server_logout.php", false);

        if(networkDriver.IsCreated)
        {
            networkDriver.Dispose();
            connections.Dispose();
        }
    }
}
