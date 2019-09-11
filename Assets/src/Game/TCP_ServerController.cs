using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
public class TCP_ServerController : MonoBehaviour
{
    public string IPAddr = "127.0.0.1";
    public int port = 12345;
    private TCP_Server socket = new TCP_Server();
    private GameController gameController;


    private StateMachine<Header.ID> state = new StateMachine<Header.ID>();
    private byte[] recvData;
    private HeaderClass header = new HeaderClass();
    private Tcp_Server_Socket sendSocket;

    // Start is called before the first frame update
    void Start()
    {
        gameController = this.GetComponent<GameController>();

        //TCPServerサーバー設定
        socket.Init(IPAddr, port);

        //待ち受け開始
        var task = socket.StartAccept();

        state.AddState(Header.ID.DEBUG, () =>
        {
            //TestSend((byte)Header.ID.DEBUG);
            DebugSend((byte)Header.ID.DEBUG);

            state.ChangeState(Header.ID.INIT);
        });
        state.AddState(Header.ID.INIT, () => { }, InitUpdate);
        state.AddState(Header.ID.GAME, () => { }, GameUpdate);

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

                //送信元のソケットセット
                sendSocket = client;

                //受信データごとの処理
                header.SetHeader(recvData);
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
                    KEY addData = (KEY)BitConverter.ToInt16(recvData, sizeof(byte) * 2 + Header.USERID_LENGTH);

                    user.AddInputKeyList(addData);
                }
            }
        }

    }

    void TestSend(byte _id, byte _code = 0x0001)
    {
        List<byte> sendData = new List<byte>();
        var header = new HeaderClass();
        header.CreateNewData((Header.ID)_id, "Debug", (Header.GameCode)_code);
        sendData.AddRange(header.GetHeader());
        var task = sendSocket.Send(sendData.ToArray(), sendData.Count);
    }
    void DebugSend(byte _id, byte _code = 0x0001)
    {
        List<byte> sendData = new List<byte>();
        var header = new HeaderClass();
        header.CreateNewData((Header.ID)_id, "Debug", (Header.GameCode)_code);
        sendData.AddRange(header.GetHeader());
        sendData.AddRange(Convert.Conversion(gameController.users.Count - 1));
        var task = sendSocket.Send(sendData.ToArray(), sendData.ToArray().Length);

    }

}
