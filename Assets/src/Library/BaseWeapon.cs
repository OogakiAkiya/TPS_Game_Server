using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class BaseWeapon
{
    public StateMachine<WEAPONSTATE> state = new StateMachine<WEAPONSTATE>();

    protected long interval;                                //撃つ間隔
    public int power { get; protected set; }                //威力
    protected int reloadTime;                               //リロード時間
    public int magazine { get; protected set; }             //弾数
    public int remainingBullet { get; protected set; }      //残弾数
    public float range { get; protected set; }              //射程
    public WEAPONTYPE type { get; protected set; }
    protected Action atackMethod;                           //攻撃時メソッド
    protected System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

    protected byte[] GetStatus(WEAPONTYPE _type)
    {
        List<byte> returnData = new List<byte>();
        returnData.AddRange(Convert.Conversion((int)_type));
        returnData.AddRange(Convert.Conversion((int)state.currentKey));
        returnData.AddRange(Convert.Conversion(remainingBullet));
        return returnData.ToArray();
    }
    public virtual byte[] GetStatus() { return null; }
    public void SetStatus(byte[] _data)
    {
    }

}


public class MachineGun : BaseWeapon
{
    public MachineGun(Action _atack)
    {
        interval = 50;
        power = 5;
        reloadTime = 1000;      //1秒
        magazine = 60;
        remainingBullet = magazine;
        range = 10;
        atackMethod = _atack;
        type = WEAPONTYPE.MACHINEGUN;

        state.AddState(WEAPONSTATE.WAIT);
        state.AddState(WEAPONSTATE.ATACK,
            () =>
            {
                timer.Restart();
            },
            () =>
            {
                if (timer.ElapsedMilliseconds > interval)
                {
                    if (remainingBullet <= 0)
                    {
                        state.ChangeState(WEAPONSTATE.RELOAD);
                        return;
                    }
                    atackMethod();
                    remainingBullet--;
                    state.ChangeState(WEAPONSTATE.WAIT);
                }
            },
            () =>
            {
                timer.Stop();
            }
            );
        state.AddState(WEAPONSTATE.RELOAD,
            () =>
            {
            },
            () =>
            {
            },
            () =>
            {
                remainingBullet = magazine;
            }
            );

        state.ChangeState(WEAPONSTATE.WAIT);
    }

    public override byte[] GetStatus()
    {
        return GetStatus(type);
    }

}

public class HandGun : BaseWeapon
{
    public HandGun(Action _atack)
    {
        interval = 200;
        power = 10;
        reloadTime = 1000;      //1秒
        magazine = 12;
        remainingBullet = magazine;
        range = 5;
        atackMethod = _atack;
        type = WEAPONTYPE.HANDGUN;

        state.AddState(WEAPONSTATE.WAIT);
        state.AddState(WEAPONSTATE.ATACK,
            () =>
            {
                timer.Restart();
            },
            () =>
            {
                if (timer.ElapsedMilliseconds > interval)
                {
                    if (remainingBullet <= 0)
                    {
                        state.ChangeState(WEAPONSTATE.RELOAD);
                        return;
                    }
                    atackMethod();
                    remainingBullet--;
                    state.ChangeState(WEAPONSTATE.WAIT);
                }
            },
            () =>
            {
                timer.Stop();
            }
            );
        state.AddState(WEAPONSTATE.RELOAD,
            () =>
            {
            },
            () =>
            {
            },
            () =>
            {
                remainingBullet = magazine;
            }
            );

        state.ChangeState(WEAPONSTATE.WAIT);
    }

    public override byte[] GetStatus()
    {
        return GetStatus(type);
    }

}

