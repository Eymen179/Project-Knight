using UnityEngine;

public class InGameMenuManager : MonoBehaviour
{
    public static InGameMenuManager Instance;

    //Olum Ekrani
    public void RespawnAgainButton()
    {
        SceneController.Instance.globalDifficultyMultiplier += 0.1f;

        SceneController.Instance.LoadScene(SceneController.GameScenes.Dungeon_Main);
    }
    public void RestartButton()
    {
        SceneController.Instance.ResetAllSavedData();

        SceneController.Instance.LoadScene(SceneController.GameScenes.Map);
    }
    public void BackToMainMenuButton()
    {
        SceneController.Instance.ResetAllSavedData();

        SceneController.Instance.LoadScene(SceneController.GameScenes.MainMenu);
    }
}
