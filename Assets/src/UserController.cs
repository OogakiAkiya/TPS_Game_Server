using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Key : short
{
    W = 0x001,
    S = 0x002,
    A = 0x004,
    D = 0x008,
    G = 0x010,
    R = 0x020,
    SHIFT = 0x040,
    LEFT_BUTTON = 0x080,
    RIGHT_BUTTON = 0x100
}


public class UserController : MonoBehaviour
{


    private List<byte[]> recvDataList = new List<byte[]>();
    private List<Key> inputKeyList = new List<Key>();

    public string userId;
    public string IPaddr { get; set; }
    public float moveSpeed=1.0f;
    private Key nowKey=0;

    void Start()
    {
    }

    public void SetUserID(string _userId)
    {
        userId = _userId;
        this.name = _userId;

    }

    // Update is called once per frame
    void Update()
    {
        if (recvDataList.Count > 0)
        {
            byte[] recvData = GetRecvData();
        }

        while (inputKeyList.Count > 0)
        {
            //入力値取得
            Key inputKey = GetInputKey();

            //現在のキーを保存
            Key oldKey = nowKey;

            //新しいキー入力を加算
            nowKey |= inputKey;
            //二度目のキー入力でフラグOFF
            nowKey = oldKey ^ inputKey;
        }

        //移動量算出
        Vector3 velocity = Vector3.zero;
        if(nowKey.HasFlag(Key.W)) velocity += this.transform.forward;
        if (nowKey.HasFlag(Key.S)) velocity += -this.transform.forward;
        if (nowKey.HasFlag(Key.A)) velocity += -this.transform.right;
        if (nowKey.HasFlag(Key.D)) velocity += this.transform.right;

        //移動
        this.transform.Translate(velocity.normalized *moveSpeed*Time.deltaTime);

    }


    public void AddRecvData(byte[] _addData)
    {
        recvDataList.Add(_addData);
    }

    public void AddInputKeyList(Key _addData)
    {
        inputKeyList.Add(_addData);
    }

    public Key GetInputKey()
    {
        Key returnData;
        returnData = inputKeyList[0];
        inputKeyList.RemoveAt(0);
        return returnData;
    }

    private byte[] GetRecvData()
    {
        byte[] returnData;
        returnData = recvDataList[0];
        recvDataList.RemoveAt(0);
        return returnData;
    }
}
