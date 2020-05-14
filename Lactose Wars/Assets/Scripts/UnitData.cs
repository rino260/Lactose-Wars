using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//The following code has been adapted from "Quill18creates" tile movement video tutorials series found here:
//https://www.youtube.com/watch?v=kYeTW2Zr8NA and here https://www.youtube.com/watch?v=td3O1tkbqYQ

public class UnitData : MonoBehaviour
{
    public int hexX;
    public int hexY;
    public GridManager grid;
    public int movespeed;
    [Range(0, 20)]
    public int animationSpeed;
    public int turningAnimationMultiplier;
    string tileTag = "Tile";
    public List<Transform> pieceSegments;

    //Internal references that need to be public for the CollisionManagement scripts
    [HideInInspector]
    public int remainingMovement;
    [HideInInspector]
    public bool shouldMove;
    Vector3 destination;
    [HideInInspector]
    public GameObject endTile;

    public List<Node> currentPath = null;


    private void Start()
    {
        //Initialize our desitination to be set later
        destination = transform.position;
        StartCoroutine(ToggleOccupiedHexes(1, 0f, false));
    }


    void Update()
    {
        DrawPathingLine();
        AnimateMovement();
    }


    void DrawPathingLine()
    {
        //We only want to draw a line if we have a path
        if (currentPath != null)
        {
            int currentNode = 0;

            while (currentNode < currentPath.Count - 1)
            {
                //Using our path information from our gridmanager, find our current node and the next node
                Vector3 start = grid.ConvertTileCoordToWorldCoord(currentPath[currentNode].x, currentPath[currentNode].y);
                Vector3 end = grid.ConvertTileCoordToWorldCoord(currentPath[currentNode + 1].x, currentPath[currentNode + 1].y);
//TEMPORARY SOLUTION FOR THE PURPOSESE OF TESTING, ADD IN LINE DRAWING FUNCTIONALITY INTO PLAY MODE LATER
//ONLY MAKE THIS LINE VISIBLE TO THE SHIPS PLAYER
                //Draw a line between our current node and our next node
                Debug.DrawLine(start, end, Color.white);
                //Increment our while loop and repeat the above for each node in our path
                currentNode++;
            }
        }
    }


    public void InitializeMovement()
    {
        //Each time the "next turn" button is pressed it initializes the next "step" of gameplay for all units tied to the button
        //Reset our unit's total amount of movement
        remainingMovement = movespeed;
        //Because we need to toggle the hexes below the ship to be walkable before movement begins, we check if the ship has a valid path
        //If the ship has a valid path we know it is ready to move so we toggle the hexes to be walkable and pass our coroutine the go ahead to execute the "stepforward" method
        if (currentPath != null) { StartCoroutine(ToggleOccupiedHexes(0, 0.1f, true)); }
        //StepForward();
    }


    void StepForward()
    {
        if (remainingMovement > 0)
        {
            //Make sure we actually have a valid path, if not return out of the function
            if (currentPath == null) { return; }
            //Update our unit's current tile position data
            hexX = currentPath[1].x;
            hexY = currentPath[1].y;
            //Set our destination to be the next tile in the path
            destination = grid.ConvertTileCoordToWorldCoord(hexX, hexY);
            shouldMove = true;
        }
    }


    void AnimateMovement()
    {
        if (shouldMove)
        {
            //Calculate the vector from where we are to where we need to go
            Vector3 distFromTarget = destination - transform.position;

            //We want to start the unit's rotation before its movement animation for a more natural turning look
            //Calculate the rotation the unit will need to make to point towards the target
            Quaternion rotation = Quaternion.LookRotation(distFromTarget);
            //Lock the rotation to the Y axis
            distFromTarget.y = 0;
            //Slerp the rotation over time (with an added speed multipler to make sharp turns look more natural)
            transform.GetChild(0).rotation = Quaternion.Slerp(transform.GetChild(0).rotation, rotation, animationSpeed * turningAnimationMultiplier * Time.deltaTime);

            //We then need to normalize our "distFromTarget" vector to be a size of 1 and multiply it be our speed and a set timestep
            Vector3 targetLocation = distFromTarget.normalized * animationSpeed * Time.deltaTime;
            //Then clamp our "targetLocation" value to be no larger than the distance we have to travel to prevent our unit from wiggling back and forth if it slightly overshoots the target
            targetLocation = Vector3.ClampMagnitude(targetLocation, distFromTarget.magnitude);
            //Move our unit to our target location
            transform.Translate(targetLocation);
            //Once the unit has moved to its destination, tell it to stop moving, toggle the hexes under it to occupied, and move to the next step method
            if (transform.position == destination)
            {
                shouldMove = false;
                NextStep();
            }
        }
    }


    void NextStep()
    {
        //Remove the previous tile from the list
        currentPath.RemoveAt(0);
        //If the tile we just moved to is the only tile left in the list, we have reached our target so clear our list
        if (currentPath.Count == 1) { currentPath = null; }
        //We then need to decrement our movement cost
        remainingMovement--;
        //If we run out of movement or reach our desintation toggle the hexes underneath us to be occupied
        if (remainingMovement == 0 || transform.position == destination) { StartCoroutine(ToggleOccupiedHexes(1, 0.25f, false)); }
        //Continue moving if applicaple
        StepForward();
    }


    IEnumerator ToggleOccupiedHexes(int status, float delay, bool move)
    {
        //We want to control when our hexes will be toggled on a more predicatble/precise interval than in the update function so we will use a delay variable to control this behavior
        yield return new WaitForSeconds(delay);

        for (int i = 0; i < pieceSegments.Count; i++)
        {
            RaycastHit hit;
            //Send a raycast downward from each segment in our game piece and toggle all tiles below it 
            if (Physics.Raycast(pieceSegments[i].position, Vector3.down, out hit, Mathf.Infinity) && hit.transform.tag == tileTag)
            {
                int hitX = hit.transform.parent.gameObject.GetComponent<HexData>().xCoord;
                int hitY = hit.transform.parent.gameObject.GetComponent<HexData>().yCoord;

                grid.ToggleHex(hitX, hitY, status);
            }
        }
        //Instead of handling this call in the update function we need it to be imbedded within our toggle hex method to ensure we will be able to execute the toggle function before the unit moves
        if (move) { StepForward(); }
    }


    public void ToggleClickableHex(bool enable)
    {
        if(enable) { endTile.transform.GetChild(0).GetComponent<Collider>().enabled = true; }
        else { endTile.transform.GetChild(0).GetComponent<Collider>().enabled = false; }
    }
}
