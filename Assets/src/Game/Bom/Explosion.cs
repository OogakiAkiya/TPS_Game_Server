using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] int damage=100;
    protected System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
    private float range=4.5f;
    public BaseController userController;
    public List<BaseController> targets=new List<BaseController>();


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
        if (this.transform.localScale.x >= range) Destroy(this.gameObject);

        for(int i = 0; i < targets.Count; i++)
        {
            if (targets[i].Damage(damage)) userController.killAmount++;
        }
        targets.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "users") return;
        targets.Add(other.GetComponent<BaseController>());
    }
}
