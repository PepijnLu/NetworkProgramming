using System;
using System.Collections.Generic;
using UnityEngine.UI;
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
            if (success == 1)
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

        dataProcessing["setPlayerChips"] = (success, intData, stringData) =>
        {
            if (success == 1)
            {
                Poker.instance.userChips = (int)intData[0];
                UIManager.instance.GetTextElementFromDict("YourChips").text = $"Chips: {Poker.instance.userChips}";

                if(Poker.instance.userChips < 20)
                {
                    Debug.LogWarning("Not enough chips to play");
                    StartCoroutine(UIManager.instance.HandleNotEnoughChips());
                    return;
                }

                UIManager.instance.GetUIElementFromDict("PreMatchSetup").SetActive(false);
                UIManager.instance.GetUIElementFromDict("Matchmaking").SetActive(true);
                UIManager.instance.GetUIElementFromDict("LeaveButton").SetActive(true);
                ClientBehaviour.instance.SendInt(new uint[1]{(uint)ClientBehaviour.instance.GetUserInfo().userID}, "findMatch");
            }

        };

        dataProcessing["findMatch"] = (success, intData, stringData) =>
        {
            if (success == 1)
            {
                //Match found
                //Setup match
                Poker.instance.userMatchID = (int)intData[0];    
                UIManager.instance.ToggleUIElement("UserInfo", false);

                //UIManager.instance.GetUIElementFromDict("Lobby").SetActive(true);

                ClientBehaviour.instance.SendInt(new uint[1]{(uint)userInfo.userID}, "setUserChips");
            }

        };

        dataProcessing["getCard"] = (success, intData, stringData) =>
        {
            if (success == 1)
            {
                //Show cards in hand
                UIManager.instance.ToggleUIElement("PokerScreen", true);
                Poker.instance.GenerateHand((int)intData[0], (int)intData[1]);
            }

        };

        dataProcessing["getSharedCard"] = (success, intData, stringData) =>
        {
            if (success == 1)
            {
                //Show cards in hand
                UIManager.instance.ToggleUIElement("PokerScreen", true);
                Poker.instance.GenerateSharedCard((int)intData[0], (int)intData[1]);
            }

        };

        //First int is order in turn, second int is userID
        dataProcessing["setTurnOrder"] = (success, intData, stringData) =>
        {
            if (success == 1)
            {
                UIManager.instance.GetUIElementFromDict("Lobby").SetActive(false);
                
                Debug.Log($"Setting turn order for user: {(int)intData[1]}");
                //Set turn order locally
                PokerPlayer newPlayer = new()
                {
                    orderInTurn = (int)intData[0],
                    userID = (int)intData[1]
                };
                Poker.instance.playersByUserID.Add(intData[1], newPlayer);

                Debug.Log($"trying to get element from dict: P{(int)intData[0] + 1}Icon");
                GameObject playerIcon = UIManager.instance.GetUIElementFromDict($"P{(int)intData[0] + 1}Icon");
                playerIcon.transform.parent.gameObject.SetActive(true);

                //Check if its you
                if(intData[1] == userInfo.userID)
                {
                    //Enable 'you' text
                    Poker.instance.StartPokerRound();
                    playerIcon.transform.GetChild(1).gameObject.SetActive(true);
                    //Get your current chip amount from database
                }
            }

        };
        //First int is userID, second int is betAmount
        dataProcessing["setBet"] = (success, intData, stringData) =>
        {
            if (success == 1)
            {
                //Set bet amount locally
                int newBetAmount = (int)intData[1];
                Poker.instance.playersByUserID[intData[0]].betAmount = newBetAmount;
                bool newBetIsLower = false;
                foreach(var kvp in Poker.instance.playersByUserID)
                {
                    if(kvp.Value.betAmount > newBetAmount)
                    {
                        newBetIsLower = true;
                    }
                }
                if(newBetIsLower) 
                {
                    Debug.LogWarning($"New bet {newBetAmount} is LOWER than existing bet");
                    return;
                }
                else Poker.instance.currentMatchBet = newBetAmount;

                //Do UI shit
                int playerTurnOrder = Poker.instance.playersByUserID[intData[0]].orderInTurn + 1;
                Debug.Log($"Trying to set bets for userID: {intData[0]}, order in turn {playerTurnOrder}, betAmount: {intData[1]}");
                UIManager.instance.GetTextElementFromDict($"P{playerTurnOrder}Bet").text = $"Bet: {intData[1]}";
            }
        };
        //userID of new turn player
        dataProcessing["startPlayerTurn"] = (success, intData, stringData) =>
        {
            if (success == 1)
            {
                int playerTurnOrder = Poker.instance.playersByUserID[intData[0]].orderInTurn + 1;

                Debug.Log($"Trying to set start player for userID: {intData[0]}, order in turn {playerTurnOrder}");
                for(int i  = 1; i < 5; i++)
                {
                    if(i == playerTurnOrder) UIManager.instance.GetUIElementFromDict($"P{i}Icon").GetComponent<Image>().color = new Color(0, 0.7f, 1);
                    else UIManager.instance.GetUIElementFromDict($"P{i}Icon").GetComponent<Image>().color = new Color(1, 1, 1);
                }

                //Check if its you
                if(intData[0] == userInfo.userID)
                {
                    UIManager.instance.GetTextElementFromDict("TurnText").text = $"Your turn!";
                    Poker.instance.isYourTurn = true;
                    UIManager.instance.GetUIElementFromDict("BetButtons").SetActive(true);

                }
                else
                {
                    UIManager.instance.GetTextElementFromDict("TurnText").text = $"Player {playerTurnOrder}'s turn!";
                }
                
                
            }
        };

        dataProcessing["setLobbyStatus"] = (success, intData, stringData) =>
        {
            UIManager.instance.GetUIElementFromDict("Matchmaking").SetActive(false);
            UIManager.instance.GetUIElementFromDict("Lobby").SetActive(true);
            UIManager.instance.GetTextElementFromDict("LobbyStatus").text = stringData[0];
        };  

        dataProcessing["disableLeaveButton"] = (success, intData, stringData) =>
        {
            UIManager.instance.GetUIElementFromDict("LeaveButton").SetActive(false);
        }; 

    }
}
