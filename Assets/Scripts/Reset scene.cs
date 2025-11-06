using Unity.VisualScripting;
using UnityEngine;

public class Resetscene : MonoBehaviour
{
    public void ResetScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}
