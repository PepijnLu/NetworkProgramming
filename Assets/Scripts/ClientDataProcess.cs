using System;
using System.Collections.Generic;
using UnityEngine;

public class ClientDataProcess : MonoBehaviour
{
    Dictionary<string, Action<uint, uint[], string[]>> dataProcessing = new();
    public UserInfo userInfo;

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
                userInfo = new()
                {
                    userID = int.Parse(stringData[0]),
                    Username = stringData[1],
                    Email = stringData[2],
                    Country = stringData[3],
                    DateOfBirth = stringData[4]
                };
                //Disable buttons and show user info
                UIManager.instance.ToggleUIElement("LoginScreen", false);
                UIManager.instance.ToggleUIElement("UserInfo", true);
                UIManager.instance.DisplayUserInfo(stringData[1], stringData[2], stringData[3], stringData[4]);
            }
            else
            {
                //Login failed
                Debug.Log($"Login failed: {stringData[0]}");
            }
        };

        dataProcessing["findMatch"] = (success, intData, stringData) =>
        {
            if(success == 1)
            {
                //Match found
                //Setup match
                UIManager.instance.ToggleUIElement("UserInfo", false);
                //ClientBehaviour.instance.SendInt(new uint[2]{(uint)ClientBehaviour.instance.GetUserInfo().userID, intData[0]}, "setupMatch");
            }

        };

        // dataProcessing["cardInfo"] = (success, intData, stringData) =>
        // {
        //     if(success == 1)
        //     {
        //         //Show cards in hand
        //         UIManager.instance.ToggleUIElement("PokerScreen", true);
        //         Poker.instance.GenerateHand((int)intData[0], (int)intData[1]);
        //     }

        // };

        dataProcessing["playerInfo"] = (success, intData, stringData) =>
        {
            if(success == 1)
            {
                //Show chips/bet
                UIManager.instance.ToggleUIElement("PokerScreen", true);
                UIManager.instance.GetTextElementFromDict("Chips").text = "Chips: " + intData[2];
                UIManager.instance.GetTextElementFromDict("Bet").text = "Bet: " + intData[1];
            }
        };

        dataProcessing["getCard"] = (success, intData, stringData) =>
        {
            if(success == 1)
            {
                //Show cards in hand
                UIManager.instance.ToggleUIElement("PokerScreen", true);
                Poker.instance.GenerateHand((int)intData[0], (int)intData[1]);
            }

        };

        
    }
}
