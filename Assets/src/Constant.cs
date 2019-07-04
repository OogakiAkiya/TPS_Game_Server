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