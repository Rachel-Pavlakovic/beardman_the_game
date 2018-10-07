﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeardController : MonoBehaviour
{

    public float followDistance = 0f;
    public bool isLimitedByDistance = true;
    public Rigidbody2D beardman;
    private MovementController movementController;
    private BeardAnimationController beardAnimator;

    public float grappleForce = 3f;
    private float grappleStrength = 0f;
    Camera mainCamera;
    // Use this for initialization
    void Start()
    {
        if (beardman == null)
        {
            beardman = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody2D>();
        }
        mainCamera = Camera.main;
        movementController = beardman.GetComponent<MovementController>();
        beardAnimator = beardman.GetComponentInChildren<BeardAnimationController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isLimitedByDistance)
        {
            Vector2 mousePos = Input.mousePosition;
            Vector2 followVector = (Vector2)mainCamera.ScreenToWorldPoint(mousePos) - beardman.position;
            this.transform.position = Vector2.ClampMagnitude(followVector, followDistance) + beardman.position;
        }
        //if (Input.GetKey(KeyCode.E))
        //{
        //    if (followDistance < 3.75)
        //        followDistance += 0.25f;
        //}
        //if (Input.GetKey(KeyCode.Q))
        //{
        //    if (followDistance > 0.25)
        //        followDistance -= 0.25f;
        //}
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            UseBeard();
        }
    }
    public void UseBeard()
    {
        Vector2 targetPosition = this.transform.position;
        RaycastHit2D targetHit = Physics2D.Raycast(targetPosition, Vector2.zero);
        GameObject targetObject = targetHit ? targetHit.collider.gameObject : null;

        // TODO: here I assume that all enemies/grappleable objects will have an associated component, we can change this later based on the actual components' names/different critereon
        if (targetObject && targetObject.name == "Grapple Point")
        {
            GrappleBeard(targetObject);
        }
        else
        {
            WhipBeard(this.gameObject);
        }
    }


    // assuming the target is in range, not range-limited
    private void WhipBeard(GameObject targetObject)
    {
        beardAnimator.WhipBeard(targetObject.transform);
        Debug.Log("whip");
    }

    // assuming the target is in range, not range-limited
    private void GrappleBeard(GameObject grappleObject)
    {
        var dir = (Vector2) grappleObject.transform.position- beardman.position;

        beardman.AddForce(new Vector2(dir.x, 0) * grappleForce, ForceMode2D.Impulse);
        beardman.AddForce(dir * grappleForce, ForceMode2D.Impulse);
        Debug.Log("grapple");
    }
}//end beard controller