using UnityEngine;

public class DisableDebugger : MonoBehaviour
{
    void OnDisable()
    {
        Debug.Log($"{name} foi desativado!\n{System.Environment.StackTrace}");
    }
}