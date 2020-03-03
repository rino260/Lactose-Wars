using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

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
        this.transform.position = new Vector3(this.transform.position.x - ((gridX / 2) * xOffset), this.transform.position.y, this.transform.position.y - ((gridY / 2) * zOffset));

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
    
    //Custom class to keep track of our graph nodes and their neighbors
    //Here we are essentially creating a list of two dimensional arrays
    class Node
    {
        public List<Node> neighbors;
        
        //Default constructor that initializes our list of nodes
        public Node()
        {
            neighbors = new List<Node>();
        }
    }

    //Here we define the type of array we will pass into our list above
    Node[,] graph;

    void GeneratePathfindingGraph()
    {
        //Here we need to specify the maximum number of possible "Node" arrays
        graph = new Node[gridX, gridY];

        //Initialize positive X nodes
        for (int x = 0; x < gridX; x++)
        {
            //Initialize positive Y nodes
            for (int y = 0; y < gridY; y++)
            {
                //Here we need to actually create a node array that will contain the neighbors for each specific tile
                graph[x, y] = new Node();

                if (x > 0)
                {
                    graph[x, y].neighbors.Add(graph[x - 1, y]);

                    if (y > 0)
                    {
                        if (Mathf.Abs(y) % 2 == 1) { graph[x, y].neighbors.Add(graph[x, y - 1]); }
                        else { graph[x, y].neighbors.Add(graph[x - 1, y - 1]); }
                    }
                    if (y < gridY - 1)
                    {
                        if (Mathf.Abs(y) % 2 == 1) { graph[x, y].neighbors.Add(graph[x, y + 1]); }
                        else { graph[x, y].neighbors.Add(graph[x - 1, y + 1]); }

                    }
                }
                if (x < gridX - 1)
                {
                    graph[x, y].neighbors.Add(graph[x + 1, y]);

                    if (y > 0)
                    {
                        if (Mathf.Abs(y) % 2 == 1) { graph[x, y].neighbors.Add(graph[x, y - 1]); }
                        else { graph[x, y].neighbors.Add(graph[x + 1, y - 1]); }
                    }
                    if (y < gridY - 1)
                    {
                        if (Mathf.Abs(y) % 2 == 1) { graph[x, y].neighbors.Add(graph[x, y + 1]); }
                        else { graph[x, y].neighbors.Add(graph[x + 1, y + 1]); }

                    }
                } 
            }
        }
    }

    
    void AddNeighbors(int x, int y)
    {
        //As long as our x coordinate is greater than the minimum value of our negative x quadrant size:
        if (x > -gridX + 1)
        {
            //We can add a neighbor at our next x coordinate to our left
            graph[x, y].neighbors.Add(graph[x - 1, y]);

            //As long as our y coordinate is greater than the minimum value of our negative y quadrant size:
            if (y > -gridY + 1)
            {
                //If we are on an odd row we can add a neighbor one y value below us on the current x coordinate (because the odd rows have their x values shifted and not the evens)
                if (Mathf.Abs(y) % 2 == 1) { graph[x, y].neighbors.Add(graph[x, y - 1]); }
                //Otherwise our neighbor beneath us is on the previous x column
                else { graph[x, y].neighbors.Add(graph[x - 1, y - 1]); }
            }
            //As long as our y coordinate is less than the maximum value of our negative y quadrant size:
            if (y < gridY - 1)
            {
                //If we are on an odd row we can add a neighbor one y value above us on the current x coordinate
                if (Mathf.Abs(y) % 2 == 1) { graph[x, y].neighbors.Add(graph[x, y + 1]); }
                //Otherwise our neighbor above us is on the previous x column
                else { graph[x, y].neighbors.Add(graph[x - 1, y + 1]); }
                
            }
        }
        //As long as our x coordinate is less than the minimum value of our negative x quadrant size:
        if (x < gridX - 1)
        {
            //We can add a neighbor at our next x coordinate to our right
            graph[x, y].neighbors.Add(graph[x + 1, y]);

            //As long as our y coordinate is greater than the minimum value of our negative y quadrant size:
            if (y > -gridY + 1)
            {
                //If we are on an odd row we can add a neighbor one y value below us on the current x coordinate
                if (Mathf.Abs(y) % 2 == 1) { graph[x, y].neighbors.Add(graph[x, y - 1]); }
                //Otherwise our neighbor beneath us is on the next x column
                else { graph[x, y].neighbors.Add(graph[x + 1, y - 1]); }
            }
            //As long as our y coordinate is less than the maximum value of our negative y quadrant size:
            if (y < gridY - 1)
            {
                //If we are on an odd row we can add a neighbor one y value above us on the current x coordinate
                if (Mathf.Abs(y) % 2 == 1) { graph[x, y].neighbors.Add(graph[x, y + 1]); }
                //Otherwise our neighbor above us is on the previous x column
                else { graph[x, y].neighbors.Add(graph[x + 1, y + 1]); }
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

    public void MoveSelectedUnit(int x, int y)
    {
        selectedUnit.GetComponent<UnitData>().hexX = x;
        selectedUnit.GetComponent<UnitData>().hexY = y;
        selectedUnit.transform.position = ConvertTileCoordToWorldCoord(x, y);
    }

    public Vector3 ConvertTileCoordToWorldCoord(int x, int y)
    {




        //TODO: Implement some code here that uses math to convert a tile's coordinates to into a world coordinate and then pass that through the "return new" below





        //Our Z position is 0 on account of our rotational issue associated with importing assets from Blender
        return new Vector3(x, 0, y);
    }
}
