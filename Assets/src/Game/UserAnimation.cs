using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserAnimation : MonoBehaviour
{
    public Key nowKey = 0;
    public StateMachine<AnimationKey> animationState { get; private set; } = new StateMachine<AnimationKey>();
    private UserController userController;
    private Animator animator;
    private AnimatorBehaviour animatorBehaviour;

    public float jumpPower = 1.0f;
    public float jumpMoveSpeed = 1.0f;
    public float walkSpeed = 1.0f;
    public float runSpeed = 2.0f;
    public string checkLayer = "Ground";
    public float groundCheckRadius = 0.2f;
    public float rebornRange = 2.0f;
    public bool groundflg=true;
    private int layerNo=0;
    private bool jumpFlg = false;
    void Start()
    {
        
        userController = this.GetComponent<UserController>();
        animator = this.GetComponent<Animator>();
        animatorBehaviour = animator.GetBehaviour<AnimatorBehaviour>();
        
        AddStates();
        animationState.ChangeState(AnimationKey.Idle);

        // レイヤーの管理番号を取得
        layerNo = LayerMask.NameToLayer(checkLayer);

    }

    public void Update()
    {
        groundflg = true;
        if (!Physics.CheckSphere(this.transform.position - new Vector3(0, groundCheckRadius, 0), groundCheckRadius, 1 << layerNo)) groundflg = false;


        nowKey = userController.nowKey;
        animationState.Update();

    }

    void OnDrawGizmos()
    {
        Gizmos.DrawSphere(this.transform.position - new Vector3(0, groundCheckRadius, 0), groundCheckRadius);
    }
    //=================================================================
    //statesの情報設定
    //=================================================================
    private void AddStates()
    {
        //Idle
        animationState.AddState(AnimationKey.Idle,
             () =>
             {
                animator.CrossFadeInFixedTime("Idle", 0.0f);
             },

            _update: () =>
            {
                if (nowKey.HasFlag(Key.LEFT_BUTTON)) userController.Shoot();
                //ジャンプ
                if (InputTemplate(Key.SPACE, AnimationKey.JumpUP)) return;
                //歩き
                if (ExtractionKey(nowKey, 12) != 0)
                {
                    if (nowKey.HasFlag(Key.W) && nowKey.HasFlag(Key.S) || nowKey.HasFlag(Key.A) && nowKey.HasFlag(Key.D)) return;
                    animationState.ChangeState(AnimationKey.Walk);
                }
            });



        //JumpUP
        animationState.AddState(AnimationKey.JumpUP,
            () =>
            {
                animator.CrossFadeInFixedTime("JumpUP", 0.0f);
                jumpFlg = false;
            },
            () =>
            {
                if (nowKey.HasFlag(Key.LEFT_BUTTON)) userController.Shoot();
                if (animatorBehaviour.NormalizedTime >= 0.4f && !jumpFlg)
                {
                    this.GetComponent<Rigidbody>().AddForce(new Vector3(0, jumpPower * 100, 0));
                    jumpFlg = true;
                }
                if (animatorBehaviour.NormalizedTime >= 0.95f) animationState.ChangeState(AnimationKey.JumpStay);

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
                if (nowKey.HasFlag(Key.LEFT_BUTTON)) userController.Shoot();
                if (groundflg) animationState.ChangeState(AnimationKey.JumpDown);
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
                if (nowKey.HasFlag(Key.LEFT_BUTTON)) userController.Shoot();
                if (animatorBehaviour.NormalizedTime >= 0.95f)
                {
                    animationState.ChangeState(AnimationKey.Idle);
                }
            }
            );

        //Walk関係
        AddWalkState();

        //Run関係
        AddRunState();

        animationState.AddState(AnimationKey.Dying, () =>
        {
            animator.CrossFadeInFixedTime("Dying", 0.0f);
        },
        () =>
        {
            if (animatorBehaviour.NormalizedTime >= 0.95f)
            {
                animationState.ChangeState(AnimationKey.Idle);
            }
        },
        () =>
        {
            userController.hp = 100;
            userController.transform.position = new Vector3(Random.Range(-rebornRange, rebornRange), 0, Random.Range(-rebornRange, rebornRange));
        }
        );

    }

    private void AddWalkState()
    {
        //Walk
        animationState.AddState(AnimationKey.Walk,
            _update: () =>
            {
                //ジャンプ
                if (InputTemplate(Key.SPACE, AnimationKey.JumpUP)) return;

                //WASDのどれか一つでも押されているかチェック
                if (ExtractionKey(nowKey, 12) == 0)
                {
                    animationState.ChangeState(AnimationKey.Idle);
                    return;
                }
                //SHIFTが押されているかチェック
                if (InputTemplate(Key.SHIFT, AnimationKey.Run)) return;

                if (InputTemplate(Key.W, AnimationKey.WalkForward)) return;
                if (InputTemplate(Key.S, AnimationKey.WalkBack)) return;
                if (InputTemplate(Key.A, AnimationKey.WalkLeft)) return;
                if (InputTemplate(Key.D, AnimationKey.WalkRight)) return;
                Move(walkSpeed);
            });

        animationState.AddState(AnimationKey.WalkForward,
            _update: () =>
            {
                if (WalkInputTemplate()) return;
                if (InputTemplate(Key.W, Key.S, AnimationKey.Idle)) return;
                if (!nowKey.HasFlag(Key.W))
                {
                    if (InputTemplate(Key.A, AnimationKey.WalkLeft)) return;
                    if (InputTemplate(Key.D, AnimationKey.WalkRight)) return;
                }
                Move(walkSpeed);
            });
        animationState.AddState(AnimationKey.WalkBack,
            _update: () =>
            {
                if (WalkInputTemplate()) return;
                if (InputTemplate(Key.W, Key.S, AnimationKey.Idle)) return;
                if (!nowKey.HasFlag(Key.S))
                {
                    if (InputTemplate(Key.A, AnimationKey.WalkLeft)) return;
                    if (InputTemplate(Key.D, AnimationKey.WalkRight)) return;
                }

                Move(walkSpeed);
            });
        animationState.AddState(AnimationKey.WalkLeft,
            _update: () =>
            {
                if (WalkInputTemplate()) return;
                if (InputTemplate(Key.W, AnimationKey.WalkForward)) return;
                if (InputTemplate(Key.S, AnimationKey.WalkBack)) return;
                if (InputTemplate(Key.A, Key.D, AnimationKey.Idle)) return;
                Move(walkSpeed);
            });
        animationState.AddState(AnimationKey.WalkRight,
            _update: () =>
            {
                if (WalkInputTemplate()) return;
                if (InputTemplate(Key.W, AnimationKey.WalkForward)) return;
                if (InputTemplate(Key.S, AnimationKey.WalkBack)) return;
                if (InputTemplate(Key.A, Key.D, AnimationKey.Idle)) return;
                Move(walkSpeed);
            });
    }

    private void AddRunState()
    {
        //Run
        animationState.AddState(AnimationKey.Run,
            _update: () =>
            {
                //ジャンプ
                if (InputTemplate(Key.SPACE, AnimationKey.JumpUP)) return;
                //WASDのどれか一つでも押されているかチェック
                if (ExtractionKey(nowKey, 12) == 0)
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

                if (InputTemplate(Key.W, AnimationKey.RunForward)) return;
                if (InputTemplate(Key.S, AnimationKey.RunBack)) return;
                if (InputTemplate(Key.A, AnimationKey.RunLeft)) return;
                if (InputTemplate(Key.D, AnimationKey.RunRight)) return;
                Move(runSpeed);
            });

        animationState.AddState(AnimationKey.RunForward,
            _update: () =>
            {
                if (RunInputTemplate()) return;
                if (InputTemplate(Key.W, Key.S, AnimationKey.Idle)) return;
                if (!nowKey.HasFlag(Key.W))
                {
                    if (InputTemplate(Key.A, AnimationKey.RunLeft)) return;
                    if (InputTemplate(Key.D, AnimationKey.RunRight)) return;
                }

                Move(runSpeed);
            });
        animationState.AddState(AnimationKey.RunBack,
            _update: () =>
            {
                if (RunInputTemplate()) return;

                if (InputTemplate(Key.W, Key.S, AnimationKey.Idle)) return;
                if (!nowKey.HasFlag(Key.S))
                {
                    if (InputTemplate(Key.A, AnimationKey.RunLeft)) return;
                    if (InputTemplate(Key.D, AnimationKey.RunRight)) return;
                }
                Move(runSpeed);
            });
        animationState.AddState(AnimationKey.RunLeft,
            _update: () =>
            {
                if (RunInputTemplate()) return;
                if (InputTemplate(Key.W, AnimationKey.RunForward)) return;
                if (InputTemplate(Key.S, AnimationKey.RunBack)) return;
                if (InputTemplate(Key.A, Key.D, AnimationKey.Idle)) return;
                Move(runSpeed);
            });
        animationState.AddState(AnimationKey.RunRight,
            _update: () =>
            {
                if (RunInputTemplate()) return;
                if (InputTemplate(Key.W, AnimationKey.RunForward)) return;
                if (InputTemplate(Key.S, AnimationKey.RunBack)) return;
                if (InputTemplate(Key.A, Key.D, AnimationKey.Idle)) return;
                Move(runSpeed);
            });

    }



    private bool InputTemplate(Key _checkKey, AnimationKey _animationKey)
    {
        if (nowKey.HasFlag(_checkKey))
        {
            animationState.ChangeState(_animationKey);
            return true;
        }
        return false;
    }

    private bool InputTemplate(Key _checkKey, Key _checkKey2, AnimationKey _animationKey)
    {
        if (nowKey.HasFlag(_checkKey) && nowKey.HasFlag(_checkKey2))
        {
            animationState.ChangeState(_animationKey);
            return true;
        }
        return false;
    }

    private bool WalkInputTemplate()
    {
        if (nowKey.HasFlag(Key.LEFT_BUTTON)) userController.Shoot();
        //ジャンプ
        if (InputTemplate(Key.SPACE, AnimationKey.JumpUP)) return true;
        //WASDのどれか一つでも押されているかチェック
        if (ExtractionKey(nowKey, 12) == 0)
        {
            animationState.ChangeState(AnimationKey.Idle);
            return true;
        }

        //SHIFTが押されているかチェック
        if (nowKey.HasFlag(Key.SHIFT))
        {
            animationState.ChangeState(AnimationKey.Run);
            return true;
        }

        return false;
    }

    private bool RunInputTemplate()
    {
        if (nowKey.HasFlag(Key.LEFT_BUTTON)) userController.Shoot();
        //ジャンプ
        if (InputTemplate(Key.SPACE, AnimationKey.JumpUP)) return true;
        //WASDのどれか一つでも押されているかチェック
        if (ExtractionKey(nowKey, 12) == 0)
        {
            animationState.ChangeState(AnimationKey.Idle);
            return true;
        }

        //SHIFTが押されているかチェック
        if (!nowKey.HasFlag(Key.SHIFT))
        {
            animationState.ChangeState(AnimationKey.Walk);
            return true;
        }

        return false;
    }

    private short ExtractionKey(Key _key, int _shift)
    {
        return (short)((short)_key << _shift);
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
