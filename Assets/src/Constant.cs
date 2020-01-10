using System;

public enum KEY : short
{
    W = 0x001,
    S = 0x002,
    A = 0x004,
    D = 0x008,
    SHIFT = 0x010,
    G = 0x020,
    R = 0x040,
    LEFT_CLICK = 0x080,
    RIGHT_CLICK = 0x100,
    SPACE=0x200,
    LEFT_BUTTON = 0x400,
    RIGHT_BUTTON = 0x800,
}

public enum ANIMATION_KEY : int
{
    Idle,
    Walk,
    WalkForward,
    WalkBack,
    WalkLeft,
    WalkRight,
    Run,
    RunForward,
    RunBack,
    RunLeft,
    RunRight,
    JumpUP,
    JumpStay,
    JumpDown,
    Reloading,
    Attack,
    ModelChange,
    Dying
}


public enum WEAPONTYPE:int
{
    BASE,
    MACHINEGUN,
    HANDGUN,
    UMP45,
    CLAW
}

public enum WEAPONSTATE : int
{
    WAIT,
    ATACK,
    RELOAD
}

public static class Tags
{
    public static readonly string SOLDIER = "Soldier";
    public static readonly string MONSTER = "Monster";
}
public enum MonsterType:byte
{
    MAYNARD=0x0001,
    MUTANT=0x0002 
}