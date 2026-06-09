using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeToDemoScene : MonoBehaviour
{
    public void DemoScene()
    {
        SceneManager.LoadScene(1);
    }

    public void AnotherTestScene()
    {
        SceneManager.LoadScene(2);
    }

    public void SampleScene()
    {
        SceneManager.LoadScene(0);
    }

    public void RestartSecene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
