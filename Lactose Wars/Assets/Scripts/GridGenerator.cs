using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The following code has been adapted from "Quill18creates"'s tile movement video tutorials series found here:
//https://www.youtube.com/watch?v=kYeTW2Zr8NA
//and here https://www.youtube.com/watch?v=td3O1tkbqYQ

public class GridGenerator : MonoBehaviour
{
    public int quadrantX;
    public int quadrantY;

    public TileType[] tileTypes;
    int[,] tileCoordQuad1;
    int[,] tileCoordQuad2;
    int[,] tileCoordQuad3;
    int[,] tileCoordQuad4;

    float xOffset = 0.866f;
    float zOffset = 0.75f;

    bool nextColumn = false;
    bool stop = false;
    bool positiveX = true;

    void Start()
    {
        InitMapData();
        GenerateMapTiles();
    }


    //Creata coordinates to assign to map tiles
    //Initialize map data according to specified quadrant size and tile type
    //Set every tile to the desired type of tile in the "tiletype" array
    void InitMapData()
    {
        tileCoordQuad1 = new int[quadrantX, quadrantY];
        tileCoordQuad2 = new int[quadrantX, quadrantY];
        tileCoordQuad3 = new int[quadrantX, quadrantY];
        tileCoordQuad4 = new int[quadrantX, quadrantY];

        //Assign X tiles for quadrants 1 & 2
        for (int x = 0; x < quadrantX; x++)
        {
            //Assign Y tiles for quadrant 1
            for (int y = 0; y < quadrantY; y++) {tileCoordQuad1[x, y] = 0;}
            //Assign Y tiles for quadrant 2
            for (int y = -1; y > -quadrantY; y--) {tileCoordQuad2[x, -y] = 0;}
        }

        //Assign X tiles for quadrants 3 & 4
        for (int x = -1; x > -quadrantX; x--)
        {
            //Assign Y tiles for quadrant 3
            for (int y = -1; y > -quadrantY; y--) {tileCoordQuad3[-x, -y] = 0;}
            //Assign Y tiles for quadrant 4
            for (int y = 0; y < quadrantY; y++) {tileCoordQuad4[-x, y] = 0;}
        }
    }


    //Generate map tiles according to their respective quadrants, coordinates, tile types, and prefabs
    //Increment the "quadNum" variable after generating each quadrant to prevent one quad from being generated more than once
    void GenerateMapTiles()
    {
        if (positiveX)
        {
            for (int x = 0; x < quadrantX; x++)
            {
                //Spawn the positive Y tiles
                for (int y = 0; y < quadrantY; y++)
                {
                    SpawnTiles(x, y, 0, tileTypes[tileCoordQuad1[x, y]].hexTilePrefab);

                    if (nextColumn || stop) { break; }
                }
                //Spawn the negative Y tiles
                for (int y = -1; y > -quadrantY; y--)
                {
                    SpawnTiles(x, y, -1, tileTypes[tileCoordQuad2[x, -y]].hexTilePrefab);

                    if (nextColumn || stop) { break; }
                }
                if (stop) { break; }
            }
            positiveX = false;
        }
        
        if(!positiveX)
        {
            for (int x = -1; x > -quadrantX; x--)
            {
                //Spawn the negative Y tiles
                for (int y = -1; y > -quadrantY; y--)
                {
                    SpawnTiles(x, y, -1, tileTypes[tileCoordQuad3[-x, -y]].hexTilePrefab);

                    if (nextColumn || stop) { break; }
                }
                //Spawn the positive Y tiles
                for (int y = 0; y < quadrantY; y++)
                {
                    SpawnTiles(x, y, 0, tileTypes[tileCoordQuad3[-x, y]].hexTilePrefab);

                    if (nextColumn || stop) { break; }
                }
                if (stop) { break; }
            }
        }
    }


    void SpawnTiles(int x, int y, float rowNum, GameObject tileCoord)
    {
        //Reset loop checks at the start of the method so the next call will not be inaccurate
        nextColumn = false;
        stop = false;

        //Because our tiles are not a full unit in width we need to offset their x position slightly
        float xPos = x * xOffset;

        //In an odd row all tiles only need to have their x positions shifted by half as much as the tiles in an even row
        //We have to use the absolute value of Y so our negative Y quadrants will spawn properly
        if (Mathf.Abs(y) % 2 == 1) { xPos += xOffset / 2f; }

        //Because we are importing our models from Blender they have a -90 degree X rotation applied
        //Because of this we need to use the "z" axis like we would use the "y" axis if we want "y" to continue to be our "height" dimension
        GameObject go = Instantiate(tileCoord, new Vector3(xPos, 0, y * zOffset), Quaternion.identity);
        go.name = "Hex (" + x + "," + y + ")";
        go.transform.SetParent(transform);

        //After spawning each tile, check if it is touching the map boundaries, if so, delete the tile and move to the next column of tile generation
        int layerMask = 1 << 8;
        Collider[] hitColliders = Physics.OverlapSphere(go.transform.position, 0.5f, layerMask);
        if (hitColliders.Length > 0)
        {
            Destroy(go.gameObject);
            //If the start of the row is within the map bounds, start the next loop
            //If the row is invalid, cancel the whole method
            if (y != rowNum) { nextColumn = true; }
            else { stop = true; }
        }
    }
}
