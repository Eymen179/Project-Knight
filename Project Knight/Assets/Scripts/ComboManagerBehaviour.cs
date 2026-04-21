using UnityEngine;

// MonoBehaviour DEÐÝL, StateMachineBehaviour
public class ComboManagerBehaviour : StateMachineBehaviour
{
    [SerializeField] private int comboStepValue;

    public int comboIndex;

    // Bu durum (state) girildiðinde çalýþýr
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Animator'deki "comboStep" parametresini bizim belirlediðimiz deðere ayarla
        animator.SetInteger("comboStep", comboStepValue);

        animator.ResetTrigger("attack");

        comboIndex = comboStepValue;
    }
    // YENÝ EKLENDÝ: Bu durumdan çýkarken çalýþýr
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Çýkarken hafýzada kalan sahte bir týklama varsa onu yok et.
        // Böylece kombo bitip Idle'a dönünce anlamsýzca tekrar saldýrmaz.
        animator.ResetTrigger("attack");
    }
}