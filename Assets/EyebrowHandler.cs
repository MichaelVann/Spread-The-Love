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

        Vector3 newEuler = transform.localEulerAngles;
        newEuler.z = 22f * a_loveScale;
        transform.localEulerAngles = newEuler;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
