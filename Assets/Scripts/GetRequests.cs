using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using System;
using UnityEditor.Rendering;

public class GetRequests : MonoBehaviour
{
    public static GetRequests instance;
    [SerializeField] ServerBehaviour serverBehaviour;
    TextMeshProUGUI roundText;
    Dictionary<int, UserInfo> connectedUsers = new();
    List<uint> setupMatches = new();
    private Dictionary<uint, PokerMatch> pokerMatches = new();

    [Header("InitialLogin")]
    [SerializeField] int serverID;
    [SerializeField] string password;
    [SerializeField] string baseUrl;
    [Header("TicTacToe Options")]
    [SerializeField] List<string> ticTacToeOptions;
    Dictionary<string, Func<(int _connectionIndex, uint[], string[]), Task>> getRequests;    
    void Awake()
    {
        instance = this;   
        InstantiateDictionary();
    }

    public async Task RunTask(int _connectionIndex, string _task, uint[] _intData = null, string[] _stringData = null)
    {
        if(getRequests.ContainsKey(_task)) await getRequests[_task]( (_connectionIndex, _intData, _stringData) );
        else Debug.LogWarning($"Task {_task} does not exist.");
    }
    async void Start()
    {
        //Index page
        //GetRequest(baseUrl);

        //Start server session
        LoginResponse response = await GetRequest<LoginResponse>($"server_login.php?Server_ID={serverID}&Server_pass={MD5Helper.EncryptToMD5(password)}&delOld=true");
        if (!response.success) Debug.LogError($"Server Login failed: {response.message}");

        Debug.Log($"Server login successful! Session ID: {response.sessionId}");
    }

    //Add date of birth support, password might not be getting added properly
    public async Task RegisterUser(string _username, string _password, string _email, string _country, string _dateOfBirth = "")
    {
        //Login
        LoginResponse response = await GetRequest<LoginResponse>($"register_user.php?User={_username}&Email={_email}&Pass={MD5Helper.EncryptToMD5(_password)}&Country={_country}");
        if (!response.success) { Debug.LogError($"Registration failed: {response.message}"); return; }

        Debug.Log($"User registered successfully!");

    }

    public async Task<T> GetRequest<T>(string _request, bool returnData = true)
    {
        string request = baseUrl + _request;
        Debug.Log($"Requested: {request}");
        using (UnityWebRequest _www = UnityWebRequest.Get(request))
        {
            var operation = _www.SendWebRequest();
            while (!operation.isDone) await Task.Yield(); // wait a frame

            if(!returnData) return default;

            if (_www.result == UnityWebRequest.Result.Success)
            {
                T result = ProcessData<T>(_www.downloadHandler.text);

                // Optionally, you can do logging or debugging here if T is known
                return result;
            }
            else
            {
                Debug.LogError($"Error: {_www.error}");
                return default;
            }
        }
    }

    T ProcessData<T>(string _data)
    {
        Debug.Log($"Received: {_data}");
        T processedData = JsonConvert.DeserializeObject<T>(_data);
        return processedData;
    }

    public async void SetPlayerChips(int userID, int newChipAmount)
    {
        SingleInt success = await GetRequest<SingleInt>($"poker.php?behaviour=2&UserID={userID}&NewAmount={newChipAmount}");
        if(success.value == 0) Debug.LogWarning("error setting chips");
    }

