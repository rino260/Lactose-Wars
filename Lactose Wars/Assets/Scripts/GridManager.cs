using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//The following code has been adapted from "Quill18creates"'s tile movement video tutorials series found here:
//https://www.youtube.com/watch?v=kYeTW2Zr8NA
//and here https://www.youtube.com/watch?v=td3O1tkbqYQ

public class GridManager : MonoBehaviour
{
    [Range(0,100)]
    public int gridX;
    [Range(0, 100)]
    public int gridY;

    public TileType[] tileTypes;
    int[,] tileCoord;
    Node[,] graph;

    float xOffset = 0.866f;
    float zOffset = 0.75f;


//TODO Much like we do with the hex tiles, when implementing more than one unit give each one a script and when clicked on, we tell this script what unit is now selected (quill's civ/dungeon tile video #3 10:35)
    public GameObject selectedUnit;

    void Start()
    {
        //Ensure that our grid will have an odd length and width so we will have a center tile on the grid
        if (gridX % 2 == 0) { gridX++; }
        if (gridY % 2 == 0) { gridY++; }

        //Offset the grid manager according to the grid size and X & Z offsets
        transform.position = new Vector3(transform.position.x - ((gridX / 2) * xOffset), transform.position.y, transform.position.y - ((gridY / 2) * zOffset));

//NOTE ONCE UNIT SPAWNING IS INTEGRATED THIS WILL NEED TO BE REFACTORED
//YOU'LL LIKELY HAVE TO WRITE A FUNCTION FOR A UNIT TO CHECK WHAT TILE IT IS ON SO IT CAN RE-BIND ITSELF TO THE GRID WHEN THE STAGE GENERATES NEW TILES
        //Setup the selected unit's variables
        selectedUnit.GetComponent<UnitData>().hexX = gridX / 2;
        selectedUnit.GetComponent<UnitData>().hexY = gridY / 2;
        selectedUnit.GetComponent<UnitData>().grid = this;

        InitMapData();
        GeneratePathfindingGraph();
        GenerateMapTiles();
    }


    //Populate an array of coordinates with which to assign to map tiles according to a specified quadrant size and tile type
    //Set every tile to the desired type of tile in the "tiletype" array
    void InitMapData()
    {
        tileCoord = new int[gridX, gridY];

        //Initialize X tiles
        for (int x = 0; x < gridX; x++)
        {
            //Initialize Y tiles
            for (int y = 0; y < gridY; y++) { tileCoord[x, y] = 0;}
        }
    }

