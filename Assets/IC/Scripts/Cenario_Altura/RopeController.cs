using UnityEngine;

public class RopeController : MonoBehaviour
{
    [Header("Rope setup")]
    public GameObject segmentPrefab;
    public int segmentCount = 12;
    public float segmentLength = 0.2f;

    [Header("Tension limits")]
    public float warnThreshold = 80f;
    public float breakThreshold = 120f;

    [Header("Visuals")]
    public Renderer ropeRenderer;   // arraste o objeto que mostra a corda
    public Color normalColor = Color.white;
    public Color warningColor = Color.red;
    public float blinkSpeed = 10f;

    Rigidbody[] segments;
    ConfigurableJoint[] joints;
    bool isBroken = false;
    float currentTension;

    void Start()
    {
        segments = new Rigidbody[segmentCount];
        joints = new ConfigurableJoint[segmentCount - 1];

        Vector3 spawnPos = transform.position;
        Rigidbody prevRb = null;
        for (int i = 0; i < segmentCount; i++)
        {
            GameObject go = Instantiate(segmentPrefab, spawnPos + Vector3.down * i * segmentLength, Quaternion.identity, transform);
            Rigidbody rb = go.GetComponent<Rigidbody>();
            segments[i] = rb;

            if (prevRb != null)
            {
                var joint = go.AddComponent<ConfigurableJoint>();
                joint.connectedBody = prevRb;

                joint.xMotion = ConfigurableJointMotion.Limited;
                joint.yMotion = ConfigurableJointMotion.Limited;
                joint.zMotion = ConfigurableJointMotion.Limited;

                SoftJointLimit lim = new SoftJointLimit();
                lim.limit = 0.05f;
                joint.linearLimit = lim;

                joints[i - 1] = joint;
            }
            prevRb = rb;
        }
    }

    void FixedUpdate()
    {
        if (isBroken || joints.Length == 0) return;

        // exemplo: pegar força no primeiro joint
        var joint = joints[0];
        if (joint == null) return;

        Vector3 f = joint.currentForce;
        Vector3 dir = (joint.connectedBody.worldCenterOfMass - joint.GetComponent<Rigidbody>().worldCenterOfMass).normalized;
        currentTension = Vector3.Dot(f, dir);

        //if (currentTension > breakThreshold)
        //    BreakRope();
        if (currentTension > warnThreshold)
            WarningEffect();
        else
            ResetVisual();
    }

    void WarningEffect()
    {
        if (ropeRenderer != null)
        {
            float t = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            ropeRenderer.material.color = Color.Lerp(normalColor, warningColor, t);
        }
    }

    void ResetVisual()
    {
        if (ropeRenderer != null)
            ropeRenderer.material.color = normalColor;
    }

    void BreakRope()
    {
        isBroken = true;
        ResetVisual();

        // destrói todos os joints
        foreach (var j in joints)
            if (j != null) Destroy(j);

        // opcional: feedback
        Debug.Log("Corda rompeu!");
        // aqui você pode tocar som, soltar partículas, etc
    }
}
