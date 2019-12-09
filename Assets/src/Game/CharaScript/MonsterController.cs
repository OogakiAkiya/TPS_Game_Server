using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : BaseController
{
    [SerializeField]BaseComponent next;
    private MonsterComponent castCurrent;
    public int changeCounter=0;
    override protected void Awake()
    {
        ChangeModele("Maynard");

        /*
        if (current) Destroy(current.gameObject);
        current = Instantiate((GameObject)Resources.Load("Maynard"), this.transform).GetComponent<BaseComponent>();
        current.myController = this;
        base.Awake();
        base.Start(); // 親クラスのメソッドを呼ぶ
        */
    }

    override protected void Update()
    {
        if (!IsInvoking("Second5000Invoke")) Invoke("Second5000Invoke", 5f);

        if (changeCounter > 100&&castCurrent.monsterType==MonsterType.MAYNARD)
        {
            changeCounter = 0;
            ChangeModele("Mutant");
        }
        base.Update();    // 親クラスのメソッドを呼ぶ
    }

    //virtual
    override public byte[] GetStatus()
    {
        List<byte> returnData = new List<byte>();
        GameHeader header = new GameHeader();
        header.CreateNewData(GameHeader.ID.GAME, this.type, this.name, (byte)GameHeader.GameCode.BASICDATA);
        userData.SetData(this.transform.position, this.transform.localEulerAngles, (int)userAnimation.animationState.currentKey, hp);
        returnData.AddRange(header.GetHeader());
        returnData.Add((byte)castCurrent.monsterType);

        returnData.AddRange(userData.GetData());
        returnData.AddRange(Convert.Conversion(changeCounter));
        if (current.weapon != null) returnData.AddRange(current.weapon.GetStatus());

        return returnData.ToArray();
    }

    override public byte[] GetStatusComplete()
    {
        List<byte> returnData = new List<byte>();
        GameHeader header = new GameHeader();
        header.CreateNewData(GameHeader.ID.GAME, this.type, this.name, (byte)GameHeader.GameCode.CHECKDATA);
        userData.SetData(this.transform.position, this.transform.localEulerAngles, (int)userAnimation.animationState.currentKey, hp);
        returnData.AddRange(header.GetHeader());
        returnData.Add((byte)castCurrent.monsterType);

        returnData.AddRange(userData.GetCompleteData());
        if (current.weapon != null) returnData.AddRange(current.weapon.GetStatus());
        return returnData.ToArray();

    }

    public void ChangeModele(string _pass)
    {
        if(current)Destroy(current.gameObject);
        current = Instantiate((GameObject)Resources.Load(_pass), this.transform).GetComponent<BaseComponent>();
        current.myController = this;
        current.Init();
        castCurrent = (MonsterComponent)current;
        base.Start(); // 親クラスのメソッドを呼ぶ

    }

    public override void End()
    {
        base.End();
        changeCounter = 0;
    }

    private void Second5000Invoke()
    {
        changeCounter+=10;

    }
}