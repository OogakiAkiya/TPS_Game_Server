using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UserController : MonoBehaviour
{
    public string userId;
    public string IPaddr { get; set; }
    public Key nowKey { get; private set; } = 0;

    private List<byte[]> recvDataList = new List<byte[]>();
    private List<Key> inputKeyList = new List<Key>();
    private Animator animator;
    private AnimatorBehaviour animatorBehaviour;
    private UserAnimation userAnimation;
    public int hp  = 100;

    void Start()
    {
        animator = this.GetComponent<Animator>();
        animatorBehaviour = animator.GetBehaviour<AnimatorBehaviour>();
        userAnimation = this.GetComponent<UserAnimation>();
    }

    // Update is called once per frame
    void Update()
    {
        if (inputKeyList.Count > 0)
        {
            //入力値取得
            Key inputKey = GetInputKey();
            //現在のキーを保存
            Key oldKey = nowKey;
            //新しいキー入力を加算
            nowKey |= inputKey;
            //二度目のキー入力でフラグOFF
            nowKey = oldKey ^ inputKey;
        }

        if (recvDataList.Count > 0)
        {
            byte[] recvData = GetRecvData();
        }
    }

    public void SetUserID(string _userId)
    {
        userId = _userId;
        this.name = userId;

    }

    public void AddRecvData(byte[] _addData)
    {
        recvDataList.Add(_addData);
    }

    public void AddInputKeyList(Key _addData)
    {
        inputKeyList.Add(_addData);
    }

    public Key GetInputKey()
    {
        Key returnData;
        returnData = inputKeyList[0];
        inputKeyList.RemoveAt(0);
        return returnData;
    }

    private byte[] GetRecvData()
    {
        byte[] returnData;
        returnData = recvDataList[0];
        recvDataList.RemoveAt(0);
        return returnData;
    }


    public byte[] GetStatus()
    {
        List<byte> returnData = new List<byte>();
        System.Text.Encoding enc = System.Text.Encoding.UTF8;
        byte[] userName = enc.GetBytes(System.String.Format("{0, -" + HeaderConstant.USERID_LENGTH + "}",this.name));              //12byteに設定する
        byte[] positionData = Convert.GetByteVector3(this.transform.position);
        byte[] rotationData = Convert.GetByteVector3(this.transform.localEulerAngles);
        int currentKey = 0;
        if (userAnimation) currentKey = (int)userAnimation.animationState.currentKey;

        returnData.Add(HeaderConstant.ID_GAME);
        returnData.AddRange(userName);
        returnData.Add(HeaderConstant.CODE_GAME_BASICDATA);
        returnData.AddRange(positionData);
        returnData.AddRange(rotationData);
        returnData.AddRange(BitConverter.GetBytes(currentKey));
        returnData.AddRange(BitConverter.GetBytes(hp));
        return returnData.ToArray();
    }

}
