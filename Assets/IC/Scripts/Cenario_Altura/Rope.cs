using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Rope : MonoBehaviour
{
    [Header("References")]
    public Transform anchorPoint;        // ponto fixo (colete)
    public Rigidbody attachEndRigidbody; // garra (o objeto que pode cair)
    public GameObject segmentPrefab;     // prefab do segmento (com Rigidbody + Collider)

    [Header("Rope settings")]
    public int segmentCount = 20;
    public float segmentLength = 0.15f;
    public float segmentMass = 0.2f;

    [Header("Stretch / Warning")]
    [Range(1.0f, 2.0f)] public float maxAllowedStretch = 1.15f; // nunca permitir� *excessivo*, s� monitoramos
    [Range(1.0f, 1.2f)] public float warningStretchRatio = 1.05f; // quando come�ar a piscar
    public float stretchStiffness = 200f; // for�a corretiva para reduzir alongamento (maior = mais r�gido)
    public float stretchDamping = 5f;

    [Header("Blinking")]
    public Color normalColor = Color.white;
    public Color alertColor = Color.red;
    public float blinkSpeed = 6f; // quanto maior, mais r�pido pisca

    // Runtime
    private List<Rigidbody> segments = new List<Rigidbody>();
    private LineRenderer lr;
    private float restLength = 0f;
    private bool isWarning = false;
    private Coroutine blinkCoroutine;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }

    void Start()
    {
        BuildRope();
        restLength = CalculateLength(); // comprimento inicial
        SetLineColor(normalColor);
    }

    void FixedUpdate()
    {
        ApplyStretchCorrection();
        float current = CalculateLength();

        // Warning check
        if (current / restLength >= warningStretchRatio)
        {
            if (!isWarning) StartWarning();
        }
        else
        {
            if (isWarning) StopWarning();
        }
    }

    // --- BUILD ---
    public void BuildRope()
    {
        ClearExisting();

        if (segmentPrefab == null || anchorPoint == null)
        {
            Debug.LogError("Rope: falta segmentPrefab ou anchorPoint.");
            return;
        }

        Vector3 startPos = anchorPoint.position;
        Vector3 endPos = attachEndRigidbody ? attachEndRigidbody.position : startPos - Vector3.up * (segmentCount * segmentLength);

        // create segments linearly between anchorPoint and endPos
        for (int i = 0; i < segmentCount; i++)
        {
            float t = (float)(i + 1) / (segmentCount + 1); // keep free both ends usable
            Vector3 pos = Vector3.Lerp(startPos, endPos, t);

            GameObject go = Instantiate(segmentPrefab, pos, Quaternion.identity, transform);
            Rigidbody rb = go.GetComponent<Rigidbody>();
            if (rb == null) rb = go.AddComponent<Rigidbody>();

            rb.mass = segmentMass;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            segments.Add(rb);

            // connect to previous
            if (i > 0)
            {
                Rigidbody prev = segments[i - 1];
                HingeJoint hj = go.GetComponent<HingeJoint>();
                if (hj == null) hj = go.AddComponent<HingeJoint>();
                hj.connectedBody = prev;
                hj.anchor = Vector3.zero;
                hj.autoConfigureConnectedAnchor = true;
                hj.useLimits = false;
            }
        }

        // fix top: attach first segment rigidbody to anchorPoint by making a ConfigurableJoint / or kinematic anchor
        Rigidbody first = segments[0];
        // we'll keep anchor fixed by kinematic anchor object
        GameObject anchorHolder = new GameObject("RopeAnchorHolder");
        anchorHolder.transform.position = anchorPoint.position;
        anchorHolder.transform.SetParent(anchorPoint, true);
        Rigidbody anchorRb = anchorHolder.AddComponent<Rigidbody>();
        anchorRb.isKinematic = true;

        HingeJoint topHj = first.GetComponent<HingeJoint>();
        if (topHj == null) topHj = first.gameObject.AddComponent<HingeJoint>();
        topHj.connectedBody = anchorRb;
        topHj.autoConfigureConnectedAnchor = true;

        // connect last segment to attachEndRigidbody (garra) if fornecido
        if (attachEndRigidbody != null && segments.Count > 0)
        {
            Rigidbody last = segments[segments.Count - 1];
            FixedJoint fj = last.gameObject.GetComponent<FixedJoint>();
            if (fj == null) fj = last.gameObject.AddComponent<FixedJoint>();
            fj.connectedBody = attachEndRigidbody;
            // ensure joint doesn't break: don't set breakForce (ou set muito alto)
            fj.breakForce = Mathf.Infinity;
            fj.breakTorque = Mathf.Infinity;
        }

        // Setup LineRenderer
        lr.positionCount = segments.Count + 2; // anchor + segments + end (attach)
        UpdateLinePositions();
    }

    void ClearExisting()
    {
        // delete old segments children
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        segments.Clear();
    }

    // --- LENGTH & RENDER ---
    public float CalculateLength()
    {
        float len = 0f;
        Vector3 prev = anchorPoint.position;

        foreach (var rb in segments)
        {
            len += Vector3.Distance(prev, rb.position);
            prev = rb.position;
        }

        // include last connection to attachEnd if present
        if (attachEndRigidbody != null)
            len += Vector3.Distance(prev, attachEndRigidbody.position);
        else
            len += Vector3.Distance(prev, prev + Vector3.down * segmentLength);

        UpdateLinePositions();
        return len;
    }

    void UpdateLinePositions()
    {
        int idx = 0;
        lr.SetPosition(idx++, anchorPoint.position);
        foreach (var rb in segments)
        {
            lr.SetPosition(idx++, rb.position);
        }

        Vector3 lastPos = (attachEndRigidbody != null) ? (Vector3)attachEndRigidbody.position : segments[segments.Count - 1].position + Vector3.down * segmentLength;
        lr.SetPosition(idx++, lastPos);
    }

    // --- STRETCH CORRECTION (soft constraints) ---
    void ApplyStretchCorrection()
    {
        // between anchor and first
        Vector3 aPos = anchorPoint.position;
        if (segments.Count == 0) return;

        // handle each adjacent pair
        for (int i = 0; i < segments.Count; i++)
        {
            Rigidbody a = (i == -1) ? null : (i == 0 ? null : segments[i - 1]); // not used
            Rigidbody b = segments[i];
            Vector3 prevPos = (i == 0) ? anchorPoint.position : segments[i - 1].position;
            float target = segmentLength;

            Vector3 dir = b.position - prevPos;
            float curDist = dir.magnitude;
            if (curDist == 0f) continue;
            dir /= curDist;

            float error = curDist - target;
            if (error > 0f)
            {
                // compute corrective force (soft)
                Vector3 force = dir * (error * stretchStiffness) - b.linearVelocity * stretchDamping;
                if (!b.isKinematic)
                {
                    b.AddForce(-force * Time.fixedDeltaTime, ForceMode.VelocityChange);
                }
                // apply opposite to previous segment if not anchor
                if (i > 0)
                {
                    Rigidbody prevRb = segments[i - 1];
                    if (!prevRb.isKinematic)
                        prevRb.AddForce(force * Time.fixedDeltaTime, ForceMode.VelocityChange);
                }
                else
                {
                    // prev is anchor (kinematic) - nothing to apply
                }
            }
        }

        // also between last segment and attached object (if present)
        if (attachEndRigidbody != null && segments.Count > 0)
        {
            Rigidbody last = segments[segments.Count - 1];
            Vector3 prevPos = last.position;
            Vector3 dir = attachEndRigidbody.position - prevPos;
            float curDist = dir.magnitude;
            if (curDist > segmentLength)
            {
                dir.Normalize();
                float error = curDist - segmentLength;
                Vector3 force = dir * (error * stretchStiffness) - attachEndRigidbody.linearVelocity * stretchDamping;
                if (!attachEndRigidbody.isKinematic)
                    attachEndRigidbody.AddForce(-force * Time.fixedDeltaTime, ForceMode.VelocityChange);
                if (!last.isKinematic)
                    last.AddForce(force * Time.fixedDeltaTime, ForceMode.VelocityChange);
            }
        }
    }

    // --- WARNING / BLINKING ---
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
            float alpha = Mathf.PingPong(t, 1f);
            Color c = Color.Lerp(normalColor, alertColor, alpha);
            SetLineColor(c);
            yield return null;
        }
    }

    void SetLineColor(Color c)
    {
        lr.startColor = c;
        lr.endColor = c;
        // if material supports color:
        if (lr.material != null && lr.material.HasProperty("_Color"))
            lr.material.color = c;
    }

    // --- Utility for debugging / editor ---
    private void OnDrawGizmosSelected()
    {
        if (segments != null && segments.Count > 0)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < segments.Count - 1; i++)
            {
                Gizmos.DrawLine(segments[i].position, segments[i + 1].position);
            }
        }
    }
}
