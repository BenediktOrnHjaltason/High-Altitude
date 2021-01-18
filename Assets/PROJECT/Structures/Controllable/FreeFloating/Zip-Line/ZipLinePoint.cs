﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;
using UnityEditor;

public class ZipLinePoint : MonoBehaviour
{
    [SerializeField]
    EZipLine point;

    GameObject PF_TransportLine;

    [SerializeField]
    GameObject otherPoint;

    GameObject transportLine;
    ZipLineTransport zipLineTransport;

    Vector3 startToEnd;


    // Start is called before the first frame update
    void Start()
    {
        PF_TransportLine = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/PROJECT/Structures/Controllable/FreeFloating/Zip-Line/Resources/PF_TransportLine.prefab", typeof(GameObject));
        transportLine = Instantiate<GameObject>(PF_TransportLine, transform);
       

        GetComponentInParent<StructureLocal>().AddSubObject(transportLine);

        zipLineTransport = transportLine.GetComponent<ZipLineTransport>();
    }

    void FixedUpdate()
    {
        if (transportLine)
        {
            startToEnd = otherPoint.transform.position - transform.position;
            zipLineTransport.TransportDirection = startToEnd;

            if (Vector3.Dot(startToEnd.normalized, transform.forward) > 0.96f && Vector3.Dot(-startToEnd.normalized, otherPoint.transform.forward) > 0.96f)
            {
                transportLine.transform.SetPositionAndRotation(transform.position + (startToEnd / 2), Quaternion.LookRotation(startToEnd));
                transportLine.transform.localScale = new Vector3(0.25f, 0.25f, startToEnd.magnitude);
            }

            else
            {
                transportLine.transform.position = transform.position;
                transportLine.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
            }
        }
    }
}
