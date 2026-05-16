using UnityEngine;

public class GateController : MonoBehaviour
{
    [Header("Kilit Ayarlarý")]
    public bool isLocked = true;

    [Tooltip("Bu kapýnýn açýlmasý için kaç farklý görevin (spawner/boss) bitmesi gerekiyor?")]
    public int requiredConditionsCount = 1;
    private int currentConditionsMet = 0;

    [Header("Kontrol Edilecek Objeler")]
    public Collider invisibleBlocker;      // Geçiþi engelleyen görünmez duvar

    [Header("Kapý Objeleri")]
    public GameObject door1; // Ýsim karmaþasýný önlemek için door1 yapýldý
    public GameObject door2;

    [Header("Görsel Efektler")]
    public GameObject[] activationEffects; // Açýlýnca çýkacak ýþýklar, partiküller

    void Start()
    {
        if (isLocked)
        {
            LockGate();
        }
    }

    private void LockGate()
    {
        // Kapýyý kitle, duvarý aç
        if (invisibleBlocker != null) invisibleBlocker.enabled = true;

        // Kapýlarý ayrý ayrý kontrol et ve kesin olarak KAPALI açýlarýna (0,0,0) sabitle
        // localRotation kullanýyoruz çünkü objenin kendi ekseninde dönmesini istiyoruz
        if (door1 != null) door1.transform.localRotation = Quaternion.Euler(0, 0, 0);
        if (door2 != null) door2.transform.localRotation = Quaternion.Euler(0, 0, 0);

        foreach (GameObject effect in activationEffects)
        {
            if (effect != null) effect.SetActive(false);
        }
    }

    // Spawner veya Boss öldüðünde bu metot tetiklenecek
    public void AddConditionMet()
    {
        if (!isLocked) return;

        currentConditionsMet++;
        Debug.Log($"{gameObject.name} için þart saðlandý: {currentConditionsMet}/{requiredConditionsCount}");

        if (currentConditionsMet >= requiredConditionsCount)
        {
            UnlockGate();
        }
    }

    private void UnlockGate()
    {
        isLocked = false;

        // Görünmez duvarý kaldýr, geçiþe izin ver
        if (invisibleBlocker != null) invisibleBlocker.enabled = false;

        // Kapýlarý ayrý ayrý kontrol et ve kesin olarak AÇIK açýlarýna sabitle
        if (door1 != null) door1.transform.localRotation = Quaternion.Euler(0, -113, 0);
        if (door2 != null) door2.transform.localRotation = Quaternion.Euler(0, 113, 0);

        // Büyülü ýþýklarý/partikülleri yak
        foreach (GameObject effect in activationEffects)
        {
            if (effect != null) effect.SetActive(true);
        }

        Debug.Log(gameObject.name + " kapýsý AÇILDI!");
    }
}