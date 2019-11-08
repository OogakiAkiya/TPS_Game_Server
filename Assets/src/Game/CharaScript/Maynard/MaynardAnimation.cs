﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaynardAnimation : BaseAnimation
{
    private MaynardController maynardController;
    private bool jumpFlg=false;

    // Start is called before the first frame update
    void Start()
    {
        base.Init();
        animationState.ChangeState(ANIMATION_KEY.Idle);
        maynardController = this.GetComponent<MaynardController>();
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
                if (ExtractionKey(nowKey, 12) != 0)
                {
                    if (nowKey.HasFlag(KEY.W) && nowKey.HasFlag(KEY.S) || nowKey.HasFlag(KEY.A) && nowKey.HasFlag(KEY.D)) return;
                    animationState.ChangeState(ANIMATION_KEY.Walk);
                }
            });



        animationState.AddState(ANIMATION_KEY.Reloading,
            () =>
            {
                maynardController.weapon.state.ChangeState(WEAPONSTATE.RELOAD);
            },
            () =>
            {

                animationState.ChangeState(ANIMATION_KEY.Idle);
            },
            () =>
            {
                maynardController.weapon.state.ChangeState(WEAPONSTATE.WAIT);
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
        },
        () =>
        {
            maynardController.hp = 100;
            maynardController.transform.position = new Vector3(Random.Range(-rebornRange, rebornRange), 0, Random.Range(-rebornRange, rebornRange));
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
                if (animatorBehaviour.NormalizedTime >= 0.3f) Move(jumpMoveSpeed);

                //攻撃
                Atack();

                //
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
                Atack();
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


    private void Atack()
    {
        /*
        //グレネード投擲
        if (nowKey.HasFlag(KEY.G))
        {
            maynardController.ThrowGrenade();
        }

        //リロード
        if (nowKey.HasFlag(KEY.R))
        {
            animationState.ChangeState(ANIMATION_KEY.Reloading);
            return;
        }
        if (maynardController.weapon.state.currentKey == WEAPONSTATE.RELOAD && animationState.currentKey != ANIMATION_KEY.Reloading)
        {
            animationState.ChangeState(ANIMATION_KEY.Reloading);
            return;
        }

        //既に攻撃していた場合の処理
        if (maynardController.weapon.state.currentKey != WEAPONSTATE.WAIT) return;

        //攻撃
        if (nowKey.HasFlag(KEY.LEFT_CLICK)) maynardController.weapon.state.ChangeState(WEAPONSTATE.ATACK);
        */
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
