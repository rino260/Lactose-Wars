using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitData : MonoBehaviour
{
    public int hexX;
    public int hexY;
    public GridManager grid;

    public List<Node> currentPath = null;

    void Update()
    {
        //We only want to draw a line if we have a path
        if(currentPath != null)
        {
            int currentNode = 0;

            while (currentNode < currentPath.Count - 1)
            {
                Vector3 start = grid.ConvertTileCoordToWorldCoord(currentPath[currentNode].x, currentPath[currentNode].y);
                Vector3 end = grid.ConvertTileCoordToWorldCoord(currentPath[currentNode + 1].x, currentPath[currentNode + 1].y); ;

                //Temporary line for the purposes of initial setup
                Debug.DrawLine(start, end, Color.white);

                currentNode++;
            }
        }
    }
}
