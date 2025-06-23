using System.Collections.Generic;
using UnityEngine;

public class PokerClient : MonoBehaviour
{
    [SerializeField] Poker _poker;
    [SerializeField] ClientDataProcess clientDataProcess;
    [SerializeField] GameObject placeholderRiver;
    [SerializeField] Transform pokerHandC1Transform;
    [SerializeField] Transform pokerHandC2Transform;
    List<Transform> sharedCardTransforms;
    public Dictionary<uint, PokerPlayer> playersByUserID = new();
    public int userMatchID;
    public bool isYourTurn;
    public bool joiningNextRound;
    public int currentMatchBet;
    public int userChips;
    public int userStartingChips;
    public int userBet;
    public int raiseBetAmount;
    GameObject instantiatedRiver;
    Dictionary<int, PokerCard> pokerCardsDict;

    void Start()
    {
        pokerCardsDict = _poker.GetCardDict();
    }
    public void GenerateHand(int card1ID, int card2ID)
    {
        Instantiate(pokerCardsDict[card1ID], pokerHandC1Transform);
        Instantiate(pokerCardsDict[card2ID], pokerHandC2Transform);
    }

    public void GenerateSharedCard(int _cardNumber, int _cardID)
    {
        Debug.Log($"Generate Shared Card {_cardNumber}, count: {sharedCardTransforms.Count}");
        Transform cardTransform = sharedCardTransforms[_cardNumber - 1];
        Destroy(cardTransform.GetChild(0).gameObject);
        PokerCard newCard = Instantiate(pokerCardsDict[_cardID], cardTransform);
        newCard.transform.localRotation = Quaternion.Euler(0, 180, 0);
        Debug.Log($"Generated Shared Card {_cardNumber}"); 
    }

    //1 = fold, 2 is call, 3 = raise
    public void PlayTurn(int _action)
    {
        if(!isYourTurn) return;

        int betAmount = 0;
        int newMatchBet = 0;

        if(_action == 2)
        {
            betAmount = currentMatchBet - userBet;
            newMatchBet = currentMatchBet;
        }
        else if (_action == 3)
        {
            betAmount = raiseBetAmount - userBet;
            newMatchBet = raiseBetAmount;
        }

        if(betAmount > userChips)
        {
            Debug.Log("Cannot bet more than you have");
            return;
        }

        if((raiseBetAmount < currentMatchBet + 20) && (_action == 3))
        {
            Debug.Log("Cannot bet less than current bet + minimal raise amount of 20");
            return;
        }

        int userID = clientDataProcess.userInfo.userID;
        int matchID = userMatchID;

        userChips -= betAmount;

        UIManager.instance.GetTextElementFromDict("YourChips").text = $"Chips: {userChips}";
        EndPlayerTurn();

        ClientBehaviour.instance.SendInt(new uint[3]{(uint)userID, (uint)matchID, (uint)userChips}, "setUserChips");
        ClientBehaviour.instance.SendInt(new uint[4]{(uint)userID, (uint)matchID, (uint)_action, (uint)newMatchBet}, "playTurn");
    }

    public void StartPokerRound()
    {
        instantiatedRiver = Instantiate(placeholderRiver);
        sharedCardTransforms = new();
        for(int i = 0; i < 5; i++)
        {
            sharedCardTransforms.Add(instantiatedRiver.transform.GetChild(i));
        }
        userStartingChips = userChips;
    }

    public void EndPlayerTurn()
    {
        isYourTurn = false;
        UIManager.instance.GetUIElementFromDict("BetButtons").SetActive(false);
    }

    public void ChangeRaiseAmount(bool _increment)
    {
        if(_increment)
        {
            if((raiseBetAmount + 20) <= userChips) 
            {
                raiseBetAmount += 20;
            }
            else 
            {
                Debug.LogWarning("User doesnt have enough chips to increment bet");
            }
        }
        else
        {
            if((raiseBetAmount - 20) >= 0) raiseBetAmount -= 20;
        }

        UIManager.instance.GetTextElementFromDict("RaiseBetAmount").text = $"{raiseBetAmount}";
    }

    //Runs on client
    public void ResetMatchClient()
    {
        //Destroy river
        if(instantiatedRiver != null) Destroy(instantiatedRiver);
        //Destroy hand cards
        if(pokerHandC1Transform.GetChild(0) != null) Destroy(pokerHandC1Transform.GetChild(0).gameObject);
        if(pokerHandC2Transform.GetChild(0) != null) Destroy(pokerHandC2Transform.GetChild(0).gameObject);

        UIManager.instance.ToggleUIElement("PokerScreen", false);
        UIManager.instance.ToggleUIElement("GameOver", true);
        UIManager.instance.GetTextElementFromDict("GameScore").text = $"Score: {userChips - userStartingChips}";
        
    }
}
