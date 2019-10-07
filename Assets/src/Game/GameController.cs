using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using UnityEngine;


public class GameController : MonoBehaviour
{
    public GameObject userList;
    private GameObject userPrefab;
    private int count = 0;
    //public List<UserController> users { get; private set; } = new List<UserController>();
    public UserController[] users;
    private UDP_ServerController udp_Controller;
    //FPS回数
    int frameCount;
    float prevTime;


    // Start is called before the first frame update
    void Start()
    {
        users = userList.GetComponentsInChildren<UserController>();
        udp_Controller = this.GetComponent<UDP_ServerController>();
        userPrefab = (GameObject)Resources.Load("user");

        //FPS回数
        frameCount = 0;
        prevTime = 0.0f;

        //Update回数制御
        QualitySettings.vSyncCount = 0; // VSyncをOFFにする
        
    }

    // Update is called once per frame
    void Update()
    {
        //FPS表示
        float time = Time.realtimeSinceStartup - prevTime;

        //60fps
        if (time >= 0.016f)
        {
            //データ送信(全ユーザーの情報送信)
            udp_Controller.SendAllClientData();

            prevTime = Time.realtimeSinceStartup;
            frameCount++;

            //一秒に一回
            if (frameCount >= 60)
            {
                udp_Controller.SendAllClientScoreData();
                frameCount = 0;
            }
        }
    }

    public void AddNewUser(string _userID)
    {
        //ユーザーの追加
        var add = Instantiate(userPrefab, userList.transform) as GameObject;
        add.name = _userID;
        add.transform.position = new Vector3(count, 0.0f, 0.0f);
        add.GetComponent<UserController>().SetUserID(_userID);
        count++;
    }

    public void UsersUpdate()
    {
        users = userList.GetComponentsInChildren<UserController>();
    }
}

