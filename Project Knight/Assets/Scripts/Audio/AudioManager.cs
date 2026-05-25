using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource audioSource;
    [SerializeField] private List<AudioClip> audioSounds; // Inspector'dan atamak için

    // Optimizasyon için sesleri hafýzada tutacaðýmýz Sözlük (Dictionary)
    private Dictionary<string, AudioClip> audioDictionary;

    public float volume = 1f;

    /*
     ButtonTickSound
     ChestOpenSound
     LootPickupSound
     
     */

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDictionary(); // Oyun baþlarken listeyi sözlüðe çevir
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeDictionary()
    {
        audioDictionary = new Dictionary<string, AudioClip>();
        foreach (AudioClip clip in audioSounds)
        {
            if (!audioDictionary.ContainsKey(clip.name))
            {
                audioDictionary.Add(clip.name, clip);
            }
        }
    }
    //Kod bazli
    public void PlayClip(string audioName)
    {
        // Döngü yok! Sözlükte o isimde ses varsa anýnda bul ve çal
        if (audioDictionary.TryGetValue(audioName, out AudioClip clip))
        {
            audioSource.PlayOneShot(clip, volume);
        }
        else
        {
            Debug.LogWarning("Ses bulunamadý: " + audioName);
        }
    }
}