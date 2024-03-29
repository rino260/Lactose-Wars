﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//The following code has been adapted from "Quill18creates" tile movement video tutorials series found here:
//https://www.youtube.com/watch?v=kYeTW2Zr8NA and here https://www.youtube.com/watch?v=td3O1tkbqYQ

//Custom class to keep track of our graph nodes and their neighbors
//Here we are essentially creating a list of two dimensional arrays
public class Node
{
    public List<Node> neighbors;
    public int x;
    public int y;

    //Default constructor that initializes our list of nodes
    public Node()
    {
        neighbors = new List<Node>();
    }

    public float DistanceTo(Node n)
    {
        //We utilize our currents nodes current x and y grid position as well as the x and y of our target node to calculate the distance between the two points
        //NOTE: this will return the euclidean value between the two points which will cause it to prefer straighter lines, as diagonal movements will be SLIGHTLY farther away
        return Vector2.Distance(new Vector2(x, y), new Vector2(n.x, n.y));
    }
}
