﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Handles chunks in world space allows you to edit blocks at world points rather than at points within a chunk
public class World : MonoBehaviour
{
    public string worldName = "World";
    public static int seed = 0;
    public static World world;
    //List of all loaded chunks
    public Dictionary<WorldPos, Chunk> chunks = new Dictionary<WorldPos, Chunk>();

    public GameObject chunkPrefab;
    public LoadChunks player;
    
    private void Awake()
    {
        //seed = Random.Range(0, 10000);
        LoadWorld(worldName);
        NetworkWorldManager.world = this;
        world = this;
    }

    //Creates a new chunk at given position
    public void CreateChunk(int x, int y, int z)
    {
        WorldPos worldPos = new WorldPos(x, y, z);

        //Instantiate the chunk at the cordinates using chunk prefab
        GameObject newChunkObject = Instantiate(chunkPrefab, new Vector3(x, y, z), Quaternion.Euler(Vector3.zero)) as GameObject;
        Chunk newChunk = newChunkObject.GetComponent<Chunk>();
        newChunk.pos = worldPos;
        newChunk.world = this;
        //Adds it to chunk dictonary
        chunks.Add(worldPos, newChunk);

        //Terrain Generation
        TerrainGen terrainGen = new TerrainGen();
        newChunk = terrainGen.ChunkGen(newChunk);

        //Sets the generated blocks to unmodified and tries to load any modified blocks from the save file
        newChunk.SetBlocksUnmodified();
        Serialization.Load(newChunk);
    }
    
    public void DestroyChunk(int x, int y, int z) //Unloads chunk
    {
        Chunk chunk = null;
        if (chunks.TryGetValue(new WorldPos(x, y, z), out chunk))
        {
            Serialization.SaveChunk(chunk); //Saves chunk to file before unloading
            Object.Destroy(chunk.gameObject);
            chunks.Remove(new WorldPos(x, y, z));
        }
    }

    public Chunk GetChunk(int x, int y, int z)
    {
        WorldPos pos = new WorldPos();
        float multiple = Chunk.chunkSize;
        pos.x = Mathf.FloorToInt(x / multiple) * Chunk.chunkSize;
        pos.y = Mathf.FloorToInt(y / multiple) * Chunk.chunkSize;
        pos.z = Mathf.FloorToInt(z / multiple) * Chunk.chunkSize;
        Chunk containerChunk = null;
        chunks.TryGetValue(pos, out containerChunk);

        return containerChunk;
    }

    //Gets the block at a given world position
    public Block GetBlock(int x, int y, int z)
    {
        Chunk containerChunk = GetChunk(x, y, z);
        if (containerChunk != null)
        {
            Block block = containerChunk.GetBlock(x - containerChunk.pos.x, y - containerChunk.pos.y, z - containerChunk.pos.z);
            return block;
        }
        else
        {
            return new BlockAir();
        }
    }
    //Sets block at a given world positon
    public void SetBlock(int x, int y, int z, Block block)
    {
        Chunk chunk = GetChunk(x, y, z);

        if (chunk != null)
        {
            chunk.SetBlock(x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z, block);
            chunk.update = true;
            //Checks if we need to update bordering chunks
            UpdateIfEqual(x - chunk.pos.x, 0, new WorldPos(x - 1, y, z));
            UpdateIfEqual(x - chunk.pos.x, Chunk.chunkSize - 1, new WorldPos(x + 1, y, z));
            UpdateIfEqual(y - chunk.pos.y, 0, new WorldPos(x, y - 1, z));
            UpdateIfEqual(y - chunk.pos.y, Chunk.chunkSize - 1, new WorldPos(x, y + 1, z));
            UpdateIfEqual(z - chunk.pos.z, 0, new WorldPos(x, y, z - 1));
            UpdateIfEqual(z - chunk.pos.z, Chunk.chunkSize - 1, new WorldPos(x, y, z + 1));
        }
    }

    void UpdateIfEqual(int value1, int value2, WorldPos pos)
    {
        if (value1 == value2)
        {
            Chunk chunk = GetChunk(pos.x,pos.y,pos.z);
            if(chunk !=null)
            {
                chunk.update = true;
            }
        }
    }

    public void SaveAndQuit()
    {
        player.loadChunks = false;
        foreach (KeyValuePair<WorldPos,Chunk> entry in chunks)
        {
            DestroyChunk(entry.Value.pos.x, entry.Value.pos.y, entry.Value.pos.z);
        }
    }

    public void LoadWorld(string worldName)
    {
        WorldConfig config = WorldConfig.LoadConfig(worldName);
        if (config == null)
        {
            config = new WorldConfig();
            config.worldName = "World";
            config.seed = Random.Range(0, 10000);
            config.SaveConfig();
        }
        else
        {
            seed = config.seed;
            worldName = config.worldName;
        }
    }
}