    void InstantiateDictionary()
    {
        getRequests = new();

        getRequests["loginUser"] = async (args) =>
        {
            string _username = args.Item3[0];
            string _password = args.Item3[1];

            //Login
            LoginResponse response = await GetRequest<LoginResponse>($"user_login.php?User={_username}&pass={MD5Helper.EncryptToMD5(_password)}");
            if (!response.success)
            {
                Debug.Log($"Login failed: {response.message}");
                string[] errorMessage = new string[1] { response.message };
                serverBehaviour.SendDataToClient(args.Item1, "loginUser", 0, _stringData: errorMessage);
                return;
            }

            Debug.Log($"User login successful!");

            //Get user info
            UserInfo userInfo = await GetRequest<UserInfo>("user_info.php");

            if(connectedUsers.ContainsKey(userInfo.userID))
            {
                Debug.LogWarning("User already connected");
                return;
            }
            connectedUsers.Add(userInfo.userID, userInfo);
            string[] userInfoArray = new string[5] { userInfo.userID.ToString(), userInfo.Username, userInfo.Email, userInfo.Country, userInfo.DateOfBirth };
            serverBehaviour.SendDataToClient(args.Item1, "loginUser", 1, _stringData: userInfoArray);
        };

        //Gets match ID back
        getRequests["findMatch"] = async (args) =>
        {
            Debug.Log($"Now matchmaking!");
            SingleInt matchFound = new();
            while (matchFound.value == 0)
            {
                matchFound = await GetRequest<SingleInt>($"find_match.php?behaviour=2&UserID={args.Item2[0]}");
                await Task.Delay(1000); // Wait 1 second
            }

            Debug.Log($"Match Found! Match ID: {matchFound.value}");

            //Create dictionary entry if it doesnt exist yet
            //if(!playersInMatches.ContainsKey((uint)matchFound.value)) playersInMatches.Add((uint)matchFound.value, new List<PokerPlayer>());

            if (!pokerMatches.ContainsKey((uint)matchFound.value)) pokerMatches.Add((uint)matchFound.value, new PokerMatch());

            //Add user to dictionary
            PokerPlayer newPlayer = new()
            {
                userID = (int)args.Item2[0],
                connectionID = args.Item1
            };
            //playersInMatches[(uint)matchFound.value].Add(newPlayer);  
            pokerMatches[(uint)matchFound.value].connectedPlayers.Add(newPlayer);


            uint[] matchID = new uint[2] { (uint)matchFound.value, 0 };
            serverBehaviour.SendDataToClient(args.Item1, "findMatch", 1, _intData: matchID);

            //Call setup match
            if (!setupMatches.Contains(matchID[0]))
            {
                await RunTask(args.Item1, "waitForPlayers", matchID);
                if (!setupMatches.Contains(matchID[0]))
                {
                    foreach(PokerPlayer _player in pokerMatches[(uint)matchFound.value].connectedPlayers)
                    {
                        serverBehaviour.SendDataToClient(_player.connectionID, "setLobbyStatus", 1, _stringData: new string[1] { "Waiting for players..." });
                    }
                }
            }
        };

        getRequests["waitForPlayers"] = async (args) =>
        {
            uint matchID = args.Item2[0];
            if (setupMatches.Contains(matchID) && args.Item2[1] == 0) return;
            if(pokerMatches[matchID].waitingForPlayersComplete) return;
            setupMatches.Add(matchID);
            //Check if all players are connected (5 means ready to start)
            SingleInt waitForPlayers = new();
            waitForPlayers = await GetRequest<SingleInt>($"find_match.php?behaviour=1&MatchID={matchID}&connPlayers={waitForPlayers.value2}");

            Debug.Log($"Waiting: totalPlayersInMatch: {waitForPlayers.value3}, connectedPlayers: {waitForPlayers.value2}");

            if(waitForPlayers.value2 < 2)
            {
                await Task.Delay(1000);
                Debug.Log($"Less than 2 players in match: {matchID}");
                foreach(PokerPlayer _player in pokerMatches[matchID].connectedPlayers)
                {
                    serverBehaviour.SendDataToClient(_player.connectionID, "setLobbyStatus", 1, _stringData: new string[1] {$"FindingPlayers"});
                }
                await RunTask(args.Item1, "waitForPlayers", new uint[2] { matchID, 1 });
                return;
            }

            Debug.Log("2 or more players");

            switch (waitForPlayers.value)
            {
                //Wait longer, not all players have connected
                case 1:
                    Debug.Log("Not all players connected");
                    await Task.Delay(500); // Wait .5 second
                    await RunTask(args.Item1, "waitForPlayers", new uint[2] { matchID, 1 });
                    break;
                //Wait 10s to allow more people to join (set to 3 for testing)
                case 2:
                    Debug.Log("Allowing more people to join");
                    //Set lobby usernames
                    // foreach(PokerPlayer _player in pokerMatches[matchID].playersInMatch)
                    // {
                    //     serverBehaviour.SendDataToClient(_player.connectionID, "setLobbyStatus", 1, _stringData: new string[2] { $"Starting in: {(10 - tenSecondTimer)}" });
                    // }

                    int tenSecondTimer = 0;
                    while (tenSecondTimer < 10)
                    {
                        if(pokerMatches[matchID].waitingForPlayersComplete) return;
                        //Change timer

                        Debug.Log("Updating 10 second timer");
                        foreach(PokerPlayer _player in pokerMatches[matchID].connectedPlayers)
                        {
                            serverBehaviour.SendDataToClient(_player.connectionID, "setLobbyStatus", 1, _stringData: new string[1] { $"Starting in: {(10 - tenSecondTimer)}" });
                        }

                        if(tenSecondTimer == 7)
                        {
                            //Disable leave button
                            pokerMatches[matchID].playersInRound.Clear();
                            foreach(PokerPlayer _player in pokerMatches[matchID].connectedPlayers)
                            {
                                pokerMatches[matchID].playersInRound.Add(_player);
                                serverBehaviour.SendDataToClient(_player.connectionID, "disableLeaveButton", 1, _intData: new uint[0] { });
                            }
                        }

                        await Task.Delay(1000);

                        waitForPlayers = await GetRequest<SingleInt>($"find_match.php?behaviour=1&MatchID={matchID}&connPlayers={waitForPlayers.value}");
                        if ((waitForPlayers.value != 2) || (waitForPlayers.value2 < 2)) 
                        {
                            Debug.Log("Restarting timer");
                            await RunTask(args.Item1, "waitForPlayers", new uint[2] { matchID, 1 });
                        }
                        tenSecondTimer++;
                    }

                    //Setup match
                    Debug.Log("Setup match timer");
                    await RunTask(args.Item1, "setupMatch", new uint[1] { matchID });
                    break;
                //Start in 5 seconds
                case 3:
                    Debug.Log("Starting in 5");
                    //Set lobby usernames
                    // foreach(PokerPlayer _player in pokerMatches[matchID].playersInMatch)
                    // {
                    //     serverBehaviour.SendDataToClient(_player.connectionID, "setLobbyStatus", 1, _stringData: new string[2] { $"Starting in: {(10 - tenSecondTimer)}" });
                    // }
                    int fiveSecondTimer = 0;
                    while (fiveSecondTimer < 5)
                    {
                        if(pokerMatches[matchID].waitingForPlayersComplete) return;
                        //Check if no one left

                        if(fiveSecondTimer == 2)
                        {
                            //Disable leave button
                            pokerMatches[matchID].playersInRound.Clear();
                            foreach(PokerPlayer _player in pokerMatches[matchID].connectedPlayers)
                            {
                                pokerMatches[matchID].playersInRound.Add(_player);
                                serverBehaviour.SendDataToClient(_player.connectionID, "disableLeaveButton", 1, _intData: new uint[0] { });
                            }
                        }
                        //Change timer
                        Debug.Log("Updating 5 second timer");
                        foreach(PokerPlayer _player in pokerMatches[matchID].connectedPlayers)
                        {
                            serverBehaviour.SendDataToClient(_player.connectionID, "setLobbyStatus", 1, _stringData: new string[1] { $"Starting in: {(5 - fiveSecondTimer)}" });
                        }

                        await Task.Delay(1000);
                        fiveSecondTimer++;
                    }

                    Debug.Log("Setup match timer");
                    //Setup match
                    await RunTask(args.Item1, "setupMatch", new uint[1] { matchID });
                    break;
            }
        };

        getRequests["setupMatch"] = async (args) =>
        {
            Debug.Log("Setup match!");

            uint matchID = args.Item2[0];
            pokerMatches[matchID].waitingForPlayersComplete = true;

            //Initialize list of poker players
            List<PokerPlayer> connectedPlayers = pokerMatches[matchID].playersInRound;

            foreach (PokerPlayer pokerPlayer in connectedPlayers)
            {
                pokerMatches[matchID].playersByUserID.Add((uint)pokerPlayer.userID, pokerPlayer);
            }

            //Make a deck with enough cards for each player + the shared cards
            List<PokerCard> deck = Poker.instance.GetShuffledCards(connectedPlayers.Count);

            pokerMatches[matchID].matchDeck = deck;

            //Deal cards

            foreach (PokerPlayer _player in connectedPlayers)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (pokerMatches[matchID].matchDeck.Count == 0)
                    {
                        Debug.LogError($"Deck empty while dealing to player {_player.connectionID}!");
                        return;
                    }

                    if (i == 0)
                    {
                        _player.handCard1 = pokerMatches[matchID].matchDeck[0].cardID;
                    }
                    else
                    {
                        _player.handCard2 = pokerMatches[matchID].matchDeck[0].cardID;
                    }
                    pokerMatches[matchID].matchDeck.RemoveAt(0);
                }
                serverBehaviour.SendDataToClient(_player.connectionID, "getCard", 1, _intData: new uint[2] { (uint)_player.handCard1, (uint)_player.handCard2 });
            }

