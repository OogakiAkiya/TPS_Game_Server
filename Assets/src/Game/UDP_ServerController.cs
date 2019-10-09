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

    // Start is called before the first frame update
    void Start()
    {
        gameController = this.GetComponent<GameController>();
        socket.Init(port, sendPort);

        state.AddState(GameHeader.ID.INIT, () => { }, InitUpdate);
        state.AddState(GameHeader.ID.GAME, () => { }, GameUpdate);

    }

    // Update is called once per frame
    void Update()
    {
        socket.Update();
        if (socket.server.GetRecvDataSize() > 0)
        {
            //受信データの取得
            recvData = socket.server.GetRecvData();
            header.SetHeader(recvData.Value, sizeof(uint));

            //受信データごとの処理
            state.ChangeState(header.id);
            state.Update();
        }
    }

    public void SendAllClientData()
    {
        //sendData作成
        List<byte[]> sendData = new List<byte[]>();
        List<string> ipLi = new List<string>();
        foreach (var user in gameController.users)
        {
            sendData.Add(user.GetStatus());
            if (user.socket != null) ipLi.Add(user.GetIPAddress());
        }

        //グレネードの送信データ作成
        var boms=GameObject.FindGameObjectsWithTag("Bom");
        foreach(var bom in boms)
        {
            sendData.Add(bom.GetComponent<Grenade>().GetStatus());
        }

        //送信処理
        socket.AllClietnSend(ipLi, sendData);

    }

    public void SendAllClientScoreData()
    {
        List<string> ipLi = new List<string>();
        //sendData作成
        List<byte[]> sendData = new List<byte[]>();
        foreach (var user in gameController.users)
        {
            sendData.Add(user.GetScore());
            if (user.socket != null) ipLi.Add(user.GetIPAddress());
        }

        //送信処理
        socket.AllClietnSend(ipLi, sendData);

    }


    public void InitUpdate()
    {
    }

    public void GameUpdate()
    {
        string userName = header.userName.Trim();

        uint sequence = BitConverter.ToUInt32(recvData.Value, 0);

        Vector3 vect = Convert.GetVector3(recvData.Value, sizeof(uint)+header.GetHeaderLength());
        foreach (var obj in gameController.users)
        {
            if (obj.userId == userName)
            {
                if (!SequenceCheck(obj.sequence, sequence)) return;
                obj.sequence = sequence;
                obj.rotat = vect;
                vect.x = 0;
                vect.z = 0;
                obj.transform.rotation = Quaternion.Euler(vect);
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

