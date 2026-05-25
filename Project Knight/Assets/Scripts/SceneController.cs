using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;

    //Item Datalarini tutacak struct
    [System.Serializable]
    public struct ItemSaveData
    {
        public Item item;
        public int count;
        public int slotIndex;
    }

    //Kiliclari ve diger esyalari sahneler arasi tasimak icin olumsuz listeler
    [HideInInspector] public List<ItemSaveData> savedSwordSlots = new List<ItemSaveData>();
    [HideInInspector] public List<ItemSaveData> savedOtherSlots = new List<ItemSaveData>();

    //Toolbar'daki kilic slotu
    [HideInInspector] public ItemSaveData savedToolbarSword;
    [HideInInspector] public bool hasSavedToolbarSword = false;

    [Header("Spawn Settings")]
    public string targetSpawnPointID = "";

    [Header("Player Stats Save Data")]
    public int savedCurrentHealth = -1;
    public int savedMaxHealth = -1;

    [Header("Permanent Effect Save Data")]
    public bool hasSavedPermanentEffects = false;

    //UI degiskenleri (Kalici Efektler)
    public int savedPermanentHealthCount = 0;
    public int savedPermanentDamageCount = 0;
    public int savedPermanentSpeedCount = 0;

    //Kalici Efekt Degiskenleri
    public int savedPermanentBonusDamage = 0;
    public int savedPermanentBonusCritChance = 0;
    public float savedPermanentBonusCritMultiplier = 0f;
    public float savedPermanentBonusAttackSpeed = 0f;

    [Header("Difficulty Scaling")]
    public float globalDifficultyMultiplier = 1.0f;
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
    //Sahne Yukleme Metodu
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
            case GameScenes.Game_End:
                SceneManager.LoadScene("Game_End");
                break;
            default:
                Debug.Log("Sahne Bulunamadý.");
                break;
        }
    }

    //Ana Menuye Donunce Verileri Temizleme Metodu
    public void ResetAllSavedData()
    {
        //Envanter
        savedSwordSlots.Clear();
        savedOtherSlots.Clear();
        hasSavedToolbarSword = false;

        //Can ve Spawn Noktasi
        savedCurrentHealth = -1;
        savedMaxHealth = -1;
        targetSpawnPointID = "";

        //Kalici efektler
        hasSavedPermanentEffects = false;
        savedPermanentHealthCount = 0;
        savedPermanentDamageCount = 0;
        savedPermanentSpeedCount = 0;
        savedPermanentBonusDamage = 0;
        savedPermanentBonusCritChance = 0;
        savedPermanentBonusCritMultiplier = 0f;
        savedPermanentBonusAttackSpeed = 0f;

        Debug.Log("SceneController hafýzasý baþarýyla temizlendi.");
    }

    //Sahne enum degerleri
    public enum GameScenes
    {
        MainMenu,
        Map,
        Dungeon_Entrance,
        Dungeon_Hall,
        Dungeon_Main,
        Dungeon_Bossroom,
        CharacterTest,
        Game_End
    }
}
