using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
public class TCP_ServerController : MonoBehaviour
{
    public string IPAddr="127.0.0.1";
    public int port=12345;
    private TCP_Server socket = new TCP_Server();
    
    // Start is called before the first frame update
    void Start()
    {
        //Server
        socket.Init(IPAddr, port);
        var task = socket.StartAccept();

    }

    // Update is called once per frame
    void Update()
    {
        socket.BeginUpdate();

        if (socket.clientList.Count > 0)
        {
            foreach (Tcp_Server_Socket client in socket.clientList)
            {
                while (client.RecvDataListCount() > 0)
                {
                    byte[] data = client.GetRecvDataList();
                    RecvRoutine(data);

                }
                //this.GetComponent<UDP_ServerController>().SendAllClientData();
            }
        }

        socket.EndUpdate();


    }

    private void RecvRoutine(byte[] _data)
    {
        //初接続
        if (_data[0] == HeaderConstant.ID_INIT)
        {
            byte[] b_userId = new byte[12];
            Array.Copy(_data, sizeof(byte), b_userId, 0, b_userId.Length);
            string userId = System.Text.Encoding.UTF8.GetString(b_userId);

            //同じユーザーで複数ログインを防ぐ
            if (!GameObject.Find(userId.Trim()))
            {
                this.GetComponent<GameController>().AddNewUser(userId.Trim());
            }
            

        }

        //ゲーム処理
        if (_data[0] == HeaderConstant.ID_GAME)
        {
            byte[] b_userId = new byte[HeaderConstant.USERID_LENGTH];
            Array.Copy(_data, sizeof(byte), b_userId, 0, b_userId.Length);
            string userId = System.Text.Encoding.UTF8.GetString(b_userId);

            var objects = GameObject.FindGameObjectsWithTag("users");
            foreach (var obj in objects)
            {
                if (obj.name.Equals(userId.Trim()))
                {
                    if (_data[sizeof(byte) + HeaderConstant.USERID_LENGTH] == HeaderConstant.CODE_GAME_BASICDATA)
                    {
                        obj.GetComponent<UserController>().AddInputKeyList(_data[sizeof(byte) * 2 + 12]);
                    }
                }
            }
        }

    }
}
