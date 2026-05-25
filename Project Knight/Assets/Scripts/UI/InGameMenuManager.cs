using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InGameMenuManager : MonoBehaviour
{
    public static InGameMenuManager Instance;

    [SerializeField] private InputActionReference toggleEscMenu;

    private bool isEscMenuOpen = false;

    public GameObject player;
    public GameObject pnlEscMenu;

    // SADECE BU DEÐÝÞKEN EKLEDÝ: Options panelini editörden baðlamak için
    public GameObject pnlOptions;

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "Game_End")
        {
            CursorVisibility(true);
        }
    }

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
        // DEÐÝÞÝKLÝK YAPILMADI: Eðer Options paneli açýksa ESC'ye basýnca önce onu kapatsýn ve ana menüye dönsün
        if (pnlOptions != null && pnlOptions.activeSelf)
        {
            CloseOptionsButton();
            return;
        }

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

            // ESC ile menü tamamen kapatýlýyorsa Options da kapansýn
            if (pnlOptions != null) pnlOptions.SetActive(false);
        }
    }

    //ESC Ekrani
    public void ResumeButton()
    {
        isEscMenuOpen = false;
        pnlEscMenu.SetActive(isEscMenuOpen);

        // Resume basýldýðýnda eðer arkada options açý kaldýysa onu da kapatýr
        if (pnlOptions != null) pnlOptions.SetActive(false);

        CursorVisibility(false);

        player.GetComponent<PlayerCombatManager>().enabled = true;
        player.GetComponent<PlayerMovement>().enabled = true;
        player.GetComponent<PlayerInteraction>().enabled = true;

        UIManager.Instance.toolBarBarrier.enabled = false;
    }

    // SADECE BU ÝKÝ FONKSÝYON EKLEDÝ: Options butonlarý için
    public void OpenOptionsButton()
    {
        pnlEscMenu.SetActive(false); // Ana ESC menüsünü gizle
        pnlOptions.SetActive(true);  // Options menüsünü aç
    }

    public void CloseOptionsButton()
    {
        pnlOptions.SetActive(false); // Options menüsünü kapat
        pnlEscMenu.SetActive(true);  // Yeniden ana ESC menüsünü göster
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