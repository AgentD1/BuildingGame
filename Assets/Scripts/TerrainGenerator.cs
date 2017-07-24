using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TerrainGenerator {

    static System.Random prng;

    public static int GenerateHeightForBlock(Vector2 position, float scale, float heightMultiplier) {
        return Mathf.RoundToInt(Mathf.PerlinNoise(position.x * scale, position.y * scale) * heightMultiplier);
    }

}
