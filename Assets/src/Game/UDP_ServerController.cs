using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using UnityEngine;


public class UDP_ServerController : MonoBehaviour
{
    private UDP_Server socket = new UDP_Server();
    public int port = 12344;
    public int sendPort = 12343;
    public List<string> clientIPList = new List<string>();
    private GameController gameController;

    // Start is called before the first frame update
    void Start()
    {
        gameController = this.GetComponent<GameController>();

        socket.Init(port, sendPort);
    }

    // Update is called once per frame
    void Update()
    {
        if (socket.server.GetRecvDataSize() > 0)
        {
            var data = socket.server.GetRecvData();
            if (data.Value[sizeof(uint)+HeaderConstant.USERID_LENGTH] == HeaderConstant.ID_INIT)
            {
                clientIPList.Add(data.Key.Address.ToString());

            }

            if(data.Value[sizeof(uint) + HeaderConstant.USERID_LENGTH] == HeaderConstant.ID_GAME)
            {
                byte[] b_userId = new byte[HeaderConstant.USERID_LENGTH];
                System.Array.Copy(data.Value, sizeof(uint), b_userId, 0, b_userId.Length);
                string userId = System.Text.Encoding.UTF8.GetString(b_userId);

                Vector3 vect = GetVector3(data.Value, sizeof(uint) + sizeof(byte) * 1 + HeaderConstant.USERID_LENGTH,_x:false,_z:false);
                foreach(var obj in gameController.users)
                {
                    if (obj.name == userId.Trim())
                    {
                        obj.transform.rotation = Quaternion.Euler(vect);
                    }
                }
            }
        }

        SendAllClientData();

    }

    public void SendAllClientData()
    {
        //sendData作成
        //var userList = GameObject.FindGameObjectsWithTag("users");
        List<byte[]> sendData = new List<byte[]>();
        foreach (var user in gameController.users)
        {
            sendData.Add(StatusGet(user));
        }

        //送信処理
        socket.AllClietnSend(clientIPList,sendData);
    }

    private byte[] StatusGet(GameObject _user)
    {
        List<byte> returnData=new List<byte>();
        System.Text.Encoding enc = System.Text.Encoding.UTF8;
        byte[] userName = enc.GetBytes(System.String.Format("{0, -" + HeaderConstant.USERID_LENGTH + "}", _user.name));              //12byteに設定する
        byte[] positionData = GetByteVector3(_user.transform.position);
        byte[] rotationData = GetByteVector3(_user.transform.localEulerAngles);
        byte[] state = new byte[sizeof(int) *2];
        Buffer.BlockCopy(BitConverter.GetBytes((int)_user.GetComponent<UserController>().animationState.currentKey), 0, state, 0, sizeof(int));
        Buffer.BlockCopy(BitConverter.GetBytes((int)_user.GetComponent<UserController>().hp), 0, state,sizeof(int), sizeof(int));

        returnData.Add(HeaderConstant.ID_GAME);
        returnData.AddRange(userName);
        returnData.Add(HeaderConstant.CODE_GAME_BASICDATA);
        returnData.AddRange(positionData);
        returnData.AddRange(rotationData);
        returnData.AddRange(state);

        return returnData.ToArray();
    }

    private Vector3 GetVector3(byte[] _data, int _beginPoint = 0, bool _x = true, bool _y = true, bool _z = true)
    {
        Vector3 vect = Vector3.zero;
        if (_data.Length < sizeof(float) * 3) return vect;
        if (_x) vect.x = BitConverter.ToSingle(_data, _beginPoint + 0 * sizeof(float));
        if (_y) vect.y = BitConverter.ToSingle(_data, _beginPoint + 1 * sizeof(float));
        if (_z) vect.z = BitConverter.ToSingle(_data, _beginPoint + 2 * sizeof(float));
        return vect;
    }
    private byte[] GetByteVector3(Vector3 _vector, bool _x = true, bool _y = true, bool _z = true)
    {
        byte[] vect = new byte[sizeof(float) * 3];
        if(_x)Buffer.BlockCopy(BitConverter.GetBytes(_vector.x), 0, vect, 0 * sizeof(float), sizeof(float));
        if(_y)Buffer.BlockCopy(BitConverter.GetBytes(_vector.y), 0, vect, 1 * sizeof(float), sizeof(float));
        if(_z)Buffer.BlockCopy(BitConverter.GetBytes(_vector.z), 0, vect, 2 * sizeof(float), sizeof(float));

        return vect;
    }

}
