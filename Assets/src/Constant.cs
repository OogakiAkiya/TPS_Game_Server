using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;


public static class HeaderConstant
{
    public static readonly int USERID_LENGTH = 12;

    //ID
    public static readonly byte ID_INIT = 0x0002;
    public static readonly byte ID_GAME = 0x0003;

    //GameCode
    public static readonly byte CODE_GAME_BASICDATA = 0x0001;
}

public enum Key : short
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

public enum AnimationKey : int
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
    JumpDown
}
