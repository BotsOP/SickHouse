using UnityEngine;
using UnityEngine.UI;

public class InfoPopup : MonoBehaviour
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
        if (popup.activeSelf == false)
        {
            //instance = Instantiate(popup, transform.position, Quaternion.identity);
            
        }

    }

    public void UnShow()
    {
        popup.SetActive(false);
    }
}
