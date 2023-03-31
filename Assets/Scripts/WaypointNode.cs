using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointNode : MonoBehaviour
{
    
    [Header("This is the waypoint we are going towards")]
    public float minDistanceToReachWaypoint = 5f;

    public bool isStartingPoint = false;
    public bool isEndingPoint = false;
    public bool isSplitNode = false;
    
    public bool isActiveNode = false;
    private SpriteRenderer _sr;
    
    public WaypointNode[] nextWaypoint;
    
    void Awake ()
    {

        List<WaypointNode> wp = new List<WaypointNode>();

        //will ignore is the node has multiple endings
        if (isSplitNode)
        {
            return;
        }
        
        // will continue if single nodes
        if (!isEndingPoint)
        {
            
            wp.Add(GetNextChild().GetComponent<WaypointNode>());
            nextWaypoint = wp.ToArray();
        }
        else
        {
            wp.Add(transform.parent.GetChild(0).GetComponent<WaypointNode>());
            nextWaypoint = wp.ToArray();
        }

        //visualization
        _sr = GetComponent<SpriteRenderer>();

    }

    private void Update()
    {
        if (isActiveNode)
        {
           _sr.color = Color.red;
        }
        else
        {
            _sr.color = Color.white;
        }
    }

    private Transform GetNextChild ()
    {
        // Check where we are
        int thisIndex = transform.GetSiblingIndex ();
 
        // We have a few cases to rule out
        if ( transform.parent == null )
            return null;
        if ( transform.parent.childCount <= thisIndex + 1 )
            return null;
 
        // Then return whatever was next, now that we're sure it's there
        return transform.parent.GetChild (thisIndex + 1);
    }
}
