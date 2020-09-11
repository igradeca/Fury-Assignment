using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridLogic : MonoBehaviour
{
    public GameObject gridElement;
    public Vector2Int gridSize;

    public ElementData[][] grid;

    private Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;

        var initGridPos = new Vector3(0.5f, 0.5f, 0f);

        grid = new ElementData[gridSize.y][];
        for (int i = 0; i < gridSize.y; i++)
        {
            var tempPos = initGridPos;
            tempPos.y += i;

            grid[i] = new ElementData[gridSize.x];
            for (int j = 0; j < gridSize.x; j++)
            {
                var newElement = Instantiate(gridElement, transform);
                newElement.transform.localPosition = tempPos;
                newElement.name = "Element_" + i.ToString() + "_" + j.ToString();

                grid[i][j] = new ElementData { GO = newElement };

                tempPos.x += 1f;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var vector = mainCamera.ScreenPointToRay(Input.mousePosition); //mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log(vector);

            RaycastHit hit;
            if (Physics.Raycast(vector, out hit))
            {
                Debug.Log(hit.transform.name);
            }

        }

    }
}

public class ElementData
{
    public int F;
    public int G;
    public int H;

    public GameObject GO;
}
