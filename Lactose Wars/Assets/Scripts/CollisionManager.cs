using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CollisionManager : MonoBehaviour
{
    Rigidbody rb;
    Collider col;
    Vector3 startPos;
    Quaternion startRot;
    public UnitData shipPathing;

    Node lastNode;
    Node conflictNode;
    bool ship = false;
    string shipTag = "Ship";


    void Awake()
    {
        //Initialize and record the ship's components and transform
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        col = GetComponent<Collider>();
        startPos = transform.localPosition;
        startRot = transform.localRotation;

        if (this.gameObject.tag == shipTag) { ship = true; }
    }


    void OnCollisionEnter(Collision otherCol)
    {
        if(ship && otherCol.gameObject.tag == shipTag)
        {
            //If the ship has enough movement left to move its entire unit length out of the way AND the ship's current path is long enough to allow this
            if (shipPathing.remainingMovement >= shipPathing.pieceSegments.Count + 1 && shipPathing.currentPath.Count - 1 > shipPathing.pieceSegments.Count + 1) 
            {
                //Tell our coroutine to temporarily ignore the collision between the two ships for a specified period of time
                StartCoroutine(PhaseThrough(otherCol.collider, 0.5f));
                return;
            }
            else
            {
                //On collision, grab both the node the ship is coming from and the node the ship is attempting to move to
                lastNode = shipPathing.currentPath[0];
                conflictNode = shipPathing.currentPath[1];
                //Eject out of the current pathfinding, cancel the current path, disable the selected tile FX, and clear the path visual
                shipPathing.shouldMove = false;
                shipPathing.currentPath = null;
                shipPathing.selectedTileFX.SetActive(false);
                shipPathing.path.positionCount = 0;
                //Using the node data, calculate the position of the node before the collision occured and reset the ship's position to that node
                int nodeX = lastNode.x;
                int nodeY = lastNode.y;
                transform.root.position = shipPathing.grid.ConvertTileCoordToWorldCoord(nodeX, nodeY);
                //Update our ship's unitdata with the updated node position since we weren't able to complete our path
                shipPathing.hexX = nodeX;
                shipPathing.hexY = nodeY;

                //Convert the conflict node into a vector3
                Vector3 conflictNodePos = shipPathing.grid.ConvertTileCoordToWorldCoord(conflictNode.x, conflictNode.y);
                //Calculate the vector from where we are to where we need to look at
                Vector3 rotationVector = conflictNodePos - transform.root.position;
                //Calculate the rotation the unit will need to make to point towards the target
                Quaternion rotation = Quaternion.LookRotation(rotationVector);
                //Lock the rotation to the Y axis
                rotationVector.y = 0;
                //Re-align the ship's rotation after the collision
                transform.parent.rotation = rotation;

                //Zero out the ship's velocity to prevent drifting
                rb.velocity = Vector3.zero;
                //Reset the ship model's native position and rotation
                transform.localPosition = startPos;
                transform.localRotation = startRot;
            }
        }
    }


    IEnumerator PhaseThrough(Collider otherCol, float delay)
    {
        Physics.IgnoreCollision(otherCol, col, true);

        yield return new WaitForSeconds(delay);

        Physics.IgnoreCollision(otherCol, col, false);
    }
}
