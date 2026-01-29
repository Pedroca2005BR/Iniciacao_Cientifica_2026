using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RopeConfigurableJoint : MonoBehaviour
{
    [Header("References")]
    public Transform anchorPoint;                // colete
    public Rigidbody attachEndRigidbody;         // garra
    public GameObject segmentPrefab;             // prefab com Collider + Rigidbody

    [Header("Rope")]
    public int segmentCount = 18;
    public float segmentLength = 0.12f;
    public float segmentMass = 0.2f;

    [Header("Joint limits / stiffness")]
    public float linearLimitSpring = 800f;       // for�a que evita alongamento
    public float linearLimitDamping = 50f;
    public float linearLimit = 0.13f;            // toler�ncia (>= segmentLength)
    [Range(0f, 180f)] public float angularLimitDegrees = 30f;

    [Header("Blinking")]
    public float warningStretchRatio = 1.06f;
    public Color normalColor = Color.white;
    public Color alertColor = Color.red;
    public float blinkSpeed = 6f;

    [Header("Physics tuning (segments)")]
    public float linearDrag = 0.2f;
    public float angularDrag = 0.6f;

    private List<Rigidbody> segments = new List<Rigidbody>();
    private LineRenderer lr;
    private float restLength;
    private bool isWarning = false;
    private Coroutine blinkCoroutine;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }

    void Start()
    {
        BuildRope();
        restLength = CalculateLength();
        SetLineColor(normalColor);
    }

    void FixedUpdate()
    {
        UpdateLinePositions();
        float current = CalculateLength();
        if (current / restLength >= warningStretchRatio)
        {
            if (!isWarning) StartWarning();
        }
        else
        {
            if (isWarning) StopWarning();
        }
    }

    public void BuildRope()
    {
        ClearExisting();

        if (segmentPrefab == null || anchorPoint == null)
        {
            Debug.LogError("RopeConfigurableJoint: falta segmentPrefab ou anchorPoint.");
            return;
        }

        Vector3 start = anchorPoint.position;
        Vector3 end = attachEndRigidbody ? attachEndRigidbody.position : start - Vector3.up * (segmentCount * segmentLength);

        // Create anchor holder (kinematic) so colete n�o recebe for�as estranhas
        GameObject anchorHolder = new GameObject("RopeAnchorHolder");
        anchorHolder.transform.position = start;
        anchorHolder.transform.SetParent(anchorPoint, true);
        Rigidbody anchorRb = anchorHolder.AddComponent<Rigidbody>();
        anchorRb.isKinematic = true;

        for (int i = 0; i < segmentCount; i++)
        {
            // position equally spaced
            float t = (float)(i + 1) / (segmentCount + 1);
            Vector3 pos = Vector3.Lerp(start, end, t);

            GameObject go = Instantiate(segmentPrefab, pos, Quaternion.identity, transform);
            Rigidbody rb = go.GetComponent<Rigidbody>();
            if (rb == null) rb = go.AddComponent<Rigidbody>();

            rb.mass = segmentMass;
            rb.linearDamping = linearDrag;
            rb.angularDamping = angularDrag;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            // Ensure collider center/pivot is roughly centered to avoid torque bias
            segments.Add(rb);

            // Create configurable joint connecting this segment to previous (or anchor)
            ConfigurableJoint cj = go.GetComponent<ConfigurableJoint>();
            if (cj == null) cj = go.AddComponent<ConfigurableJoint>();

            if (i == 0)
            {
                // connect first segment to anchor holder
                cj.connectedBody = anchorRb;
            }
            else
            {
                cj.connectedBody = segments[i - 1];
            }

            // Linear motion: limited so we can resist stretch
            cj.xMotion = ConfigurableJointMotion.Limited;
            cj.yMotion = ConfigurableJointMotion.Limited;
            cj.zMotion = ConfigurableJointMotion.Limited;

            SoftJointLimit sjl = new SoftJointLimit();
            sjl.limit = linearLimit; // allow small slack
            cj.linearLimit = sjl;

            SoftJointLimitSpring sjls = new SoftJointLimitSpring();
            sjls.spring = linearLimitSpring;
            sjls.damper = linearLimitDamping;
            cj.linearLimitSpring = sjls;

            // Angular: limited a pouco para evitar tor��o excessiva (reduz bug de "helic�ptero")
            cj.angularXMotion = ConfigurableJointMotion.Limited;
            cj.angularYMotion = ConfigurableJointMotion.Limited;
            cj.angularZMotion = ConfigurableJointMotion.Limited;

            SoftJointLimit angLimit = new SoftJointLimit();
            angLimit.limit = angularLimitDegrees;
            cj.lowAngularXLimit = angLimit;
            cj.highAngularXLimit = angLimit;
            cj.angularYLimit = angLimit;
            cj.angularZLimit = angLimit;

            // Some damping: use slerp drive lightly to stabilize rotations
            JointDrive slerp = new JointDrive();
            slerp.positionSpring = 0f;
            slerp.positionDamper = 5f;
            slerp.maximumForce = 1000f;
            cj.slerpDrive = slerp;

            cj.swapBodies = false;
            // set anchors so rest distance roughly equals segmentLength
            // anchors default zero; we can leave them as is since initial positions set the distance.
        }

        // Connect last to attach object (optional)
        if (attachEndRigidbody != null && segments.Count > 0)
        {
            // attach last segment to attachEndRigidbody with a configurable joint as fixed
            Rigidbody lastRb = segments[segments.Count - 1];
            GameObject attConnector = lastRb.gameObject;
            ConfigurableJoint cj = attConnector.GetComponent<ConfigurableJoint>();
            if (cj == null) cj = attConnector.AddComponent<ConfigurableJoint>();
            cj.connectedBody = attachEndRigidbody;

            // make connection fairly tight
            cj.xMotion = ConfigurableJointMotion.Limited;
            cj.yMotion = ConfigurableJointMotion.Limited;
            cj.zMotion = ConfigurableJointMotion.Limited;
            SoftJointLimit sjl = new SoftJointLimit() { limit = linearLimit };
            cj.linearLimit = sjl;
            SoftJointLimitSpring sjls = new SoftJointLimitSpring() { spring = linearLimitSpring, damper = linearLimitDamping };
            cj.linearLimitSpring = sjls;
        }

        lr.positionCount = segments.Count + 2;
        UpdateLinePositions();
    }

    void ClearExisting()
    {
        StopWarning();
        foreach (Transform child in transform) Destroy(child.gameObject);
        segments.Clear();
    }

    public float CalculateLength()
    {
        float len = 0f;
        Vector3 prev = anchorPoint.position;
        foreach (var rb in segments)
        {
            len += Vector3.Distance(prev, rb.position);
            prev = rb.position;
        }
        if (attachEndRigidbody != null)
            len += Vector3.Distance(prev, attachEndRigidbody.position);
        else
            len += segmentLength;
        return len;
    }

    void UpdateLinePositions()
    {
        int idx = 0;
        lr.SetPosition(idx++, anchorPoint.position);
        foreach (var rb in segments) lr.SetPosition(idx++, rb.position);
        Vector3 lastPos = attachEndRigidbody ? (Vector3)attachEndRigidbody.position : segments[segments.Count - 1].position + Vector3.down * segmentLength;
        lr.SetPosition(idx++, lastPos);
    }

    // --- Blinking ---
    void StartWarning()
    {
        isWarning = true;
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        blinkCoroutine = StartCoroutine(BlinkCoroutine());
    }

    void StopWarning()
    {
        isWarning = false;
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        SetLineColor(normalColor);
    }

    IEnumerator BlinkCoroutine()
    {
        float t = 0f;
        while (true)
        {
            t += Time.deltaTime * blinkSpeed;
            float a = Mathf.PingPong(t, 1f);
            Color c = Color.Lerp(normalColor, alertColor, a);
            SetLineColor(c);
            yield return null;
        }
    }

    void SetLineColor(Color c)
    {
        lr.startColor = c;
        lr.endColor = c;
        if (lr.material != null && lr.material.HasProperty("_Color")) lr.material.color = c;
    }
}