    /*
    //Get the tile type from the specified tile and return its movement cost
    float CostToEnter(int sourceX, int sourceY, int targetX, int targetY)
    {
        TileType tt = tileTypes[tileCoord[targetX, targetY]];

        float cost = tt.moveCost;

        if(sourceX != targetX && sourceY != targetY)
        {
            //We are now making a weird diagonal movement, tiime to fudge the cost of movement for tie breaking
            cost += 100f;
        }

        return cost;
    }*/


//A POTENTIAL ISSUE WITH  THIS CODE IS THAT WE WILL BE GENERATING GRAPH DATA FOR OUR ENTIRE GRID EVEN THOUGH WE ARE ONLY SPAWNING/UTILIZING A SELECT PORTION OF OUR TILES
//SO, UNIT PATHING THAT HAPPENS AT THE EDGE OF OUR USABLE TILES MAY CHOOSE TO PATH OUTSIDE OF THE GRID
    void GeneratePathfindingGraph()
    {
        //Here we need to specify the maximum number of possible "Node" arrays
        graph = new Node[gridX, gridY];

        //Initialize a "Node" for each spot in the array
        for (int x = 0; x < gridX; x++)
        {
            for (int y = 0; y < gridY; y++)
            {
                //Here we need to actually create a node array that will contain the neighbors for each specific tile
                graph[x, y] = new Node();

                //We need each node to be self aware of their position
                graph[x, y].x = x;
                graph[x, y].y = y;
            }
        }

        //Now that all the nodes exist, calculate their neighbors
        for (int x = 0; x < gridX; x++)
        {
            for (int y = 0; y < gridY; y++)
            {
                //If the tile is not on the left edge of the grid
                if (x > 0)
                {
                    //Add the tile to our left to its list of neighbors
                    graph[x, y].neighbors.Add(graph[x - 1, y]);
                    //If the tile is not on the bottom edge of the grid
                    if (y > 0)
                    {
                        //Add the tile to its bottom left to its list of neighbors (the x row that tile is in is shifted depending on wether or not we are on an even or odd row)
                        if (Mathf.Abs(y) % 2 == 1) { graph[x, y].neighbors.Add(graph[x, y - 1]); }
                        else { graph[x, y].neighbors.Add(graph[x - 1, y - 1]); }
                    }
                    //If the tile is not on the top edge of the grid
                    if (y < gridY - 1)
                    {
                        //Add the tile to its top left to its list of neighbors (the x row that tile is in is shifted depending on wether or not we are on an even or odd row)
                        if (Mathf.Abs(y) % 2 == 1) { graph[x, y].neighbors.Add(graph[x, y + 1]); }
                        else { graph[x, y].neighbors.Add(graph[x - 1, y + 1]); }

                    }
                }
                //If the tile is not on the right edge of the grid
                if (x < gridX - 1)
                {
                    //Add the tile to our left to its list of neighbors
                    graph[x, y].neighbors.Add(graph[x + 1, y]);
                    //If the tile is not on the bottom edge of the grid
                    if (y > 0)
                    {
                        //Add the tile to its bottom right to its list of neighbors (the x row that tile is in is shifted depending on wether or not we are on an even or odd row)
                        if (Mathf.Abs(y) % 2 == 1) { graph[x, y].neighbors.Add(graph[x + 1, y - 1]); }
                        else { graph[x, y].neighbors.Add(graph[x, y - 1]); }
                    }
                    //If the tile is not on the top edge of the grid
                    if (y < gridY - 1)
                    {
                        //Add the tile to its top right to its list of neighbors (the x row that tile is in is shifted depending on wether or not we are on an even or odd row)
                        if (Mathf.Abs(y) % 2 == 1) { graph[x, y].neighbors.Add(graph[x + 1, y + 1]); }
                        else { graph[x, y].neighbors.Add(graph[x, y + 1]); }

                    }
                } 
            }
        }
    }


    //Generate map tiles according to their respective quadrants, coordinates, tile types, and prefabs
    //Increment the "quadNum" variable after generating each quadrant to prevent one quad from being generated more than once
    void GenerateMapTiles()
    {
        for (int x = 0; x < gridX; x++)
        {
            for (int y = 0; y < gridY; y++)
            {
                SpawnTiles(x, y, 0, tileTypes[tileCoord[x, y]].hexTilePrefab);
            }
        }
    }


    void SpawnTiles(int x, int y, float rowNum, GameObject tileCoord)
    {
        //Because our tiles are not a full unit in width we need to offset their x position slightly
        float xPos = x * xOffset;

        //In an odd row all tiles only need to have their x positions shifted by half as much as the tiles in an even row
        //We have to use the absolute value of Y so our negative Y quadrants will spawn properly
        if (Mathf.Abs(y) % 2 == 1) { xPos += xOffset / 2f; }

        //Create a layermask for our boundary objects
        int layerMask = 1 << 8;
        RaycastHit hit;
        //Send a raycast downward from each potential coordinate point and only spawn a tile when it is inside the stage boundary
        if (Physics.Raycast(new Vector3(transform.position.x + xPos, 0, transform.position.z + y * zOffset), Vector3.down, out hit, Mathf.Infinity, layerMask))
        {
            //Because we are importing our models from Blender they have a -90 degree X rotation applied
            //Because of this we need to use the "z" axis like we would use the "y" axis if we want "y" to continue to be our "height" dimension
            //Because we are also offsetting our grid manager object we need to ensure the spawned hexes will originate from that new position and not the world coordinates
            GameObject go = Instantiate(tileCoord, new Vector3(transform.position.x + xPos, 0, transform.position.z + y * zOffset), Quaternion.identity);
            //We know each hex will have a script on it containing empty X and Y integers so when we instantiate a hex object, pass its X and Y data to each hex
            HexData data = go.GetComponent<HexData>();
            data.hexX = x;
            data.hexY = y;
            //Give each hex a name according to their grid coordinates and make them children of the grid master object for a cleaner heirarchy
            go.name = "Hex (" + x + "," + y + ")";
            go.transform.SetParent(transform);

            //After spawning a valid tile, check if it is touching the map boundaries, if so, delete it
            Collider[] hitColliders = Physics.OverlapSphere(go.transform.position, 0.5f, layerMask);
            if (hitColliders.Length > 0)
            {
                Destroy(go.gameObject);
            }
        }
    }

