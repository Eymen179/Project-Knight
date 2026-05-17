using UnityEngine;

public class GateController : MonoBehaviour
{
    [Header("Lock Settings")]
    public bool isLocked = true;

    [Tooltip("Bu kapýnýn açýlmasý için kaç farklý görevin (spawner/boss) bitmesi gerekiyor?")]
    public int requiredConditionsCount = 1;
    private int currentConditionsMet = 0;

    [Header("Invisible Blocker")]
    public Collider invisibleBlocker;

    [Header("Door Objects")]
    public GameObject door1;
    public GameObject door2;

    [Header("Visual Effects")]
    public GameObject[] activationEffects;

    void Start()
    {
        if (isLocked)
        {
            LockGate();
        }
    }

    private void LockGate()//Isinlanma Noktasini Kapama Metodu
    {
        //Gorunmez duvari aktif et.
        if (invisibleBlocker != null) invisibleBlocker.enabled = true;

        //Kapilari Kapat.
        if (door1 != null) door1.transform.localRotation = Quaternion.Euler(0, 0, 0);
        if (door2 != null) door2.transform.localRotation = Quaternion.Euler(0, 0, 0);

        foreach (GameObject effect in activationEffects)
        {
            if (effect != null) effect.SetActive(false);
        }
    }

    //Unity Event Metodu: Sarta gore metot cagirilacak.
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

    private void UnlockGate()// Isinlanma Noktasini Acma Metodu
    {
        isLocked = false;

        //Gorunmez duvari kapat.
        if (invisibleBlocker != null) invisibleBlocker.enabled = false;

        //Kapilari ac.
        if (door1 != null) door1.transform.localRotation = Quaternion.Euler(0, -113, 0);
        if (door2 != null) door2.transform.localRotation = Quaternion.Euler(0, 113, 0);

        //Efektleri aktif et.
        foreach (GameObject effect in activationEffects)
        {
            if (effect != null) effect.SetActive(true);
        }

        Debug.Log(gameObject.name + " kapýsý AÇILDI!");
    }
}