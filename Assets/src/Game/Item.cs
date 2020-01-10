using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] int ItemID=0;
    private TCP_ServerController tcpController;
    public bool flg { get; private set; } = false;
    // Start is called before the first frame update
    void Start()
    {
        tcpController=GameObject.FindGameObjectWithTag("Server").GetComponent<TCP_ServerController>();
    }

    void Update()
    {
     if(flg) Destroy(this.gameObject);
    }
    public WEAPONTYPE GetItem()
    {
        flg = true;
        tcpController.AllClientSend((byte)GameHeader.ID.ALERT,(byte)GameHeader.GameCode.GRENEDEDATA,Convert.Conversion(ItemID));      
        return WEAPONTYPE.UMP45;
    }
}
