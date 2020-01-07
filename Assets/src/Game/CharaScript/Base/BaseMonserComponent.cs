using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterComponent : BaseComponent
{
    public MonsterType monsterType { get; protected set; } = MonsterType.MAYNARD;

    public override void Init()
    {
        type = GameHeader.UserTypeCode.MONSTER;

        base.Init();
    }
}
