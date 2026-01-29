using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RopeVerlet : MonoBehaviour
{
    [Header("References")]
    public Transform anchorPoint;
    public Rigidbody attachEndRigidbody;    // optional (garra)
    public GameObject debugNodePrefab;     // opcional: esfera pequena para visualizar n�s

    [Header("Rope")]
    public int nodeCount = 18;
    public float segmentLength = 0.12f;
    public float gravity = -9.81f;
    public int constraintIterations = 6;    // aumento = menos alongamento

    [Header("Damping / physics")]
    public float velocityDamping = 0.99f;   // simula drag
    public float nodeMass = 0.2f;

    [Header("Blinking")]
    public float warningStretchRatio = 1.06f;
    public Color normalColor = Color.white;
    public Color alertColor = Color.red;
    public float blinkSpeed = 6f;

    private Vector3[] positions;
    private Vector3[] prevPositions;
    private Vector3[] velocities;
    private GameObject[] debugNodes;
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
        InitializeNodes();
        restLength = CalculateLength();
        SetLineColor(normalColor);
    }

    void InitializeNodes()
    {
        positions = new Vector3[nodeCount + 2];   // anchor + nodes + end
        prevPositions = new Vector3[positions.Length];
        velocities = new Vector3[positions.Length];
        debugNodes = new GameObject[positions.Length];

        Vector3 start = anchorPoint.position;
        Vector3 end = attachEndRigidbody ? attachEndRigidbody.position : start + Vector3.down * (nodeCount * segmentLength);

        // index 0 = anchor (pinned), last = attach (if provided) or free
        for (int i = 0; i < positions.Length; i++)
        {
            float t = (float)i / (positions.Length - 1);
            positions[i] = Vector3.Lerp(start, end, t);
            prevPositions[i] = positions[i];
            velocities[i] = Vector3.zero;
            if (debugNodePrefab != null)
            {
                debugNodes[i] = Instantiate(debugNodePrefab, positions[i], Quaternion.identity, transform);
            }
        }

        lr.positionCount = positions.Length;
    }

    void FixedUpdate()
    {
        Simulate(Time.fixedDeltaTime);
        UpdateLine();
        float cur = CalculateLength();
        if (cur / restLength >= warningStretchRatio)
        {
            if (!isWarning) StartWarning();
        }
        else
        {
            if (isWarning) StopWarning();
        }
    }

    void Simulate(float dt)
    {
        // Verlet-style integration using positions and previous positions
        Vector3 gravityVec = new Vector3(0f, gravity, 0f);

        // 0 = anchor (pinned)
        for (int i = 1; i < positions.Length - 1; i++) // leave last for special (attached)
        {
            Vector3 pos = positions[i];
            Vector3 vel = (positions[i] - prevPositions[i]) / dt;
            vel *= velocityDamping;
            vel += gravityVec * dt; // apply gravity to velocity
            Vector3 newPos = pos + vel * dt;
            prevPositions[i] = positions[i];
            positions[i] = newPos;
            velocities[i] = vel;
        }

        // handle last point (attached to rigidbody?) -> sync with attach rigidbody if exists
        int lastIdx = positions.Length - 1;
        if (attachEndRigidbody != null)
        {
            // drive last node to attachEndRigidbody position (so object is supported)
            positions[lastIdx] = attachEndRigidbody.position;
            prevPositions[lastIdx] = positions[lastIdx];
            velocities[lastIdx] = attachEndRigidbody.linearVelocity;
        }
        else
        {
            // free dynamic last node
            Vector3 pos = positions[lastIdx];
            Vector3 vel = (positions[lastIdx] - prevPositions[lastIdx]) / dt;
            vel *= velocityDamping;
            vel += gravityVec * dt;
            Vector3 newPos = pos + vel * dt;
            prevPositions[lastIdx] = positions[lastIdx];
            positions[lastIdx] = newPos;
            velocities[lastIdx] = vel;
        }

        // constraints iterations: keep pairwise distances = segmentLength
        for (int iter = 0; iter < constraintIterations; iter++)
        {
            // constrain first segment between anchor (pinned) and node 1
            ConstrainPair(0, 1);

            for (int i = 1; i < positions.Length - 2; i++)
            {
                ConstrainPair(i, i + 1);
            }

            // constrain last pair (node N to attach or last node)
            ConstrainPair(positions.Length - 2, positions.Length - 1);
        }

        // update debug nodes transforms
        for (int i = 0; i < debugNodes.Length; i++)
        {
            if (debugNodes[i] != null)
            {
                debugNodes[i].transform.position = positions[i];
            }
        }
    }

    void ConstrainPair(int idxA, int idxB)
    {
        Vector3 a = positions[idxA];
        Vector3 b = positions[idxB];

        float target = segmentLength;
        Vector3 delta = b - a;
        float dist = delta.magnitude;
        if (dist == 0f) return;

        float diff = (dist - target) / dist;
        // If anchor (idxA == 0) is pinned, move only B
        if (idxA == 0)
        {
            positions[idxB] -= delta * diff;
        }
        // if last is attached to rigidbody, keep last pinned
        else if (idxB == positions.Length - 1 && attachEndRigidbody != null)
        {
            positions[idxA] += delta * diff;
        }
        else
        {
            // move both halves according to equal mass
            positions[idxA] += delta * diff * 0.5f;
            positions[idxB] -= delta * diff * 0.5f;
        }
    }

    void UpdateLine()
    {
        for (int i = 0; i < positions.Length; i++) lr.SetPosition(i, positions[i]);
    }

    public float CalculateLength()
    {
        float len = 0f;
        for (int i = 0; i < positions.Length - 1; i++)
            len += Vector3.Distance(positions[i], positions[i + 1]);
        return len;
    }

    // Blinking
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
