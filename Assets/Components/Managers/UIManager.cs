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
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text selectionText;
    [SerializeField] private TMP_Text selectionTextCost;
    
    [SerializeField] private List<RawImage> brushUIImages;

    [SerializeField] private Color selectedColor;
    [SerializeField] private Color notSelectedColor;

    [SerializeField] private AudioClip applesUp;
    int previousApples = 0;
    private void OnEnable()
    {
        EventSystem<int>.Subscribe(EventType.AMOUNT_APPLES, UpdateAppleAmount);
        EventSystem<GameObject>.Subscribe(EventType.DESTROY_OBJECT, DestroyObject);
        EventSystem<int, int, Color, Vector3>.Subscribe(EventType.UPDATE_SELECTION_TEXT, UpdateSelectionText);
        EventSystem.Subscribe(EventType.DISABLE_SELECTION_TEXT, DisableSelectionText);

        foreach (RawImage image in brushUIImages)
        {
            image.color = notSelectedColor;
        }
        brushUIImages[0].color = selectedColor;
    }
    private void OnDisable()
    {
        EventSystem<int>.Unsubscribe(EventType.AMOUNT_APPLES, UpdateAppleAmount);
        EventSystem<GameObject>.Subscribe(EventType.DESTROY_OBJECT, DestroyObject);
        EventSystem<int, int, Color, Vector3>.Unsubscribe(EventType.UPDATE_SELECTION_TEXT, UpdateSelectionText);
        EventSystem.Unsubscribe(EventType.DISABLE_SELECTION_TEXT, DisableSelectionText);
    }

    private void UpdateSelectionText(int amount, int max, Color color, Vector3 position)
    {
        selectionText.text = amount + " / " + max;
        selectionText.color = color;
        selectionText.transform.position = mainCamera.WorldToScreenPoint(position);
        selectionTextCost.transform.position = mainCamera.WorldToScreenPoint(position);
        Vector3 pos = selectionTextCost.transform.position;
        pos.y -= 50;
        selectionTextCost.transform.position = pos;
        selectionTextCost.text = "Cost: 3";
    }

    private void DisableSelectionText()
    {
        selectionText.transform.position = Vector3.zero;
        selectionTextCost.transform.position = Vector3.zero;
        selectionText.text = "";
        selectionTextCost.text = "";
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
        scoreText.text = ((int)(Time.timeSinceLevelLoad * 100) / 100.0f).ToString();
    }

    private void UpdateAppleAmount(int amountApples)
    {
        appleText.text = amountApples.ToString();
        if (amountApples > previousApples)
        {
            SoundManager.instance.PlaySoundClip(applesUp, transform, 1f);
        }
        previousApples = amountApples;
    }

    public void ChangeBrush(int tileID)
    {
        EventSystem<EntityTileID>.RaiseEvent(EventType.CHANGE_BRUSH, (EntityTileID)tileID);
    }
    public void ChangeBrushUI(RawImage brushImage)
    {
        foreach (RawImage image in brushUIImages)
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
}
