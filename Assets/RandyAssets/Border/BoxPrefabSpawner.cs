using UnityEngine;
using System.Collections.Generic;

public class BoxPrefabSpawner : MonoBehaviour
{
    [Header("Spawn Area")]
    public Vector3 boxSize = new Vector3(10f, 10f, 10f); // Define the size of the box

    [Header("Spawn Settings")]
    public GameObject prefabToSpawn; // Prefab to spawn
    public int numberOfPrefabs = 10; // Number of prefabs to spawn
    public float SpawnScaleRangeMIN = 0.75f;
    public float SpawnScaleRangeMAX = 2f;

    [Header("Oscillation Settings")]
    public float amplitude = 2f; // Oscillation amplitude
    public float baseSpeed = 1f; // Base speed of oscillation
    public bool randomizeSpeed = true; // Randomize speed

    [Header("Density Settings")]
    public float densityFactor = 2f; // Controls the bias toward the center

    [Header("Overlap Settings")]
    public float minimumDistance = 1.0f; // Minimum distance to prevent overlap, takes scale into account

    private bool showGizmos = true; // Controls gizmo visibility

    private List<GameObject> spawnedObjects = new List<GameObject>();

        
    void Start()
    {
        if (prefabToSpawn != null)
        {
            SpawnPrefabs();
        }
        showGizmos = false; // Hide gizmo when the game starts
    }

    private void SpawnPrefabs()
    {
        // Destroy all children of this GameObject
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            Destroy(child.gameObject); // Destroy in play mode
        }

        spawnedObjects.Clear();

        // Spawn new prefabs with higher density near the world origin (0, 0, 0)
        for (int i = 0; i < numberOfPrefabs; i++)
        {
            Vector3 randomPosition;
            int attempts = 0;

            // Ensure prefabs do not overlap
            do
            {
                randomPosition = GetRandomPositionWithDensity();
                attempts++;

                if (attempts > 100) // Prevent infinite loops
                {
                    Debug.LogWarning("Could not place prefab without overlap after 100 attempts.");
                    break;
                }
            } while (IsOverlapping(randomPosition));

            GameObject spawnedObject = Instantiate(prefabToSpawn, randomPosition, Quaternion.identity, transform);

            // Randomize scale
            float randomScale = Random.Range(SpawnScaleRangeMIN, SpawnScaleRangeMAX);
            spawnedObject.transform.localScale = new Vector3(randomScale, randomScale, randomScale);

            // Add to list of spawned objects
            spawnedObjects.Add(spawnedObject);

            // Initialize the Oscillator script on the prefab
            Oscillator oscillator = spawnedObject.GetComponent<Oscillator>();
            if (oscillator != null)
            {
                oscillator.Initialize(amplitude, baseSpeed, randomizeSpeed);
            }
        }
    }

    private Vector3 GetRandomPositionWithDensity()
    {
        // Generate positions with higher density near the origin
        float distanceFactor = Mathf.Pow(Random.value, densityFactor); // Apply the density factor to control bias

        float x = Random.Range(-boxSize.x / 2, boxSize.x / 2) * distanceFactor;
        float y = Random.Range(-boxSize.y / 2, boxSize.y / 2) * distanceFactor;
        float z = Random.Range(-boxSize.z / 2, boxSize.z / 2) * distanceFactor;

        return new Vector3(x, y, z) + transform.position;
    }

    private bool IsOverlapping(Vector3 position)
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj == null) continue;

            float distance = Vector3.Distance(position, obj.transform.position);
            float combinedRadius = minimumDistance * 0.5f + (obj.transform.localScale.x * 0.5f);

            if (distance < combinedRadius)
            {
                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return; // Skip drawing if showGizmos is false

        // Draw the box area in the editor
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawCube(transform.position, boxSize);
    }

}
