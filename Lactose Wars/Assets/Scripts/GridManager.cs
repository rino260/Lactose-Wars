using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//The following code has been adapted from "Quill18creates" tile movement video tutorials series found here:
//https://www.youtube.com/watch?v=kYeTW2Zr8NA and here https://www.youtube.com/watch?v=td3O1tkbqYQ

public class GridManager : MonoBehaviour
{
    [Range(0,100)]
    public int gridSizeX;
    [Range(0, 100)]
    public int gridSizeY;

    public TileType[] tileTypes;
    int[,] tileCoord;
    Node[,] graph;

    float xOffset = 0.866f;
    float zOffset = 0.75f;

    public GameObject hexTilePrefab;
    public GameObject selectedUnit;


    void Start()
    {
        InitMapData();
        GeneratePathfindingGraph();
        GenerateMapTiles();
    }


    void InitMapData()
    {
        //Ensure that our grid will have an odd length and width so we will have a center tile on the grid
        if (gridSizeX % 2 == 0) { gridSizeX++; }
        if (gridSizeY % 2 == 0) { gridSizeY++; }
        //Offset the grid manager according to the grid size and X & Z offsets
        transform.position = new Vector3(transform.position.x - ((gridSizeX / 2) * xOffset), transform.position.y, transform.position.y - ((gridSizeY / 2) * zOffset));
        //Populate an array of coordinates with which to assign to map tiles according to a specified grid size
        tileCoord = new int[gridSizeX, gridSizeY];
        //Assign each tile to be walkable
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            { tileCoord[x, y] = 0; }
        }
    }


    void GeneratePathfindingGraph()
    {
        //Here we need to specify the maximum number of possible "Node" arrays
        graph = new Node[gridSizeX, gridSizeY];

        //Initialize a "Node" for each spot in the array
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                //Here we need to actually create a node array that will contain the neighbors for each specific tile
                graph[x, y] = new Node();

                //We need each node to be self aware of their position
                graph[x, y].x = x;
                graph[x, y].y = y;
            }
        }

        //Now that all the nodes exist, calculate their neighbors
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
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
                    if (y < gridSizeY - 1)
                    {
                        //Add the tile to its top left to its list of neighbors (the x row that tile is in is shifted depending on wether or not we are on an even or odd row)
                        if (Mathf.Abs(y) % 2 == 1) { graph[x, y].neighbors.Add(graph[x, y + 1]); }
                        else { graph[x, y].neighbors.Add(graph[x - 1, y + 1]); }

                    }
                }
                //If the tile is not on the right edge of the grid
                if (x < gridSizeX - 1)
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
                    if (y < gridSizeY - 1)
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
    void GenerateMapTiles()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                SpawnTiles(x, y, 0, hexTilePrefab);
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
            data.xCoord = x;
            data.yCoord = y;
            //Give each hex a name according to their grid coordinates and make them children of the grid master object for a cleaner heirarchy
            go.name = "Hex (" + x + "," + y + ")";
            go.transform.SetParent(transform);

            //After spawning a valid tile, check if it is touching the map boundaries, if so, set it to occupied and then delete it
            Collider[] hitColliders = Physics.OverlapSphere(go.transform.position, 0.5f, layerMask);
            if (hitColliders.Length > 0)
            {
                ToggleHex(data.xCoord, data.yCoord);
                Destroy(go.gameObject);
            }
        }
    }


    //Toggle a specified node's tile type between occupied or not occupied
    public void ToggleHex (int targetX, int targetY)
    {
        if (tileCoord[targetX, targetY] == 0) { tileCoord[targetX, targetY] = 1; }
        else { tileCoord[targetX, targetY] = 0; }
    }


    //Check the occupied status of the target tile and adjust the cost to move into that tile
    public float CostToEnter(int targetX, int targetY)
    {
        TileType tt = tileTypes[tileCoord[targetX, targetY]];
        //By default our movement cost is 1
        float cost = 1;
        //But if the tile is occupied our movement cost will be infinate and the unit will not be able to move there
        if(CanEnterTile(targetX, targetY) == false) { return Mathf.Infinity; }
        return cost;
    }


    //Check a specified tile's occupied status and give our "GeneratePathT0" method the go ahead or not
    public bool CanEnterTile(int x, int y)
    {
        if(tileCoord[x, y] == 0) { return true; }
        else { return false; }
    }

    public void GeneratePathTo(int x, int y)
    {
        //Clear out any old paths
        selectedUnit.GetComponent<UnitData>().currentPath = null;
        //If we click on an occipied tile cancel the pathfinding
        if (!CanEnterTile(x, y)) { return;  }
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
                    //float alt = dist[u] + u.DistanceTo(v);
                //Instead of simply using a distance calculation to decide which tile to move to, instead use the tile's movement cost to move from the source tile to the target tile
                    float alt = dist[u] + CostToEnter(v.x, v.y);
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
