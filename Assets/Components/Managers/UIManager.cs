using System;
using Managers;
using TMPro;
using UnityEngine;
using EventType = Managers.EventType;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text appleText;
    [SerializeField] private TMP_Text beaverText;
    [SerializeField] private TMP_Text raccoonText;
    [SerializeField] private TMP_Text scoreText;
    private void OnEnable()
    {
        EventSystem<int>.Subscribe(EventType.AMOUNT_APPLES, UpdateAppleAmount);
        EventSystem<int>.Subscribe(EventType.AMOUNT_BEAVERS, UpdateBeaverText);
        EventSystem<int>.Subscribe(EventType.AMOUNT_RACCOONS, UpdateRaccoonText);
        EventSystem<GameObject>.Subscribe(EventType.DESTROY_OBJECT, DestroyObject);
    }
    private void OnDisable()
    {
        EventSystem<int>.Unsubscribe(EventType.AMOUNT_APPLES, UpdateAppleAmount);
        EventSystem<int>.Unsubscribe(EventType.AMOUNT_BEAVERS, UpdateBeaverText);
        EventSystem<int>.Unsubscribe(EventType.AMOUNT_RACCOONS, UpdateRaccoonText);
        EventSystem<GameObject>.Subscribe(EventType.DESTROY_OBJECT, DestroyObject);
    }

    private void DestroyObject(GameObject gameObject)
    {
        Destroy(gameObject);
    }

    private void Update()
    {
        scoreText.text = "Score: " + Time.timeSinceLevelLoad.ToString("#.##");
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

    private void UpdateBeaverText(int newAmount)
    {
        beaverText.text = "Amount beavers: " + newAmount.ToString();
    }

    private void UpdateRaccoonText(int newAmount)
    {
        raccoonText.text = "Amount raccoons: " + newAmount.ToString();
    }
}
