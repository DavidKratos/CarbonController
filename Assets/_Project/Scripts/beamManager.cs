using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
 [System.Serializable]
public class DistanceCarbonEntry
{
    public string CityA;
    public string CityB;
    public float Distance;
    public float CarbonFootPrint;
}
[System.Serializable]
public class DistanceCarbonList
{
    public List<DistanceCarbonEntry> _list;
}
public class beamManager : MonoBehaviour
{
  Camera cam;
  public List<Transform> allplaces=new List<Transform>();
  public List<Transform> mapList=new List<Transform>();
 [SerializeField] public Transform[] points;
  [SerializeField] private GameObject lineController;
  [SerializeField] Collider mumbai;
    public TMP_Dropdown dropdown;
    public TMP_Dropdown toDropdown;
    public TMP_Text clickText;
    string SelectionValue;
    public GameObject selectionItem,worldMap,greyScreen,flight;
    Color32 originalColor;
    bool addItems=false;
    public Transform controlPoint;
    [SerializeField] GameObject moveFlight;
    private float distanceTraveled;
    public bool isMove=false;
  public  string toValue, fromValue,countryA,countryB;
    public jsonReaderCode jsonCode;

   


    void Start()
    {
        cam = GetComponent<Camera>();
        foreach(Renderer r in worldMap.transform.GetComponentsInChildren<Renderer>()){
            originalColor=  r.material.color;
         }
    }

   

    public void OnSelection()
    {
       // Debug.Log("value::"+dropdown.options[dropdown.value].text);
        fromValue=dropdown.options[dropdown.value].text;
      
       

    }
   public void OnToSelection()
    {
      //  Debug.Log("value::"+toDropdown.options[toDropdown.value].text);
        toValue=toDropdown.options[toDropdown.value].text;
        Invoke("OnSelectionDone",0.6f);

    }
   /* void Update()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);
        if(Physics.Raycast(ray.origin, ray.direction,out hit))
        {
            if(Input.GetMouseButtonDown(0) && addItems)
            {
                 hit.collider.gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
                 mapList.Add(hit.collider.transform);
                 addItems=false;
                 Invoke("OnSelectionDone",0.4f);
            }
        }

    }*/
    void OnSelectionDone()
    {
        selectionItem.SetActive(false);
        worldMap.SetActive(true);
        addItems=true;
        jsonCode.city_A=fromValue;
        jsonCode.city_B=toValue;
        jsonCode.GetMapData();
        countryA=jsonCode.c1;
        countryB=jsonCode.c2;
        OnYes();
       // greyScreen.SetActive(true);
    }
    public void OnYes()
    {
      for(int i=0;i<allplaces.Count;i++)
      {
        if(allplaces[i].name==countryA)
        {
            mapList.Add(allplaces[i]);
            break;
        }
       
      }
        for(int i=0;i<allplaces.Count;i++)
      {
        if(allplaces[i].name==toValue)
        {
            mapList.Add(allplaces[i]);
            break;
        }
       
      }
      // greyScreen.SetActive(false);
       points=mapList.ToArray();
       isMove=true;
       lineController.SetActive(true);
       lineController.GetComponent<BezierCurve>().ismove=isMove;
       flight.SetActive(true);
       flight.transform.position=mapList[0].transform.position;

    }
    public void OnNo()
    {
        greyScreen.SetActive(false);
       foreach(Renderer r in worldMap.transform.GetComponentsInChildren<Renderer>()){
             r.material.color= originalColor;
         }
         mapList.Remove(mapList[1]);
         Debug.Log("count::"+mapList.Count);
        worldMap.SetActive(false);
        selectionItem.SetActive(true);

    }
}
