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
    public static BlockType[] blockTypes = { new BlockType(0, 0), new BlockType(1, 0), new BlockType(2, 0), new BlockType(3, 0), new BlockType(0, 1), new BlockType(1, 1), new BlockType(2, 1), new BlockType(3, 1), new BlockType(0, 2), new BlockType(1, 2), new BlockType(2, 2), new BlockType(3, 2), new BlockType(0, 3), new BlockType(1, 3), new BlockType(2, 3), new BlockType(3, 3), };
    public float scale = 0.025f;
    public float heightMultiplier = 15f;
    public Vector2 offset;
    public int blockBottom;
    public Mesh blockMesh;
    public Material blockMat;
    public Texture2D blockMap;
    public int pixelSizeForBlockMap;

    void Start () {
        chunksVisibleInRenderDistance = Mathf.RoundToInt(renderDistance / CHUNKSIZE);
        chunks = new Dictionary<Vector2, Chunk>();
        previouslyRenderedChunks = new List<Chunk>();
        blocks = new Dictionary<Vector3, Block>();
        previousPlayerChunkCoords = new Vector2(1234,1234);
        offset = new Vector2(Random.Range(-100000, 100000), Random.Range(-100000, 100000));
        Debug.Log("Started");
    }
	
    public void BlockClick(bool breaking, RaycastHit hit, int id) {
        if (breaking) {
            Vector3 unRounded = hit.point - hit.normal * 0.5f;
            Vector3 blockPos = new Vector3 (Mathf.Round(unRounded.x), Mathf.Round(unRounded.y), Mathf.Round(unRounded.z));
            DestroyBlock(blockPos);
            UpdateChunkMesh(new Vector2(Mathf.Round(blockPos.x / CHUNKSIZE), Mathf.Round(blockPos.z / CHUNKSIZE)));
        } else {
            Vector3 unRounded = hit.point + hit.normal * 0.25f;
            Vector3 blockPos = new Vector3(Mathf.Round(unRounded.x), Mathf.Round(unRounded.y), Mathf.Round(unRounded.z));
            CreateBlock(blockPos);
            blocks[blockPos].id = id;
            UpdateChunkMesh(new Vector2(Mathf.Round(blockPos.x / CHUNKSIZE), Mathf.Round(blockPos.z / CHUNKSIZE)));
        }
        
    }
    
    Chunk GenerateChunk(Vector2 position) {
        CreateChunk(position);
        for (int x = -CHUNKSIZE / 2; x < CHUNKSIZE / 2 + 1; x++) { 
            for (int y = -CHUNKSIZE / 2; y < CHUNKSIZE / 2 + 1; y++) {
                
                for (int i = blockBottom; i < TerrainGenerator.GenerateHeightForBlock(new Vector2(x + (position.x * CHUNKSIZE), y + (position.y * CHUNKSIZE)) + offset, scale, heightMultiplier); i++) {
                    CreateBlock(new Vector3(x + (position.x * CHUNKSIZE), i, y + (position.y * CHUNKSIZE)));
                }
                //CreateBlock(new Vector3(x + (position.x * CHUNKSIZE), TerrainGenerator.GenerateHeightForBlock(new Vector2(x + (position.x * CHUNKSIZE), y + (position.y * CHUNKSIZE)) + offset, scale, heightMultiplier), y + (position.y * CHUNKSIZE)));
            }
        }
        UpdateChunkMesh(position);
        return chunks[position];
    }

    Chunk CreateChunk(Vector2 position) {
        if (chunks.ContainsKey(position)) {
            Debug.Log("Chunk already exists at " + position);
            return chunks[position];
        } else {
            GameObject go = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity, transform);
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            go.AddComponent<MeshCollider>();
            go.layer = 8;
            Chunk chunk = new Chunk(position, go);
            chunks.Add(position, chunk);
            return chunk;
        }
    }

    Block CreateBlock(Vector3 position) {
        if (chunks.ContainsKey(new Vector2(Mathf.Round(position.x / CHUNKSIZE), Mathf.Round(position.z / CHUNKSIZE))) && !blocks.ContainsKey(position)) {
            Block block = new Block(position);
            chunks[new Vector2(Mathf.Round(position.x / CHUNKSIZE), Mathf.Round(position.z / CHUNKSIZE))].blocksInChunk.Add(block);
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
            chunks[new Vector2(Mathf.Round(position.x / CHUNKSIZE), Mathf.Round(position.z / CHUNKSIZE))].blocksInChunk.Remove(blocks[position]);
            blocks.Remove(position);
            blockCount--;
            UpdateChunkMesh(new Vector2(Mathf.Round(position.x / CHUNKSIZE), Mathf.Round(position.z / CHUNKSIZE)));
            return true;
        } else {
            Debug.Log("Block doesn't exist! " + position);
            return false;
        }
    }

    void UpdateChunkMesh(Vector2 chunk) {
        Mesh mesh = new Mesh();
        List<Vector3> verticies = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        for (int i = 0; i < chunks[chunk].blocksInChunk.Count; i++) {
            //TODO fix neighboring chunks not updating on block destroy, resulting in transparent face

            if (!blocks.ContainsKey(chunks[chunk].blocksInChunk[i].pos + Vector3.up)) {
                triangles.Add(verticies.Count + 0);
                triangles.Add(verticies.Count + 1);
                triangles.Add(verticies.Count + 2);
                triangles.Add(verticies.Count + 2);
                triangles.Add(verticies.Count + 1);
                triangles.Add(verticies.Count + 3);
                verticies.Add(new Vector3(chunks[chunk].blocksInChunk[i].pos.x - 0.5f, chunks[chunk].blocksInChunk[i].pos.y + 0.5f, chunks[chunk].blocksInChunk[i].pos.z - 0.5f));
                verticies.Add(new Vector3(chunks[chunk].blocksInChunk[i].pos.x - 0.5f, chunks[chunk].blocksInChunk[i].pos.y + 0.5f, chunks[chunk].blocksInChunk[i].pos.z + 0.5f));
                verticies.Add(new Vector3(chunks[chunk].blocksInChunk[i].pos.x + 0.5f, chunks[chunk].blocksInChunk[i].pos.y + 0.5f, chunks[chunk].blocksInChunk[i].pos.z - 0.5f));
                verticies.Add(new Vector3(chunks[chunk].blocksInChunk[i].pos.x + 0.5f, chunks[chunk].blocksInChunk[i].pos.y + 0.5f, chunks[chunk].blocksInChunk[i].pos.z + 0.5f));
                uvs.Add(new Vector2(blockTypes[chunks[chunk].blocksInChunk[i].id].texX, blockTypes[chunks[chunk].blocksInChunk[i].id].texY));
                uvs.Add(new Vector2(blockTypes[chunks[chunk].blocksInChunk[i].id].texX, blockTypes[chunks[chunk].blocksInChunk[i].id].texY + 0.0625f));
                uvs.Add(new Vector2(blockTypes[chunks[chunk].blocksInChunk[i].id].texX + 0.0625f, blockTypes[chunks[chunk].blocksInChunk[i].id].texY));
                uvs.Add(new Vector2(blockTypes[chunks[chunk].blocksInChunk[i].id].texX + 0.0625f, blockTypes[chunks[chunk].blocksInChunk[i].id].texY + 0.0625f));

            }

            if (!blocks.ContainsKey(chunks[chunk].blocksInChunk[i].pos + Vector3.down)) {
                triangles.Add(verticies.Count + 0);
                triangles.Add(verticies.Count + 2);
                triangles.Add(verticies.Count + 1);
                triangles.Add(verticies.Count + 1);
                triangles.Add(verticies.Count + 2);
                triangles.Add(verticies.Count + 3);
                verticies.Add(new Vector3(chunks[chunk].blocksInChunk[i].pos.x - 0.5f, chunks[chunk].blocksInChunk[i].pos.y - 0.5f, chunks[chunk].blocksInChunk[i].pos.z - 0.5f));
                verticies.Add(new Vector3(chunks[chunk].blocksInChunk[i].pos.x - 0.5f, chunks[chunk].blocksInChunk[i].pos.y - 0.5f, chunks[chunk].blocksInChunk[i].pos.z + 0.5f));
                verticies.Add(new Vector3(chunks[chunk].blocksInChunk[i].pos.x + 0.5f, chunks[chunk].blocksInChunk[i].pos.y - 0.5f, chunks[chunk].blocksInChunk[i].pos.z - 0.5f));
                verticies.Add(new Vector3(chunks[chunk].blocksInChunk[i].pos.x + 0.5f, chunks[chunk].blocksInChunk[i].pos.y - 0.5f, chunks[chunk].blocksInChunk[i].pos.z + 0.5f));
                uvs.Add(new Vector2(blockTypes[chunks[chunk].blocksInChunk[i].id].texX, blockTypes[chunks[chunk].blocksInChunk[i].id].texY));
                uvs.Add(new Vector2(blockTypes[chunks[chunk].blocksInChunk[i].id].texX, blockTypes[chunks[chunk].blocksInChunk[i].id].texY + 0.0625f));
                uvs.Add(new Vector2(blockTypes[chunks[chunk].blocksInChunk[i].id].texX + 0.0625f, blockTypes[chunks[chunk].blocksInChunk[i].id].texY));
                uvs.Add(new Vector2(blockTypes[chunks[chunk].blocksInChunk[i].id].texX + 0.0625f, blockTypes[chunks[chunk].blocksInChunk[i].id].texY + 0.0625f));

            }


            if (!blocks.ContainsKey(chunks[chunk].blocksInChunk[i].pos + Vector3.back)) {
                triangles.Add(verticies.Count + 0);
                triangles.Add(verticies.Count + 1);
                triangles.Add(verticies.Count + 2);
                triangles.Add(verticies.Count + 2);
                triangles.Add(verticies.Count + 1);
                triangles.Add(verticies.Count + 3);
                verticies.Add(new Vector3(chunks[chunk].blocksInChunk[i].pos.x - 0.5f, chunks[chunk].blocksInChunk[i].pos.y - 0.5f, chunks[chunk].blocksInChunk[i].pos.z - 0.5f));
                verticies.Add(new Vector3(chunks[chunk].blocksInChunk[i].pos.x - 0.5f, chunks[chunk].blocksInChunk[i].pos.y + 0.5f, chunks[chunk].blocksInChunk[i].pos.z - 0.5f));
                verticies.Add(new Vector3(chunks[chunk].blocksInChunk[i].pos.x + 0.5f, chunks[chunk].blocksInChunk[i].pos.y - 0.5f, chunks[chunk].blocksInChunk[i].pos.z - 0.5f));
                verticies.Add(new Vector3(chunks[chunk].blocksInChunk[i].pos.x + 0.5f, chunks[chunk].blocksInChunk[i].pos.y + 0.5f, chunks[chunk].blocksInChunk[i].pos.z - 0.5f));
                uvs.Add(new Vector2(blockTypes[chunks[chunk].blocksInChunk[i].id].texX, blockTypes[chunks[chunk].blocksInChunk[i].id].texY));
                uvs.Add(new Vector2(blockTypes[chunks[chunk].blocksInChunk[i].id].texX, blockTypes[chunks[chunk].blocksInChunk[i].id].texY + 0.0625f));
                uvs.Add(new Vector2(blockTypes[chunks[chunk].blocksInChunk[i].id].texX + 0.0625f, blockTypes[chunks[chunk].blocksInChunk[i].id].texY));
                uvs.Add(new Vector2(blockTypes[chunks[chunk].blocksInChunk[i].id].texX + 0.0625f, blockTypes[chunks[chunk].blocksInChunk[i].id].texY + 0.0625f));

            }


            if (!blocks.ContainsKey(chunks[chunk].blocksInChunk[i].pos + Vector3.forward)) {
                triangles.Add(verticies.Count + 0);
                triangles.Add(verticies.Count + 2);
                triangles.Add(verticies.Count + 1);
                triangles.Add(verticies.Count + 1);
                triangles.Add(verticies.Count + 2);
                triangles.Add(verticies.Count + 3);
                verticies.Add(new Vector3(chunks[chunk].blocksInChunk[i].pos.x + 0.5f, chunks[chunk].blocksInChunk[i].pos.y + 0.5f, chunks[chunk].blocksInChunk[i].pos.z + 0.5f));
                verticies.Add(new Vector3(chunks[chunk].blocksInChunk[i].pos.x + 0.5f, chunks[chunk].blocksInChunk[i].pos.y - 0.5f, chunks[chunk].blocksInChunk[i].pos.z + 0.5f));
                verticies.Add(new Vector3(chunks[chunk].blocksInChunk[i].pos.x - 0.5f, chunks[chunk].blocksInChunk[i].pos.y + 0.5f, chunks[chunk].blocksInChunk[i].pos.z + 0.5f));
                verticies.Add(new Vector3(chunks[chunk].blocksInChunk[i].pos.x - 0.5f, chunks[chunk].blocksInChunk[i].pos.y - 0.5f, chunks[chunk].blocksInChunk[i].pos.z + 0.5f));
                uvs.Add(new Vector2(blockTypes[chunks[chunk].blocksInChunk[i].id].texX, blockTypes[chunks[chunk].blocksInChunk[i].id].texY));
                uvs.Add(new Vector2(blockTypes[chunks[chunk].blocksInChunk[i].id].texX, blockTypes[chunks[chunk].blocksInChunk[i].id].texY + 0.0625f));
                uvs.Add(new Vector2(blockTypes[chunks[chunk].blocksInChunk[i].id].texX + 0.0625f, blockTypes[chunks[chunk].blocksInChunk[i].id].texY));
                uvs.Add(new Vector2(blockTypes[chunks[chunk].blocksInChunk[i].id].texX + 0.0625f, blockTypes[chunks[chunk].blocksInChunk[i].id].texY + 0.0625f));

            }


            if (!blocks.ContainsKey(chunks[chunk].blocksInChunk[i].pos + Vector3.left)) {
                triangles.Add(verticies.Count + 0);
                triangles.Add(verticies.Count + 1);
                triangles.Add(verticies.Count + 2);
                triangles.Add(verticies.Count + 2);
                triangles.Add(verticies.Count + 1);
                triangles.Add(verticies.Count + 3);
                verticies.Add(new Vector3(chunks[chunk].blocksInChunk[i].pos.x - 0.5f, chunks[chunk].blocksInChunk[i].pos.y - 0.5f, chunks[chunk].blocksInChunk[i].pos.z - 0.5f));
                verticies.Add(new Vector3(chunks[chunk].blocksInChunk[i].pos.x - 0.5f, chunks[chunk].blocksInChunk[i].pos.y - 0.5f, chunks[chunk].blocksInChunk[i].pos.z + 0.5f));
                verticies.Add(new Vector3(chunks[chunk].blocksInChunk[i].pos.x - 0.5f, chunks[chunk].blocksInChunk[i].pos.y + 0.5f, chunks[chunk].blocksInChunk[i].pos.z - 0.5f));
                verticies.Add(new Vector3(chunks[chunk].blocksInChunk[i].pos.x - 0.5f, chunks[chunk].blocksInChunk[i].pos.y + 0.5f, chunks[chunk].blocksInChunk[i].pos.z + 0.5f));
                uvs.Add(new Vector2(blockTypes[chunks[chunk].blocksInChunk[i].id].texX, blockTypes[chunks[chunk].blocksInChunk[i].id].texY));
                uvs.Add(new Vector2(blockTypes[chunks[chunk].blocksInChunk[i].id].texX, blockTypes[chunks[chunk].blocksInChunk[i].id].texY + 0.0625f));
                uvs.Add(new Vector2(blockTypes[chunks[chunk].blocksInChunk[i].id].texX + 0.0625f, blockTypes[chunks[chunk].blocksInChunk[i].id].texY));
                uvs.Add(new Vector2(blockTypes[chunks[chunk].blocksInChunk[i].id].texX + 0.0625f, blockTypes[chunks[chunk].blocksInChunk[i].id].texY + 0.0625f));

            }


            if (!blocks.ContainsKey(chunks[chunk].blocksInChunk[i].pos + Vector3.right)) {
                triangles.Add(verticies.Count + 0);
                triangles.Add(verticies.Count + 2);
                triangles.Add(verticies.Count + 1);
                triangles.Add(verticies.Count + 1);
                triangles.Add(verticies.Count + 2);
                triangles.Add(verticies.Count + 3);
                verticies.Add(new Vector3(chunks[chunk].blocksInChunk[i].pos.x + 0.5f, chunks[chunk].blocksInChunk[i].pos.y - 0.5f, chunks[chunk].blocksInChunk[i].pos.z + 0.5f));
                verticies.Add(new Vector3(chunks[chunk].blocksInChunk[i].pos.x + 0.5f, chunks[chunk].blocksInChunk[i].pos.y + 0.5f, chunks[chunk].blocksInChunk[i].pos.z + 0.5f));
                verticies.Add(new Vector3(chunks[chunk].blocksInChunk[i].pos.x + 0.5f, chunks[chunk].blocksInChunk[i].pos.y - 0.5f, chunks[chunk].blocksInChunk[i].pos.z - 0.5f));
                verticies.Add(new Vector3(chunks[chunk].blocksInChunk[i].pos.x + 0.5f, chunks[chunk].blocksInChunk[i].pos.y + 0.5f, chunks[chunk].blocksInChunk[i].pos.z - 0.5f));
                uvs.Add(new Vector2(blockTypes[chunks[chunk].blocksInChunk[i].id].texX, blockTypes[chunks[chunk].blocksInChunk[i].id].texY));
                uvs.Add(new Vector2(blockTypes[chunks[chunk].blocksInChunk[i].id].texX, blockTypes[chunks[chunk].blocksInChunk[i].id].texY + 0.0625f));
                uvs.Add(new Vector2(blockTypes[chunks[chunk].blocksInChunk[i].id].texX + 0.0625f, blockTypes[chunks[chunk].blocksInChunk[i].id].texY));
                uvs.Add(new Vector2(blockTypes[chunks[chunk].blocksInChunk[i].id].texX + 0.0625f, blockTypes[chunks[chunk].blocksInChunk[i].id].texY + 0.0625f));

            }


        }
        mesh.SetVertices(verticies);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.SetUVs(0, uvs);

        chunks[chunk].gameObject.GetComponent<MeshFilter>().mesh = mesh;
        chunks[chunk].gameObject.GetComponent<MeshRenderer>().material = blockMat;
        chunks[chunk].gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
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
    public Vector3 pos;
    public int id;

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
    public List<Block> blocksInChunk;

    public Chunk(Vector2 position, GameObject go) {
        pos = position;
        gameObject = go;
        bounds = new Bounds(new Vector3(pos.x * BlockManager.CHUNKSIZE, 0, pos.y * BlockManager.CHUNKSIZE), new Vector3(BlockManager.CHUNKSIZE, 500, BlockManager.CHUNKSIZE));
        SetVisible(true);
        blocksInChunk = new List<Block>();
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
    public float texX;
    public float texY;
    public string name;
    public BlockType(float x, float y) {
        texX = x * 0.0625f;
        texY = y * 0.0625f;
        name = x + " " + y;
    }
}
