using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeaderClass
{
    public string userName { get; private set; }
    public Header.ID id { get; private set; }
    public Header.GameCode gameCode { get; private set; }

    public void CreateNewData(string _name = "", Header.ID _id=Header.ID.INIT, Header.GameCode _gameCode=Header.GameCode.BASICDATA)
    {
        if (_name != "") userName = _name;
        if (id != _id) id = _id;
        if (gameCode != _gameCode) gameCode = _gameCode;
    }

    public void SetHeader(byte[] _data,int _index=0)
    {
        id=(Header.ID)_data[_index];

        byte[] b_userId = new byte[Header.USERID_LENGTH];
        System.Array.Copy(_data, _index + sizeof(byte), b_userId, 0, b_userId.Length);
        userName = System.Text.Encoding.UTF8.GetString(b_userId);

        gameCode=(Header.GameCode)_data[sizeof(uint) + sizeof(byte) + Header.USERID_LENGTH];
    }

    public byte[] GetHeader()
    {
        List<byte> returnData=new List<byte>();

        System.Text.Encoding enc = System.Text.Encoding.UTF8;
        byte[] b_userName = enc.GetBytes(System.String.Format("{0, -" + Header.USERID_LENGTH + "}", userName));              //12byteに設定する
        byte[] sendData = new byte[sizeof(byte) * 2 + userName.Length];
        returnData.Add((byte)id);
        returnData.AddRange(b_userName);
        returnData.Add((byte)gameCode);

        return returnData.ToArray();
    }
}
