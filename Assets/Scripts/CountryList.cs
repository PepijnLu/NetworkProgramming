using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

public class CountryList : MonoBehaviour
{
    public static List<string> countryList;
    void Awake()
    {
        countryList = GetCountryNames();
        countryList.Insert(0, "");
    }

    public List<string> GetCountryNames()
    {
        return CultureInfo
            .GetCultures(CultureTypes.SpecificCultures)
            .Select(ci => new RegionInfo(ci.LCID).EnglishName)
            .Distinct()
            .OrderBy(name => name)
            .ToList();
    }
}
