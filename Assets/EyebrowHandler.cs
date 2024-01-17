using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyebrowHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    internal void SetEybrowRotation(float a_loveScale)
    {
        float scale = 2f * (a_loveScale - 0.5f);

        Vector3 newEuler = transform.localEulerAngles;
        newEuler.z = 22f * scale;
        transform.localEulerAngles = newEuler;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
