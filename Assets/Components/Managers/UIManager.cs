using System;
using System.Collections.Generic;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using EventType = Managers.EventType;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private TMP_Text appleText;
    [SerializeField] private TMP_Text beaverText;
    [SerializeField] private TMP_Text raccoonText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text selectionText;
    
    [SerializeField] private List<Image> brushUIImages;

    [SerializeField] private Color selectedColor;
    [SerializeField] private Color notSelectedColor;
    
    private void OnEnable()
    {
        EventSystem<int>.Subscribe(EventType.AMOUNT_APPLES, UpdateAppleAmount);
        EventSystem<int>.Subscribe(EventType.AMOUNT_BEAVERS, UpdateBeaverText);
        EventSystem<int>.Subscribe(EventType.AMOUNT_RACCOONS, UpdateRaccoonText);
        EventSystem<GameObject>.Subscribe(EventType.DESTROY_OBJECT, DestroyObject);
        EventSystem<int, int, Color, Vector3>.Subscribe(EventType.UPDATE_SELECTION_TEXT, UpdateSelectionText);
        EventSystem.Subscribe(EventType.DISABLE_SELECTION_TEXT, DisableSelectionText);

        foreach (Image image in brushUIImages)
        {
            image.color = notSelectedColor;
        }
        brushUIImages[0].color = selectedColor;
    }
    private void OnDisable()
    {
        EventSystem<int>.Unsubscribe(EventType.AMOUNT_APPLES, UpdateAppleAmount);
        EventSystem<int>.Unsubscribe(EventType.AMOUNT_BEAVERS, UpdateBeaverText);
        EventSystem<int>.Unsubscribe(EventType.AMOUNT_RACCOONS, UpdateRaccoonText);
        EventSystem<GameObject>.Subscribe(EventType.DESTROY_OBJECT, DestroyObject);
        EventSystem<int, int, Color, Vector3>.Unsubscribe(EventType.UPDATE_SELECTION_TEXT, UpdateSelectionText);
        EventSystem.Unsubscribe(EventType.DISABLE_SELECTION_TEXT, DisableSelectionText);
    }

    private void UpdateSelectionText(int amount, int max, Color color, Vector3 position)
    {
        selectionText.text = amount + " / " + max;
        selectionText.color = color;
        selectionText.transform.position = mainCamera.WorldToScreenPoint(position);
    }

    private void DisableSelectionText()
    {
        selectionText.text = "";
    }

    public void ToggleInput(bool value)
    {
        EventSystem<bool>.RaiseEvent(EventType.TOGGLE_INPUT, value);
    }

    private void DestroyObject(GameObject gameObject)
    {
        Destroy(gameObject);
    }

    private void Update()
    {
        scoreText.text = "Score: " + (int)(Time.timeSinceLevelLoad * 100) / 100.0f;
    }

    private void UpdateAppleAmount(int amountApples)
    {
        appleText.text = "Amount Apples: " + amountApples;
    }

    public void ChangeBrush(int tileID)
    {
        EventSystem<EntityTileID>.RaiseEvent(EventType.CHANGE_BRUSH, (EntityTileID)tileID);
    }
    public void ChangeBrushUI(Image brushImage)
    {
        foreach (Image image in brushUIImages)
        {
            image.color = notSelectedColor;
        }
        brushImage.color = selectedColor;
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
