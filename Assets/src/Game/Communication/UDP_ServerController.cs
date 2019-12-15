using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using UnityEngine;


public class UDP_ServerController : MonoBehaviour
{
    [SerializeField] int port = 12344;
    private UDP_Server socket = new UDP_Server();
    private GameController gameController;
    private StateMachine<GameHeader.ID> state = new StateMachine<GameHeader.ID>();
    private KeyValuePair<IPEndPoint, byte[]> recvData;
    private GameHeader header = new GameHeader();

    private Transform bomList;

    Task clientDataSendTask;
    Task clientCompDataSendTask;
    Task timeTask;
    Task ScoreDataSendTask;

    // Start is called before the first frame update
    void Start()
    {
        bomList = GameObject.FindGameObjectWithTag("BomList").transform;
        gameController = this.GetComponent<GameController>();
        socket.Init(port);

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
        List<KeyValuePair<string, int>> addressList = new List<KeyValuePair<string, int>>();
        List<byte[]> sendData = new List<byte[]>();

        for (int i = 0; i < gameController.users.Length; i++)
        {
            BaseController user = gameController.users[i];
            sendData.Add(user.GetStatusComplete());
            if (user.port >= 0)addressList.Add(user.GetUserAddress());
            

        }

        //グレネードの送信データ作成
        Grenade[] boms = bomList.GetComponentsInChildren<Grenade>();
        for (int i = 0; i < boms.Length; i++)
        {
            sendData.Add(boms[i].GetStatus());
        }
        if (addressList.Count <= 0) return;
        if (clientCompDataSendTask != null) Task.WaitAll(clientCompDataSendTask);

        clientCompDataSendTask = Task.Run(() =>
        {
            //送信処理
            socket.AllClientSend(addressList, sendData);
            return 0;
        });
    }

    public void SendAllClientData()
    {
        List<byte[]> sendData = new List<byte[]>();
        List<KeyValuePair<string, int>> addressList = new List<KeyValuePair<string, int>>();

        for (int i = 0; i < gameController.users.Length; i++)
        {
            BaseController user = gameController.users[i];
            sendData.Add(user.GetStatus());
            if (user.port >= 0)
            {
                addressList.Add(user.GetUserAddress());
            }
        }

        //グレネードの送信データ作成
        Grenade[] boms = bomList.GetComponentsInChildren<Grenade>();
        for (int i = 0; i < boms.Length; i++)
        {
            sendData.Add(boms[i].GetStatus());
        }
        if (addressList.Count <= 0) return;
        if (clientCompDataSendTask != null) Task.WaitAll(clientCompDataSendTask);

        clientCompDataSendTask = Task.Run(() =>
        {
            //送信処理
            socket.AllClientSend(addressList, sendData);
            return 0;
        });


    }

    public void SendAllClientScoreData()
    {
        List<KeyValuePair<string, int>> addressList = new List<KeyValuePair<string, int>>();
        //sendData作成
        List<byte[]> sendData = new List<byte[]>();
        for (int i = 0; i < gameController.users.Length; i++)
        {
            BaseController user = gameController.users[i];
            sendData.Add(user.GetScore());
            if (user.port>=0) addressList.Add(user.GetUserAddress());
        }
        if (addressList.Count <= 0) return;
        //送信処理
        if (ScoreDataSendTask != null) Task.WaitAll(ScoreDataSendTask);

        ScoreDataSendTask = Task.Run(() =>
        {
            //送信処理
            socket.AllClientSend(addressList, sendData);
            return 0;
        });


    }
    public void SendTimeData()
    {
        List<KeyValuePair<string, int>> addressList = new List<KeyValuePair<string, int>>();
        //sendData作成
        List<byte> sendData = new List<byte>();
        GameHeader lheader = new GameHeader();

        for (int i = 0; i < gameController.users.Length; i++)
        {
            BaseController user = gameController.users[i];
            if (user.port >= 0) addressList.Add(user.GetUserAddress());
        }

        if (addressList.Count <= 0) return;

        lheader.CreateNewData(GameHeader.ID.ALERT, GameHeader.UserTypeCode.SOLDIER, "Timer", (byte)GameHeader.GameCode.CHECKDATA);
        sendData.AddRange(lheader.GetHeader());
        sendData.AddRange(Convert.Conversion(gameController.timer.Elapsed.Minutes));
        sendData.AddRange(Convert.Conversion(gameController.timer.Elapsed.Seconds));


        //送信処理
        if (timeTask != null) Task.WaitAll(timeTask);

        timeTask = Task.Run(() =>
        {
            //送信処理
            socket.AllClientSend(addressList, sendData);
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

        Vector2 vect = Convert.GetVector2(recvData.Value, sizeof(uint) + GameHeader.HEADER_SIZE);

        for (int i = 0; i < gameController.users.Length; i++)
        {
            BaseController user = gameController.users[i];
            if (user.userId == userName)
            {
                //ポート番号設定
                user.port = recvData.Key.Port;


                //キャラの回転を受け取る
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

