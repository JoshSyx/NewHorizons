using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TreeSpawnerByTexture : MonoBehaviour
{
    public Terrain terrain;
    public int treePrototypeIndex = 0; // Index of the tree in the Terrain's tree prototypes
    public int textureLayerIndex = 0;  // Index of the texture layer to check
    public int treeCount = 100;
    public float requiredWeight = 0.5f;

    void Start()
    {
        TerrainData tData = terrain.terrainData;
        int alphamapWidth = tData.alphamapWidth;
        int alphamapHeight = tData.alphamapHeight;
        float[,,] alphamaps = tData.GetAlphamaps(0, 0, alphamapWidth, alphamapHeight);
        List<TreeInstance> trees = new List<TreeInstance>(tData.treeInstances);
        
        int placed = 0;
        int attempts = 0;
        int maxAttempts = treeCount * 10; // Prevent infinite loops

        while (placed < treeCount && attempts < maxAttempts)
        {
            float normX = Random.value;
            float normZ = Random.value;

            int mapX = Mathf.RoundToInt(normX * (alphamapWidth - 1));
            int mapZ = Mathf.RoundToInt(normZ * (alphamapHeight - 1));

            float weight = alphamaps[mapZ, mapX, textureLayerIndex];

            if (weight > requiredWeight)
            {
                float y = tData.GetInterpolatedHeight(normX, normZ) / tData.size.y;

                TreeInstance tree = new TreeInstance();
                tree.position = new Vector3(normX, y, normZ);
                tree.prototypeIndex = treePrototypeIndex;
                tree.widthScale = 1f;
                tree.heightScale = 1f;
                tree.color = Color.white;
                tree.lightmapColor = Color.white;

                trees.Add(tree);
                tData.treeInstances = trees.ToArray();
                
                placed++;
            }
            attempts++;
        }
        terrain.Flush();
    }
}