﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierAnimation : BaseAnimation
{
    private bool jumpFlg = false;

    //コライダー
    protected CapsuleCollider collider;
    protected Vector3 center;

    // Start is called before the first frame update
    public void Start()
    {
        base.Init();

        collider = this.GetComponent<CapsuleCollider>();
        animationState.ChangeState(ANIMATION_KEY.Idle);
        baseController = this.GetComponent<BaseController>();
    }

    // Update is called once per frame
    public void Update()
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
                if (baseController.current.weapon.type == WEAPONTYPE.MACHINEGUN) animator.CrossFadeInFixedTime("Reloading", 0.0f);
                if (baseController.current.weapon.type == WEAPONTYPE.HANDGUN) animator.CrossFadeInFixedTime("Pistol Reloadad", 0.0f);
                baseController.current.weapon.state.ChangeState(WEAPONSTATE.RELOAD);
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
                baseController.current.weapon.state.ChangeState(WEAPONSTATE.WAIT);
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
            if (collider) center = collider.center;
        },
        () =>
        {
            if (!collider) return;
            if (animatorBehaviour.NormalizedTime <= 0.1f && animatorBehaviour.NormalizedTime >= 0.0f)
            {
                if (collider.center.y < 0.89) collider.center += new Vector3(0, 0.001f, 0);
            }

            if (animatorBehaviour.NormalizedTime >= 0.4f)
            {
                if (collider.center.y < 1.7) collider.center += new Vector3(0, 0.003f, 0);
            }

            if (animatorBehaviour.NormalizedTime >= 0.95f)
            {
                collider.center = new Vector3(0, 0.85f, 0.1f);
                animationState.ChangeState(ANIMATION_KEY.Idle);
            }
        },
        () =>
        {

            if (collider) collider.center = center;
            baseController.hp = 100;
            baseController.transform.position = new Vector3(Random.Range(-rebornRange, rebornRange), 0, Random.Range(-rebornRange, rebornRange));
            baseController.End();
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
                //移動
                if (animatorBehaviour.NormalizedTime >= 0.3f) Move(jumpMoveSpeed);

                //攻撃
                Atack();

                //
                if (animatorBehaviour.NormalizedTime >= 0.4f && !jumpFlg)
                {
                    //this.GetComponent<Rigidbody>().AddForce(new Vector3(0, jumpPower * 100, 0));
                    this.GetComponent<Rigidbody>().AddForce(this.transform.up*jumpPower*100);

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
        //グレネード投擲
        if (nowKey.HasFlag(KEY.G))
        {
            SoldierComponent component = (SoldierComponent)baseController.current;
            component.ThrowGrenade();
        }

        //リロード
        if (nowKey.HasFlag(KEY.R))
        {
            animationState.ChangeState(ANIMATION_KEY.Reloading);
            return;
        }
        if (baseController.current.weapon.state.currentKey == WEAPONSTATE.RELOAD && animationState.currentKey != ANIMATION_KEY.Reloading)
        {
            animationState.ChangeState(ANIMATION_KEY.Reloading);
            return;
        }

        //既に攻撃していた場合の処理
        if (baseController.current.weapon.state.currentKey != WEAPONSTATE.WAIT) return;

        //攻撃
        if (nowKey.HasFlag(KEY.LEFT_CLICK)) baseController.current.weapon.state.ChangeState(WEAPONSTATE.ATACK);

    }

}