            //should still be 5 cards left in the deck
            Debug.Log($"Cards left in deck for match {matchID}: {pokerMatches[matchID].matchDeck.Count}");

            //Set random turn order
            int playerOne = UnityEngine.Random.Range(0, connectedPlayers.Count);

            //randomize turn order
            for (int i = connectedPlayers.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (connectedPlayers[i], connectedPlayers[j]) = (connectedPlayers[j], connectedPlayers[i]); // swap
            }

            pokerMatches[matchID].playersInRound = connectedPlayers;
            pokerMatches[matchID].bettingPlayers = connectedPlayers;

            foreach (PokerPlayer _player in connectedPlayers)
            {
                SingleInt setWaiting = await GetRequest<SingleInt>($"poker.php?behaviour=4&UserID={_player.userID}&MatchID={matchID}&Waiting={0}");
            }

            for (int i = 0; i < connectedPlayers.Count; i++)
            {
                foreach (PokerPlayer _player in connectedPlayers)
                {
                    //UserID, betAmount
                    //First int is order in turn, second int is userID
                    serverBehaviour.SendDataToClient(_player.connectionID, "setTurnOrder", 1, _intData: new uint[2] { (uint)i, (uint)connectedPlayers[i].userID });
                }
            }

            Debug.Log("Set turn order!");

