using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineController : MonoBehaviour
{
  private  LineRenderer lr;
    private Transform[] points;
    // Start is called before the first frame update
    void Start()
    {
        lr=GetComponent<LineRenderer>();
    }

   public void SetupLines(Transform[] points)
   {
    lr.positionCount=points.Length;
    this.points=points;
   }

   void Update()
   {
    if(points.Length!=null)
    {
    for(int i=0;i<points.Length;i++)
    {
        lr.SetPosition(i,points[i].position);
    }
    }
   }
}
