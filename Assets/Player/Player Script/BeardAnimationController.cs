﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BeardAnimationController : MonoBehaviour {


    private const int HARDMAXSEGMENTS = 100; // the max number of beard segments that can ever exist (max of max beard length)
    private const float SEGMENTDISTANCE = .1f; // how far apart the segments are
    private Vector3 beardTipOffset = new Vector3(0f, .3f, 0f); // so the beard tip is centered on the beard
    [SerializeField] private int BEARDSPEED = 3;
	[SerializeField] private LineRenderer lineRender;
    [SerializeField] private GameObject beardTip; // the collider on the tip of the beard
    private int visibleSegments = 0; // segments currently "active" (actually active + were active but disabled since behind the player)
    private int trailingSegments = 0; // segments currently "active" but behind the player (so not actually active)
    private int maxSegments; // the current max length of the beard in segments

    private Vector2 target;
    private Vector2 beardPath;
    private Vector2 beardOrigin;

    private BeardState nextState = BeardState.IDLE; // which state to transition to next after the current one

	// Use this for initialization
	void Start () {
        target = gameObject.transform.position;
        // uses object pooling so we don't waste resources spawning and destroying beard segments
		for(int i=0; i<HARDMAXSEGMENTS; i++)
        {
			lineRender.SetPosition (i, new Vector3 (0, 0));
        }
	}

    public void WhipBeard(Vector2 target)
    {
        if(PlayerState.CurrentBeardState != BeardState.IDLE) { return; }
        nextState = BeardState.RETRACTING;
        ExtendBeard(target);
    }

    public void GrappleBeard(Vector2 target)
    {
        Debug.Log("grappling");
        if (PlayerState.CurrentBeardState != BeardState.IDLE) { return; }
        Debug.Log("next state: " + nextState);
        nextState = BeardState.PULLING;
        ExtendBeard(target);
    }

    // extend the beard out to a point
    private void ExtendBeard(Vector2 target)
    {
        this.target = target;
        maxSegments = (int)((target - beardOrigin).magnitude / SEGMENTDISTANCE); // can't use beardPath here as it hasn't been updated yet
        PlayerState.CurrentBeardState = BeardState.EXTENDING;
        beardTip.GetComponent<Collider2D>().enabled = true;
    }

    private void FixedUpdate()
    {
        beardOrigin = transform.position;
        beardPath = target - beardOrigin;
        switch (PlayerState.CurrentBeardState)
        {
            case BeardState.EXTENDING:
                for(int i=0; i<BEARDSPEED; i++)
                    AddBeardSegment();
                break;
            case BeardState.RETRACTING:
                for (int i = 0; i < BEARDSPEED; i++)
                    RemoveBeardSegment();
                break;
            case BeardState.IDLE:
                break;
            case BeardState.PULLING:
                RemoveTrailingBeardSegments();
                break;
            default:
                Debug.Log("Invalid beard animation state");
                break;
        }
        UpdateCurrentBeardSegments();
    }

    private void UpdateCurrentBeardSegments()
    { 
        // relocate the beard tip
        if(visibleSegments > 0)
            beardTip.transform.localPosition = lineRender.GetPosition(visibleSegments) + beardTipOffset;

        

    }

    private void Update()
    {
        float distanceToTarget = Vector2.Distance(new Vector2(transform.position.x, transform.position.y) + beardOrigin, target);
        for (int i = 0; i < visibleSegments; i++)
        {
            if (distanceToTarget < Vector2.Distance(new Vector2(transform.position.x, transform.position.y), transform.TransformPoint(lineRender.GetPosition(i))))
                lineRender.SetPosition(i, Vector2.zero);
        }
    }

    public void CheckForTrailingBeardSegments()
    {
        if (PlayerState.CurrentBeardState == BeardState.PULLING)
        {
            Debug.Log("DKLSJFHDSKJFHKDJSHFRKJDSHFKJDH");
            RemoveTrailingBeardSegments();

        }
    }

    private void RemoveTrailingBeardSegments()
    {
        Debug.Log("visible: " + visibleSegments + " trailing: " + trailingSegments);

        // find segments no longer between the player and grapple point, set them invisible, and update the counter
        int newTrailingSegments = 0;
        for(int i=trailingSegments; i<visibleSegments; i++)
        {
            Vector2 segmentPosition = transform.TransformPoint(lineRender.GetPosition(i));
            Vector2 origin = transform.position;

            if(!(segmentPosition.x > origin.x && segmentPosition.x < target.x || segmentPosition.x > target.x && segmentPosition.x < origin.x
                && segmentPosition.y > origin.y && segmentPosition.y < target.y || segmentPosition.y > target.y && segmentPosition.y < origin.y))
            {
                lineRender.SetPosition(i, Vector2.zero);
                newTrailingSegments++;
            }
        }
        trailingSegments += newTrailingSegments;

        // if we've pulled the player all the way, transition to the next state, and remove segments that are now trailing behind the player
        // NOTE: the actual physics of pulling the player should be handled somewhere else, this class is for animation states only
        if (visibleSegments == trailingSegments)
        {
            visibleSegments = 0;
            trailingSegments = 0;
            PlayerState.CurrentBeardState = BeardState.IDLE;
            Debug.Log("IDLE");
        }
    }

    private void AddBeardSegment()
    {
        // if we've reached max length, transition to the next state, otherwise, lengthen the beard
        if (visibleSegments >= maxSegments)
        {
            PlayerState.CurrentBeardState = nextState;
        }
        else
        {
			lineRender.SetPosition(visibleSegments, new Vector3(0.05f * visibleSegments, 0));
            visibleSegments++;
        }
    }

    private void RemoveBeardSegment()
    {
        // if we've retracted fully, transition to idle state, otherwize, shorten the beard
        if(visibleSegments == 0)
        {
            beardTip.GetComponent<Collider2D>().enabled = false;
            PlayerState.CurrentBeardState = BeardState.IDLE;
			lineRender.SetPosition(visibleSegments, new Vector3(0, 0));
        }
        else
        {
			lineRender.SetPosition(visibleSegments, new Vector3(0, 0));
            visibleSegments--;
        }
    }
}
