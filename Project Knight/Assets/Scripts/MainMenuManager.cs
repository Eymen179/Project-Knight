using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public GameObject pnlMainMenu;
    public GameObject pnlOptionsMenu;

    //Scene Transition debugging
    public SceneController.GameScenes debugDestinationScene;
    void Start()
    {
        pnlMainMenu.SetActive(true);
        pnlOptionsMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //Ana menüdeki butonlarýn fonksiyonlarý
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
