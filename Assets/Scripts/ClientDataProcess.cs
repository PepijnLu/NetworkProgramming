using System;
using System.Collections.Generic;
using UnityEngine;

public class ClientDataProcess : MonoBehaviour
{
    Dictionary<string, Action<uint, uint[], string[]>> dataProcessing = new();

    void Awake()
    {
        InstantiateDictionary();   
    }
    public void ProcessData(string _behaviour, uint success, uint[] intData = null, string[] stringData = null)
    {
        dataProcessing[_behaviour](success, intData, stringData);
    }

    void InstantiateDictionary()
    {
        dataProcessing["loginUser"] = (success, intData, stringData) =>
        {
            if(success == 1)
            {
                //Login success
                //Disable buttons and show user info
                UIManager.instance.ToggleUIElement("LoginScreen", false);
                UIManager.instance.ToggleUIElement("UserInfo", true);
                UIManager.instance.DisplayUserInfo(stringData[0], stringData[1], stringData[2], stringData[3]);
            }
            else
            {
                //Login failed
                Debug.Log($"Login failed: {stringData[0]}");
            }
        };
    }
}
