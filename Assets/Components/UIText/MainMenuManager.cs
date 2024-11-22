using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private string mainSceneName = "DenisScene";
    public void LoadMainScene()
    {
        SceneManager.LoadScene("DenisScene");
    }
}
