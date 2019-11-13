using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class GameHeader
{
    public static readonly int USERID_LENGTH = 14;
    public enum ID : byte
    {
        DEBUG = 0x001,
        INIT = 0x002,
        TITLE=0x003,
        GAME = 0x004
    }

    public enum LoginCode : byte
    {
        LOGINCHECK=0x0001,
        LOGINSUCCES=0x0002,
        LOGINFAILURE=0x0003
    }

    public enum UserTypeCode : byte
    {
        SOLDIER = 0x0001,
        MAYNARD = 0x0002
    }


    public enum GameCode : byte
    {
        BASICDATA = 0x0001,
        SCOREDATA = 0x0002,
        GRENEDEDATA=0x0003,
        CHECKDATA=0x0004
    }

    public string userName { get; private set; }
    public ID id { get; private set; } = ID.INIT;
    public byte gameCode { get; private set; } = 0x00ff;

    public void CreateNewData(ID _id = ID.INIT, string _name = "", byte _gameCode=0x00ff)
    {
        if (_name != "") userName = _name;
        if (id != _id) id = _id;
        if (gameCode != _gameCode) gameCode = _gameCode;
    }

    public void SetHeader(byte[] _data,int _index=0)
    {
        id = (ID)_data[_index];
        byte[] b_userId = new byte[USERID_LENGTH];
        System.Array.Copy(_data, _index+sizeof(byte), b_userId, 0, b_userId.Length);
        userName = System.Text.Encoding.UTF8.GetString(b_userId);
        gameCode = _data[_index + sizeof(byte) + USERID_LENGTH];
    }

    public byte[] GetHeader()
    {
        List<byte> returnData=new List<byte>();

        System.Text.Encoding enc = System.Text.Encoding.UTF8;
        byte[] b_userName = enc.GetBytes(System.String.Format("{0, -" + USERID_LENGTH + "}", userName));
        byte[] sendData = new byte[sizeof(byte) * 2 + userName.Length];
        returnData.Add((byte)id);
        returnData.AddRange(b_userName);
        returnData.Add((byte)gameCode);

        return returnData.ToArray();
    }
    public int GetHeaderLength()
    {
        return sizeof(ID) + sizeof(GameCode) + USERID_LENGTH;
    }
}
