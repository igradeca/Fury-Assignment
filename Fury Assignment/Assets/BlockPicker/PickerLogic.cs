using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

public class PickerLogic : MonoBehaviour
{
    public IPickerState currentState;
    public IdleState IdleState = new IdleState();
    public MovingToState MovingToState = new MovingToState();
    public PickUpState PickUpState = new PickUpState();
    public SetTargetState SetTargetState = new SetTargetState();

    public float Speed;
    public List<GameObject> blocksInRange;
    public GameObject carryingBlock;
    public Transform target;
    public Transform depositBlue;
    public Transform depositRed;
    public Transform middlePos;

    public Vector2 movement; // 1 to -1

    private Rigidbody rb;
    

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        blocksInRange = new List<GameObject>();

        currentState = IdleState;
    }

    // Update is called once per frame
    void Update()
    {
        //movement = new Vector2(Input.GetAxis("Horizontal"), 0f);
        //// Pick up object
        //if (Input.GetKey(KeyCode.Space))
        //{
        //    pickUpBlock();
        //}

        //if (target != null)
        //{
        //    var distanceX = target.position.x - transform.position.x;
        //    Debug.Log("distanceX " + distanceX);

        //    if (Mathf.Abs(distanceX) <= 0.08f)
        //    {
        //        movement = Vector2.zero;
        //    }
        //    else
        //    {
        //        movement = new Vector2(distanceX, 0f).normalized;
        //    }
        //}

        currentState = currentState.DoState(this);
    }

    void FixedUpdate()
    {
        rb.MovePosition((Vector2)transform.position + (movement * Speed * Time.deltaTime));
    }

    void OnTriggerEnter(Collider other)
    {
        var detectedGO = other.gameObject;
        if (detectedGO.tag == "Block" && !blocksInRange.Contains(detectedGO))
        {
            blocksInRange.Add(detectedGO.transform.parent.gameObject);
            //Debug.Log("enter: " + other.gameObject.name);
        }
    }

    void OnTriggerExit(Collider other)
    {
        var detectedGO = other.gameObject;
        if (detectedGO.tag == "Block" && blocksInRange.Contains(detectedGO))
        {
            blocksInRange.Remove(detectedGO);
            //Debug.Log("exit: " + detectedGO.name);
        }
    }

}
