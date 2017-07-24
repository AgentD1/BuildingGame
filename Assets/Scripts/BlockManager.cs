using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockManager : MonoBehaviour {

    public int width = 10;
    public int height = 10;
    public GameObject blockPrefab;
    public GameObject chunkPrefab;
    Dictionary<Vector3, Block> blocks;
    public const int CHUNKSIZE = 15;
    public GameObject player;
    Dictionary<Vector2,Chunk> chunks;
    List<Chunk> previouslyRenderedChunks;
    int chunksVisibleInRenderDistance;
    public int blockCount = 0;
    public float renderDistance = 64;
    Vector2 previousPlayerChunkCoords;
    public static BlockType[] blockTypes = { new BlockType(Color.white), new BlockType(Color.red), new BlockType(Color.yellow), new BlockType(Color.cyan), new BlockType(Color.black), new BlockType(Color.blue) };
    public float scale = 0.025f;
    public float heightMultiplier = 15f;
    public Vector2 offset;
    public int blockBottom;

    void Start () {
        chunksVisibleInRenderDistance = Mathf.RoundToInt(renderDistance / CHUNKSIZE);
        chunks = new Dictionary<Vector2, Chunk>();
        previouslyRenderedChunks = new List<Chunk>();
        blocks = new Dictionary<Vector3, Block>();
        previousPlayerChunkCoords = new Vector2(1234,1234);
        offset = new Vector2(Random.Range(-100000, 100000), Random.Range(-100000, 100000));

    }
	
    public void BlockClick(bool breaking, RaycastHit hit, int id) {
        if (breaking) {
            DestroyBlock(hit.transform.position);
        } else {
            int clickChunkCoordX = Mathf.CeilToInt(player.transform.position.x / CHUNKSIZE);
            int clickChunkCoordY = Mathf.CeilToInt(player.transform.position.z / CHUNKSIZE);
            CreateBlock(hit.transform.position + hit.normal);
            blocks[hit.transform.position + hit.normal].gameObject.GetComponent<MeshRenderer>().material.color = blockTypes[id].color;
        }
        
    }
    
    Chunk GenerateChunk(Vector2 position) {
        
        CreateChunk(position);
        for (int x = -CHUNKSIZE / 2; x < CHUNKSIZE / 2 + 1; x++) { 
            for (int y = -CHUNKSIZE / 2; y < CHUNKSIZE / 2 + 1; y++) {
                //CreateBlock(new Vector3(x + (position.x * CHUNKSIZE), 0, y + (position.y * CHUNKSIZE)));
                for (int i = blockBottom; i < TerrainGenerator.GenerateHeightForBlock(new Vector2(x + (position.x * CHUNKSIZE), y + (position.y * CHUNKSIZE)) + offset, scale, heightMultiplier); i++) {
                    CreateBlock(new Vector3(x + (position.x * CHUNKSIZE), i, y + (position.y * CHUNKSIZE)));
                }
                
            }
        }
        return chunks[position];
    }

    Chunk CreateChunk(Vector2 position) {
        if (chunks.ContainsKey(position)) {
            Debug.Log("Chunk already exists at " + position);
            return chunks[position];
        } else {
            GameObject go = Instantiate(chunkPrefab, new Vector3(position.x * CHUNKSIZE, 0, position.y * CHUNKSIZE), Quaternion.identity, transform);
            Chunk chunk = new Chunk(position, go);
            chunks.Add(position, chunk);
            return chunk;
        }
    }

    Block CreateBlock(Vector3 position) {
        if (chunks.ContainsKey(new Vector2(Mathf.Round(position.x / CHUNKSIZE), Mathf.Round(position.z / CHUNKSIZE)))) {
            GameObject go = Instantiate(blockPrefab, position, Quaternion.identity, chunks[new Vector2(Mathf.Round(position.x / CHUNKSIZE), Mathf.Round(position.z / CHUNKSIZE))].gameObject.transform);
            Block block = new Block(position, go);
            blocks.Add(position, block);
            blockCount++;
            return block;
        } else if (blocks.ContainsKey(position)) {
            Debug.LogError("Block already exists at " + position);
            return blocks[position];
        } else {
            Debug.LogError("No chunk at position " + new Vector2(Mathf.Round(position.x / CHUNKSIZE), Mathf.Round(position.z / CHUNKSIZE)));
            return null;
        }
    }

    bool DestroyBlock(Vector3 position) {
        if (blocks.ContainsKey(position)) {
            Destroy(blocks[position].gameObject, 0.0001f);
            blocks.Remove(position);
            blockCount--;
            return true;
        } else {
            Debug.Log("Block doesn't exist! " + position);
            return false;
        }
    }

    void UpdateVisibleChunks() {
        Vector2 playerCoords = new Vector2(Mathf.Round(player.transform.position.x / CHUNKSIZE), Mathf.Round(player.transform.position.z / CHUNKSIZE));
        if (playerCoords == previousPlayerChunkCoords) {
            return;
        } else {
            Debug.Log("Chunk was entered by player: " + playerCoords);
        }
        foreach (Chunk chunk in previouslyRenderedChunks) {
            chunk.SetVisible(false);
        }
        previouslyRenderedChunks.Clear();

        for (int x = -chunksVisibleInRenderDistance; x < chunksVisibleInRenderDistance; x++) {
            for (int y = -chunksVisibleInRenderDistance; y < chunksVisibleInRenderDistance; y++) {
                Vector2 currentChunkCoord = new Vector2(playerCoords.x + x, playerCoords.y + y);
                if (chunks.ContainsKey(currentChunkCoord)) {
                    chunks[currentChunkCoord].UpdateChunkVisibility(player.transform.position, renderDistance);
                    if (chunks[currentChunkCoord].gameObject.activeSelf) {
                        previouslyRenderedChunks.Add(chunks[currentChunkCoord]);
                    }
                } else {
                    previouslyRenderedChunks.Add(GenerateChunk(currentChunkCoord));
                }
            }
        }
        previousPlayerChunkCoords = playerCoords;
    }
    
    void Update () {
        UpdateVisibleChunks();
	}
}

public class Block {
    public GameObject gameObject;

    public Vector3 pos;
    
    public Block(Vector3 position, GameObject go) {
        pos = position;
        gameObject = go;
    }
    public Block(int xPos, int yPos, int zPos, GameObject go) {
        pos = new Vector3(xPos, yPos, zPos);
        gameObject = go;
    }
    public Block(Vector3 position) {
        pos = position;
    }
    public Block(int xPos, int yPos, int zPos) {
        pos = new Vector3(xPos, yPos, zPos);
    }
}

public class Chunk {
    public GameObject gameObject;
    public Vector2 pos;
    Bounds bounds;

    public Chunk(Vector2 position, GameObject go) {
        pos = position;
        gameObject = go;
        bounds = new Bounds(new Vector3(pos.x * BlockManager.CHUNKSIZE, 0, pos.y * BlockManager.CHUNKSIZE), new Vector3(BlockManager.CHUNKSIZE, 500, BlockManager.CHUNKSIZE));
        SetVisible(true);
    }

    public void UpdateChunkVisibility(Vector3 playerPos, float renderDistance) {
        float viewerClosestDist = Mathf.Sqrt(bounds.SqrDistance(playerPos));
        bool visible = viewerClosestDist <= renderDistance;
        SetVisible(visible);
    }

    public void SetVisible(bool isVisible) {
        gameObject.SetActive(isVisible);
    }
    
}

public class BlockType {
    public Color color;
    public string name;
    public BlockType(Color c) {
        color = c;
        name = color.ToString();
    }
}
