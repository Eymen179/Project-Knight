using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("In-Game UI")]
    public GameObject mainInventoryGroup;
    public GameObject toolBar;
    public Image toolBarBarrier;

    public TextMeshProUGUI txtCount;
    public TextMeshProUGUI txtPrompt;

    public Slider playerHealthSlider;
    public Image crosshair;

    [Header("In-Game UI/Crystal Effects")]
    public GameObject pnlCrystalEffectStatus;
    public TextMeshProUGUI txtEffectDuration;
    public TextMeshProUGUI txtEffects;

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        mainInventoryGroup.SetActive(false);

        toolBarBarrier.enabled = true;
    }

}
