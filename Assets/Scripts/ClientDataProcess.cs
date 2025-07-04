using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ClientDataProcess : MonoBehaviour
{
    Dictionary<string, Action<uint, uint[], string[]>> dataProcessing = new();
    public UserInfo userInfo;
    [SerializeField] PokerClient pokerClient;

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
                };
                //Disable buttons and show user info
                UIManager.instance.ToggleUIElement("LoginScreen", false);
                UIManager.instance.ToggleUIElement("UserInfo", true);
                pokerClient.FetchTop5UserScores(int.Parse(stringData[0]));
                UIManager.instance.DisplayUserInfo(stringData[1], stringData[2], stringData[3]);
            }
            else
            {
                //Login failed
                StartCoroutine(UIManager.instance.ShowTextForSeconds("LoginError", stringData[0], 2));
                Debug.Log($"Login failed: {stringData[0]}");
            }
        };

        dataProcessing["registerUser"] = (success, intData, stringData) =>
        {
            if(success == 1)
            {
                UIManager.instance.ToggleUIElement("RegisterScreen", false);
                UIManager.instance.ToggleUIElement("LoginScreen", true);
            }
        };

        dataProcessing["setPlayerChips"] = (success, intData, stringData) =>
        {
            if (success == 1)
            {
                pokerClient.userChips = (int)intData[0];
                UIManager.instance.GetTextElementFromDict("YourChips").text = $"Chips: {pokerClient.userChips}";

                if(pokerClient.userChips < 20)
                {
                    Debug.LogWarning("Not enough chips to play");
                    StartCoroutine(UIManager.instance.HandleNotEnoughChips());
                    return;
                }

                UIManager.instance.GetUIElementFromDict("PreMatchSetup").SetActive(false);
                UIManager.instance.GetUIElementFromDict("Matchmaking").SetActive(true);
                Debug.Log($"Set player chips to {pokerClient.userChips}");
                UIManager.instance.GetTextElementFromDict("MMChips").text = $"Chips: {pokerClient.userChips}";
                //UIManager.instance.GetUIElementFromDict("LeaveButton").SetActive(true);
                ClientBehaviour.instance.SendInt(new uint[1]{(uint)ClientBehaviour.instance.GetUserInfo().userID}, "findMatch");
            }

        };

        dataProcessing["findMatch"] = (success, intData, stringData) =>
        {
            if (success == 1)
            {
                //Match found
                //Setup match
                pokerClient.userMatchID = (int)intData[0];    
                UIManager.instance.ToggleUIElement("UserInfo", false);

                //UIManager.instance.GetUIElementFromDict("Lobby").SetActive(true);
            }

        };

        dataProcessing["getCard"] = (success, intData, stringData) =>
        {
            if (success == 1)
            {
                //Show cards in hand
                Debug.Log($"Got Cards: {(int)intData[0]}, {(int)intData[1]}");
                UIManager.instance.ToggleUIElement("PokerScreen", true);
                pokerClient.GenerateHand((int)intData[0], (int)intData[1]);
            }

        };

        dataProcessing["getSharedCard"] = (success, intData, stringData) =>
        {
            if (success == 1)
            {
                //Show cards in hand
                UIManager.instance.ToggleUIElement("PokerScreen", true);
                pokerClient.GenerateSharedCard((int)intData[0], (int)intData[1]);
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
                pokerClient.playersByUserID.Add(intData[1], newPlayer);

                Debug.Log($"trying to get element from dict: P{(int)intData[0] + 1}Icon");
                GameObject playerIcon = UIManager.instance.GetUIElementFromDict($"P{(int)intData[0] + 1}Icon");
                playerIcon.transform.parent.gameObject.SetActive(true);

                //Check if its you
                if(intData[1] == userInfo.userID)
                {
                    //Enable 'you' text
                    ClientBehaviour.instance.SendInt(new uint[3]{(uint)userInfo.userID, (uint)pokerClient.userMatchID, (uint)pokerClient.userChips}, "setUserChips");
                    pokerClient.StartPokerRound();
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
                pokerClient.playersByUserID[intData[0]].betAmount = newBetAmount;
                bool newBetIsLower = false;
                foreach(var kvp in pokerClient.playersByUserID)
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
                else pokerClient.currentMatchBet = newBetAmount;
                //Check if its you
                if(intData[0] == userInfo.userID)
                {
                    pokerClient.HandleBet((int)intData[1]);
                }

                //Do UI shit
                int playerTurnOrder = pokerClient.playersByUserID[intData[0]].orderInTurn + 1;
                Debug.Log($"Trying to set bets for userID: {intData[0]}, order in turn {playerTurnOrder}, betAmount: {intData[1]}");
                UIManager.instance.GetTextElementFromDict($"P{playerTurnOrder}Bet").text = $"Bet: {intData[1]}";
            }
        };
        //userID of new turn player
        dataProcessing["startPlayerTurn"] = (success, intData, stringData) =>
        {
            if (success == 1)
            {
                int playerTurnOrder = pokerClient.playersByUserID[intData[0]].orderInTurn + 1;

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
                    pokerClient.isYourTurn = true;
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
            string newStatus = stringData[0];
            Debug.Log($"Processing lobby status data: {newStatus}");
            if(newStatus == "FindingPlayers")
            {
                UIManager.instance.GetUIElementFromDict("Matchmaking").SetActive(true);
                UIManager.instance.GetUIElementFromDict("Lobby").SetActive(false);
            }
            else if(newStatus == "toUserInfo")
            {
                pokerClient.ResetMatchClient();
            }
            else
            {
                UIManager.instance.GetUIElementFromDict("Matchmaking").SetActive(false);
                UIManager.instance.GetUIElementFromDict("Lobby").SetActive(true);
                UIManager.instance.GetTextElementFromDict("LChips").text = $"Chips: {pokerClient.userChips}";
                UIManager.instance.GetTextElementFromDict("LobbyStatus").text = newStatus;
            }
        };  

        dataProcessing["disableLeaveButton"] = (success, intData, stringData) =>
        {
            UIManager.instance.GetUIElementFromDict("LeaveButton").SetActive(false);
        }; 

        dataProcessing["addChips"] = (success, intData, stringData) =>
        {
            pokerClient.userChips += (int)intData[0];
        }; 

        dataProcessing["fetchUserScores"] = (success, intData, stringData) =>
        {
            UIManager.instance.DisplayUserScores(intData);
        };

    }
}
