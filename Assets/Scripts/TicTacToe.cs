using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TicTacToe : MonoBehaviour
{
    public static TicTacToe instance;
    public bool isPlayersTurn;
    public Dictionary<string, int> boardSpaces;
    public bool isCrosses;
    public bool firstRound;
    public List<string> filledSpaces = new();
    [Header("Sprites/Colors")]
    [SerializeField] Image colorIndicator;
    [SerializeField] Sprite crossSprite;
    [SerializeField] Sprite circleSprite;
    [SerializeField] Color crossColor;
    [SerializeField] Color circleColor;
    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        boardSpaces = new()
        {
            ["Top Left"] = 1,
            ["Top Middle"] = 2,
            ["Top Right"] = 3,
            ["Middle Left"] = 4,
            ["Middle"] = 5,
            ["Middle Right"] = 6,
            ["Bottom Left"] = 7,
            ["Bottom Middle"] = 8,
            ["Bottom Right"] = 9
        };
    }

    public void ProcessOthersInput(string _position)
    {
        if(isPlayersTurn) Debug.LogError("Shouldnt be able to happen this");
        filledSpaces.Add(_position);
        SetSpace(_position, !isCrosses);
    }
    public void InputPosition(string _position)
    {
        if(!isPlayersTurn) return;

        if(!filledSpaces.Contains(_position))
        {
            filledSpaces.Add(_position);
            SetSpace(_position, isCrosses);
            GetRequests.instance.InputAction(_position);
            isPlayersTurn = false;
        }
    }

    public void SetSpace(string _position, bool _isCrosses)
    {
        Image spaceToSet = UIManager.instance.GetUIElementFromDict(_position).GetComponent<Image>();

        if(_isCrosses)
        {
            spaceToSet.sprite = crossSprite;
            spaceToSet.color = crossColor;
        }
        else
        {
            spaceToSet.sprite = circleSprite;
            spaceToSet.color = circleColor;
        }
    }

    public void SetColorIndicator(bool _isCrosses)
    {
        if(_isCrosses)
        {
            colorIndicator.sprite = crossSprite;
            colorIndicator.color = crossColor;
        }
        else
        {
            colorIndicator.sprite = circleSprite;
            colorIndicator.color = circleColor;
        }
    }
    
}
