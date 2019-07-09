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
            if (data.Value[HeaderConstant.USERID_LENGTH] == HeaderConstant.ID_INIT)
            {
                clientIPList.Add(data.Key.Address.ToString());

            }
            if(data.Value[HeaderConstant.USERID_LENGTH] == HeaderConstant.ID_GAME)
            {
                byte[] b_userId = new byte[HeaderConstant.USERID_LENGTH];
                System.Array.Copy(data.Value, 0, b_userId, 0, b_userId.Length);
                string userId = System.Text.Encoding.UTF8.GetString(b_userId);


                Vector3 vect = Vector3.zero;
                vect.x = BitConverter.ToSingle(data.Value, sizeof(byte) * 1 + 12 + 0 * sizeof(float));
                vect.y = BitConverter.ToSingle(data.Value, sizeof(byte) * 1 + 12 + 1 * sizeof(float));
                vect.z = BitConverter.ToSingle(data.Value, sizeof(byte) * 1 + 12 + 2 * sizeof(float));

                var objects=GameObject.FindGameObjectsWithTag("users");
                foreach(var obj in objects)
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
        byte[] positionData = new byte[sizeof(float) * 3];
        Buffer.BlockCopy(BitConverter.GetBytes(_user.transform.position.x), 0, positionData, 0 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(_user.transform.position.y), 0, positionData, 1 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(_user.transform.position.z), 0, positionData, 2 * sizeof(float), sizeof(float));
        byte[] rotationData = new byte[sizeof(float) * 3];
        Buffer.BlockCopy(BitConverter.GetBytes(_user.transform.localEulerAngles.x), 0, rotationData, 0 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(_user.transform.localEulerAngles.y), 0, rotationData, 1 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(_user.transform.localEulerAngles.z), 0, rotationData, 2 * sizeof(float), sizeof(float));
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
}
