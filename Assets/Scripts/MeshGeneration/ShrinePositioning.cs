using UnityEngine;

public class ShrinePositioning : MonoBehaviour
{
    public static ShrinePositioning Instance;

    public GameObject[] placementPoints;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
}
