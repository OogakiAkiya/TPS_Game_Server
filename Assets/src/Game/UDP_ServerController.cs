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
    private GameController gameController;

    IDictionary<string, int> clientIPMap = new Dictionary<string, int>();

    private StateMachine<Header.ID> state = new StateMachine<Header.ID>();
    private KeyValuePair<IPEndPoint,byte[]> recvData;
    // Start is called before the first frame update
    void Start()
    {
        gameController = this.GetComponent<GameController>();
        socket.Init(port, sendPort);

        state.AddState(Header.ID.INIT, () => { }, InitUpdate);
        state.AddState(Header.ID.GAME, () => { }, GameUpdate);

    }

    // Update is called once per frame
    void Update()
    {
        socket.Update();
        if (socket.server.GetRecvDataSize() > 0)
        {
            //受信データの取得
            recvData = socket.server.GetRecvData();

            //受信データごとの処理
            state.ChangeState((Header.ID)recvData.Value[sizeof(uint) + Header.USERID_LENGTH]);
            state.Update();
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
        socket.AllClietnSend(clientIPMap, sendData);

    }

    public void InitUpdate()
    {
        //UDPでログイン処理は無駄
        if (!clientIPMap.ContainsKey(recvData.Key.Address.ToString()))
        {
            clientIPMap.Add(recvData.Key.Address.ToString(), 12343);
            FileController.GetInstance().Write("UDPLogin", recvData.Key.Address.ToString());
        }

    }

    public void GameUpdate()
    {
        byte[] b_userId = new byte[Header.USERID_LENGTH];
        System.Array.Copy(recvData.Value, sizeof(uint), b_userId, 0, b_userId.Length);
        string userId = System.Text.Encoding.UTF8.GetString(b_userId);

        Vector3 vect = Convert.GetVector3(recvData.Value, sizeof(uint) + sizeof(byte) * 1 + Header.USERID_LENGTH);
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

