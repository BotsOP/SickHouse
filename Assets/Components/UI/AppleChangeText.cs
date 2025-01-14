using System;
using Managers;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using EventType = Managers.EventType;

public class AppleChangeText : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject textUp;
    [SerializeField] private GameObject textDown;

    private void OnEnable()
    {
        EventSystem<int, Vector3>.Subscribe(EventType.CHANGE_AMOUNT_APPLES, ShowText);
    }
    
    private void OnDisable()
    {
        EventSystem<int, Vector3>.Unsubscribe(EventType.CHANGE_AMOUNT_APPLES, ShowText);
    }

    private void ShowText(int newText, Vector3 worldPosition)
    {
        Vector3 screenSpace = mainCamera.WorldToScreenPoint(worldPosition);
        if (newText > 0)
        {
            TMP_Text text = Instantiate(textUp, screenSpace, quaternion.identity, transform.parent).GetComponentInChildren<TMP_Text>();
            text.text = newText.ToString();
            Destroy(text.gameObject, 1f);
        }
        else if (newText < 0)
        {
            TMP_Text text = Instantiate(textDown, screenSpace, quaternion.identity, transform.parent).GetComponentInChildren<TMP_Text>();
            text.text = newText.ToString();
            Destroy(text.gameObject, 1f);
        }
    }
}
