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

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        mainInventoryGroup.SetActive(false);

        toolBarBarrier.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
