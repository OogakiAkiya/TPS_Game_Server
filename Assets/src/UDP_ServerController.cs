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
            clientIPList.Add(data.Key.Address.ToString());
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
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            byte[] userName = enc.GetBytes(System.String.Format("{0, -" + HeaderConstant.USERID_LENGTH + "}", user.name));              //12byteに設定する
            byte[] positionData = new byte[sizeof(float) * 3];
            Buffer.BlockCopy(BitConverter.GetBytes(user.transform.position.x), 0, positionData, 0 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(user.transform.position.y), 0, positionData, 1 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(user.transform.position.z), 0, positionData, 2 * sizeof(float), sizeof(float));

            byte[] addData = new byte[sizeof(byte) * 2 + userName.Length + sizeof(float) * 3];
            addData[0] = HeaderConstant.ID_GAME;
            userName.CopyTo(addData, sizeof(byte));
            addData[sizeof(byte) + userName.Length] = HeaderConstant.CODE_GAME_BASICDATA;
            positionData.CopyTo(addData, sizeof(byte) * 2 + userName.Length);
            sendData.Add(addData);
        }

        //送信処理
        socket.AllClietnSend(clientIPList,sendData);
    }
}
