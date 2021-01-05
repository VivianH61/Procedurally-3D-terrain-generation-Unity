using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sunMove : MonoBehaviour
{
    public static float day = 0;
    //private float speed = 0.1f;
    public Material skybox;
    private float exposure_rate=1.0f;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.transform.Rotate(90,0,0);
        skybox.SetFloat("_Exposure", 1.2f);
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.rotation = Quaternion.Euler(new Vector3(90+day, 0, 0));
        exposure_rate = (1 - System.Math.Abs(day) / 90.0f);
        skybox.SetFloat("_Exposure", 0.2f + exposure_rate);
    }

    public void setDayValue(float d) {
        day = d;
    }
}
