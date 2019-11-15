using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseAnimation : MonoBehaviour
{
    public KEY nowKey = 0;
    public StateMachine<ANIMATION_KEY> animationState { get; private set; } = new StateMachine<ANIMATION_KEY>();
    private BaseController baseController;
    protected Animator animator;
    protected AnimatorBehaviour animatorBehaviour;

    [SerializeField] public float jumpPower = 1.0f;
    [SerializeField] public float jumpMoveSpeed = 1.0f;
    [SerializeField] public float walkSpeed = 1.0f;
    [SerializeField] public float runSpeed = 2.0f;
    [SerializeField] public string checkLayer = "Ground";
    [SerializeField] public float groundCheckRadius = 0.2f;
    [SerializeField] public float rebornRange = 2.0f;
    [SerializeField] public bool groundflg = true;
    protected int layerNo = 0;


    protected void Init()
    {
        baseController = this.GetComponent<BaseController>();
        animator = this.GetComponent<Animator>();
        animatorBehaviour = animator.GetBehaviour<AnimatorBehaviour>();

        AddStates();

        // レイヤーの管理番号を取得
        layerNo = LayerMask.NameToLayer(checkLayer);


    }

    protected void BaseUpdate()
    {
        if (!IsInvoking("GrandCheck")) Invoke("GrandCheck", 1f / 10);
        nowKey = baseController.nowKey;
        animationState.Update();
    }

    protected void GrandCheck()
    {
        groundflg = true;
        if (!Physics.CheckSphere(this.transform.position - new Vector3(0, groundCheckRadius / 3, 0), groundCheckRadius, 1 << layerNo)) groundflg = false;
    }

    protected short ExtractionKey(KEY _key, int _shift)
    {
        return (short)((short)_key << _shift);
    }


    protected virtual void Move(float _moveSpeed)
    {
        //移動量算出
        Vector3 velocity = Vector3.zero;
        if (nowKey.HasFlag(KEY.W)) velocity += this.transform.forward;
        if (nowKey.HasFlag(KEY.S)) velocity += -this.transform.forward;
        if (nowKey.HasFlag(KEY.A)) velocity += -this.transform.right;
        if (nowKey.HasFlag(KEY.D)) velocity += this.transform.right;

        //移動
        this.transform.position += velocity.normalized * _moveSpeed * Time.deltaTime;
    }

    protected void WeaponChange()
    {
        if (nowKey.HasFlag(KEY.LEFT_BUTTON)) baseController.ChangeWeapon(false);
        if (nowKey.HasFlag(KEY.RIGHT_BUTTON)) baseController.ChangeWeapon(true);
    }

    protected bool InputTemplate(KEY _checkKey, ANIMATION_KEY _animationKey)
    {
        if (nowKey.HasFlag(_checkKey))
        {
            animationState.ChangeState(_animationKey);
            return true;
        }
        return false;
    }

    protected bool InputTemplate(KEY _checkKey, KEY _checkKey2, ANIMATION_KEY _animationKey)
    {
        if (nowKey.HasFlag(_checkKey) && nowKey.HasFlag(_checkKey2))
        {
            animationState.ChangeState(_animationKey);
            return true;
        }
        return false;
    }


    protected virtual void AddStates() { }
    /*
    void OnDrawGizmos()
    {
        Gizmos.DrawSphere(this.transform.position - new Vector3(0, groundCheckRadius, 0), groundCheckRadius);
    }
    */
}
