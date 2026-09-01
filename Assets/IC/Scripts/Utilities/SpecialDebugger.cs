using UnityEngine;

public class SpecialDebugger : MonoBehaviour
{
    //void OnDisable()
    //{
    //    Debug.Log($"{name} foi desativado!\n{System.Environment.StackTrace}");
    //}

    public void Log(string message)
    {
        Debug.Log(message);
    }
}