            //set game state
            pokerMatches[matchID].gameState = GAME_STATE.PRE_FLOP;

            Debug.Log("Set game state!");

            //small and big blind on server
            connectedPlayers[0].betAmount = 20;
            connectedPlayers[1].betAmount = 40;

            pokerMatches[matchID].currentBet = 40;
            //Set big blind to "last raise"
            pokerMatches[matchID].lastRaiseUserID = connectedPlayers[1].userID;

            //small and big blind to clients
            for (int i = 0; i < 2; i++)
            {
                //Set small blind and big blind
                foreach (PokerPlayer _player in connectedPlayers)
                {
                    //UserID, betAmount
                    serverBehaviour.SendDataToClient(_player.connectionID, "setBet", 1, _intData: new uint[2] { (uint)connectedPlayers[i].userID, (uint)(20 + (i * 20)) });
                }
            }

            Debug.Log("Set blinds!");

            //set current turn
            if(connectedPlayers.Count <= 2) pokerMatches[matchID].currentTurnUserID = connectedPlayers[0].userID;
            else pokerMatches[matchID].currentTurnUserID = connectedPlayers[2].userID;

            
            foreach (PokerPlayer _player in connectedPlayers)
            {
                //user id of new turn player
                serverBehaviour.SendDataToClient(_player.connectionID, "startPlayerTurn", 1, _intData: new uint[1] { (uint)pokerMatches[matchID].currentTurnUserID });
            }

