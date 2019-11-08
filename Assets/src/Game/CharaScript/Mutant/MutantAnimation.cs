using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MutantAnimation : BaseAnimation
{
    private MutantController mutantController;
    // Start is called before the first frame update
    void Start()
    {
        base.Init();
        animationState.ChangeState(ANIMATION_KEY.Idle);
        mutantController = this.GetComponent<MutantController>();
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
                 //animator.CrossFadeInFixedTime("Walk", 0.0f);
             },

            _update: () =>
            {
                WeaponChange();
                Atack();

                //落下
                if (!groundflg)
                {
                    //animationState.ChangeState(ANIMATION_KEY.JumpStay);
                    return;
                }
                //ジャンプ
                //if (InputTemplate(KEY.SPACE, ANIMATION_KEY.JumpUP)) return;
                //歩き
                if (ExtractionKey(nowKey, 12) != 0)
                {
                    if (nowKey.HasFlag(KEY.W) && nowKey.HasFlag(KEY.S) || nowKey.HasFlag(KEY.A) && nowKey.HasFlag(KEY.D)) return;
                    animationState.ChangeState(ANIMATION_KEY.Walk);
                }
            });


    }

    private void Atack()
    {

    }
}
