using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseManager : MonoBehaviour
{
    public Camera navyCam;

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
    Transform hitTile; //Create a reference for the hit tile gameobject
    Vector3 defaultPos; //Create a reference to the hit tile's default position
    Vector3 targetPos; //Create a dynamic/temporary reference to the position we want to lerp the hit tile to
    Vector3 defaultScale; //Create a reference to the hit tile's default scale
    Material defaultMat; //Create a reference to the hit tile's default material



    void Update()
    {
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
                //If the tile we hithas not been defined:
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
        }
    }


    void Highlight()
    {
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
        //If we click the left mouse button and we are highlighting a tile:
        if(Input.GetMouseButtonDown(0) && hitTile != null)
        {
            //Extract the X and Y coordinates from the clicked hex and call the "MoveSelectedUnit" function on our GridManager using its coordinates
            int x = hitTile.GetComponentInChildren<HexData>().hexX;
            int y = hitTile.GetComponentInChildren<HexData>().hexY;
            hitTile.root.GetComponent<GridManager>().MoveSelectedUnit(x, y);
        }
    }
}
