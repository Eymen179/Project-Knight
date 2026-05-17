using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public GameObject pnlMainMenu;
    public GameObject pnlOptionsMenu;

    //Test Sahnesi icin enum degeri
    public SceneController.GameScenes debugDestinationScene;
    void Start()
    {
        pnlMainMenu.SetActive(true);
        pnlOptionsMenu.SetActive(false);
    }

    void Update()
    {
        
    }
    //Ana menudeki butonlarin fonksiyonlari
    public void PlayButton()
    {
        SceneController.Instance.LoadScene(SceneController.GameScenes.Map);
    }
    public void OptionsButton()
    {
        pnlMainMenu.SetActive(false);
        pnlOptionsMenu.SetActive(true);
    }
    public void QuitButton()
    {
        Application.Quit();
    }
    public void DeveloperButton()
    {
        SceneController.Instance.LoadScene(debugDestinationScene);
    }
    //Options
    public void OptionsToMainMenuButton()
    {
        pnlMainMenu.SetActive(true);
        pnlOptionsMenu.SetActive(false);
    }
}
