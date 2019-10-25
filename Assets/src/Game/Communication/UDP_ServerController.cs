using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using UnityEngine;


public class UDP_ServerController : MonoBehaviour
{
    [SerializeField] int port = 12344;
    [SerializeField] int sendPort = 12343;
    private UDP_Server socket = new UDP_Server();
    private GameController gameController;
    private StateMachine<GameHeader.ID> state = new StateMachine<GameHeader.ID>();
    private KeyValuePair<IPEndPoint,byte[]> recvData;
    private GameHeader header = new GameHeader();

    private Transform bomList;

    Task clientDataSendTask;
    Task clientCompDataSendTask;

    Task ScoreDataSendTask;

    // Start is called before the first frame update
    void Start()
    {
        bomList = GameObject.FindGameObjectWithTag("BomList").transform;
        gameController = this.GetComponent<GameController>();
        socket.Init(port, sendPort);

        state.AddState(GameHeader.ID.INIT, () => { }, InitUpdate);
        state.AddState(GameHeader.ID.GAME, () => { }, GameUpdate);

    }

    public Task<int> UPdata()
    {
        return Task.Run(() =>
        {

            //socket.Update();
            if (socket.server.GetRecvDataSize() > 0)
            {
                //受信データの取得
                recvData = socket.server.GetRecvData();
                header.SetHeader(recvData.Value, sizeof(uint));

                //受信データごとの処理
                state.ChangeState(header.id);
                state.Update();
            }
            return 0;
        });
        
    }

    public void SendClientCompData()
    {
        List<byte[]> sendData = new List<byte[]>();
        List<string> ipList = new List<string>();

        for (int i = 0; i < gameController.users.Length; i++)
        {
            UserController user = gameController.users[i];
            sendData.Add(user.GetStatusComplete());
            if (user.socket != null)ipList.Add(user.GetIPAddress());

        }

        //グレネードの送信データ作成
        Grenade[] boms = bomList.GetComponentsInChildren<Grenade>();
        for (int i = 0; i < boms.Length; i++)
        {
            sendData.Add(boms[i].GetStatus());
        }

        if (clientCompDataSendTask != null) Task.WaitAll(clientCompDataSendTask);
        clientCompDataSendTask = Task.Run(() =>
        {
            //送信処理
            socket.AllClietnSend(ipList, sendData);
            return 0;
        });
    }
    private int sendCount = 0;
    public void SendAllClientData()
    {
        //20人づつデータ送信
        //sendData作成
        List<byte[]> sendData = new List<byte[]>();
        List<string> ipList = new List<string>();
        for (int i = 0; i < gameController.users.Length; i++)
        {
            UserController user = gameController.users[i];
            sendData.Add(user.GetStatus());
            if (user.socket != null)
            {
                if (i < sendCount + 20 && i >= sendCount) ipList.Add(user.GetIPAddress());
            }
        }
        sendCount += 20;
        if (sendCount > gameController.users.Length) sendCount = 0;

        //グレネードの送信データ作成
        Grenade[] boms = bomList.GetComponentsInChildren<Grenade>();
        for (int i = 0; i < boms.Length; i++)
        {
            sendData.Add(boms[i].GetStatus());
        }

        if (clientDataSendTask != null) Task.WaitAll(clientDataSendTask);
        clientDataSendTask = Task.Run(() =>
        {
            //送信処理
            socket.AllClietnSend(ipList, sendData);
            return 0;
        });
    }

    public void SendAllClientScoreData()
    {
        List<string> ipList = new List<string>();
        //sendData作成
        List<byte[]> sendData = new List<byte[]>();
        for (int i = 0; i < gameController.users.Length; i++)
        {
            UserController user = gameController.users[i];
            sendData.Add(user.GetScore());
            if (user.socket != null) ipList.Add(user.GetIPAddress());
        }

        //送信処理
        if (ScoreDataSendTask != null) Task.WaitAll(ScoreDataSendTask);

        ScoreDataSendTask = Task.Run(() =>
        {
            //送信処理
            socket.AllClietnSend(ipList, sendData);
            return 0;
        });


    }



    public void InitUpdate()
    {
    }

    public void GameUpdate()
    {
        string userName = header.userName.Trim();

        uint sequence = BitConverter.ToUInt32(recvData.Value, 0);

        Vector2 vect = Convert.GetVector2(recvData.Value, sizeof(uint) + header.GetHeaderLength());

        for (int i = 0; i < gameController.users.Length; i++)
        {
            UserController user = gameController.users[i];
            if (user.userId == userName)
            {
                if (!SequenceCheck(user.sequence, sequence)) return;
                user.sequence = sequence;
                user.rotat.x = vect.x;
                user.rotat.y = vect.y;

                break;
            }
        }

    }

    private bool SequenceCheck(uint _nowSequence, uint _sequence)
    {
        if (_nowSequence > _sequence)
        {
            if (Math.Abs(_nowSequence - _sequence) < 2000000000) return false;
            if (_nowSequence < 1000000000 && _sequence > 3000000000) return false;
        }
        return true;
    }
}

