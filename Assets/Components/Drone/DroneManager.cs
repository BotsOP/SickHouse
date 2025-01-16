using System;
using System.Collections.Generic;
using Managers;
using Unity.Mathematics;
using UnityEngine;
using UnityEditor;
using EventType = Managers.EventType;
using Random = UnityEngine.Random;

/*
 * Spawns randomly from the city and flies south. Clicking on it will cause it to fall down and permanently destroy a + shaped set of 5 tiles where it lands. It gives you a considerable random amount of apples when destroyed.
      - Minimum random time between spawns
      - Maximum random time between spawns
      - Apples given upon death
      - Movement speed
 */

public class DroneManager : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private float droneYPos;
    [SerializeField] private float minTimeBetweenSpawns = 1f;
    [SerializeField] private float maxTimeBetweenSpawns = 2f;
    [SerializeField] private int amountApplesUponDeath;
    [SerializeField] private float speed;
    [SerializeField] private float deadDroneSpeedX;
    [SerializeField] private float deadDroneSpeedY;
    [SerializeField] private GameObject dronePrefab;
    [SerializeField] private AudioClip droneDeath;
    [SerializeField] private RectTransform cursorPrefab;
    [SerializeField] private RectTransform canvasTransform;
    [SerializeField] private Camera mainCamera;

    private bool hasClicked;
    private List<GameObject> drones = new List<GameObject>();
    private List<GameObject> deadDrones = new List<GameObject>();
    private Dictionary<GameObject, RectTransform> droneCursors = new Dictionary<GameObject, RectTransform>();
    private float timeBetweenSpawns;
    private float cachedTime;
    private void Awake()
    {
        EventSystem<GameObject>.Subscribe(EventType.HIT_DRONE, RemoveDrone);
        if (maxTimeBetweenSpawns < minTimeBetweenSpawns)
        {
            Debug.LogError($"maxTimeBetweenSpawns ({maxTimeBetweenSpawns}) shouldnt be smaller than minTimeBetweenSpawns ({minTimeBetweenSpawns})");
        }
    }
    private void OnDisable()
    {
        EventSystem<GameObject>.Unsubscribe(EventType.HIT_DRONE, RemoveDrone);
    }

    private void Update()
    {
        if (Time.timeSinceLevelLoad > cachedTime + timeBetweenSpawns)
        {
            cachedTime = Time.timeSinceLevelLoad;
            timeBetweenSpawns = math.lerp(minTimeBetweenSpawns, maxTimeBetweenSpawns, Random.Range(0, 1));
            
            drones.Add(Instantiate(dronePrefab, new Vector3(Mathf.RoundToInt(Random.Range(-25, 25)) + 0.5f, droneYPos, gridManager.wallDistance), quaternion.identity));
            if(!hasClicked)
                droneCursors.Add(drones[^1], Instantiate(cursorPrefab, canvasTransform));
        }

        if (!hasClicked)
        {
            foreach (KeyValuePair<GameObject,RectTransform> keyValuePair in droneCursors)
            {
                keyValuePair.Value.position = mainCamera.WorldToScreenPoint(keyValuePair.Key.transform.position);
            }
        }
        
        
        for (int i = 0; i < drones.Count; i++)
        {
            GameObject drone = drones[i];
            drone.transform.Translate(Vector3.back * (Time.deltaTime * speed));

            if (drone.transform.position.z < -50)
            {
                droneCursors.Remove(drone);
                drones.Remove(drone);
                Destroy(drone);
            }
        }

        for (int i = 0; i < deadDrones.Count; i++)
        {
            GameObject drone = deadDrones[i];
            drone.transform.Translate(Vector3.back * (Time.deltaTime * deadDroneSpeedX));
            drone.transform.Translate(Vector3.down * (Time.deltaTime * deadDroneSpeedY));

            if (drone.transform.position.y < 0)
            {
                Vector3 position = GridManager.instance.GetPosition(GridManager.instance.WorldPosToIndexPos(drone.transform.position));
                
                EventSystem<Vector3, EntityTileID[]>.RaiseEvent(EventType.FORCE_CHANGE_TILE, position, new[] { EntityTileID.EMPTY, EntityTileID.DIRT, EntityTileID.EMPTY });
                
                position.x += 1;
                EventSystem<Vector3, EntityTileID[]>.RaiseEvent(EventType.FORCE_CHANGE_TILE, position, new[] { EntityTileID.EMPTY, EntityTileID.DIRT, EntityTileID.EMPTY });
                position.x -= 2;
                EventSystem<Vector3, EntityTileID[]>.RaiseEvent(EventType.FORCE_CHANGE_TILE, position, new[] { EntityTileID.EMPTY, EntityTileID.DIRT, EntityTileID.EMPTY });
                position.x += 1;
                
                position.z += 1;
                EventSystem<Vector3, EntityTileID[]>.RaiseEvent(EventType.FORCE_CHANGE_TILE, position, new[] { EntityTileID.EMPTY, EntityTileID.DIRT, EntityTileID.EMPTY });
                position.z -= 2;
                EventSystem<Vector3, EntityTileID[]>.RaiseEvent(EventType.FORCE_CHANGE_TILE, position, new[] { EntityTileID.EMPTY, EntityTileID.DIRT, EntityTileID.EMPTY });
                deadDrones.Remove(drone);
                DroneExplodeEffect(drone);
                EventSystem<int, Vector3>.RaiseEvent(EventType.GAIN_APPLES, amountApplesUponDeath, drone.transform.position);
                Destroy(drone);
                i--;
            }
        }
    }

    private void RemoveDrone(GameObject drone)
    {
        hasClicked = true;
        foreach (KeyValuePair<GameObject,RectTransform> keyValuePair in droneCursors)
        {
            Destroy(keyValuePair.Value.gameObject);
        }
        droneCursors.Clear();
        if (!drones.Remove(drone))
        {
            Debug.LogError($"drone: {drone.name} was not found");
            return;
        }
        deadDrones.Add(drone);
        DroneFallEffect(drone);
        SoundManager.instance.PlaySoundClip(droneDeath, transform, 1f);
        Destroy(drone.GetComponent<BoxCollider>());
    }

    private void DroneFallEffect(GameObject drone)
    {
        DroneAnimatorBehaviour component = drone.GetComponent<DroneAnimatorBehaviour>();
        if (component != null)
        {
            component.DoFall();
        }
        else
        {
            Debug.LogWarning("no component found");
        }
    }

    private void DroneExplodeEffect(GameObject drone)
    {
        DroneAnimatorBehaviour component = drone.GetComponent<DroneAnimatorBehaviour>();
        if (component != null)
        {
            component.DoExplode();
        }
        else
        {
            Debug.LogWarning("no component found");
        }
    }
}
