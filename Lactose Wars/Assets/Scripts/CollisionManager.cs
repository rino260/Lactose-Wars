using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CollisionManager : MonoBehaviour
{
    Rigidbody rb;
    Collider col;
    public UnitData shipPathing;

    Node lastNode;
    Quaternion lastRot;
    bool ship = false;
    string shipTag = "Ship";


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        col = GetComponent<Collider>();

        if (this.gameObject.tag == shipTag) { ship = true; }
    }


    void OnCollisionEnter(Collision otherCol)
    {
        if(ship && otherCol.gameObject.tag == shipTag)
        {
            //We need to do a little math to ensure this ship can move entirely out of the way of the other ship
            if (shipPathing.remainingMovement >= shipPathing.pieceSegments.Count + 1)
            {
                //If so we can tell our coroutine to temporarily ignore the collision between the two ships
                StartCoroutine(PhaseThrough(otherCol.collider, 0.5f));
            }
            else
            {
                //Quickly save the ships rotation information from the last node
                lastRot = shipPathing.currentRot;
                //When the ships collide, grab the node they are coming from
                lastNode = shipPathing.currentPath[0];
                //Eject out of the current pathfinding, and cancel the current path
                shipPathing.shouldMove = false;
                shipPathing.currentPath = null;
                //Using the node data, calculate the position of the node before the collision occured and reset the ship's position to that node
                int nodeX = lastNode.x;
                int nodeY = lastNode.y;
                transform.root.position = shipPathing.grid.ConvertTileCoordToWorldCoord(nodeX, nodeY);
                
                //Reset the ships rotation
                transform.parent.rotation = lastRot; 
//MAYBE INSTEAD OF THIS YOU CAN SAVE THE LAST TWO NODES AND DO A REVERSE LOOK AT WHERE THE END OF THE SHIP LOOKS AT IT INSTEAD OF THE FRONT???
               
                //Zero out the velocity of both ships to prevent drifting
                rb.velocity = Vector3.zero;
                otherCol.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;


//CALCULATE THE SHIPS ROTATIONS BEFORE THE COLLISION AND RESET THEIR ROTATIONS AS WELL AS THEIR VELOCITIES
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
