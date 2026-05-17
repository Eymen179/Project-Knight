using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("In-Game UI")]
    public GameObject mainInventoryGroup;
    public GameObject toolBar;
    public Image toolBarBarrier;

    [Header("Item Details")]
    public GameObject pnlItemDetails;
    public TextMeshProUGUI txtItemName;
    public TextMeshProUGUI txtItemDescription;
    public TextMeshProUGUI[] txtStats;

    public TextMeshProUGUI txtCount;
    public TextMeshProUGUI txtPrompt;

    public Slider playerHealthSlider;
    public TextMeshProUGUI txtHealth;
    public Image crosshair;
    public Slider bossHealthSlider;
    public TextMeshProUGUI txtBossHealth;

    [Header("In-Game UI/Crystal Effects")]
    public GameObject pnlCrystalEffectStatus;
    public TextMeshProUGUI txtEffectDuration;
    public TextMeshProUGUI txtEffects;

    public GameObject pnlDieScreen;
    public Button btnRespawn;
    public bool isRespawnButtonActive = false;

    public GameObject pnlESCMenu;

    [Header("Permanent Crystals")]
    public GameObject pnlPermanentCrystals;
    public TextMeshProUGUI txtHealthPermanent;
    public TextMeshProUGUI txtDamagePermanent;
    public TextMeshProUGUI txtSpeedPermanent;

    private void Awake()
    {
        Instance = this;
    }
    void Start()//UI Objelerinin Baslangic Ayarlari
    {
        mainInventoryGroup.SetActive(false);
        pnlItemDetails.SetActive(false);
        toolBarBarrier.enabled = true;
        pnlDieScreen.SetActive(false);
        if (playerHealthSlider != null)
        {
            playerHealthSlider.maxValue = 1f;
            playerHealthSlider.value = 1f;
        }
        if(SceneManager.GetActiveScene().name == "Dungeon_BossRoom")
        {
            isRespawnButtonActive = true;
            bossHealthSlider.gameObject.SetActive(true);
        }
        if(isRespawnButtonActive)
        {
            btnRespawn.gameObject.SetActive(true);
        }else
        {
            btnRespawn.gameObject.SetActive(false);
        }
    }
    
    
}
