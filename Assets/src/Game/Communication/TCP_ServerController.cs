using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class TCP_ServerController : MonoBehaviour
{
    [SerializeField] string IPAddr = "127.0.0.1";
    [SerializeField] int port = 12345;
    private TCP_Server socket = new TCP_Server();
    private GameController gameController;


    private StateMachine<GameHeader.ID> state = new StateMachine<GameHeader.ID>();
    private byte[] recvData;
    private GameHeader header = new GameHeader();
    private Tcp_Server_Socket sendSocket;

    // Start is called before the first frame update
    void Start()
    {
        gameController = this.GetComponent<GameController>();

        //TCPServerサーバー設定
        socket.Init(IPAddr, port);

        //待ち受け開始
        var task = socket.StartAccept();

        state.AddState(GameHeader.ID.DEBUG, () =>
        {
            DebugSend((byte)GameHeader.ID.DEBUG);
        });
        state.AddState(GameHeader.ID.INIT, () => { }, InitUpdate);
        state.AddState(GameHeader.ID.TITLE, () => { }, TitleUpdate);
        state.AddState(GameHeader.ID.GAME, () => { }, GameUpdate);


    }

    public Task<int> UPdata()
    {
        socket.BeginUpdate();
        return Task.Run(() =>
        {
            //ログインユーザーのチェック
            if (socket.clientList.Count == 0) return 0;

            for (int i = 0; i < socket.clientList.Count; i++)
            {
                Tcp_Server_Socket client = socket.clientList[i];
                //受信処理
                while (client.RecvDataListCount() > 0)
                {
                    //受信データのセット
                    recvData = client.GetRecvDataList();

                    //送信元のソケットセット
                    sendSocket = client;

                    //受信データごとの処理
                    header.SetHeader(recvData);
                    state.ChangeState(header.id);
                    state.Update();
                }

            }
            socket.EndUpdate();

            return 0;
        });
    }


    private void InitUpdate()
    {
        //同じユーザーで複数ログインを防ぐ
        bool addFlg = true;
        for (int i = 0; i < gameController.users.Length; i++)
        {
            if (gameController.users[i].userId == header.userName.Trim())addFlg = false;
        }
        if (addFlg)gameController.AddUserList(header.gameCode, header.userName.Trim(), sendSocket);

    }

    private void TitleUpdate()
    {
        if ((GameHeader.LoginCode)header.gameCode == GameHeader.LoginCode.LOGINCHECK)
        {
            for (int i = 0; i < gameController.users.Length; i++)
            {
                if (gameController.users[i].userId == header.userName.Trim())
                {
                    TestSend((byte)GameHeader.ID.TITLE, (byte)GameHeader.LoginCode.LOGINFAILURE);
                    return;
                }
            }
            //ログイン成功
            TestSend((byte)GameHeader.ID.TITLE, (byte)GameHeader.LoginCode.LOGINSUCCES);
        }
    }

    private void GameUpdate()
    {
        for (int i = 0; i < gameController.users.Length; i++)
        {
            BaseController user = gameController.users[i];
            if (user.userId.Equals(header.userName.Trim()))
            {
                if (header.gameCode == (byte)GameHeader.GameCode.BASICDATA)
                {
                    KEY addData = (KEY)BitConverter.ToInt16(recvData, GameHeader.HEADER_SIZE);

                    user.AddInputKeyList(addData);
                }

                if (header.gameCode == (byte)GameHeader.GameCode.CHECKDATA)
                {
                    user.SetCheckKey((KEY)BitConverter.ToInt16(recvData, GameHeader.HEADER_SIZE));
                }


            }

        }
    }

    void TestSend(byte _id, byte _code = 0x0001)
    {
        List<byte> sendData = new List<byte>();
        header.CreateNewData((GameHeader.ID)_id,GameHeader.UserTypeCode.SOLDIER, "Debug", _code);
        sendData.AddRange(header.GetHeader());
        Task task = sendSocket.Send(sendData.ToArray(), sendData.Count);
    }
    void DebugSend(byte _id, byte _code = 0x0001)
    {
        List<byte> sendData = new List<byte>();
        header.CreateNewData((GameHeader.ID)_id, GameHeader.UserTypeCode.SOLDIER, "Debug", _code);
        sendData.AddRange(header.GetHeader());
        sendData.AddRange(Convert.Conversion(gameController.users.Length - 1));
        Task task = sendSocket.Send(sendData.ToArray(), sendData.ToArray().Length);
    }

    public void FinishAlertSend(Tcp_Server_Socket _socket,byte _code = 0x0001)
    {
        List<byte> sendData = new List<byte>();
        header.CreateNewData((GameHeader.ID)GameHeader.ID.ALERT, GameHeader.UserTypeCode.SOLDIER, "Timer", _code);
        sendData.AddRange(header.GetHeader());
        for(int i = 0; i < gameController.users.Length; i++)
        {
            sendData.AddRange(gameController.users[i].GetFinishStatus());
        }
        Task task = _socket.Send(sendData.ToArray(), sendData.ToArray().Length);

    }


    private byte[] FinishData()
    {
        List<byte> data = new List<byte>();
        for(int i = 0; i < gameController.users.Length;i++)
        {
            data.AddRange(gameController.users[i].GetFinishStatus());
        }
        return data.ToArray();
    } 

}
