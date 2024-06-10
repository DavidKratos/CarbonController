using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraController : MonoBehaviour
{
    public GameObject targetObject;
    Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
        offset = transform.position - targetObject.transform.position;
//        Debug.Log(offset);
        transform.position = targetObject.transform.position + offset;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = targetObject.transform.position + offset;
    }
}
