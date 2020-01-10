using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanController : BaseController
{
    [SerializeField] Vector3 serchRange = new Vector3(0.55f, 0.3f, 0.55f);

    override protected void Awake()
    {
        type = GameHeader.UserTypeCode.SOLDIER;
        base.Awake();
    }

    override protected void Update()
    {
        base.Update();

        //現在はE
        if (nowKey.HasFlag(KEY.RIGHT_CLICK))
        {
            SerchItem();
        }
    }

    private void SerchItem()
    {
        Vector3 vector = this.transform.position + this.transform.forward * 0.3f + this.transform.up;
        //Vector3 vector = this.transform.forward * 0.4f + new Vector3(0, 1, 0.2f);
        Collider[] colliders = Physics.OverlapBox(vector, serchRange, this.transform.localRotation, 1 << 11);
        for (int i = 0; i < colliders.Length; i++)
        {
            Item item=colliders[i].GetComponent<Item>();
            if (item==null||item.flg) break;
            WEAPONTYPE type = item.GetItem();
            if(type==WEAPONTYPE.UMP45)current.weaponList.Add(new UMP45(current.Attack));
        }

    }
}