            Debug.Log("Set current turn, setup complete!");
        };

        //first int is userID, second int is matchID, third int is action(1: fold 2: call 3: raise), fourth int is amount bet
        getRequests["playTurn"] = async (args) =>
        {
            uint userID = args.Item2[0];
            uint matchID = args.Item2[1];

            Debug.Log($"Start handling turn for {userID}");

            PokerMatch pokerMatch = pokerMatches[matchID];
            PokerPlayer pokerPlayer = pokerMatch.playersByUserID[userID];

            int indexOfCurrentPlayer = pokerMatch.bettingPlayers.IndexOf(pokerPlayer);
            int indexOfNextPlayer = 0;

            if (pokerMatch.bettingPlayers.Count - 1 < indexOfCurrentPlayer + 1) indexOfNextPlayer = 0;
            else indexOfNextPlayer = indexOfCurrentPlayer + 1;

            int userIDOfNextPlayer = pokerMatch.bettingPlayers[indexOfNextPlayer].userID;

            switch (args.Item2[2])
            {
                //fold
                case 1:
                    Debug.Log("Turn: Fold");
                    pokerMatch.bettingPlayers.Remove(pokerPlayer);
                    foreach (PokerPlayer _player in pokerMatch.playersInRound)
                    {
                        //UserID, betAmount
                        serverBehaviour.SendDataToClient(_player.connectionID, "setBet", 1, _intData: new uint[2] { (uint)pokerPlayer.userID, 0 });
                    }
                    if (pokerMatch.bettingPlayers.Count <= 1)
                    {
                        Poker.instance.EndPokerRound(pokerMatch);
                        return;
                    }
                    break;
                //call
                case 2:
                    Debug.Log("Turn: Call");
                    foreach (PokerPlayer _player in pokerMatch.playersInRound)
                    {
                        //UserID, betAmount
                        serverBehaviour.SendDataToClient(_player.connectionID, "setBet", 1, _intData: new uint[2] { (uint)pokerPlayer.userID, (uint)pokerMatch.currentBet });
                    }
                    break;
                //raise
                case 3:
                    Debug.Log("Turn: Raise");
                    uint newBet = args.Item2[3];
                    foreach (PokerPlayer _player in pokerMatch.playersInRound)
                    {
                        //UserID, betAmount
                        serverBehaviour.SendDataToClient(_player.connectionID, "setBet", 1, _intData: new uint[2] { (uint)pokerPlayer.userID, newBet });
                    }
                    pokerMatch.currentBet = (int)newBet;
                    pokerMatch.lastRaiseUserID = pokerPlayer.userID;
                    break;
                default:
                    Debug.LogError($"Action non existant: ID: {args.Item2[2]}");
                    break;
            }
            pokerPlayer.betAmount = (int)args.Item2[3];
            Debug.Log("User input handling complete");
            //Check for round end / change game state

            bool advanceGameState = false;
            pokerMatch.currentTurnUserID = userIDOfNextPlayer;

            if(args.Item2[2] != 3)
            {
                Debug.Log($"Current User ID: {pokerPlayer.userID}, next user ID: {userIDOfNextPlayer}, last raised user ID: {pokerMatch.lastRaiseUserID}");

                //If true, big blind gets opportunity to raise again
                PokerPlayer nextPlayer = pokerMatch.playersByUserID[(uint)userIDOfNextPlayer];
                //Debug.Log($"cond1: {pokerMatch.gameState} , {GAME_STATE.PRE_FLOP} - cond2: {pokerMatch.playersInMatch.IndexOf(nextPlayer)} , {1} - cond3: {pokerMatches[matchID].currentBet} , {40}");
                if(pokerMatch.gameState == GAME_STATE.PRE_FLOP)
                {
                    if(pokerMatch.playersInRound.IndexOf(nextPlayer) == 1 && (pokerMatches[matchID].currentBet == 40) && (!pokerMatch.bigBlindReraised) && (nextPlayer.betAmount == pokerMatches[matchID].currentBet))
                    {
                        //big blind gets to reraise
                        pokerMatch.lastRaiseUserID = userIDOfNextPlayer;
                        pokerMatch.bigBlindReraised = true;
                        Debug.Log("Big blind gets to reraise");
                    }
                    else if (pokerMatch.lastRaiseUserID == userIDOfNextPlayer)
                    {
                        advanceGameState = true;
                    }
                }
                else if(pokerMatch.gameState == GAME_STATE.SHOWDOWN)
                {
                    if (pokerMatch.lastRaiseUserID == userIDOfNextPlayer)
                    {
                        Poker.instance.EndPokerRound(pokerMatch);
                        return;
                    }
                }
                else
                {
                    if (pokerMatch.lastRaiseUserID == userIDOfNextPlayer)
                    {
                        advanceGameState = true;
                    }
                }


            }
            
            if(advanceGameState)
            {
                //Move to the next round
                pokerMatch.gameState = (GAME_STATE)((int)pokerMatch.gameState + 1);
                pokerMatch.lastRaiseUserID = pokerMatch.playersInRound[0].userID;
                pokerMatch.currentTurnUserID = pokerMatch.playersInRound[0].userID;
                switch (pokerMatch.gameState)
                {
                    //Reveal cards 1,2,3
                    case GAME_STATE.FLOP:
                        foreach (PokerPlayer _player in pokerMatch.playersInRound)
                        {
                            //cardNumber, cardID
                            serverBehaviour.SendDataToClient(_player.connectionID, "getSharedCard", 1, _intData: new uint[2] { 1,  (uint)pokerMatches[matchID].matchDeck[0].cardID});
                        }
                        foreach (PokerPlayer _player in pokerMatch.playersInRound)
                        {
                            //cardNumber, cardID
                            serverBehaviour.SendDataToClient(_player.connectionID, "getSharedCard", 1, _intData: new uint[2] { 2,  (uint)pokerMatches[matchID].matchDeck[1].cardID});
                        }
                        foreach (PokerPlayer _player in pokerMatch.playersInRound)
                        {
                            //cardNumber, cardID
                            serverBehaviour.SendDataToClient(_player.connectionID, "getSharedCard", 1, _intData: new uint[2] { 3,  (uint)pokerMatches[matchID].matchDeck[2].cardID});
                        }

                        break;
                    //Reveal card 4
                    case GAME_STATE.TURN:
                        foreach (PokerPlayer _player in pokerMatch.playersInRound)
                        {
                            //cardNumber, cardID
                            serverBehaviour.SendDataToClient(_player.connectionID, "getSharedCard", 1, _intData: new uint[2] { 4,  (uint)pokerMatches[matchID].matchDeck[3].cardID});
                        }
                        break;
                    //Reveal card 5
                    case GAME_STATE.RIVER:
                        foreach (PokerPlayer _player in pokerMatch.playersInRound)
                        {
                            //cardNumber, cardID
                            serverBehaviour.SendDataToClient(_player.connectionID, "getSharedCard", 1, _intData: new uint[2] { 5,  (uint)pokerMatches[matchID].matchDeck[4].cardID});
                        }
                        break;
                }
            }

            //Start next turn
            foreach (PokerPlayer _player in pokerMatch.playersInRound)
            {
                //user id of new turn player
                serverBehaviour.SendDataToClient(_player.connectionID, "startPlayerTurn", 1, _intData: new uint[1] { (uint)pokerMatch.currentTurnUserID });
            }

            Debug.Log($"Handling turn complete for {userID}");
        };
        //first int is userID
        getRequests["preMatchSetup"] = async (args) =>
        {
            uint userID = args.Item2[0];
            SingleInt newUserChipsRequest = await GetRequest<SingleInt>($"poker.php?behaviour=1&UserID={userID}");
            int newUserChips = newUserChipsRequest.value;
            serverBehaviour.SendDataToClient(args.Item1, "setPlayerChips", 1, _intData: new uint[1] { (uint)newUserChips });
        };

        //first int is userID, second int is matchID, third int is chip amount
        getRequests["setUserChips"] = async (args) =>
        {
            uint userID = args.Item2[0];
            uint matchID = args.Item2[1];
            uint chipAmount = args.Item2[2];

            pokerMatches[matchID].playersByUserID[userID].userChips = (int)chipAmount;
        };

        //first int is userID, second int is matchID
        getRequests["leaveMatch"] = async (args) =>
        {
            uint userID = args.Item2[0];
            uint matchID = args.Item2[1];

            Debug.Log($"Leave Request for player {userID} in match {matchID}");

            PokerPlayer playerToRemove = null;

            foreach(PokerPlayer _player in pokerMatches[matchID].playersInRound)
            {
                if(_player.userID == userID)
                {
                    playerToRemove = _player;
                }
            }

            if(playerToRemove != null) pokerMatches[matchID].playersInRound.Remove(playerToRemove);
            
            SingleInt leaveMatchRequest = await GetRequest<SingleInt>($"poker.php?behaviour=3&UserID={userID}&MatchID={matchID}");

            Debug.Log($"Leave Request for player {userID} in match {matchID} complete");
        };

    }
}
