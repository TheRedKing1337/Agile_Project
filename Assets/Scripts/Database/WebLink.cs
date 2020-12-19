using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WebLink : MonoBehaviour
{
    [SerializeField]
    private Text text;

    public void SetText(string input)
    {
        text.text = input;
    }
    public void ReloadLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
