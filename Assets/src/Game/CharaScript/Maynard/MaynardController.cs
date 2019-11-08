using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaynardController : BaseController
{
    // Start is called before the first frame update
    void Start()
    {
        base.Init();

        //武器関係
        weaponList.Add(new MachineGun(Atack));
        weaponList.Add(new HandGun(Atack));
        weapon = weaponList[weaponListIndex];

    }

    // Update is called once per frame
    void Update()
    {
        base.BaseUpdate();
    }

    public override void Atack()
    {

    }

}
