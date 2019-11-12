using System;
using UnityEngine;

public class AnimatorBehaviour : StateMachineBehaviour
{
    float enterTime = 0.0f;
    public float NormalizedTime { get; private set; }
    public bool IsTransition { get; private set; }
    public Action EndCallBack { private get; set; } = () => { };

    public virtual void StateEnter(Animator animator,AnimatorStateInfo stateinfo,int layerIndex)
    {

    }
    public virtual void StateUpdate(Animator animator, AnimatorStateInfo stateinfo, int layerIndex)
    {

    }
    public virtual void StateExit(Animator animator, AnimatorStateInfo stateinfo, int layerIndex)
    {

    }
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        NormalizedTime = 0.0f;
        IsTransition = animator.IsInTransition(layerIndex);
        enterTime = Time.time;
        StateEnter(animator, stateInfo, layerIndex);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
            //アニメーションの再生時間が10秒(length)だとしてアプリケーションが始まってからの経過時間が22秒
            //アニメーションが再生された時間14秒
            //22-14=8
            //8/10=0.8
            //よって0~1までのNormalizeTime
            NormalizedTime = ((Time.time - enterTime) * stateInfo.speed) / stateInfo.length;
            IsTransition = animator.IsInTransition(layerIndex);
            StateUpdate(animator, stateInfo, layerIndex);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        StateExit(animator, stateInfo, layerIndex);
    }

    public void ResetTime()
    {
        enterTime = Time.time;
        NormalizedTime = 0.0f;
        EndCallBack();
    }

    // OnStateEnter is called before OnStateEnter is called on any state inside this state machine
    //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called before OnStateExit is called on any state inside this state machine
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called before OnStateMove is called on any state inside this state machine
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateIK is called before OnStateIK is called on any state inside this state machine
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMachineEnter is called when entering a state machine via its Entry Node
    //override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    //{
    //    
    //}

    // OnStateMachineExit is called when exiting a state machine via its Exit Node
    //override public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    //{
    //    
    //}
}
