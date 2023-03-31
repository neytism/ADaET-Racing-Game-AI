using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawPath : MonoBehaviour
{
    private Transform[] waypoints;

    public GameObject waypointsHolder;
    public List<Transform> childNodeList = new List<Transform>();
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        
        FillNodes();

        for (int i = 0; i < childNodeList.Count; i++)
        {
            Vector3 currentPos = childNodeList[i].position;
            if (i>0)
            {
                Vector3 prevpos = childNodeList[i - 1].position;
                Gizmos.DrawLine(prevpos,currentPos);
            }
        }
    }
    
    void FillNodes()
    {
        childNodeList.Clear();
        
        var childrenList = new List<Transform>();
        
        foreach(Transform child in waypointsHolder.transform)
        {
            childNodeList.Add(child);
        }

        waypoints = childrenList.ToArray();

        foreach (Transform child in waypoints)
        {
            childNodeList.Add(child);
        }
    }
}
