using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

public class PickerLogic : MonoBehaviour
{
    public float Speed;
    public List<GameObject> detectedBlocks;
    public GameObject carryingBlock;

    private Rigidbody rb;
    private Vector2 movement; // 1 to -1

    private enum PickerState
    {
        Idle,
        GoingToPickUp,
        Carrying,
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        detectedBlocks = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        // Pick up object
        if (Input.GetKey(KeyCode.Space))
        {
            pickUpBlock();
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition((Vector2)transform.position + (movement * Speed * Time.deltaTime));
    }

    void OnTriggerEnter(Collider other)
    {
        var detectedGO = other.gameObject;
        if (detectedGO.tag == "Block" && !detectedBlocks.Contains(detectedGO))
        {
            detectedBlocks.Add(detectedGO);
            //Debug.Log("enter: " + other.gameObject.name);
        }
        else if (detectedGO.tag == "Target" && carryingBlock != null)
        {
            if (detectedGO.GetComponent<Block>().color == carryingBlock.GetComponent<Block>().color)
            {
                BlockInstantiator.instance.InsertBlockIntoTarget(carryingBlock);
                carryingBlock = null;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        var detectedGO = other.gameObject;
        if (detectedGO.tag == "Block" && detectedBlocks.Contains(detectedGO))
        {
            detectedBlocks.Remove(detectedGO);
            //Debug.Log("exit: " + detectedGO.name);
        }
    }

    void pickUpBlock()
    {
        if (detectedBlocks.Count == 0 || carryingBlock != null)
        {
            return;
        }

        var selectedBlock = detectedBlocks[0];
        detectedBlocks.RemoveAt(0);

        carryingBlock = selectedBlock;
        carryingBlock.transform.parent.gameObject.SetActive(false);
    }

}
