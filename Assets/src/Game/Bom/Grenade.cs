using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    protected System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
    private bool posFlg = false;
    private Vector3 pos;
    private Vector3 direction;
    public bool destroyFlg { get; private set; } = false;
    public string player;
    GameObject explosion;
    // Start is called before the first frame update
    void Start()
    {
        timer.Start();
        direction = this.transform.forward + this.transform.up;
        this.GetComponent<Rigidbody>().AddForce(direction.normalized * 250);
    }

    // Update is called once per frame
    void Update()
    {
        if (timer.ElapsedMilliseconds > 5000)
        {
            
            explosion = Instantiate(Resources.Load("Explosion") as GameObject) as GameObject;
            explosion.SetActive(true);
            explosion.transform.position = this.transform.position;
           
            destroyFlg = true;
            timer.Reset();
        }
    }

    public byte[] GetStatus()
    {
        if (!posFlg)
        {
            pos = this.transform.position;
            posFlg = true;
        }


        List<byte> data = new List<byte>();
        GameHeader head = new GameHeader();
        head.CreateNewData(GameHeader.ID.GAME,GameHeader.UserTypeCode.GRENEDED,this.name, (byte)GameHeader.GameCode.GRENEDEDATA);
        data.AddRange(head.GetHeader());
        data.AddRange(Convert.GetByteVector3(pos));
        data.AddRange(Convert.GetByteVector3(direction));

        return data.ToArray();
    }

    public void Delete(BaseController userController)
    {
        explosion.GetComponent<Explosion>().userController = userController;
        Destroy(this.gameObject);
    }
}
