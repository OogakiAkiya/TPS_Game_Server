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
        socket.Update();
        if (socket.server.GetRecvDataSize() > 0)
        {
            var data = socket.server.GetRecvData();
            if (data.Value[sizeof(uint)+HeaderConstant.USERID_LENGTH] == HeaderConstant.ID_INIT)
            {
                clientIPList.Add(data.Key.Address.ToString());
                //Debug.LogFormat("UDP:Login={0}", data.Key.Address.ToString());
                FileController.GetInstance().Write("UDPLogin",data.Key.Address.ToString());

            }

            if (data.Value[sizeof(uint) + HeaderConstant.USERID_LENGTH] == HeaderConstant.ID_GAME)
            {
                byte[] b_userId = new byte[HeaderConstant.USERID_LENGTH];
                System.Array.Copy(data.Value, sizeof(uint), b_userId, 0, b_userId.Length);
                string userId = System.Text.Encoding.UTF8.GetString(b_userId);

                //Vector3 vect = Convert.GetVector3(data.Value, sizeof(uint) + sizeof(byte) * 1 + HeaderConstant.USERID_LENGTH,_x:false,_z:false);
                Vector3 vect = Convert.GetVector3(data.Value, sizeof(uint) + sizeof(byte) * 1 + HeaderConstant.USERID_LENGTH);
                foreach (var obj in gameController.users)
                {
                    if (obj.userId == userId.Trim())
                    {
                        obj.rotat = vect;
                        vect.x = 0;
                        vect.z = 0;
                        obj.transform.rotation = Quaternion.Euler(vect);
                    }
                }

            }
        }

        //SendAllClientData();

    }

    public void SendAllClientData()
    {
        //sendData作成
        List<byte[]> sendData = new List<byte[]>();
        foreach (var user in gameController.users)
        {
            sendData.Add(user.GetStatus());
        }

        //送信処理
        socket.AllClietnSend(clientIPList,sendData);
    }
}

