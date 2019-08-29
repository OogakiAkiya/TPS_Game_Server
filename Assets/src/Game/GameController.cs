﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using UnityEngine;


public class GameController : MonoBehaviour
{
    private GameObject userPrefab;
    private int count = 0;
    //public GameObject[] users { get; private set; } = new GameObject[0];
    public List<UserController> users { get; private set; } = new List<UserController>();
    private UDP_ServerController udp_Controller;
    //FPS回数
    int frameCount;
    float prevTime;


    // Start is called before the first frame update
    void Start()
    {
        udp_Controller=this.GetComponent<UDP_ServerController>();
        userPrefab = (GameObject)Resources.Load("user");

        //FPS回数
        frameCount = 0;
        prevTime = 0.0f;

        //Update回数制御
        
        QualitySettings.vSyncCount = 0; // VSyncをOFFにする
        //Application.targetFrameRate = 160; // ターゲットフレームレートを160に設定
        
    }

    // Update is called once per frame
    void Update()
    {
        //FPS表示
        float time = Time.realtimeSinceStartup - prevTime;

        //60fps
        if (time >= 0.016f)
        {
            //Debug.LogFormat("{0}fps", frameCount / time);
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
        /*
        Dictionary<string, int> dic = new Dictionary<string, int>();
        foreach (var user in users)
        {
            dic.Add(user.name, user.killAmount);
        }
        var sorted = dic.OrderByDescending((x) => x.Value);     //降順ソート
        */

        /*
        KeyValuePair<String, int>[] ranking = new KeyValuePair<string, int>[5];
        for (int i = 0; i < ranking.Length; i++) ranking[i] = new KeyValuePair<string, int>("", 0);
        foreach (var user in users)
        {
            for (int i = 0; i < ranking.Length; i++)
            {
                if (ranking[i].Value < user.killAmount) {

                }
            }
        }
        */
    }

    public void AddNewUser(string _userID)
    {
        //ユーザーの追加
        var add = Instantiate(userPrefab, new Vector3(count, 0.0f, 0.0f), Quaternion.identity) as GameObject;
        add.name = _userID;
        add.GetComponent<UserController>().SetUserID(_userID);
        count++;

    }

    public void UsersUpdate()
    {
        users.Clear();
        foreach(var user in GameObject.FindGameObjectsWithTag("users"))
        {
            users.Add(user.GetComponent<UserController>());
        }
    }
}

