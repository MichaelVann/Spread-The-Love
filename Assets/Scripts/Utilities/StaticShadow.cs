using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;

public class StaticShadow : ObjectShadow
{
    public override void Start()
    {
        base.Start();
        float eulerAnglesForShadow = transform.eulerAngles.z + _shadowAngle;
        float x = Mathf.Sin(eulerAnglesForShadow * Mathf.PI / 180f) / transform.parent.transform.localScale.x;
        float y = Mathf.Cos(eulerAnglesForShadow * Mathf.PI / 180f) / transform.parent.transform.localScale.y;
        float z = m_height / transform.parent.transform.localScale.y;
        transform.localPosition = new Vector3(x, y) * _shadowDistance + new Vector3(0f, -z);
    }

    public override void Update()
    {

    }
}
