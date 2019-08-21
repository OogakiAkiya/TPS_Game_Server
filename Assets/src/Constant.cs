using System;


public static class Header
{
    public static readonly int USERID_LENGTH = 12;
    public enum ID : byte
    {
        DEBUG = 0x001,
        INIT = 0x002,
        GAME = 0x003
    }
    public enum GameCode : byte
    {
        BASICDATA = 0x0001
    }

}

public enum KEY : short
{
    W = 0x001,
    S = 0x002,
    A = 0x004,
    D = 0x008,
    SHIFT = 0x010,
    G = 0x020,
    R = 0x040,
    LEFT_BUTTON = 0x080,
    RIGHT_BUTTON = 0x100,
    SPACE=0x200
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
    Dying
}

public enum UDPSTATE : int
{
    INIT,
    GAME,
    RESULT
}

public enum WEAPONTYPE:int
{
    MACHINEGUN
}

public enum WEAPONSTATE : int
{
    WAIT,
    ATACK,
    RELOAD

}

