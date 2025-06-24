using System.Collections;
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

    public void DisplayUserInfo(string _username, string _email, string _country)
    {
        GetTextElementFromDict("INFO_Username").text = _username;
        GetTextElementFromDict("INFO_Email").text = _email;
        GetTextElementFromDict("INFO_Country").text = _country;
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

    public IEnumerator HandleNotEnoughChips()
    {
        yield return null;
    }

    public IEnumerator ShowTextForSeconds(string _element, string _text, float duration)
    {
        TextMeshProUGUI _tmpro =  GetTextElementFromDict(_element);
        _tmpro.text = _text;
        yield return new WaitForSeconds(duration);
        _tmpro.text = "";

    }

    public void DisplayUserScores(uint[] intData)
    {
        for(int i = 0; i < intData.Length; i++)
        {
            Debug.Log($"Score: {intData[i]}");
            if(intData[i] != 0)
            {
                GetTextElementFromDict($"INScore{i + 1}").text = $"Score: {intData[i]}";
                GetTextElementFromDict($"GOScore{i + 1}").text = $"Score: {intData[i]}";
            }
        } 
    }
}
