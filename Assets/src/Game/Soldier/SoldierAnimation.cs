﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierAnimation : UserAnimation
{
    private SoldierController soldierController;
    private bool jumpFlg = false;

    //コライダー
    protected CapsuleCollider collider = null;
    protected Vector3 center;

    // Start is called before the first frame update
    public void Start()
    {
        base.Init();

        collider = this.GetComponent<CapsuleCollider>();
        animationState.ChangeState(ANIMATION_KEY.Idle);
        soldierController = this.GetComponent<SoldierController>();
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
                if (soldierController.weapon.type == WEAPONTYPE.MACHINEGUN) animator.CrossFadeInFixedTime("Reloading", 0.0f);
                if (soldierController.weapon.type == WEAPONTYPE.HANDGUN) animator.CrossFadeInFixedTime("Pistol Reloadad", 0.0f);
                soldierController.weapon.state.ChangeState(WEAPONSTATE.RELOAD);
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
                soldierController.weapon.state.ChangeState(WEAPONSTATE.WAIT);
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
            soldierController.hp = 100;
            soldierController.transform.position = new Vector3(Random.Range(-rebornRange, rebornRange), 0, Random.Range(-rebornRange, rebornRange));
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


    private void Atack()
    {
        //グレネード投擲
        if (nowKey.HasFlag(KEY.G))
        {
            soldierController.ThrowGrenade();
        }

        //リロード
        if (nowKey.HasFlag(KEY.R))
        {
            animationState.ChangeState(ANIMATION_KEY.Reloading);
            return;
        }
        if (soldierController.weapon.state.currentKey == WEAPONSTATE.RELOAD && animationState.currentKey != ANIMATION_KEY.Reloading)
        {
            animationState.ChangeState(ANIMATION_KEY.Reloading);
            return;
        }

        //既に攻撃していた場合の処理
        if (soldierController.weapon.state.currentKey != WEAPONSTATE.WAIT) return;

        //攻撃
        if (nowKey.HasFlag(KEY.LEFT_CLICK)) soldierController.weapon.state.ChangeState(WEAPONSTATE.ATACK);

    }

}
