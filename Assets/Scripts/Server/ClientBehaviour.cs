using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;

public class ClientBehaviour : MonoBehaviour
{
    public static ClientBehaviour instance;
    [SerializeField] ServerBehaviour serverBehaviour;
    [SerializeField] ClientDataProcess clientDataProcess;
    NetworkDriver networkDriver;
    NetworkConnection connection;

    void Awake()
    {
        instance = this;
    }
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

                SendInt(new uint[1]{7}, "debugInt");
                SendString(new string[2]{"Hello", "World"}, "debugString");
            }
            else if(_cmd == NetworkEvent.Type.Data)
            {
                // uint _value = _stream.ReadUInt();
                // Debug.Log($"Got value {_value} back from server");
                ReadDataFromServer(_stream);
                //connection.Disconnect(networkDriver);
                //connection = default;
            }
            else if(_cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client got disconnected from server");
                connection = default;
            }
        }
    }

    public void SendInt(uint[] _value, string _behaviour)
    {
        networkDriver.BeginSend(connection, out DataStreamWriter _writer);

        //Message Type ID
        _writer.WriteUInt(1);
        //Number of Fields
        _writer.WriteUInt((uint)_value.Length);
        //Behaviour
        _writer.WriteFixedString32(_behaviour);
        //Data
        foreach(int _val in _value) _writer.WriteUInt((uint)_val);

        networkDriver.EndSend(_writer);
    }

    public void SendString(string[] _string, string _behaviour)
    {
        networkDriver.BeginSend(connection, out DataStreamWriter _writer);
        //Message Type ID
        _writer.WriteUInt(2);
        //Number of Fields
        _writer.WriteUInt((uint)_string.Length);
        //Behaviour
        _writer.WriteFixedString32(_behaviour);
        //Data
        foreach(string _str in _string) _writer.WriteFixedString32(_str);

        networkDriver.EndSend(_writer);
    }

    void ReadDataFromServer(DataStreamReader _stream)
    {
        uint dataType = _stream.ReadUInt();
        string behaviour = _stream.ReadFixedString32().ToString();
        uint success = _stream.ReadUInt();
        uint numberOfFields = _stream.ReadUInt();
        Debug.Log($"Got following data from server: dataType = {dataType}, success = {success}, numberOfFields = {numberOfFields}, behaviour = {behaviour}");

        //Int
        if(dataType == 1)
        {
            uint[] data = new uint[numberOfFields];

            for (int i = 0; i < numberOfFields; i++)
            {
                data[i] = _stream.ReadUInt();
            }

            clientDataProcess.ProcessData(behaviour, success, intData: data);

            // foreach(uint _uint in data)
            // {
            //     Debug.Log($"Data: {_uint}");
            // };  
            //GetRequests.instance.RunTask(index, behaviour, _intData: data);
        }

        //String
        if(dataType == 2)
        {
            string[] data = new string[numberOfFields];

            for (int i = 0; i < numberOfFields; i++)
            {
                data[i] = _stream.ReadFixedString32().ToString();
            }

            clientDataProcess.ProcessData(behaviour, success, stringData: data);

            // foreach(string _str in data)
            // {
            //     Debug.Log($"Data: {_str}");
            // };  
            //GetRequests.instance.RunTask(index, behaviour, _stringData: data);
        }
    }

    void OnDestroy()
    {
        if(networkDriver.IsCreated)
        {
            networkDriver.Dispose();
        }
    }
}
