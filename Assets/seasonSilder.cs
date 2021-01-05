using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class seasonSilder : MonoBehaviour
{
    public GameObject Snow;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void seasonChange(float s) {
        int season = (int) (Mathf.Floor (s / 0.125f)) + 1;
        if (season <= 6) {
            Snow.SetActive(false);
        } else {
            Snow.SetActive(true);
        }

    }
}
