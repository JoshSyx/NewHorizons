using UnityEngine;
using System.Collections.Generic;

public class EnvironmentManager : MonoBehaviour
{
    public GameObject objectToPlace;

    // Number of objects to spawn randomly
    public int numberToSpawn = 3;

    void Start()
    {
        GameObject[] placementPoints = ShrinePositioning.Instance.placementPoints;

        if (objectToPlace == null || placementPoints == null || placementPoints.Length == 0)
        {
            Debug.LogWarning("Missing objectToPlace or placementPoints");
            return;
        }

        // Make sure numberToSpawn doesn't exceed the available points
        int spawnCount = Mathf.Min(numberToSpawn, placementPoints.Length);

        // Create a list from the array to allow removal of chosen points
        List<GameObject> pointsList = new List<GameObject>(placementPoints);

        for (int i = 0; i < spawnCount; i++)
        {
            // Pick a random index
            int randomIndex = Random.Range(0, pointsList.Count);

            GameObject chosenPoint = pointsList[randomIndex];

            if (chosenPoint != null)
            {
                Instantiate(objectToPlace, chosenPoint.transform.position, Quaternion.identity);
            }

            // Remove the chosen point so it won't be picked again
            pointsList.RemoveAt(randomIndex);
        }
    }
}
