using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour
{
    Rigidbody body;
    public float speed = 5;
    public Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody>();
    }
    

    // Update is called once per frame

    void Update()
    {
       var x = Input.GetAxis("Horizontal");
       var y = Input.GetAxis("Vertical");

       transform.Rotate(0, x * (speed) , 0);
      // body.velocity = new Vector3(x,body.velocity.y,y) * speed;
       body.velocity = transform.forward * speed * Mathf.Abs(y);

       if(y!=0 || x!=0)
       {
            anim.SetBool("walk", true);
       }
       else
       {
            anim.SetBool("walk", false);
       }
       
    }
}
