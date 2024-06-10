using UnityEngine;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class CityDistance
{
    public string CityA;
    public string CityB;
    public float Distance;
    public float CarbonFootPrint;
    public string CountryA;
    public string CountryB;
}

[System.Serializable]
public class CityDistanceData
{
    public List<CityDistance> cityDistances;
}

public class jsonReaderCode : MonoBehaviour
{
    List<CityDistance> cityDistances;
   public  string city_A, city_B,c1,c2;
  // public BeamManager beamManager;

    void Start()
    {
   //  GetMapData();
    }

   public void GetMapData()
    {
       Debug.Log("Application.dataPath + " + Application.dataPath);
        // Load data from JSON file
        string jsonText = File.ReadAllText(Application.dataPath + "/city_distances.json");
        cityDistances = new List<CityDistance>();
        ParseJSON(jsonText);

        // Example cities
        string city1 = city_A;
        string city2 = city_B;

        // Find distance between the cities
        float[] distance = FindDistance(city1, city2);

        CityDistance d = new CityDistance();
        d = FindIt(city1,city2);

        if(d!=null)
        {
             Debug.Log("city1::"+d.CityA + " city2::" + d.CityB + " distance::" + d.Distance + " carbonfootprint::" + d.CarbonFootPrint+"  country1"+d.CountryA+" country2"+d.CountryB);
             c1=d.CountryA;
             c2=d.CountryB;
        }
        

        if (distance[0] != -1)
        {
            Debug.Log($"The distance between {city1} and {city2} is {distance[0]} km.");
            Debug.Log($"The carboon generated in between {city1} and {city2} is {distance[1]}.");
        
        }
        else
        {
            Debug.Log($"Distance between {city1} and {city2} not found in the data.");
        }
    }

    void ParseJSON(string jsonText)
    {
        CityDistanceData data = JsonUtility.FromJson<CityDistanceData>(jsonText);
        if (data != null)
        {
            cityDistances = data.cityDistances;
        }
    }

    float[] FindDistance(string cityA, string cityB)
    {
        float[] vals= new float[2];

        foreach (var entry in cityDistances)
        {
//             Debug.Log("City 1 " + entry.CityA + " city 2, " + entry.CityB + " Distance " + entry.Distance+" carbon " + entry.CarbonFootPrint);
            if ((entry.CityA == cityA && entry.CityB == cityB) || (entry.CityA == cityB && entry.CityB == cityA))
            {
                vals[0] = entry.Distance;
                vals[1] = entry.CarbonFootPrint;
            }
        }
        return vals; // Return -1 if distance not found
    }

    CityDistance FindIt(string cityA, string cityB)
    {
        CityDistance value = new CityDistance();

        foreach (var entry in cityDistances)
        {
          //  Debug.Log("City 1 " + entry.CityA + " city 2, " + entry.CityB + " Distance " + entry.Distance+" carbon " + entry.CarbonFootPrint);
            if ((entry.CityA == cityA && entry.CityB == cityB))
            {
                Debug.Log("City 1 " + entry.CityA + " city 2, " + entry.CityB + " Distance " + entry.Distance+" carbon " + entry.CarbonFootPrint);
               value = entry;
               Debug.Log("Value::"+value.CityA);
             //  beamManager.countryA=value.CountryA;
             //  beamManager.countryB=value.CountryB;
            }

          /*  if(cityA==entry.CityA && cityB==entry.CityB)
            {
                Debug.Log("City 1 " + entry.CityA + " city 2, " + entry.CityB + " Distance " + entry.Distance+" carbon " + entry.CarbonFootPrint+"   countryA"+entry.CountryA+"  countryB"+entry.CountryB);
            }*/
        }
        return value; // Return -1 if distance not found
    }
}
