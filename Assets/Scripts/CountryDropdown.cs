using TMPro;
using UnityEngine;
public class CountryDropdown : MonoBehaviour
{
    [SerializeField] TMP_Dropdown countryDropdown;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        countryDropdown.ClearOptions();
        countryDropdown.AddOptions(CountryList.countryList);
    }

    public string GetSelectedCountry()
    {
        int index = countryDropdown.value;
        return CountryList.countryList[index];
    }
}
