using UnityEngine;

[ExecuteInEditMode]
public class BurstSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject particlePrefab; // Prefab to spawn
    [SerializeField] private Vector3 boxSize = new Vector3(5, 5, 5); // Scalable box dimensions
    [SerializeField] private int burstCount = 50; // Number of particles to spawn in a burst
     private bool previewInEditor = true; // Toggle to show preview in editor

    [Header("Oscillation Settings")]
    [SerializeField] private float amplitude = 1.0f; // Maximum height of oscillation
    [SerializeField] private float baseSpeed = 1.0f; // Base speed of oscillation
    [SerializeField] private bool randomizeSpeed = true; // Should each object have a random speed?

    private GameObject previewParent; // Parent container for spawned objects in Editor Mode
    private const string PreviewParentName = "EditorPreviewObjects"; // Consistent name for easy cleanup

    private void OnValidate()
    {
        // Check if preview is disabled
        if (!previewInEditor)
        {
            DespawnPreviewObjects(); // Clear objects if preview is disabled
            return;
        }

        // Respawn objects whenever settings are changed
        DespawnPreviewObjects(); // Remove old objects
        SpawnPreviewObjects();   // Spawn updated objects
    }

    private void OnDisable()
    {
        // Ensure cleanup when the script is disabled in the editor
        DespawnPreviewObjects();
    }

    private void OnDestroy()
    {
        // Ensure cleanup when the GameObject is deleted
        DespawnPreviewObjects();
    }

    private void SpawnPreviewObjects()
    {
        if (particlePrefab == null) return;

        // Create a new parent GameObject for organizing spawned objects
        previewParent = new GameObject(PreviewParentName);
        previewParent.transform.SetParent(transform);
        previewParent.transform.localPosition = Vector3.zero;

        for (int i = 0; i < burstCount; i++)
        {
            // Generate a random position inside the box
            Vector3 randomPosition = new Vector3(
                Random.Range(-boxSize.x / 2, boxSize.x / 2),
                Random.Range(-boxSize.y / 2, boxSize.y / 2),
                Random.Range(-boxSize.z / 2, boxSize.z / 2)
            );

            // Offset the position relative to the spawner's position
            Vector3 worldPosition = transform.position + randomPosition;

            // Instantiate the prefab as a child of the previewParent
            GameObject previewObject = Instantiate(particlePrefab, worldPosition, Quaternion.identity, previewParent.transform);
            previewObject.name = $"PreviewObject_{i}";

            // Add oscillation logic for movement preview
            Oscillator oscillator = previewObject.AddComponent<Oscillator>();
            oscillator.Initialize(amplitude, baseSpeed, randomizeSpeed);
        }
    }

    private void DespawnPreviewObjects()
    {
        // Find and destroy the preview parent and all its children
        Transform existingParent = transform.Find(PreviewParentName);
        if (existingParent != null)
        {
            DestroyImmediate(existingParent.gameObject);
        }

        previewParent = null; // Reset the cached reference
    }

    private void OnDrawGizmosSelected()
    {
        // Draw a wireframe box in the Scene View to visualize the spawn area
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, boxSize);
    }
}
