using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserAnimation : MonoBehaviour
{
    public KEY nowKey = 0;
    public StateMachine<ANIMATION_KEY> animationState { get; private set; } = new StateMachine<ANIMATION_KEY>();
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
    public bool groundflg = true;
    private int layerNo = 0;
    private bool jumpFlg = false;

    void Start()
    {

        userController = this.GetComponent<UserController>();
        animator = this.GetComponent<Animator>();
        animatorBehaviour = animator.GetBehaviour<AnimatorBehaviour>();

        AddStates();
        animationState.ChangeState(ANIMATION_KEY.Idle);

        // レイヤーの管理番号を取得
        layerNo = LayerMask.NameToLayer(checkLayer);


    }

    public void Update()
    {

        groundflg = true;
        if (!Physics.CheckSphere(this.transform.position - new Vector3(0, groundCheckRadius/3, 0), groundCheckRadius, 1 << layerNo)) groundflg = false;


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
        animationState.AddState(ANIMATION_KEY.Idle,
             () =>
             {
                 animator.CrossFadeInFixedTime("Idle", 0.0f);
             },

            _update: () =>
            {
                WeaponChange();
                Atack();
                //落下
                if(!groundflg) animationState.ChangeState(ANIMATION_KEY.JumpStay);
                //ジャンプ
                if (InputTemplate(KEY.SPACE, ANIMATION_KEY.JumpUP)) return;
                //歩き
                if (ExtractionKey(nowKey, 12) != 0)
                {
                    if (nowKey.HasFlag(KEY.W) && nowKey.HasFlag(KEY.S) || nowKey.HasFlag(KEY.A) && nowKey.HasFlag(KEY.D)) return;
                    animationState.ChangeState(ANIMATION_KEY.Walk);
                }
            });



        animationState.AddState(ANIMATION_KEY.Reloading,
            () =>
            {
                animator.CrossFadeInFixedTime("Reloading", 0.0f);
                userController.weapon.state.ChangeState(WEAPONSTATE.RELOAD);
            },
            () =>
            {
                if (animatorBehaviour.NormalizedTime >= 0.95f)
                {
                    animationState.ChangeState(ANIMATION_KEY.Idle);
                }
            },
            () =>
            {
                userController.weapon.state.ChangeState(WEAPONSTATE.WAIT);
            }
            );

        //Walk関係
        AddWalkState();

        //Run関係
        AddRunState();

        //Jump関係
        AddJump();

        animationState.AddState(ANIMATION_KEY.Dying, () =>
        {
            animator.CrossFadeInFixedTime("Dying", 0.0f);
        },
        () =>
        {
            if (animatorBehaviour.NormalizedTime >= 0.95f)
            {
                animationState.ChangeState(ANIMATION_KEY.Idle);
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
        animationState.AddState(ANIMATION_KEY.Walk,
            _update: () =>
            {
                //ジャンプ
                if (InputTemplate(KEY.SPACE, ANIMATION_KEY.JumpUP)) return;

                //WASDのどれか一つでも押されているかチェック
                if (ExtractionKey(nowKey, 12) == 0)
                {
                    animationState.ChangeState(ANIMATION_KEY.Idle);
                    return;
                }
                //SHIFTが押されているかチェック
                if (InputTemplate(KEY.SHIFT, ANIMATION_KEY.Run)) return;

                if (InputTemplate(KEY.W, ANIMATION_KEY.WalkForward)) return;
                if (InputTemplate(KEY.S, ANIMATION_KEY.WalkBack)) return;
                if (InputTemplate(KEY.A, ANIMATION_KEY.WalkLeft)) return;
                if (InputTemplate(KEY.D, ANIMATION_KEY.WalkRight)) return;
                Move(walkSpeed);
            });

        animationState.AddState(ANIMATION_KEY.WalkForward,
            _update: () =>
            {
                if (WalkInputTemplate()) return;
                if (InputTemplate(KEY.W, KEY.S, ANIMATION_KEY.Idle)) return;
                if (!nowKey.HasFlag(KEY.W))
                {
                    if (InputTemplate(KEY.A, ANIMATION_KEY.WalkLeft)) return;
                    if (InputTemplate(KEY.D, ANIMATION_KEY.WalkRight)) return;
                }
                Move(walkSpeed);
            });
        animationState.AddState(ANIMATION_KEY.WalkBack,
            _update: () =>
            {
                if (WalkInputTemplate()) return;
                if (InputTemplate(KEY.W, KEY.S, ANIMATION_KEY.Idle)) return;
                if (!nowKey.HasFlag(KEY.S))
                {
                    if (InputTemplate(KEY.A, ANIMATION_KEY.WalkLeft)) return;
                    if (InputTemplate(KEY.D, ANIMATION_KEY.WalkRight)) return;
                }

                Move(walkSpeed);
            });
        animationState.AddState(ANIMATION_KEY.WalkLeft,
            _update: () =>
            {
                if (WalkInputTemplate()) return;
                if (InputTemplate(KEY.W, ANIMATION_KEY.WalkForward)) return;
                if (InputTemplate(KEY.S, ANIMATION_KEY.WalkBack)) return;
                if (InputTemplate(KEY.A, KEY.D, ANIMATION_KEY.Idle)) return;
                Move(walkSpeed);
            });
        animationState.AddState(ANIMATION_KEY.WalkRight,
            _update: () =>
            {
                if (WalkInputTemplate()) return;
                if (InputTemplate(KEY.W, ANIMATION_KEY.WalkForward)) return;
                if (InputTemplate(KEY.S, ANIMATION_KEY.WalkBack)) return;
                if (InputTemplate(KEY.A, KEY.D, ANIMATION_KEY.Idle)) return;
                Move(walkSpeed);
            });
    }

    private void AddRunState()
    {
        //Run
        animationState.AddState(ANIMATION_KEY.Run,
            _update: () =>
            {
                //ジャンプ
                if (InputTemplate(KEY.SPACE, ANIMATION_KEY.JumpUP)) return;
                //WASDのどれか一つでも押されているかチェック
                if (ExtractionKey(nowKey, 12) == 0)
                {
                    animationState.ChangeState(ANIMATION_KEY.Idle);
                    return;
                }

                //SHIFTが押されているかチェック
                if (!nowKey.HasFlag(KEY.SHIFT))
                {
                    animationState.ChangeState(ANIMATION_KEY.Walk);
                    return;
                }

                if (InputTemplate(KEY.W, ANIMATION_KEY.RunForward)) return;
                if (InputTemplate(KEY.S, ANIMATION_KEY.RunBack)) return;
                if (InputTemplate(KEY.A, ANIMATION_KEY.RunLeft)) return;
                if (InputTemplate(KEY.D, ANIMATION_KEY.RunRight)) return;
                Move(runSpeed);
            });

        animationState.AddState(ANIMATION_KEY.RunForward,
            _update: () =>
            {
                if (RunInputTemplate()) return;
                if (InputTemplate(KEY.W, KEY.S, ANIMATION_KEY.Idle)) return;
                if (!nowKey.HasFlag(KEY.W))
                {
                    if (InputTemplate(KEY.A, ANIMATION_KEY.RunLeft)) return;
                    if (InputTemplate(KEY.D, ANIMATION_KEY.RunRight)) return;
                }

                Move(runSpeed);
            });
        animationState.AddState(ANIMATION_KEY.RunBack,
            _update: () =>
            {
                if (RunInputTemplate()) return;

                if (InputTemplate(KEY.W, KEY.S, ANIMATION_KEY.Idle)) return;
                if (!nowKey.HasFlag(KEY.S))
                {
                    if (InputTemplate(KEY.A, ANIMATION_KEY.RunLeft)) return;
                    if (InputTemplate(KEY.D, ANIMATION_KEY.RunRight)) return;
                }
                Move(runSpeed);
            });
        animationState.AddState(ANIMATION_KEY.RunLeft,
            _update: () =>
            {
                if (RunInputTemplate()) return;
                if (InputTemplate(KEY.W, ANIMATION_KEY.RunForward)) return;
                if (InputTemplate(KEY.S, ANIMATION_KEY.RunBack)) return;
                if (InputTemplate(KEY.A, KEY.D, ANIMATION_KEY.Idle)) return;
                Move(runSpeed);
            });
        animationState.AddState(ANIMATION_KEY.RunRight,
            _update: () =>
            {
                if (RunInputTemplate()) return;
                if (InputTemplate(KEY.W, ANIMATION_KEY.RunForward)) return;
                if (InputTemplate(KEY.S, ANIMATION_KEY.RunBack)) return;
                if (InputTemplate(KEY.A, KEY.D, ANIMATION_KEY.Idle)) return;
                Move(runSpeed);
            });

    }

    private void AddJump()
    {
        //JumpUP
        animationState.AddState(ANIMATION_KEY.JumpUP,
            () =>
            {
                animator.CrossFadeInFixedTime("JumpUP", 0.0f);
                jumpFlg = false;
            },
            () =>
            {
                Atack();
                if (animatorBehaviour.NormalizedTime >= 0.4f && !jumpFlg)
                {
                    this.GetComponent<Rigidbody>().AddForce(new Vector3(0, jumpPower * 100, 0));
                    jumpFlg = true;
                }
                if (animatorBehaviour.NormalizedTime >= 0.95f) animationState.ChangeState(ANIMATION_KEY.JumpStay);

            }
            );

        //JumpStay
        animationState.AddState(ANIMATION_KEY.JumpStay,
            () =>
            {
                animator.CrossFadeInFixedTime("JumpStay", 0.0f);
            },
            () =>
            {
                Atack();
                if (groundflg) animationState.ChangeState(ANIMATION_KEY.JumpDown);
                Move(jumpMoveSpeed);
            }
            );


        //JumpDown
        animationState.AddState(ANIMATION_KEY.JumpDown,
            () =>
            {
                animator.CrossFadeInFixedTime("JumpDown", 0.0f);
            },
            () =>
            {
                Atack();
                if (animatorBehaviour.NormalizedTime >= 0.95f)
                {
                    animationState.ChangeState(ANIMATION_KEY.Idle);
                }
            }
            );

    }



    private bool InputTemplate(KEY _checkKey, ANIMATION_KEY _animationKey)
    {
        if (nowKey.HasFlag(_checkKey))
        {
            animationState.ChangeState(_animationKey);
            return true;
        }
        return false;
    }

    private bool InputTemplate(KEY _checkKey, KEY _checkKey2, ANIMATION_KEY _animationKey)
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
        Atack();
        //ジャンプ
        if (InputTemplate(KEY.SPACE, ANIMATION_KEY.JumpUP)) return true;
        //WASDのどれか一つでも押されているかチェック
        if (ExtractionKey(nowKey, 12) == 0)
        {
            animationState.ChangeState(ANIMATION_KEY.Idle);
            return true;
        }

        //SHIFTが押されているかチェック
        if (nowKey.HasFlag(KEY.SHIFT))
        {
            animationState.ChangeState(ANIMATION_KEY.Run);
            return true;
        }

        return false;
    }

    private bool RunInputTemplate()
    {
        Atack();
        //ジャンプ
        if (InputTemplate(KEY.SPACE, ANIMATION_KEY.JumpUP)) return true;
        //WASDのどれか一つでも押されているかチェック
        if (ExtractionKey(nowKey, 12) == 0)
        {
            animationState.ChangeState(ANIMATION_KEY.Idle);
            return true;
        }

        //SHIFTが押されているかチェック
        if (!nowKey.HasFlag(KEY.SHIFT))
        {
            animationState.ChangeState(ANIMATION_KEY.Walk);
            return true;
        }

        return false;
    }

    private short ExtractionKey(KEY _key, int _shift)
    {
        return (short)((short)_key << _shift);
    }


    private void Move(float _moveSpeed)
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

    private void WeaponChange()
    {
        if (nowKey.HasFlag(KEY.LEFT_BUTTON)) userController.ChangeWeapon(false);
        if (nowKey.HasFlag(KEY.RIGHT_BUTTON)) userController.ChangeWeapon(true);
    }

    private void Atack()
    {
        //グレネード投擲
        if (nowKey.HasFlag(KEY.G))
        {
            userController.ThrowGrenade();
        }

        //リロード
        if (nowKey.HasFlag(KEY.R))
        {
            animationState.ChangeState(ANIMATION_KEY.Reloading);
            return;
        }
        if (userController.weapon.state.currentKey == WEAPONSTATE.RELOAD && animationState.currentKey != ANIMATION_KEY.Reloading)
        {
            animationState.ChangeState(ANIMATION_KEY.Reloading);
            return;
        }

        //既に攻撃していた場合の処理
        if (userController.weapon.state.currentKey != WEAPONSTATE.WAIT) return;

        //攻撃
        if (nowKey.HasFlag(KEY.LEFT_CLICK))userController.weapon.state.ChangeState(WEAPONSTATE.ATACK);

    }
}
