using UnityEngine;

public class RopeWarningOnly : MonoBehaviour
{
    [Header("Physics source (optional)")]
    public HingeJoint[] joints; // se tiver a cadeia física
    public Transform anchorA; // fallback se não houver joints
    public Transform anchorB;
    public float restDistance = 2f; // só para fallback
    public float k = 100f; // constante elástica para fallback

    [Header("Threshold")]
    public float warnThreshold = 80f;

    [Header("Visuals (Skinned)")]
    public SkinnedMeshRenderer skinnedRenderer;
    public Color normalColor = Color.white;
    public Color warningColor = Color.red;
    public float blinkSpeed = 6f;
    public string colorProperty1 = "_Color";
    public string colorProperty2 = "_BaseColor";

    [Header("Smoothing")]
    [Range(0f, 1f)] public float smoothing = 0.12f;

    MaterialPropertyBlock mpb;
    float smoothedTension = 0f;

    void Start()
    {
        mpb = new MaterialPropertyBlock();
        if (skinnedRenderer == null)
            Debug.LogWarning("SkinnedMeshRenderer não setado — verifique inspector.");

        if (joints == null || joints.Length == 0)
        {
            var component = GetComponentInChildren<HingeJointHelper>();

            if (component != null)
            {
                joints = component.joints.ToArray();
            }
        }
    }

    void FixedUpdate()
    {
        float tension = 0f;
        if (joints != null && joints.Length > 0)
            tension = MaxTensionFromJoints();
        else
            tension = ApproxTensionFromAnchors();

        if (float.IsNaN(tension) || float.IsInfinity(tension)) tension = 0f;
        smoothedTension = Mathf.Lerp(smoothedTension, tension, smoothing);

        if (smoothedTension > warnThreshold)
            WarningEffect();
        else
            ResetVisual();
    }

    float MaxTensionFromJoints()
    {
        float maxT = 0f;
        foreach (var j in joints)
        {
            if (j == null) continue;
            Vector3 f = j.currentForce;
            Rigidbody thisRb = j.GetComponent<Rigidbody>();
            var connected = j.connectedBody;
            if (thisRb == null || connected == null) continue;
            Vector3 dir = (connected.worldCenterOfMass - thisRb.worldCenterOfMass).normalized;
            float t = Vector3.Dot(f, dir);
            if (t > maxT) maxT = t;
        }
        return maxT;
    }

    float ApproxTensionFromAnchors()
    {
        if (anchorA == null || anchorB == null) return 0f;
        float cur = Vector3.Distance(anchorA.position, anchorB.position);
        float x = Mathf.Max(0f, cur - restDistance);
        return k * x;
    }

    void WarningEffect()
    {
        if (skinnedRenderer == null) return;
        float t = Mathf.PingPong(Time.time * blinkSpeed, 1f);
        Color c = Color.Lerp(normalColor, warningColor, t);

        mpb.Clear();
        mpb.SetColor(colorProperty1, c);      // usa _Color (padrao)
        mpb.SetColor(colorProperty2, c);      // e tenta _BaseColor (URP/HDRP)
        skinnedRenderer.SetPropertyBlock(mpb);
    }

    void ResetVisual()
    {
        if (skinnedRenderer == null) return;
        mpb.Clear();
        mpb.SetColor(colorProperty1, normalColor);
        mpb.SetColor(colorProperty2, normalColor);
        skinnedRenderer.SetPropertyBlock(mpb);
    }
}
