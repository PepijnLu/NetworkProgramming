using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using System;

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

    // public async Task AwaitMatchInput()
    // {
    //     //Wait until its your players turn
    //     if((!TicTacToe.instance.isCrosses && TicTacToe.instance.firstRound) || (!TicTacToe.instance.firstRound))
    //     {
    //         SingleInt isPlayersTurn = new();
    //         while (isPlayersTurn.value == 0)
    //         {
    //             Debug.Log("Await opponents input");
    //             roundText.text = "Await opponents input";
    //             isPlayersTurn = await GetRequest<SingleInt>($"play_tictactoe.php?behaviour=1");
    //             await Task.Delay(500); // Wait .5 second
    //         }

    //         //Process other players input
    //         SingleString otherPlayersInput = new();
    //         while (!ticTacToeOptions.Contains(otherPlayersInput.result))
    //         {
    //             otherPlayersInput = await GetRequest<SingleString>($"play_tictactoe.php?behaviour=2");
    //             Debug.Log($"Processing opponents input: {otherPlayersInput.result}");
    //             roundText.text = "Processing opponents input";
    //             await Task.Delay(500); // Wait .5 second
    //         }

    //         //Process the input in game
    //         Debug.Log("Opponents input processed");
    //         roundText.text = "Opponents input processed";
    //         TicTacToe.instance.ProcessOthersInput(otherPlayersInput.result);
    //     }

    //     if(TicTacToe.instance.firstRound) TicTacToe.instance.firstRound = false;

    //     //Now your turn
    //     Debug.Log("Your turn");
    //     roundText.text = "Awaiting  your input";
    //     TicTacToe.instance.isPlayersTurn = true;
    // }

    // public async Task InputAction(string _position)
    // {
    //     SingleString inputAction = await GetRequest<SingleString>($"play_tictactoe.php?behaviour=3&Option={_position}");
    //     if(!ticTacToeOptions.Contains(inputAction.result)) {Debug.LogError("Couldnt set position"); return;}
    //     TicTacToe.instance.isPlayersTurn = false;
    //     _ = AwaitMatchInput();
    // }

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

    void InstantiateDictionary()
    {
        getRequests = new();
        getRequests["debugInt"] = async (args) =>
        {
            uint[] intData = args.Item2;
            foreach (uint _uint in intData)
            {
                Debug.Log($"Received int: {_uint}");
            }
        };

        getRequests["debugString"] = async (args) =>
        {
            string[] stringData = args.Item3;
            foreach (string _string in stringData)
            {
                Debug.Log($"Received string: {_string}");
            }
        };

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
            pokerMatches[(uint)matchFound.value].playersInMatch.Add(newPlayer);


            uint[] matchID = new uint[1] { (uint)matchFound.value };
            serverBehaviour.SendDataToClient(args.Item1, "findMatch", 1, _intData: matchID);

            //Call setup match
            if (!setupMatches.Contains(matchID[0]))
            {
                await RunTask(args.Item1, "waitForPlayers", matchID);
            }
        };

        getRequests["waitForPlayers"] = async (args) =>
        {
            if (setupMatches.Contains(args.Item2[0])) return;
            setupMatches.Add(args.Item2[0]);
            //Check if all players are connected (5 means ready to start)
            SingleInt waitForPlayers = new();
            waitForPlayers = await GetRequest<SingleInt>($"find_match.php?behaviour=1&MatchID={args.Item2[0]}&connPlayers={waitForPlayers.value2}");

            switch (waitForPlayers.value)
            {
                //Wait longer, not all players have connected
                case 1:
                    await Task.Delay(500); // Wait .5 second
                    await RunTask(args.Item1, "waitForPlayers", new uint[1] { args.Item2[0] });
                    break;
                //Wait 10s to allow more people to join
                case 2:
                    int tenSecondTimer = 0;
                    while (tenSecondTimer < 10)
                    {
                        //Change timer

                        await Task.Delay(1000);
                        tenSecondTimer++;

                        waitForPlayers = await GetRequest<SingleInt>($"find_match.php?behaviour=1&MatchID={args.Item2[0]}&connPlayers={waitForPlayers.value}");
                        if (waitForPlayers.value == 2) await RunTask(args.Item1, "waitForPlayers", new uint[1] { args.Item2[0] });
                        else if (tenSecondTimer < 10) break;
                    }

                    //Setup match
                    await RunTask(args.Item1, "setupMatch", new uint[1] { args.Item2[0] });
                    break;
                //Start in 3 seconds
                case 3:
                    int threeSecondTimer = 0;
                    while (threeSecondTimer < 3)
                    {
                        //Change timer

                        await Task.Delay(1000);
                        threeSecondTimer++;
                    }

                    //Setup match
                    await RunTask(args.Item1, "setupMatch", new uint[1] { args.Item2[0] });
                    break;
            }


            //Wait 10s to let more people join

            //Setup the match when ready
        };

        getRequests["setupMatch"] = async (args) =>
        {
            Debug.Log("Setup match!");

            uint matchID = args.Item2[0];

            //Initialize list of poker players
            List<PokerPlayer> connectedPlayers = pokerMatches[matchID].playersInMatch;

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

            List<PokerPlayer> randomTurnOrder = new();

            foreach (PokerPlayer _player in connectedPlayers) randomTurnOrder.Add(null);
            for (int i = 0; i < connectedPlayers.Count; i++)
            {
                randomTurnOrder[UnityEngine.Random.Range(0, connectedPlayers.Count)] = connectedPlayers[i];
            }

            connectedPlayers = randomTurnOrder;

            pokerMatches[matchID].playersInMatch = connectedPlayers;
            pokerMatches[matchID].bettingPlayers = connectedPlayers;

            for (int i = 0; i < connectedPlayers.Count; i++)
            {
                //First int is order in turn, second int is userID
                serverBehaviour.SendDataToClient(connectedPlayers[0].connectionID, "setTurnOrder", 1, _intData: new uint[2] { (uint)i, (uint)connectedPlayers[i].userID });
            }

            //set game state
            pokerMatches[matchID].gameState = GAME_STATE.PRE_FLOP;

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
                    serverBehaviour.SendDataToClient(_player.connectionID, "setBet", 1, _intData: new uint[2] { (uint)connectedPlayers[i].userID, (uint)(i * 20) });
                }
            }

            //set current turn
            pokerMatches[matchID].currentTurnUserID = connectedPlayers[2].userID;
            foreach (PokerPlayer _player in connectedPlayers)
            {
                //user id of new turn player
                serverBehaviour.SendDataToClient(_player.connectionID, "startPlayerTurn", 1, _intData: new uint[1] { (uint)pokerMatches[matchID].currentTurnUserID });
            }

            //UI STUFF

                // UIManager.instance.ToggleUIElement("UserInfo", false);
                // UIManager.instance.ToggleUIElement("PokerScreen", true);
                // UIManager.instance.GetTextElementFromDict("Chips").text = "Chips: " + playerInfo.remainingChips;
                // UIManager.instance.GetTextElementFromDict("Bet").text = "Bet: " + playerInfo.betAmount;
                // Poker.instance.GenerateHand(cardInfo.handCard1, cardInfo.handCard2);
            };

            //first int is userID, second int is matchID, third int is action(1: fold 2: call 3: raise), fourth int is amount bet
            getRequests["playTurn"] = async (args) =>
            {
                uint matchID = args.Item2[1];
                PokerMatch pokerMatch = pokerMatches[matchID];
                PokerPlayer pokerPlayer = pokerMatch.playersByUserID[args.Item2[0]];

                int indexOfCurrentPlayer = pokerMatch.bettingPlayers.IndexOf(pokerPlayer);
                int indexOfNextPlayer = 0;

                if (pokerMatch.bettingPlayers.Count - 1 < indexOfCurrentPlayer + 1) indexOfNextPlayer = 0;
                else indexOfNextPlayer = indexOfCurrentPlayer + 1;

                int userIDOfNextPlayer = pokerMatch.bettingPlayers[indexOfNextPlayer].userID;

                switch (args.Item2[2])
                {
                    //fold
                    case 1:
                        pokerMatch.bettingPlayers.Remove(pokerPlayer);
                        foreach (PokerPlayer _player in pokerMatch.playersInMatch)
                        {
                            //UserID, betAmount
                            serverBehaviour.SendDataToClient(_player.connectionID, "setBet", 1, _intData: new uint[2] { (uint)pokerPlayer.userID, 0 });
                        }
                        if (pokerMatch.bettingPlayers.Count <= 1)
                        {
                            Poker.instance.EndPokerRound();
                            return;
                        }
                        break;
                    //call
                    case 2:
                        foreach (PokerPlayer _player in pokerMatch.playersInMatch)
                        {
                            //UserID, betAmount
                            serverBehaviour.SendDataToClient(_player.connectionID, "setBet", 1, _intData: new uint[2] { (uint)pokerPlayer.userID, args.Item2[3] });
                        }
                        break;
                    //raise
                    case 3:
                        uint newBet = args.Item2[3];
                        foreach (PokerPlayer _player in pokerMatch.playersInMatch)
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
                //Check for round end / change game state
                if (pokerMatch.lastRaiseUserID == userIDOfNextPlayer)
                {
                    if (pokerMatch.gameState == GAME_STATE.SHOWDOWN)
                    {
                        Poker.instance.EndPokerRound();
                        return;
                    }
                    //If true, big blind gets opportunity to raise again
                    else if (pokerMatch.gameState != GAME_STATE.PRE_FLOP)
                    {
                        //Move to the next round
                        pokerMatch.gameState = (GAME_STATE)((int)pokerMatch.gameState + 1);
                    }
                }

                //Handle new game state
                switch (pokerMatch.gameState)
                {
                    //Reveal cards 1,2,3
                    case GAME_STATE.FLOP:

                        break;
                    //Reveal card 4
                    case GAME_STATE.TURN:

                        break;
                    //Reveal card 5
                    case GAME_STATE.RIVER:

                        break;
                }

                //Start next turn
                pokerMatch.currentTurnUserID = userIDOfNextPlayer;
                foreach (PokerPlayer _player in pokerMatch.playersInMatch)
                {
                    //user id of new turn player
                    serverBehaviour.SendDataToClient(_player.connectionID, "startPlayerTurn", 1, _intData: new uint[1] { (uint)pokerMatch.currentTurnUserID });
                }
            };


    }
}
