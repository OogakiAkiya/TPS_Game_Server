using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] int damage=100;
    protected System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
    private float range=4.5f;
    public BaseController userController;


    // Start is called before the first frame update
    void Start()
    {
        timer.Start();
        this.transform.localScale = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (this.transform.localScale.x < range) this.transform.localScale = this.transform.localScale + new Vector3(0.2f, 0.2f, 0.2f);
        if (this.transform.localScale.x >= range){
            Collider[] hitColliders = Physics.OverlapSphere(this.transform.position, range, 1 << 10);
            for(int i=0;i< hitColliders.Length; i++)
            {
                BaseController controller=hitColliders[i].GetComponent<BaseController>();
                if (!controller) controller = hitColliders[i].transform.parent.GetComponent<BaseController>();
                if(controller)controller.Damage(damage);
            }

            Destroy(this.gameObject);
        }

    }

}
