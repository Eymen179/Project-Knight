using UnityEngine;

public class ComboManagerBehaviour : StateMachineBehaviour
{
    [SerializeField] private int comboStepValue;

    public int comboIndex;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Animator'deki "comboStep" parametresini bizim belirledigimiz degere ayarla.
        animator.SetInteger("comboStep", comboStepValue);

        animator.ResetTrigger("attack");

        comboIndex = comboStepValue;
    }
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Animasyon reset
        animator.ResetTrigger("attack");
    }
}