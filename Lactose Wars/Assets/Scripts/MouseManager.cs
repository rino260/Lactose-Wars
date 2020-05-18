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
    int shipCounter;
    bool placing;

    //Inspector variables to allow customizing highlight behavior
    string tileTag = "Tile";
    string shipTag = "Ship";
    Transform hitTile;
    Transform hitShip;
    //Tile highlight information
    public GameObject tileHilghlightFXPrefab;
    GameObject tileHighlightFX;

    //Internal reference for clicked tiles 
    int hitTileX;
    int hitTileY;


    void Start()
    {
        //Create our highlight VFX and hide it for future use
        tileHighlightFX = Instantiate(tileHilghlightFXPrefab, Vector3.zero, Quaternion.identity, null);
        tileHighlightFX.SetActive(false);
    }


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
                //Define what tile we hit
                hitTile = hitInfo.collider.transform.parent;
                //Grab our hit tile's coordinate information
                hitTileX = hitTile.GetComponentInChildren<HexData>().xCoord;
                hitTileY = hitTile.GetComponentInChildren<HexData>().yCoord;
                //Highlight that tile
                Highlight(hitInfo);
            }
            //If we are hitting something that is not a tile:
            else if (hitInfo.collider != hitTile)
            {
                //Clear the previous tile's stored information and un-highlight it
                ResetHighlight(hitInfo);
                hitTile = null;
            }
            //If we hit a ship, define a reference to its root, otherwise clear any stored info
//INCORPORATE HIGHLIGHT FUNCTIONALITY FOR SELECTED SHIPS
            if(hitInfo.collider.tag == shipTag) { hitShip = hitInfo.collider.transform.root; }
            else if (hitInfo.collider.tag != shipTag) { hitShip = null; }
        }
    }


    void Highlight(RaycastHit hitObj)
    {
        //Enable and move our highlight effect to the hit tile's position
        tileHighlightFX.SetActive(true);
        tileHighlightFX.transform.position = hitTile.transform.position;
    }


    void ResetHighlight(RaycastHit hitObj)
    {
        //Turn off our highlight effect
        tileHighlightFX.SetActive(false);
    }


    void CheckMouseClick()
    {
        //If we click the left mouse button, are selecting a tile, are placing our units, and have a selected unit:
        if (Input.GetMouseButtonDown(0) && hitTile != null && !placing && gridManager.selectedUnit != null)
        {
            UnitData selectedUnitData = gridManager.selectedUnit.GetComponent<UnitData>();
            //Use the X and Y coordinates from the clicked hex and call the "GeneratePathTo" function on our GridManager using its coordinates
            hitTile.root.GetComponent<GridManager>().GeneratePathTo(hitTileX, hitTileY);
            selectedUnitData.selectedTileFX.transform.position = hitTile.transform.position;
            selectedUnitData.selectedTileFX.SetActive(true);
            selectedUnitData.DrawPathingLine();

            //If our selected unit already has a final destination recorded, enable its collider, record the new tile, and turn off the new tile's collider
            if (selectedUnitData.endTile != null)
            {
                selectedUnitData.ToggleClickableHex(true);
                selectedUnitData.endTile = hitTile.gameObject;
                selectedUnitData.ToggleClickableHex(false);
            }
            //Otherwise record the new tile and turn off its collider
            else
            {
                selectedUnitData.endTile = hitTile.gameObject;
                selectedUnitData.ToggleClickableHex(false);
            }
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
    }
}
