using UnityEngine;
using TMPro;

public class InGameConsole : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI UIText; 
    public string logText {  get; private set; }

    [SerializeField] private bool showLog;
    [SerializeField] private bool showWarning;
    [SerializeField] private bool showError;
    [SerializeField] private bool collapse;

    private string currentlog;
    LogType logType;

    private void OnEnable()
    {
        Application.logMessageReceived += Application_logMessageReceived;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= Application_logMessageReceived;
    }

    private void Application_logMessageReceived(string condition, string stackTrace, LogType type)
    {
        currentlog = condition;
        logType = type;

        // Os dois nao podem ser verdadeiros ao mesmo tempo
        if ((!collapse) || (!logText.Contains(currentlog)))
        {
            if (logType == LogType.Error && showError)
            {
                logText = "<color=red>" + currentlog + "</color>\n---------------------\n" + logText;
            }
            if (logType == LogType.Warning && showWarning)
            {
                logText = "<color=yellow>" + currentlog + "</color>\n---------------------\n" + logText;
            }
            if (logType == LogType.Log && showLog)
            {
                logText = "<color=white>" + currentlog + "</color>\n---------------------\n" + logText;
            }
        }


        if (UIText != null)
        {
            UIText.SetText(logText);
        }
    }

    //private void Start()
    //{
    //    Debug.LogError("Test");
    //}
}
