using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    // The prefab or object you want to instantiate or move to these points
    public GameObject objectToPlace;

    void Start()
    {
        GameObject[] placementPoints = ShrinePositioning.Instance.placementPoints;
        // Example: Instantiate an object at each placement point
        foreach (GameObject point in placementPoints)
        {
            if (point != null && objectToPlace != null)
            {
                Instantiate(objectToPlace, point.transform.position, Quaternion.identity);
            }
        }
    }
}
