using System;
using Managers;
using UnityEngine;
using EventType = Managers.EventType;

public class GameManager : MonoBehaviour
{
    private void OnEnable()
    {
        EventSystem<GameObject>.Subscribe(EventType.DESTROY_GAMEOBJECT, DestroyGameObject);
    }
    private void OnDestroy()
    {
        EventSystem<GameObject>.Unsubscribe(EventType.DESTROY_GAMEOBJECT, DestroyGameObject);
    }

    private void DestroyGameObject(GameObject go)
    {
        Destroy(go);
    }
}
