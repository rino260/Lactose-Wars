using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MouseManager : MonoBehaviour
{
    public Camera navyCam;

    //References for initial unit spawning
    public GridManager gridManager;
    public Button nextTurnButton;
    public List<GameObject> ships;
    //Since we will be using this to check 
    int shipCounter;
    bool placing;

    //Inspector variables to allow customizing highlight behavior
    public Material highlightMat;
    [Range(0f, 3f)]
    public float highlightOffset;
    [Range(0f, 1f)]
    public float highlightSpeed;
    [Range(0.1f, 4f)]
    public float highlightScale;

    //Internal references from which to calculate highlight information
    string tileTag = "Tile";
    string shipTag = "Ship";
    Transform hitTile; //Create a reference for the hit tile gameobject
    Transform hitShip; //Create a reference for the hit ship gameobject
    Vector3 defaultPos; //Create a reference to the hit tile's default position
    Vector3 targetPos; //Create a dynamic/temporary reference to the position we want to lerp the hit tile to
    Vector3 defaultScale; //Create a reference to the hit tile's default scale
    Material defaultMat; //Create a reference to the hit tile's default material

    //Internal reference for clicked tiles 
    int hitTileX;
    int hitTileY;


    public void SpawnUnit()
    {
        //If we have not placed all of our ships
        if (shipCounter < ships.Count)
        {
            gridManager.selectedUnit = null;
            placing = true;
        }
    }


    void Update()
    {
        //Check to see if we are hovering over a gameobject that is a part of our event system and if so return out of our update method so we cannot click on gameobjects behind it
        if (EventSystem.current.IsPointerOverGameObject()) { return; }

        CheckMouseCollision();
        CheckMouseClick();
    }


    void CheckMouseCollision()
    {
        //Send out a raycast from the camera through the mouse to check what we are hovering over
        Ray ray = navyCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo))
        {
            //If we hit a tile:
            if (hitInfo.collider.tag == tileTag)
            {
                //If the tile we hit has not been defined:
                if (hitTile == null)
                {
                    //Define what tile we hit
                    hitTile = hitInfo.collider.transform.parent;
                    //Highlight that tile
                    Highlight();
                }
                //If the parent of the tile we are hitting is different from the parent of the tile we have defined:
                else if (hitInfo.transform.parent != hitTile)
                {
                    //Un-highlight it
                    ResetHighlight();
                    //Define the new tile we are now hitting
                    hitTile = hitInfo.collider.transform.parent;
                    //Highlight the new tile
                    Highlight();
                }
            }
            //If we are hitting something that is not a tile:
            else if (hitTile != null)
            {
                //Clear the stored information of the tile we are coming from and un-highlight it
                ResetHighlight();
                hitTile = null;
            }
            //If we hit a ship, define a reference to its root, otherwise clear any stored info
//INCORPORATE HIGHLIGHT FUNCTIONALITY FOR SELECTED SHIPS
            if(hitInfo.collider.tag == shipTag) { hitShip = hitInfo.collider.transform.root; }
            else if (hitInfo.collider.tag != shipTag) { hitShip = null; }
        }
    }


    void Highlight()
    {
        //Extract the X and Y coordinates from the hit tile
        hitTileX = hitTile.GetComponentInChildren<HexData>().xCoord;
        hitTileY = hitTile.GetComponentInChildren<HexData>().yCoord;

//TEMPORARY USER FEEDBACK, REPLACE WITH A VFX EFFECT OR TEMPORARY HEX TILE SO WE STOP GETTING THE TWITCHING ISSUE
        //Cache the hit tile's default material and default world position
        defaultMat = hitTile.GetComponentInChildren<MeshRenderer>().material;
        defaultPos = hitTile.position;
        defaultScale = hitTile.localScale;

        //Apply a highlighted material to the hit tile and disable collisions
        hitTile.GetComponentInChildren<MeshRenderer>().material = highlightMat;

        //Calculate the hit tile's target position using our pre-defined offset, and lerp it to that position
        targetPos = new Vector3(defaultPos.x, defaultPos.y + highlightOffset, defaultPos.z);
        hitTile.position = Vector3.Lerp(defaultPos, targetPos, highlightSpeed);
        //Lerp the hit tile's local scale by our pre-defined scale factor
        hitTile.localScale = Vector3.Lerp(transform.localScale, transform.localScale * highlightScale, highlightSpeed);
    }


    void ResetHighlight()
    {
        //Change the hit tile back to its default material and enable collisions
        hitTile.GetComponentInChildren<MeshRenderer>().material = defaultMat;

        //Reset the hit tile's target position to its default position, and then lerp it there
        targetPos = defaultPos;
        hitTile.position = Vector3.Lerp(targetPos, defaultPos, highlightSpeed);
        //Reset the hit tile's local scale
        hitTile.localScale = Vector3.Lerp(transform.localScale, defaultScale, highlightSpeed);
    }


    void CheckMouseClick()
    {
        //If we click the left mouse button, are selecting a tile, and have placed all of our units:
        if (Input.GetMouseButtonDown(0) && hitTile != null && !placing)
        {
            //Use the X and Y coordinates from the clicked hex and call the "GeneratePathTo" function on our GridManager using its coordinates
            hitTile.root.GetComponent<GridManager>().GeneratePathTo(hitTileX, hitTileY);
        }

        //If we click the left mouse button, are selecting a ship, and have placed all of our units:
        if (Input.GetMouseButtonDown(0) && hitShip != null && !placing)
        {
            gridManager.selectedUnit = hitShip.gameObject;
        }

        //If we have not placed all of our units
        if (Input.GetMouseButtonDown(0) && hitTile != null && placing)
        {
            //Intantiate the current ship in the list at our converted coordinates
            GameObject go = Instantiate(ships[shipCounter], gridManager.ConvertTileCoordToWorldCoord(hitTileX, hitTileY), Quaternion.identity);
            //Set the ship's x and y tile position
            go.GetComponent<UnitData>().hexX = hitTileX;
            go.GetComponent<UnitData>().hexY = hitTileY;
            go.GetComponent<UnitData>().grid = gridManager;
            //Add the "InitializeMovement" method from each spawned ship to our next turn button
            nextTurnButton.GetComponent<Button>().onClick.AddListener(() => go.GetComponent<UnitData>().InitializeMovement());
            //Set our spawned ship to be the selected unit for the time being
            gridManager.selectedUnit = go;
            //If we have not placed all of our ships increment our counter, otherwise take us out of the placement phase
            shipCounter++;
            if (shipCounter == ships.Count) { placing = false; }
        }

//TEMPORARY TEST TO SEE IF WE CAN CONTROL A TILE'S WALKABILITY
        /*if (Input.GetMouseButtonDown(1) && hitTile != null)
        {
            hitTile.root.GetComponent<GridManager>().ToggleHex(hitTileX, hitTileY);
        }*/
    }
}
