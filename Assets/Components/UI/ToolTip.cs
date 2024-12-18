using UnityEngine;
using UnityEngine.UI;

public class ToolTip : MonoBehaviour
{
    public GameObject popup;
    public GameObject instance;
    public void ShowPopup()
    {
        popup.SetActive(true);

    }

    public void UnShow()
    {
        popup.SetActive(false);
    }
}


