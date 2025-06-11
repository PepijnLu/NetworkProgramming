using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using TMPro;

public class GetRequests : MonoBehaviour
{
    public static GetRequests instance;
    TextMeshProUGUI roundText;
    [Header("InitialLogin")]
    [SerializeField] int serverID;
    [SerializeField] string password;
    [SerializeField] string baseUrl;
    [Header("TicTacToe Options")]
    [SerializeField] List<string> ticTacToeOptions;
    void Awake()
    {
        instance = this;   
    }
    async void Start()
    {
        //Index page
        //GetRequest(baseUrl);

        //Start server session
        LoginResponse response = await GetRequest<LoginResponse>($"server_login.php?Server_ID={serverID}&Server_pass={MD5Helper.EncryptToMD5(password)}&delOld=true");
        if (!response.success) Debug.LogError($"Login failed: {response.message}");

        Debug.Log($"Server login successful! Session ID: {response.sessionId}");
        //Connected to server session, enable register and login buttons
        UIManager.instance.ToggleUIElement("Login/Register", true);
        roundText = UIManager.instance.GetTextElementFromDict("RoundText");

    }

    public async void LoginUser(string _username, string _password)
    {
        //Login
        LoginResponse response = await GetRequest<LoginResponse>($"user_login.php?User={_username}&pass={MD5Helper.EncryptToMD5(_password)}");
        if (!response.success) { Debug.LogError($"Login failed: {response.message}"); return; }

        Debug.Log($"User login successful!");

        //Get user info
        UserInfo userInfo = await GetRequest<UserInfo>("user_info.php");
        //Debug.Log($"Username: {userInfo.Username} - Email: {userInfo.Email} - Country: {userInfo.Country} - DateOfBirth: {userInfo.DateOfBirth}");

        //Disable buttons and show user info
        UIManager.instance.ToggleUIElement("LoginScreen", false);
        UIManager.instance.ToggleUIElement("UserInfo", true);
        UIManager.instance.DisplayUserInfo(userInfo.Username, userInfo.Email, userInfo.Country, userInfo.DateOfBirth);
    }

    //Add date of birth support, password might not be getting added properly
    public async void RegisterUser(string _username, string _password, string _email, string _country, string _dateOfBirth = "")
    {
        //Login
        LoginResponse response = await GetRequest<LoginResponse>($"register_user.php?User={_username}&Email={_email}&Pass={MD5Helper.EncryptToMD5(_password)}&Country={_country}");
        if (!response.success) { Debug.LogError($"Registration failed: {response.message}"); return; }

        Debug.Log($"User registered successfully!");

    }

    public async Task FindMatch()
    {
        //Login
        // SingleInt matchmaking = await GetRequest<SingleInt>($"find_match.php?behaviour=1");
        // if (matchmaking.value == 0) { Debug.LogError($"Couldnt connect"); return; }

        Debug.Log($"Now matchmaking!");
        SingleInt matchFound = new();
        while (matchFound.value == 0)
        {
            matchFound = await GetRequest<SingleInt>($"find_match.php?behaviour=2");
            await Task.Delay(1000); // Wait 1 second
        }

        Debug.Log($"Match Found! Match ID: {matchFound.value}");
        //UIManager.instance.StartTicTacToe();
        _ = SetupMatch();
    }

    public async Task SetupMatch()
    {
        PokerMatchCardInfo cardInfo = new();
        while (cardInfo.handCard1 == 0)
        {
            //roundText.text = "Awaiting setup completion";
            //Debug.Log($"Awaiting setup completion: {setupComplete.value}");
            cardInfo = await GetRequest<PokerMatchCardInfo>($"find_match.php?behaviour=3");
            await Task.Delay(500); // Wait .5 second
        }
        Debug.Log($"Started Match");

        SingleInt blindSetup = new();
        //Setup blinds
        while (blindSetup.value == 0)
        {
            //roundText.text = "Awaiting setup completion";
            //Debug.Log($"Awaiting setup completion: {setupComplete.value}");
            blindSetup = await GetRequest<SingleInt>($"play_poker.php?behaviour=1");
            await Task.Delay(500); // Wait .5 second
        }

        PokerMatchPlayerInfo playerInfo = await GetRequest<PokerMatchPlayerInfo>($"play_poker.php");

        UIManager.instance.ToggleUIElement("UserInfo", false);
        UIManager.instance.ToggleUIElement("PokerScreen", true);
        UIManager.instance.GetTextElementFromDict("Chips").text = "Chips: " + playerInfo.remainingChips;
        UIManager.instance.GetTextElementFromDict("Bet").text = "Bet: " + playerInfo.betAmount;
        Poker.instance.GenerateHand(cardInfo.handCard1, cardInfo.handCard2);
    }

    public async Task AwaitMatchInput()
    {
        //Wait until its your players turn
        if((!TicTacToe.instance.isCrosses && TicTacToe.instance.firstRound) || (!TicTacToe.instance.firstRound))
        {
            SingleInt isPlayersTurn = new();
            while (isPlayersTurn.value == 0)
            {
                Debug.Log("Await opponents input");
                roundText.text = "Await opponents input";
                isPlayersTurn = await GetRequest<SingleInt>($"play_tictactoe.php?behaviour=1");
                await Task.Delay(500); // Wait .5 second
            }

            //Process other players input
            SingleString otherPlayersInput = new();
            while (!ticTacToeOptions.Contains(otherPlayersInput.result))
            {
                otherPlayersInput = await GetRequest<SingleString>($"play_tictactoe.php?behaviour=2");
                Debug.Log($"Processing opponents input: {otherPlayersInput.result}");
                roundText.text = "Processing opponents input";
                await Task.Delay(500); // Wait .5 second
            }

            //Process the input in game
            Debug.Log("Opponents input processed");
            roundText.text = "Opponents input processed";
            TicTacToe.instance.ProcessOthersInput(otherPlayersInput.result);
        }

        if(TicTacToe.instance.firstRound) TicTacToe.instance.firstRound = false;

        //Now your turn
        Debug.Log("Your turn");
        roundText.text = "Awaiting  your input";
        TicTacToe.instance.isPlayersTurn = true;
    }

    public async void InputAction(string _position)
    {
        SingleString inputAction = await GetRequest<SingleString>($"play_tictactoe.php?behaviour=3&Option={_position}");
        if(!ticTacToeOptions.Contains(inputAction.result)) {Debug.LogError("Couldnt set position"); return;}
        TicTacToe.instance.isPlayersTurn = false;
        _ = AwaitMatchInput();
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
}
