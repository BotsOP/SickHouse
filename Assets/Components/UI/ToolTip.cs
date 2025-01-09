using System;
using UnityEngine;
using UnityEngine.UI;

public class ToolTip : MonoBehaviour
{
    [SerializeField] private GameObject popup;
    [SerializeField] private float timeBeforeShow;
    private float cachedTime = float.MaxValue;
    private bool firstShow = true;
    public void ShowPopup()
    {
        if (firstShow)
        {
            firstShow = false;
            cachedTime = Time.time - timeBeforeShow;
            return;
        }
        cachedTime = Time.time;
    }

    public void UnShow()
    {
        cachedTime = float.MaxValue;
        popup.SetActive(false);
    }

    private void Update()
    {
        if (Time.time > cachedTime + timeBeforeShow)
        {
            popup.SetActive(true);
        }
    }
}


