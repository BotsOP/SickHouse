using UnityEngine;
using UnityEngine.UI;

public class ToolTip : MonoBehaviour
{
    public GameObject popup;
    public GameObject instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }
    public void ShowPopup()
    {
        Debug.Log("PPOP UP AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        popup.SetActive(true);

    }

    public void UnShow()
    {
        popup.SetActive(false);
    }
}


