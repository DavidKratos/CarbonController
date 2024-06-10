using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelection : MonoBehaviour
{
     public Vector3 turn;
  private float rotationSpeed;
    // Start is called before the first frame update
    void Start()
    {
        var saveData=new SaveAvatarDetails();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
          {
              RaycastHit hit;
              Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

              if (Physics.Raycast(ray, out hit))
              {
                  if (hit.collider.gameObject.tag == "Player1" || hit.collider.tag=="Player2")
                  {
                    var saveData=new SaveAvatarDetails();
                    AvatarData ad=saveData.GetAvatarData(0,hit.transform.tag);
                    saveData.SaveToJason(ad);
                    
                   /*  turn.x +=Input.GetAxis("Mouse X");
                    turn.y +=Input.GetAxis("Mouse Y");
                   hit.transform.localRotation= Quaternion.Euler(-turn.y,turn.x,0);*/
                 //  hit.transform.Rotate(Input.GetAxis("Mouse X")*rotationSpeed*Time.deltaTime)
                  }
              }
          }
    }
}
