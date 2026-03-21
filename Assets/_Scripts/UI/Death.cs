using UnityEngine;
using UnityEngine.SceneManagement;

public class Death : MonoBehaviour
{
    public void Click()
    {
        SceneManager.LoadScene("FirstBattle");
    }
}
