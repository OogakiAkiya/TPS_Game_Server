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
    private GameController gameController;


    private StateMachine<Header.ID> state = new StateMachine<Header.ID>();
    private byte[] recvData;

    // Start is called before the first frame update
    void Start()
    {
        gameController = this.GetComponent<GameController>();

        //TCPServerサーバー設定
        socket.Init(IPAddr, port);

        //待ち受け開始
        var task = socket.StartAccept();


        state.AddState(Header.ID.INIT,()=>{ },InitUpdate);
        state.AddState(Header.ID.GAME,()=> { },GameUpdate);

    }

    // Update is called once per frame
    void Update()
    {
        socket.BeginUpdate();

        //ログインユーザーのチェック
        if (socket.clientList.Count == 0) return;
        foreach (Tcp_Server_Socket client in socket.clientList)
        {
            //受信処理
            while (client.RecvDataListCount() > 0)
            {
                //受信データのセット
                recvData = client.GetRecvDataList();

                //受信データごとの処理
                state.ChangeState((Header.ID)recvData[0]);
                state.Update();

                /*test send
                List<byte> dlist = new List<byte>();
                dlist.Add(0x001);
                var te=client.Send(dlist.ToArray(),dlist.Count) ;
                */
            }
        }

        socket.EndUpdate();
    }

    private void InitUpdate()
    {
        byte[] b_userId = new byte[Header.USERID_LENGTH];
        Array.Copy(recvData, sizeof(byte), b_userId, 0, b_userId.Length);
        string userId = System.Text.Encoding.UTF8.GetString(b_userId);

        //同じユーザーで複数ログインを防ぐ
        if (!GameObject.Find(userId.Trim()))
        {
            gameController.AddNewUser(userId.Trim());
            gameController.UsersUpdate();
        }
    }

    private void GameUpdate()
    {
        byte[] b_userId = new byte[Header.USERID_LENGTH];
        Array.Copy(recvData, sizeof(byte), b_userId, 0, b_userId.Length);
        string userId = System.Text.Encoding.UTF8.GetString(b_userId);

        foreach (var user in gameController.users)
        {
            if (user.userId.Equals(userId.Trim()))
            {
                if (recvData[sizeof(byte) + Header.USERID_LENGTH] == (byte)Header.GameCode.BASICDATA)
                {
                    Key addData = (Key)BitConverter.ToInt16(recvData, sizeof(byte) * 2 + Header.USERID_LENGTH);

                    user.AddInputKeyList(addData);
                }
            }
        }

    }
}
