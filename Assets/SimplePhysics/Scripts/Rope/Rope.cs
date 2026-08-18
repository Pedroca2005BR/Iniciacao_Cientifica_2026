using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    [Header("RigidBodies")]
    public List<Transform> points = new List<Transform>();

    [Header("Appearance")]
    public Material lineMaterial;
    public bool showLineRenderers = true;

    [Header("Bones & Rope")]
    public Transform Bone0;
    private LineRenderer lr;

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
        lr.startWidth = 0.189f;
        lr.endWidth = 0.189f;
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
        for (int i = 0; i < points.Count; i++)
        {
            if (i == 0) lr.SetPosition(0, points[1].position);
            else lr.SetPosition(i, points[i].position);
        }
    }

    private void Update()
    {
        Bone0.transform.localPosition = new Vector3(0f, 0f, 0f);
    }

}

