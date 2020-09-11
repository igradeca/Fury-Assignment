using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockInstantiator : MonoBehaviour
{
    public static BlockInstantiator instance;

    public float InstantiationTime = 5f;
    public int BlockCountBlue = 5;
    public int BlockCountRed = 5;

    public GameObject BlockBlue;
    public GameObject BlockRed;

    public int CounterBlue;
    public int CounterRed;
    public List<GameObject> BlockList;

    private float instantiationTimer;

    BoxCollider2D col;

    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<BoxCollider2D>();
        BlockList = new List<GameObject>();

        if (instance == null)
        {
            instance = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        instantiationTimer += Time.deltaTime;

        var blockBlueCountInBound = CounterBlue <= BlockCountBlue;
        var blockRedCountInBound = CounterRed <= BlockCountRed;
        if (instantiationTimer >= InstantiationTime && blockBlueCountInBound && blockRedCountInBound)
        {
            var position = transform.position;
            position.x = Random.Range(col.bounds.min.x, col.bounds.max.x);

            GameObject block = null;
            if (Random.Range(0, 2) == 0 && blockBlueCountInBound)
            {
                block = BlockBlue;
                ++CounterBlue;
            }
            else if (blockRedCountInBound)
            {
                block = BlockRed;
                ++CounterRed;
            }

            if (block != null)
            {
                var newBlock = Instantiate(block, position, Quaternion.identity);
                BlockList.Add(newBlock);
                instantiationTimer = 0f;
            }
        }
    }

    public void InsertBlockIntoDeposit(GameObject blockGO)
    {
        if (blockGO.GetComponent<Block>().color == BlockColour.Blue)
        {
            --CounterBlue;
        }
        else
        {
            --CounterRed;
        }

        BlockList.Remove(blockGO);
        Debug.Log("Destroyed block: " + blockGO.name);
        Destroy(blockGO);
    }

}
