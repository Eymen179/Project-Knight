using UnityEngine;

public class BossComboTracking : StateMachineBehaviour
{
    [Header("Takip Ayarlarý")]
    [Tooltip("Boss'un oyuncuya dönme hýzý")]
    public float trackingSpeed = 10f;

    [Tooltip("Animasyonun % kaçýna kadar oyuncuya dönmeye devam etsin? (0.0 ile 1.0 arasý)")]
    public float trackingStopPercentage = 0.4f;

    private BossAgent bossAgent;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // BossAgent referansýný al
        if (bossAgent == null)
        {
            bossAgent = animator.GetComponent<BossAgent>();
        }
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Animasyonun yüzde kaçýnda olduðumuzu bul (Looplarý ekarte etmek için % 1 iþlemi yapýyoruz)
        float currentTime = stateInfo.normalizedTime % 1f;

        // Eðer animasyonun henüz baþlarýndaysak (Örn: %40'lýk kýlýcý kaldýrma kýsmý)
        if (currentTime < trackingStopPercentage)
        {
            if (bossAgent != null)
            {
                bossAgent.TrackPlayer(trackingSpeed);
            }
        }
    }
}