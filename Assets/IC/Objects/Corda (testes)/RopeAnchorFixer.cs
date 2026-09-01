using UnityEngine;

/// <summary>
/// Recalcula o Connected Anchor de todos os ConfigurableJoints filhos,
/// corrigindo o problema de valores errados causados por escala não-1
/// na hierarquia (ex: Connected Anchor lido como 1.5 quando a distância
/// real entre bones é 0.07).
///
/// Coloque este script no objeto "Rope" (o pai de todos os Seg_XX).
/// Ele roda em Awake, antes da primeira simulação de física, então o
/// valor corrigido já é usado desde o primeiro FixedUpdate.
/// </summary>
public class RopeAnchorFixer : MonoBehaviour
{
    [Tooltip("Se true, imprime no Console a distância recalculada de cada joint (útil pra conferir se bateu com 0.07)")]
    public bool logDistances = false;

    void Awake()
    {
        FixAllAnchors();
    }

    [ContextMenu("Fix Anchors Now")]
    public void FixAllAnchors()
    {
        ConfigurableJoint[] joints = GetComponentsInChildren<ConfigurableJoint>();

        foreach (var joint in joints)
        {
            if (joint.connectedBody == null)
            {
                continue; // segmento sem connectedBody (ex: se algum estiver preso ao mundo)
            }

            // Desliga o auto-configure: a partir de agora nós controlamos o valor.
            joint.autoConfigureConnectedAnchor = false;

            // Posição do Anchor (local ao próprio segmento) em espaço mundial.
            // TransformPoint já leva em conta escala e rotação corretamente.
            Vector3 anchorWorldPos = joint.transform.TransformPoint(joint.anchor);

            // Converte essa posição mundial para o espaço local do connectedBody.
            // InverseTransformPoint também já lida com a escala do connectedBody.
            Vector3 correctedConnectedAnchor =
                joint.connectedBody.transform.InverseTransformPoint(anchorWorldPos);

            joint.connectedAnchor = correctedConnectedAnchor;

            if (logDistances)
            {
                float dist = Vector3.Distance(
                    joint.transform.position,
                    joint.connectedBody.transform.position
                );
                Debug.Log(
                    $"[RopeAnchorFixer] {joint.name} -> {joint.connectedBody.name} | " +
                    $"connectedAnchor corrigido: {correctedConnectedAnchor} | " +
                    $"distância real: {dist:F4}"
                );
            }
        }
    }
}