    public void GeneratePathTo(int x, int y)
    {
        //Clear out any old paths
        selectedUnit.GetComponent<UnitData>().currentPath = null;
        //Implementation of Dijkstra's pathfinding algorithm as outlined by Quill18Creates in the video series linked above, specifically episode #5 https://www.youtube.com/watch?v=QhaKb5N3Hj8&list=PLAP0hCiCP_809w1sEFHazOBwrc0DIdx43&index=69&t=133s
        //Create a set of dictionaries to keep track of the distance from point A to point B and which nodes are involved
        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();
        //Create a list of nodes we have not checked yet
        List<Node> unvisited = new List<Node>();
        //Define where we are coming from and where we want to go
        Node source = graph[selectedUnit.GetComponent<UnitData>().hexX, selectedUnit.GetComponent<UnitData>().hexY];
        Node target = graph[x, y];
        //Initialize our source node to be a distance of 0 as it is the starting point, and set the previous nodes to null as we are originating from here 
        dist[source] = 0;
        prev[source] = null;
        //Initialize everything but our source to have a distance of infinity since we dont know any better at the moment
        foreach (Node v in graph)
        {
            if(v != source)
            {
                dist[v] = Mathf.Infinity;
                prev[v] = null;
            }
            unvisited.Add(v);
        }
        //While we have nodes that are unchecked
        while(unvisited.Count > 0)
        {
            //"u" is our unvisited node with the smallest distance to our source
            //If we have not found a "u" in our unvisited nodes grab the first one
            //Otherwise if the distance of our "u" is less than the distance of our previous "u", set "u" to the node on the shorter path
            Node u = null;
            foreach(Node possibleU in unvisited)
            {
                if(u == null || dist[possibleU] < dist[u])
                {
                    u = possibleU;
                }
            }
            //If the node we grab happens to be our target node, break out of the while loop because we have our path
            if(u == target) { break; }
            unvisited.Remove(u);

            foreach(Node v in u.neighbors)
            {
                //Calculate the distance between our starting node and our target node using our custom class data
                float alt = dist[u] + u.DistanceTo(v);
                //Instead of simply using a distance calculation to decide which tile to move to, instead use the tile's movement cost to move from the source tile to the target tile
                    //float alt = dist[u] + CostToEnter(u.x, u.y, v.x, v.y);
                //If the distance between the current node and the target is shorter than the distance between any previously calculated node
                if (alt < dist[v])
                {
                    //Set this as the new distance
                    dist[v] = alt;
                    //Override the path with the shorter distance node
                    prev[v] = u;
                }
            }
        }

        //If we get here we have either found the shortest route to our target, or there is not route from our target that can backtrack to our source
        if (prev[target] == null)
        {
            //There is no route between our target and source
            return;
        }

        List<Node> currentPath = new List<Node>();
        Node currentNode = target;

        //Step through the "prev" chain and add it to our path
        while (currentNode != null)
        {
            currentPath.Add(currentNode);
            currentNode = prev[currentNode];
        }

        //This path currently describes a route from our target to our source, so we need to invert it
        currentPath.Reverse();
        //Give our unit its calculated path
        selectedUnit.GetComponent<UnitData>().currentPath = currentPath;
    }


    public Vector3 ConvertTileCoordToWorldCoord(int x, int y)
    {
        //If we are in an odd row apply not only our standard x and "y" offests but an additional half of our x offset to accomodate for the shifted row
        if (Mathf.Abs(y) % 2 == 1)
        {
            return new Vector3((transform.position.x + x * xOffset) + (xOffset / 2f), transform.position.y, transform.position.z + y * zOffset);
        }
        //If we are in an even row, just apply the standard x and "y" offsets
        else
        {
            return new Vector3(transform.position.x + x * xOffset, transform.position.y, transform.position.z + y * zOffset);
        }
    }
}
