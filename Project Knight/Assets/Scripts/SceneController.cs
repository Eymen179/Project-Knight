using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;

    // --- YENÝ EKLENEN: TAÞINACAK VERÝLER (DATA CARRIER) ---
    [System.Serializable]
    public struct ItemSaveData
    {
        public Item item;
        public int count;
        public int slotIndex; // YENÝ: Eþyanýn hangi numaralý slotta durduðu bilgisi
    }

    // Kýlýçlarý ve diðer eþyalarý sahneler arasý taþýmak için ölümsüz listeler
    [HideInInspector] public List<ItemSaveData> savedSwordSlots = new List<ItemSaveData>();
    [HideInInspector] public List<ItemSaveData> savedOtherSlots = new List<ItemSaveData>();
    // Toolbar'daki özel kýlýç slotu için tekil veri alaný
    [HideInInspector] public ItemSaveData savedToolbarSword;
    [HideInInspector] public bool hasSavedToolbarSword = false;
    // -------------------------

    [Header("Player Stats Save Data")]
    public int savedCurrentHealth = -1; // -1, henüz bir kayýt yok demektir
    public int savedMaxHealth = -1;

    // --- YENÝ EKLENEN: KALICI EFEKT VERÝLERÝ ---
    [Header("Permanent Effect Save Data")]
    public bool hasSavedPermanentEffects = false; // Efekt var mý kontrolü

    // UI Sayaçlarý
    public int savedPermanentHealthCount = 0;
    public int savedPermanentDamageCount = 0;
    public int savedPermanentSpeedCount = 0;

    // Gerçek Bonus Deðerleri
    public int savedPermanentBonusDamage = 0;
    public int savedPermanentBonusCritChance = 0;
    public float savedPermanentBonusCritMultiplier = 0f;
    public float savedPermanentBonusAttackSpeed = 0f;
    // ------------------------------------------

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void LoadScene(GameScenes scene)
    {
        switch (scene)
        {
            case GameScenes.MainMenu:
                SceneManager.LoadScene("MainMenu");
                break;
            case GameScenes.Map:
                SceneManager.LoadScene("Map");
                break;
            case GameScenes.Dungeon_Entrance:
                SceneManager.LoadScene("Dungeon_Entrance");
                break;
            case GameScenes.Dungeon_Hall:
                SceneManager.LoadScene("Dungeon_Hall");
                break;
            case GameScenes.Dungeon_Main:
                SceneManager.LoadScene("Dungeon_Main");
                break;
            case GameScenes.Dungeon_Bossroom:
                SceneManager.LoadScene("Dungeon_BossRoom");
                break;
            case GameScenes.CharacterTest:
                SceneManager.LoadScene("CharacterTest");
                break;
            default:
                Debug.Log("Sahne Bulunamadý.");
                break;
        }
    }

    public enum GameScenes
    {
        MainMenu,
        Map,
        Dungeon_Entrance,
        Dungeon_Hall,
        Dungeon_Main,
        Dungeon_Bossroom,
        CharacterTest
    }
}
