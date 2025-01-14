using System;
using Managers;
using UnityEngine;
using EventType = Managers.EventType;
using Random = UnityEngine.Random;

/*
 * The mega bulldozer is a sort of boss. It spawns once per game on a random spot along the edge of the city.
 * It is a scaled up bulldozer with twice the tile width of a normal one.
 * It moves somewhat fast toward the south side of the map and permanently destroys everything it touches, replacing tiles with the blank city tile.
   The mega bulldozer can be destroyed by clicking on it rapidly, each click reducing its health a little. 
   When it’s destroyed it will stop moving and display much more smoke, preferably covering most of the bulldozer entirely. When it’s destroyed, you gain apples.
   
    Parameters:
     - Movement speed
     - Time at which it spawns
     - Health
     - Apples given upon death
 */

public class Bulldozer : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private float speed = 5;
    [SerializeField] private float timeWhenToSpawn = 5;
    [SerializeField] private int health = 10;
    [SerializeField] private float amountApples = 50;
    [SerializeField] private GameObject bulldozerPrefab;
    [SerializeField] private AudioClip megaDeath;
    [SerializeField] private AudioClip start;

    private GameObject bulldozer;
    private bool destroyed;
    private int currentZPos;

    private void OnDisable()
    {
        EventSystem.Unsubscribe(EventType.HIT_GIANT_BULLDOZER, TookDamage);
    }

    private void Awake()
    {
        EventSystem.Subscribe(EventType.HIT_GIANT_BULLDOZER, TookDamage);
    }

    private void Update()
    {
        if (Time.timeSinceLevelLoad > timeWhenToSpawn)
        {
            float xPos = Mathf.RoundToInt(Random.Range(-25, 25)) + 0.5f;
            bulldozer = Instantiate(bulldozerPrefab, new Vector3(xPos, 0, gridManager.wallDistance - 25 + 4), Quaternion.identity);
            SoundManager.instance.PlaySoundClip(start, transform, .4f);
            timeWhenToSpawn = float.MaxValue;
        }

        if (timeWhenToSpawn == float.MaxValue && !destroyed)
        {
            Vector3 pos = bulldozer.transform.position;
            int floorToInt = Mathf.FloorToInt(pos.z);
            if (floorToInt != currentZPos)
            {
                TransformTiles(pos);
                currentZPos = floorToInt;
            }
            pos.z -= speed * Time.deltaTime;
            bulldozer.transform.position = pos;
        }
        
        if (currentZPos >= -30)
            return;
            
        destroyed = true;
        Destroy(bulldozer);
    }
    
    private static void TransformTiles(Vector3 pos)
    {
        Vector3 tempPos = pos;
        tempPos.z -= 4;
        EventSystem<Vector3, EntityTileID[]>.RaiseEvent(EventType.CHANGE_TILE, tempPos, new[] { EntityTileID.EMPTY, EntityTileID.PAVEMENT, EntityTileID.EMPTY });
        tempPos.x += 1;
        EventSystem<Vector3, EntityTileID[]>.RaiseEvent(EventType.CHANGE_TILE, tempPos, new[] { EntityTileID.EMPTY, EntityTileID.PAVEMENT, EntityTileID.EMPTY });
        tempPos.x += 1;
        EventSystem<Vector3, EntityTileID[]>.RaiseEvent(EventType.CHANGE_TILE, tempPos, new[] { EntityTileID.EMPTY, EntityTileID.PAVEMENT, EntityTileID.EMPTY });
        tempPos.x += 1;
        EventSystem<Vector3, EntityTileID[]>.RaiseEvent(EventType.CHANGE_TILE, tempPos, new[] { EntityTileID.EMPTY, EntityTileID.PAVEMENT, EntityTileID.EMPTY });
        tempPos.x -= 3;
                
        tempPos.x -= 1;
        EventSystem<Vector3, EntityTileID[]>.RaiseEvent(EventType.CHANGE_TILE, tempPos, new[] { EntityTileID.EMPTY, EntityTileID.PAVEMENT, EntityTileID.EMPTY });
        tempPos.x -= 1;
        EventSystem<Vector3, EntityTileID[]>.RaiseEvent(EventType.CHANGE_TILE, tempPos, new[] { EntityTileID.EMPTY, EntityTileID.PAVEMENT, EntityTileID.EMPTY });
        tempPos.x -= 1;
        EventSystem<Vector3, EntityTileID[]>.RaiseEvent(EventType.CHANGE_TILE, tempPos, new[] { EntityTileID.EMPTY, EntityTileID.PAVEMENT, EntityTileID.EMPTY });
        tempPos.x -= 1;
        EventSystem<Vector3, EntityTileID[]>.RaiseEvent(EventType.CHANGE_TILE, tempPos, new[] { EntityTileID.EMPTY, EntityTileID.PAVEMENT, EntityTileID.EMPTY });
    }

    private void TookDamage()
    {
        health -= 1;
        if (health <= 0)
        {
            destroyed = true;
        }
        else
        {
            SoundManager.instance.PlaySoundClip(megaDeath, transform, .5f);
        }
    }
}
