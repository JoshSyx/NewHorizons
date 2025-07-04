using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
internal class LayersPrefabs
{
    public TerrainLayer layer;
    public GameObject[] prefabs;
    public int numberOfPrefabs;
    public float requiredWeight = 0.5f;
}

[ExecuteAlways]
public class MapGenerator2 : SerializedMonoBehaviour
{
    [SerializeField]
    public Terrain terrain;
    [SerializeField]
    private LayersPrefabs[] Layers;

    [Button("Generate Terrain")]
    void GenerateTerrain()
    {
        TerrainData tData = terrain.terrainData;
        int alphamapWidth = tData.alphamapWidth;
        int alphamapHeight = tData.alphamapHeight;
        float[,,] alphamaps = tData.GetAlphamaps(0, 0, alphamapWidth, alphamapHeight);
        List<TreeInstance> trees = new List<TreeInstance>();
        
        foreach (var layer in Layers)
        {
            int placed = 0;
            int attempts = 0;
            int maxAttempts = layer.numberOfPrefabs * 10; // Prevent infinite loops
            
            var treeIndexes = new List<int>();
            foreach (var prefab in layer.prefabs)
            {
                var treeIndex = terrain.terrainData.treePrototypes.ToList().FindIndex(t => t.prefab == prefab);
                if (treeIndex != -1)
                    treeIndexes.Add(treeIndex);
                else
                    Debug.LogWarning("Tree prototype not found: " + prefab.name);
            }
            
            var layerIndex = terrain.terrainData.terrainLayers.ToList().IndexOf(layer.layer);
            if (layerIndex == -1)
            {
                Debug.LogWarning("Terrain layer not found: " + layer.layer.name);
                continue;
            }
            
            while (placed < layer.numberOfPrefabs && attempts < maxAttempts)
            {
                float normX = Random.value;
                float normZ = Random.value;

                int mapX = Mathf.RoundToInt(normX * (alphamapWidth - 1));
                int mapZ = Mathf.RoundToInt(normZ * (alphamapHeight - 1));

                float weight = alphamaps[mapZ, mapX, layerIndex];

                if (weight > layer.requiredWeight)
                {
                    float y = tData.GetInterpolatedHeight(normX, normZ) / tData.size.y;

                    TreeInstance tree = new TreeInstance();
                    tree.position = new Vector3(normX, y, normZ);
                    tree.prototypeIndex = treeIndexes[Random.Range(0, treeIndexes.Count)];
                    tree.rotation = Random.Range(0f, Mathf.PI * 2f);
                    tree.widthScale = 1f;
                    tree.heightScale = 1f;
                    tree.color = Color.white;
                    tree.lightmapColor = Color.white;

                    trees.Add(tree);

                    placed++;
                }

                attempts++;
            }
            Debug.Log("Placed " + placed + " for " + attempts + " attempts" + " trees in layer " + layer.layer.name + " total: " + trees.Count);
        }
        tData.treeInstances = trees.ToArray();
        terrain.Flush();
    }
}
