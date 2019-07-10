using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UserController : MonoBehaviour
{
    public string userId;
    public float jumpPower=1.0f;
    public float jumpMoveSpeed = 0.3f;
    public float walkSpeed = 1.0f;
    public float runSpeed = 2.0f;

    public string IPaddr { get; set; }
    private List<byte[]> recvDataList = new List<byte[]>();
    private List<Key> inputKeyList = new List<Key>();
    private Key nowKey=0;
    public StateMachine<AnimationKey> animationState { get; private set; } = new StateMachine<AnimationKey>();
    private Animator animator;
    private AnimatorBehaviour animatorBehaviour;


    public int hp { get; set; } = 100;

    void Start()
    {
        animator = this.GetComponent<Animator>();
        animatorBehaviour = animator.GetBehaviour<AnimatorBehaviour>();
        AddStates();
        animationState.ChangeState(AnimationKey.Idle);

    }

    public void SetUserID(string _userId)
    {
        userId = _userId;
        this.name = _userId;

    }

    // Update is called once per frame
    void Update()
    {

        if (recvDataList.Count > 0)
        {
            byte[] recvData = GetRecvData();
        }

        InputRoutine();
        animationState.Update();
    }


    public void AddRecvData(byte[] _addData)
    {
        recvDataList.Add(_addData);
    }

    public void AddInputKeyList(Key _addData)
    {
        inputKeyList.Add(_addData);
    }

    public Key GetInputKey()
    {
        Key returnData;
        returnData = inputKeyList[0];
        inputKeyList.RemoveAt(0);
        return returnData;
    }

    private byte[] GetRecvData()
    {
        byte[] returnData;
        returnData = recvDataList[0];
        recvDataList.RemoveAt(0);
        return returnData;
    }

    private void InputRoutine()
    {
        while (inputKeyList.Count > 0)
        {
            //入力値取得
            Key inputKey = GetInputKey();

            //現在のキーを保存
            Key oldKey = nowKey;

            //新しいキー入力を加算
            nowKey |= inputKey;

            //二度目のキー入力でフラグOFF
            nowKey = oldKey ^ inputKey;
        }
    }


    //=================================================================
    //statesの情報設定
    //=================================================================
    private void AddStates()
    {
        //Idle
        animationState.AddState(AnimationKey.Idle,
            _update: () =>
            {
                //ジャンプ
                if (nowKey.HasFlag(Key.SPACE))
                {
                    animationState.ChangeState(AnimationKey.JumpUP);
                    return;
                }
                //歩き
                if ((short)nowKey << 12 > 0)
                {
                    animationState.ChangeState(AnimationKey.Walk);
                }
            });

        //Walk
        animationState.AddState(AnimationKey.Walk,
            _update: () =>
            {
                //ジャンプ
                if (nowKey.HasFlag(Key.SPACE))
                {
                    animationState.ChangeState(AnimationKey.JumpUP);
                    return;
                }
                //WASDのどれか一つでも押されているかチェック
                if ((short)nowKey << 12 == 0)
                {
                    animationState.ChangeState(AnimationKey.Idle);
                    return;
                }

                //SHIFTが押されているかチェック
                if (nowKey.HasFlag(Key.SHIFT))
                {
                    animationState.ChangeState(AnimationKey.Run);
                    return;
                }

                Move(walkSpeed);
            });

        //Run
        animationState.AddState(AnimationKey.Run,
            _update: () =>
            {
                //ジャンプ
                if (nowKey.HasFlag(Key.SPACE))
                {
                    animationState.ChangeState(AnimationKey.JumpUP);
                    return;
                }
                //WASDのどれか一つでも押されているかチェック
                if ((short)nowKey << 12 == 0)
                {
                    animationState.ChangeState(AnimationKey.Idle);
                    return;
                }

                //SHIFTが押されているかチェック
                if (!nowKey.HasFlag(Key.SHIFT))
                {
                    animationState.ChangeState(AnimationKey.Walk);
                    return;
                }                

                Move(runSpeed);
            });
        
        //JumpUP
        animationState.AddState(AnimationKey.JumpUP,
            () =>
            {
               animator.CrossFadeInFixedTime("JumpUP", 0.0f);
            },
            () =>
            {
                if (animatorBehaviour.NormalizedTime >= 1.0f) animationState.ChangeState(AnimationKey.JumpStay);
                Move(jumpMoveSpeed);
            }
            );

        //JumpStay
        animationState.AddState(AnimationKey.JumpStay,
            () =>
            {
                animator.CrossFadeInFixedTime("JumpStay", 0.0f);
            },
            () =>
            {
                if (animatorBehaviour.NormalizedTime >= 1.0f) animationState.ChangeState(AnimationKey.JumpDown);
                Move(jumpMoveSpeed);
            }
            );


        //JumpDown
        animationState.AddState(AnimationKey.JumpDown,
            () =>
            {
                animator.CrossFadeInFixedTime("JumpDown", 0.0f);
            },
            () =>
            {
                if (animatorBehaviour.NormalizedTime >= 1.0f)
                {
                    animationState.ChangeState(AnimationKey.Idle);
                }
            }
            );
            

    }

    private void Move(float _moveSpeed)
    {
        //移動量算出
        Vector3 velocity = Vector3.zero;
        if (nowKey.HasFlag(Key.W)) velocity += this.transform.forward;
        if (nowKey.HasFlag(Key.S)) velocity += -this.transform.forward;
        if (nowKey.HasFlag(Key.A)) velocity += -this.transform.right;
        if (nowKey.HasFlag(Key.D)) velocity += this.transform.right;

        //移動
        this.transform.position += velocity.normalized * _moveSpeed * Time.deltaTime;
    }
}
