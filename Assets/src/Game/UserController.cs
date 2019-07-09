using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Key : short
{
    W = 0x001,
    S = 0x002,
    A = 0x004,
    D = 0x008,
    SHIFT = 0x010,
    G = 0x020,
    R = 0x040,
    LEFT_BUTTON = 0x080,
    RIGHT_BUTTON = 0x100
}

public enum AnimationKey :int
{
    Idle,
    Walk,
    Run,
    Jump
}

public class UserController : MonoBehaviour
{
    public string userId;
    public float walkSpeed = 1.0f;
    public float runSpeed = 2.0f;

    public string IPaddr { get; set; }
    private List<byte[]> recvDataList = new List<byte[]>();
    private List<Key> inputKeyList = new List<Key>();
    private Key nowKey=0;
    public StateMachine<AnimationKey> animationState { get; private set; } = new StateMachine<AnimationKey>();

    public int hp { get; set; } = 100;

    void Start()
    {
        AddStates();
        animationState.ChangeState(AnimationKey.Idle);
    }

    public void SetUserID(string _userId)
    {
        userId = _userId;
        this.name = _userId;

    }

    // Update is called once per frame
    void Update()
    {
        if (recvDataList.Count > 0)
        {
            byte[] recvData = GetRecvData();
        }

        InputRoutine();

        animationState.Update();
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

    private void InputRoutine()
    {
        while (inputKeyList.Count > 0)
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
    }

    private void AddStates()
    {
        //Idle
        animationState.AddState(AnimationKey.Idle,
            _update: () =>
            {
                if ((short)nowKey << 12 > 0) animationState.ChangeState(AnimationKey.Walk);
            });

        //Walk
        animationState.AddState(AnimationKey.Walk,
            _update: () =>
            {
                //WASDのどれか一つでも押されているかチェック
                if((short)nowKey << 12 == 0)
                {
                    animationState.ChangeState(AnimationKey.Idle);
                    return;
                }

                //SHIFTが押されているかチェック
                if (nowKey.HasFlag(Key.SHIFT))
                {
                    animationState.ChangeState(AnimationKey.Run);
                    return;
                }

                Move(walkSpeed);
            });

        //Run
        animationState.AddState(AnimationKey.Run,
            _update: () =>
            {
                //WASDのどれか一つでも押されているかチェック
                if ((short)nowKey << 12 == 0)
                {
                    animationState.ChangeState(AnimationKey.Idle);
                    return;
                }

                //SHIFTが押されているかチェック
                if(!nowKey.HasFlag(Key.SHIFT))
                {
                    animationState.ChangeState(AnimationKey.Walk);
                    return;
                }

                Move(runSpeed);
            });

        //Jump
        animationState.AddState(AnimationKey.Jump,
            _update: () =>
            {
            }
            );
    }

    private void Move(float _moveSpeed)
    {
        //移動量算出
        Vector3 velocity = Vector3.zero;
        if (nowKey.HasFlag(Key.W)) velocity += this.transform.forward;
        if (nowKey.HasFlag(Key.S)) velocity += -this.transform.forward;
        if (nowKey.HasFlag(Key.A)) velocity += -this.transform.right;
        if (nowKey.HasFlag(Key.D)) velocity += this.transform.right;

        //移動
        this.transform.position += velocity.normalized * _moveSpeed * Time.deltaTime;
    }
}
