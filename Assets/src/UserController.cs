using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct NowKey
{
    public bool W;
    public bool S;
    public bool A;
    public bool D;

    public NowKey(bool _w=false, bool _s = false, bool _a = false, bool _d = false)
    {
        this.W = _w;
        this.S = _s;
        this.A = _a;
        this.D = _d;
    }
}

public class UserController : MonoBehaviour
{
    //Key
    enum Key : byte
    {
        W_UP = 0x0001,
        S_UP = 0x0002,
        A_UP = 0x0003,
        D_UP = 0x0004,
        W_DOWN=0x0005,
        S_DOWN = 0x0006,
        A_DOWN = 0x0007,
        D_DOWN = 0x0008

    }


    private List<byte[]> recvDataList = new List<byte[]>();
    private List<byte> inputKeyList = new List<byte>();

    public string userId;
    public string IPaddr { get; set; }
    public float moveSpeed=1.0f;
    private NowKey key = new NowKey();

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
            byte recv = GetInputKey();

            switch (recv)
            {
                case (byte)Key.W_UP:
                    key.W = false;
                    break;
                case (byte)Key.S_UP:
                    key.S = false;
                    break;
                case (byte)Key.A_UP:
                    key.A = false;
                    break;
                case (byte)Key.D_UP:
                    key.D = false;
                    break;
                case (byte)Key.W_DOWN:
                    key.W = true;
                    break;
                case (byte)Key.S_DOWN:
                    key.S = true;
                    break;
                case (byte)Key.A_DOWN:
                    key.A = true;
                    break;
                case (byte)Key.D_DOWN:
                    key.D = true;
                    break;
                default:
                    break;
            }
        }
        //移動処理
        Vector3 velocity = Vector3.zero;
        if (key.W)
        {
            velocity += this.transform.forward;
        }
        if (key.S)
        {
            velocity += -this.transform.forward;
        }
        if (key.A)
        {
            velocity += -this.transform.right;
        }
        if (key.D) velocity += this.transform.right;

        this.transform.Translate(velocity.normalized *moveSpeed*Time.deltaTime);

    }


    public void AddRecvData(byte[] _addData)
    {
        recvDataList.Add(_addData);
    }

    public void AddInputKeyList(byte _addData)
    {
        inputKeyList.Add(_addData);
    }

    public byte GetInputKey()
    {
        byte returnData;
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
