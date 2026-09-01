using System;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    [Header("RigidBodies")]
    public List<Transform> points = new List<Transform>();
    [SerializeField] public float distanciaCordas;

    [Header("Appearance")]
    public Material lineMaterial;
    public bool showLineRenderers = true;

    [Header("Bones & Rope")]
    public Rigidbody BoneGarra;
    private LineRenderer lr;
    private int primeira = 0;

    public void Init(List<Transform> pts, Material lineMaterial)
    {
        points = pts;
        this.lineMaterial = lineMaterial;
    }

    private void Awake()
    {
        if (!showLineRenderers)
            return;

        lr = gameObject.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.positionCount = points.Count;
        lr.material = lineMaterial;
        lr.startWidth = 0.02f;
        lr.endWidth = 0.02f;
        lr.generateLightingData = true;
    }

    void LateUpdate()
    {
        if (!showLineRenderers || lr == null || points == null || points.Count == 0)
            return;

        UpdateLineRenderer();
    }

    void UpdateLineRenderer()
    {
        //if (verifyBonesDistance() || 0 == primeira++)
        //{
            for (int i = 0; i < points.Count; i++)
            {
                if (i == 0) lr.SetPosition(0, points[1].position);
                else lr.SetPosition(i, points[i].position);
            }
        //}
    }

    private void Update()
    {
        points[0].localPosition = new Vector3(0f, 0f, 0f);
    }

    bool verifyBonesDistance()
    {
        for (int i = 0; i < points.Count-1; i++)
        {
            var valor = (float)Math.Round((double)Vector3.Distance(points[i].position, points[i+1].position), 2);
            if (valor <= 1.2f)
            {
                Debug.Log("Menor: " + valor);
            }
            else
            {
                if (BoneGarra.isKinematic == false) return true;
                lr.SetPosition(18, points[18].position);
                Debug.Log("Maior: "+ i + " " + valor);
                return false;
            }
            }
            return true;
    }
}

