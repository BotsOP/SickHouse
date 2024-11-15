using System;
using Managers;
using TMPro;
using UnityEngine;
using EventType = Managers.EventType;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text appleText;
    private void OnEnable()
    {
        EventSystem<int>.Subscribe(EventType.AMOUNT_APPLES, UpdateAppleAmount);
    }
    private void OnDisable()
    {
        EventSystem<int>.Unsubscribe(EventType.AMOUNT_APPLES, UpdateAppleAmount);
    }

    private void UpdateAppleAmount(int amountApples)
    {
        appleText.text = "Amount Apples: " + amountApples;
    }

    public void ChangeBrush(int tileID)
    {
        EventSystem<TileID>.RaiseEvent(EventType.CHANGE_BRUSH, (TileID)tileID);
    }

    public void SpawnRacoon()
    {
        EventSystem.RaiseEvent(EventType.SPAWN_RACOON);
    }
    
    public void SpawnBeavor()
    {
        EventSystem.RaiseEvent(EventType.SPAWN_BEAVOR);
    }

    public void EnableGameobject(GameObject gameObject)
    {
        gameObject.SetActive(true);
    }
    public void DisableGameobject(GameObject gameObject)
    {
        gameObject.SetActive(false);
    }
}
