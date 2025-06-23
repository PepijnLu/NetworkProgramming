using TMPro;
using UnityEngine;

public class MenuButtons : MonoBehaviour
{
    public void EnableLoginScreen()
    {
        UIManager.instance.ToggleUIElement("Login/Register", false);
        UIManager.instance.ToggleUIElement("LoginScreen", true);
    }

    public void EnableRegisterScreen()
    {
        UIManager.instance.ToggleUIElement("Login/Register", false);
        UIManager.instance.ToggleUIElement("RegisterScreen", true);
    }

    public void Login(Transform _inputs)
    {
        TMP_InputField usernameInput = _inputs.GetChild(0).GetComponent<TMP_InputField>();
        TMP_InputField passwordInput = _inputs.GetChild(1).GetComponent<TMP_InputField>();

        string[] usernameAndPass = new string[2]{usernameInput.text, passwordInput.text};

        ClientBehaviour.instance.SendString(usernameAndPass, "loginUser");
    }

    public void Register(Transform _inputs)
    {
        string usernameInput = _inputs.GetChild(0).GetComponent<TMP_InputField>().text;
        string passwordInput = _inputs.GetChild(1).GetComponent<TMP_InputField>().text;
        string emailInput = _inputs.GetChild(2).GetComponent<TMP_InputField>().text;
        string countryInput = _inputs.GetChild(3).GetComponent<CountryDropdown>().GetSelectedCountry();

        //Do more checks than this
        if(usernameInput.Length < 4) { Debug.Log("Username too short"); return; }
        if(passwordInput.Length < 7) { Debug.Log("Password too short"); return; }
        if(!emailInput.Contains("@") || !emailInput.Contains(".")) { Debug.Log("Invalid Email"); return; }
        if(countryInput == "") { Debug.Log("No country selected"); return; }

        GetRequests.instance.RegisterUser(usernameInput, passwordInput, emailInput, countryInput);
    }

    public void FindMatch()
    {
        //_ = GetRequests.instance.FindMatch();
        ClientBehaviour.instance.SendInt(new uint[1]{(uint)ClientBehaviour.instance.GetUserInfo().userID}, "preMatchSetup");

        UIManager.instance.ToggleUIElement("UserInfo", false);
        UIManager.instance.ToggleUIElement("PreMatchSetup", true);

        //ClientBehaviour.instance.SendInt(new uint[1]{(uint)ClientBehaviour.instance.GetUserInfo().userID}, "findMatch");
    }

    public void PlayTurn(int _action)
    {
        Poker.instance.PlayTurn(_action);
    }

    public void ChangeRaiseAmount(bool _increment)
    {
        Poker.instance.ChangeRaiseAmount(_increment);
    }

    public void LeaveMatch()
    {
        ClientBehaviour.instance.SendInt(new uint[2]{(uint)ClientBehaviour.instance.GetUserInfo().userID, (uint)Poker.instance.userMatchID}, "leaveMatch");
        Poker.instance.ResetMatchClient(true);
        UIManager.instance.ToggleUIElement("Lobby", false);
        UIManager.instance.ToggleUIElement("Matchmaking", false);
        UIManager.instance.ToggleUIElement("UserInfo", true);
    }
}
