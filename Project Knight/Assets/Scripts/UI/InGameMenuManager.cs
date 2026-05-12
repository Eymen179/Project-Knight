using UnityEngine;
using UnityEngine.InputSystem;

public class InGameMenuManager : MonoBehaviour
{
    public static InGameMenuManager Instance;

    [SerializeField] private InputActionReference toggleEscMenu;

    private bool isEscMenuOpen = false;

    public GameObject player;
    public GameObject pnlEscMenu;

    private void OnEnable()
    {
        toggleEscMenu.action.Enable();
        toggleEscMenu.action.performed += OnToggleEscMenuPerformed;
    }

    private void OnDisable()
    {
        toggleEscMenu.action.Disable();
        toggleEscMenu.action.performed -= OnToggleEscMenuPerformed;
    }

    //"ESC" tusu metodu
    private void OnToggleEscMenuPerformed(InputAction.CallbackContext context)
    {
        //ESC menusu acma - kapama kontrolcusu
        isEscMenuOpen = !isEscMenuOpen;
        pnlEscMenu.SetActive(isEscMenuOpen);

        //Envanter acikkenki/kapaliykenki kurallari ayarla.
        if (isEscMenuOpen)
        {
            CursorVisibility(true);

            player.GetComponent<PlayerCombatManager>().enabled = false;
            player.GetComponent<PlayerMovement>().enabled = false;
            player.GetComponent<PlayerInteraction>().enabled = false;

            player.GetComponent<Animator>().SetFloat("speed", 0f);

            UIManager.Instance.toolBarBarrier.enabled = true;
        }
        else
        {
            CursorVisibility(false);

            player.GetComponent<PlayerCombatManager>().enabled = true;
            player.GetComponent<PlayerMovement>().enabled = true;
            player.GetComponent<PlayerInteraction>().enabled = true;

            UIManager.Instance.toolBarBarrier.enabled = false;
        }
    }
    //ESC Ekrani
    public void ResumeButton()
    {
        isEscMenuOpen = false;
        pnlEscMenu.SetActive(isEscMenuOpen);
        CursorVisibility(false);

        player.GetComponent<PlayerCombatManager>().enabled = true;
        player.GetComponent<PlayerMovement>().enabled = true;
        player.GetComponent<PlayerInteraction>().enabled = true;

        UIManager.Instance.toolBarBarrier.enabled = false;
    }

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


    //Yardimci Metodlar
    public void CursorVisibility(bool isVisible)
    {
        Cursor.visible = isVisible;

        if (isVisible)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        UIManager.Instance.crosshair.gameObject.SetActive(!isVisible);
    }
}
