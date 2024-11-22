using System;
using Managers;
using TMPro;
using UnityEngine;
using EventType = Managers.EventType;

public class AppleChangeText : MonoBehaviour
{
    [SerializeField] private TMP_Text textUp;
    [SerializeField] private TMP_Text textDown;

    private void OnEnable()
    {
        EventSystem<int>.Subscribe(EventType.CHANGE_AMOUNT_APPLES, ShowText);
    }
    
    private void OnDisable()
    {
        EventSystem<int>.Unsubscribe(EventType.CHANGE_AMOUNT_APPLES, ShowText);
    }

    private void ShowText(int newText)
    {
        if (newText > 0)
        {
            TMP_Text text = Instantiate(textUp, transform.parent);
            text.text = newText.ToString();
            Destroy(text.gameObject, 1f);
        }
        else if (newText < 0)
        {
            TMP_Text text = Instantiate(textDown, transform.parent);
            text.text = newText.ToString();
            Destroy(text.gameObject, 1f);
        }
    }
}
