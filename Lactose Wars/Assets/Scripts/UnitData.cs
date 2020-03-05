using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UnitData : MonoBehaviour
{
    public int hexX;
    public int hexY;
    public GridManager grid;
    public int movespeed;

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


    public void Move()
    {
        //Create a temporary variable to act as our movespeed calculator
        int remainingMovement = movespeed;
        while (remainingMovement > 0)
        {
            //Make sure we actually have a valid path, if not, return out of the function
            if (currentPath == null) { return; }
            //We then need to decrement our movement cost
            remainingMovement--;
            //Update our unit's current tile position data
            hexX = currentPath[1].x;
            hexY = currentPath[1].y;
            //Then move our unit to the next tile in the path
            transform.position = grid.ConvertTileCoordToWorldCoord(hexX, hexY);
            //Then remove the previous tile from the list
            currentPath.RemoveAt(0);
            //We have just removed our starting tile and moved to the next tile on the list, if the tile we moved to is the only tile left in the list, we have reached our target so clear our list
            if (currentPath.Count == 1)
            {
                currentPath = null;
            }
        }
    }
}
