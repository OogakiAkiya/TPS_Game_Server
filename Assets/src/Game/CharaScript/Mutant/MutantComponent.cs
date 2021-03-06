﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MutantComponent : MonsterComponent
{
    [SerializeField] Vector3 attackRange = new Vector3(1.4f, 1f, 1.1f);

    // Start is called before the first frame update
    public override void Init()
    {
        monsterType = MonsterType.MUTANT;
        base.Init();
        //武器関係
        weaponList.Add(new Claw(Attack,50));
        weapon = weaponList[weaponListIndex];
    }

    private void OnDrawGizmos()
    {
        Vector3 vector = this.transform.position + this.transform.forward * 1f + this.transform.up;
        //Vector3 vector = this.transform.forward * 0.4f + new Vector3(0, 1, 0.2f);
        Gizmos.DrawCube(vector, attackRange);
    }

    public override void Attack()
    {
        Vector3 vector = this.transform.position + this.transform.forward * 1f + this.transform.up;
        //Vector3 vector = this.transform.forward * 0.4f + new Vector3(0, 1, 0.2f);
        Collider[] colliders = Physics.OverlapBox(vector, attackRange, this.transform.localRotation, 1 << 10);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].tag == Tags.SOLDIER)
            {
                if (colliders[i].GetComponent<BaseController>().Damage(weapon.power))
                {
                    if (myController) myController.killAmount++;
                }

            }
        }
    }

    public override void End()
    {
        base.End();
        MonsterController monsterCtr = (MonsterController)myController;
        monsterCtr.ChangeModele("Maynard");
    }
}
