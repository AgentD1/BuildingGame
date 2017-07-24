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

    // Use this for initialization
    void Start () {
        chunksVisibleInRenderDistance = Mathf.RoundToInt(renderDistance / CHUNKSIZE);
        chunks = new Dictionary<Vector2, Chunk>();
        previouslyRenderedChunks = new List<Chunk>();
        blocks = new Dictionary<Vector3, Block>();
        for (int i = -3; i < 4; i++) {
            for (int j = -3; j < 4; j++) {
                GenerateChunk(new Vector2(i, j));
            }
        }
	}
	
    public void BlockClick(bool breaking,RaycastHit hit) {
        if (breaking) {
//            DestroyBlock(hit.transform.position);
        } else {
            int clickChunkCoordX = Mathf.CeilToInt(player.transform.position.x / CHUNKSIZE);
            int clickChunkCoordY = Mathf.CeilToInt(player.transform.position.z / CHUNKSIZE);
            CreateBlock(hit.transform.position + hit.normal);
        }
        
    }
    
    Chunk GenerateChunk(Vector2 position) {
        CreateChunk(position);
        for (int x = -CHUNKSIZE / 2; x < CHUNKSIZE / 2 + 1; x++) { 
            for (int y = -CHUNKSIZE / 2; y < CHUNKSIZE / 2 + 1; y++) {
                CreateBlock(new Vector3(x + (position.x * CHUNKSIZE), 0, y + (position.y * CHUNKSIZE)));
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
            return block;
        } else if (blocks.ContainsKey(position)) {
            Debug.LogError("Block already exists at " + position);
            return blocks[position];
        } else {
            Debug.LogError("No chunk at position " + new Vector2(Mathf.Round(position.x / CHUNKSIZE), Mathf.Round(position.z / CHUNKSIZE)));
            return null;
        }
    }

    //bool DestroyBlock(Vector3 position) {

    //}


    /*
    public void CreateChunk(Vector3 position) {

        GameObject go = Instantiate(chunkPrefab, new Vector3(position.x*CHUNKSIZE,0,position.z*CHUNKSIZE), Quaternion.identity, transform);
        
        chunks.Add(new Vector2(position.x, position.z), new Chunk(position, go));
        Debug.Log(chunks[new Vector2(position.x, position.z)]);
    }

    public void CreateBlock(Vector3 position, GameObject chunk) {
        GameObject go = Instantiate(blockPrefab, position, Quaternion.identity, chunk.transform);
        blocks.Add(position, new Block(position,go));
        blockCount++;
        
    }

    public void DestroyBlock(Vector3 position) {
        Destroy(blocks[position].gameObject, 0.001f);
        blockCount--;
        blocks.Remove(position);
    }

    void UpdateVisibleChunks() {

        for (int i = 0; i < previouslyRenderedChunks.Count; i++) {
            previouslyRenderedChunks[i].SetVisible(false);
        }
        previouslyRenderedChunks.Clear();

        int currentChunkCoordX = Mathf.CeilToInt(player.transform.position.x / CHUNKSIZE);
        int currentChunkCoordY = Mathf.CeilToInt(player.transform.position.z / CHUNKSIZE);

        for (int yOffset = -chunksVisibleInRenderDistance; yOffset <= chunksVisibleInRenderDistance; yOffset++) {
            for (int xOffset = -chunksVisibleInRenderDistance; xOffset <= chunksVisibleInRenderDistance; xOffset++) {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                Debug.Log(chunks.Count);
                if (chunks.ContainsKey(viewedChunkCoord)) {
                    chunks[viewedChunkCoord].UpdateChunkVisibility(player.transform.position,renderDistance);
                    if (chunks[viewedChunkCoord].gameObject.activeSelf) {
                        previouslyRenderedChunks.Add(chunks[viewedChunkCoord]);
                    }
                } else {
                    GenerateChunk(new Vector3(viewedChunkCoord.x, 0, viewedChunkCoord.y));
                }

            }
        }
    }
    void GenerateChunk(Vector3 position) {
        if (chunks.ContainsKey(new Vector2(position.x, position.z))) { return; }
        CreateChunk(new Vector2(position.x, position.z));

        for (int i = (int) position.x*CHUNKSIZE; i < position.x*CHUNKSIZE + CHUNKSIZE; i++) {
            for (int j = (int)position.y * CHUNKSIZE; j < position.y * CHUNKSIZE + CHUNKSIZE; j++) {
                CreateBlock(new Vector3(i, 0, j), chunks[new Vector2(position.x, position.z)].gameObject);
            }
        }

    }

    */

    // Update is called once per frame
    void Update () {
        //UpdateVisibleChunks();
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
        SetVisible(false);
    }

    public void UpdateChunkVisibility(Vector3 playerPos, float renderDistance) {
        float viewerClosestDist = Mathf.Sqrt(bounds.SqrDistance(playerPos));
        bool visible = viewerClosestDist >= renderDistance;
        SetVisible(visible);
    }

    public void SetVisible(bool isVisible) {
        gameObject.SetActive(isVisible);
    }

}