using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserAnimation : MonoBehaviour
{
    public KEY nowKey = 0;
    public StateMachine<ANIMATION_KEY> animationState { get; private set; } = new StateMachine<ANIMATION_KEY>();
    private UserController userController;
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
        userController = this.GetComponent<UserController>();
        animator = this.GetComponent<Animator>();
        animatorBehaviour = animator.GetBehaviour<AnimatorBehaviour>();

        AddStates();

        // レイヤーの管理番号を取得
        layerNo = LayerMask.NameToLayer(checkLayer);


    }

    protected void BaseUpdate()
    {
        if (!IsInvoking("GrandCheck")) Invoke("GrandCheck", 1f / 10);
        nowKey = userController.nowKey;
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


    protected void Move(float _moveSpeed)
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
        if (nowKey.HasFlag(KEY.LEFT_BUTTON)) userController.ChangeWeapon(false);
        if (nowKey.HasFlag(KEY.RIGHT_BUTTON)) userController.ChangeWeapon(true);
    }

    protected virtual void AddStates() { }
    /*
    void OnDrawGizmos()
    {
        Gizmos.DrawSphere(this.transform.position - new Vector3(0, groundCheckRadius, 0), groundCheckRadius);
    }
    */
    //=================================================================
    //statesの情報設定
    //=================================================================
}
