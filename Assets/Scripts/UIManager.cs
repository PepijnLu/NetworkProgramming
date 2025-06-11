using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    Dictionary<string, GameObject> uiElementsDict;
    Dictionary<string, TextMeshProUGUI> textElementsDict;
    [Header("UI Elements")]
    [SerializeField] List<GameObject> uiElements;
    [Header("Text Elements")]
    [SerializeField] List<TextMeshProUGUI> textElements;

    void Awake()
    {
        instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        uiElementsDict = new();
        textElementsDict = new();

        foreach(GameObject _element in uiElements)
        {
            uiElementsDict.Add(_element.name, _element);
        }   
        foreach(TextMeshProUGUI _element in textElements)
        {
            textElementsDict.Add(_element.name, _element);
        }  
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisplayUserInfo(string _username, string _email, string _country, string _dob)
    {
        GetTextElementFromDict("INFO_Username").text = _username;
        GetTextElementFromDict("INFO_Email").text = _email;
        GetTextElementFromDict("INFO_Country").text = _country;
        GetTextElementFromDict("INFO_DoB").text = _dob;
    }

    public GameObject GetUIElementFromDict(string _element)
    {
        if(uiElementsDict.ContainsKey(_element)) return uiElementsDict[_element];
        Debug.LogWarning(_element + " not found in dictionary");
        return null;
    }

    public TextMeshProUGUI GetTextElementFromDict(string _element)
    {
        if(textElementsDict.ContainsKey(_element)) return textElementsDict[_element];
        Debug.LogWarning(_element + " not found in dictionary");
        return null;
    }

    public void ToggleUIElement(string _element, bool _active)
    {
        GameObject element = GetUIElementFromDict(_element);
        if(element == null) return; 
        element.SetActive(_active);
    }

    public void StartTicTacToe()
    {
        ToggleUIElement("UserInfo", false);
        ToggleUIElement("TicTacToe", true);
    }
}
