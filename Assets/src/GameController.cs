using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using UnityEngine;


public class GameController : MonoBehaviour
{
    private GameObject userPrefab;
    private int count = 0;

    //FPS回数
    int frameCount;
    float prevTime;


    // Start is called before the first frame update
    void Start()
    {
        userPrefab = (GameObject)Resources.Load("user");

        //FPS回数
        frameCount = 0;
        prevTime = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        //FPS表示
        ++frameCount;
        float time = Time.realtimeSinceStartup - prevTime;

        if (time >= 0.5f)
        {
            Debug.LogFormat("{0}fps", frameCount / time);

            frameCount = 0;
            prevTime = Time.realtimeSinceStartup;
        }
    }

    public void AddNewUser(string _userID)
    {
        //ユーザーの追加
        var add = Instantiate(userPrefab, new Vector3(count, 0.0f, 0.0f), Quaternion.identity) as GameObject;
        add.name = _userID;
        add.GetComponent<UserController>().SetUserID(_userID);
        count++;

    }
}
