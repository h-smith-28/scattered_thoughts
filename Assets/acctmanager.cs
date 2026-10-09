using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AccountManager : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_InputField usernameLoginInput;
    public TMP_InputField passwordLoginInput;
    public GameObject successScreen;
    public GameObject errorMessage;
    public s_GameMenus gameMenus;

    private void Start()
    {
        gameMenus = FindAnyObjectByType<s_GameMenus>();
    }

    public void CreateAccount()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        PlayerPrefs.SetString("SavedUsername", username);
        PlayerPrefs.SetString("SavedPassword", password);
        PlayerPrefs.Save();

    }

    public void Login()
    {
        if (passwordLoginInput.text.Equals("SeCr3t_w0rd5!"))
        {
            gameMenus?.ShowWin();
            errorMessage.SetActive(false);
        }
        else
        {
            errorMessage.SetActive(true);
        }
    }
}