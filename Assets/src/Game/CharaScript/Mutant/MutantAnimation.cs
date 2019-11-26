using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MutantAnimation : BaseAnimation
{
    private BaseController mutantController;
    private bool jumpFlg = false;

    // Start is called before the first frame update
    void Start()
    {
        base.Init();
        animationState.ChangeState(ANIMATION_KEY.Idle);
        mutantController = this.GetComponent<BaseController>();
    }

    // Update is called once per frame
    void Update()
    {
        base.BaseUpdate();
    }

    protected override void AddStates()
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
                if (!groundflg)
                {
                    animationState.ChangeState(ANIMATION_KEY.JumpStay);
                    return;
                }
                //ジャンプ
                if (InputTemplate(KEY.SPACE, ANIMATION_KEY.JumpUP)) return;
                //歩き
                if (nowKey.HasFlag(KEY.W)) animationState.ChangeState(ANIMATION_KEY.Walk);
            });

        animationState.AddState(ANIMATION_KEY.Attack,
            () =>
            {
                mutantController.current.weapon.state.ChangeState(WEAPONSTATE.ATACK);
                animator.CrossFadeInFixedTime("Attack", 0.1f);
            },
            () =>
            {
                if (animatorBehaviour.NormalizedTime >= 0.9f)
                {
                    animationState.ChangeState(ANIMATION_KEY.Idle);
                }
            },
            () =>
            {
                mutantController.current.weapon.state.ChangeState(WEAPONSTATE.WAIT);
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
            mutantController.hp = 100;
            mutantController.transform.position = new Vector3(Random.Range(-rebornRange, rebornRange), 0, Random.Range(-rebornRange, rebornRange));
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
                if (!nowKey.HasFlag(KEY.W))
                {
                    animationState.ChangeState(ANIMATION_KEY.Idle);
                    return;
                }
                //SHIFTが押されているかチェック
                if (InputTemplate(KEY.SHIFT, ANIMATION_KEY.Run)) return;
                if (InputTemplate(KEY.W, ANIMATION_KEY.WalkForward)) return;
                Move(walkSpeed);
            });

        animationState.AddState(ANIMATION_KEY.WalkForward,
            _update: () =>
            {
                if (WalkInputTemplate()) return;
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
                if (!nowKey.HasFlag(KEY.W))
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
                Move(runSpeed);
            });

        animationState.AddState(ANIMATION_KEY.RunForward,
            _update: () =>
            {
                if (RunInputTemplate()) return;
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
                //移動
                if (animatorBehaviour.NormalizedTime >= 0.6f) Move(jumpMoveSpeed);

                //
                if (animatorBehaviour.NormalizedTime >= 0.6f && !jumpFlg)
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
                Move(jumpMoveSpeed);
                if (groundflg) animationState.ChangeState(ANIMATION_KEY.JumpDown);
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
                if (animatorBehaviour.NormalizedTime < 0.3f) Move(jumpMoveSpeed);
                if (animatorBehaviour.NormalizedTime >= 0.95f) animationState.ChangeState(ANIMATION_KEY.Idle);
            }
            );

    }

    private bool WalkInputTemplate()
    {

        Atack();
        //ジャンプ
        if (InputTemplate(KEY.SPACE, ANIMATION_KEY.JumpUP)) return true;

        //WASDのどれか一つでも押されているかチェック
        if (!nowKey.HasFlag(KEY.W))
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
        if (!nowKey.HasFlag(KEY.W))
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


    private void Atack()
    {
        //既に攻撃していた場合の処理
        if (mutantController.current.weapon.state.currentKey != WEAPONSTATE.WAIT) return;
        //攻撃
        if (nowKey.HasFlag(KEY.LEFT_CLICK))
        {
            animationState.ChangeState(ANIMATION_KEY.Attack);
        }

    }

    protected override void Move(float _moveSpeed)
    {
        //移動量算出
        Vector3 velocity = Vector3.zero;
        if (nowKey.HasFlag(KEY.W)) velocity += this.transform.forward;

        //移動
        this.transform.position += velocity.normalized * _moveSpeed * Time.deltaTime;

    }
}
