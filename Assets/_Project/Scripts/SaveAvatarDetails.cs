using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveAvatarDetails 
{
   // private MapData mapdata;
    public void SaveToJason(AvatarData avatarData)
    {
        var jsonData=JsonUtility.ToJson(avatarData);
        File.WriteAllText(Application.persistentDataPath + "/avatarData.json", jsonData);
        Debug.Log("file saved");
    }
    public AvatarData GetAvatarData(int id,string name)
    {
        var avatarData=new AvatarData();
        if(File.Exists(Application.persistentDataPath + "/avatarData.json"))
        {
            Debug.Log("File Exists");
            avatarData=JsonUtility.FromJson<AvatarData>(File.ReadAllText(Application.persistentDataPath + "/avatarData.json"));
            foreach(DataDetails dataDetails in avatarData._list)
            {
                if(id==dataDetails.avatarid)
                {
                    Debug.Log("Exists and name is ::++"+dataDetails.avatarName);
                    break;
                }
            }
        }
       else
        {
            Debug.Log("Not Exists");
            DataDetails dataDetails=new DataDetails();
            dataDetails.avatarid=id;
            dataDetails.avatarName=name;
            avatarData._list.Add(dataDetails);
            Debug.Log("User List::"+avatarData._list.Count);
        }
        return avatarData;
    }
  
}
