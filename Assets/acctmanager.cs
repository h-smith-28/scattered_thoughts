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
        string username = usernameLoginInput.text;
        string password = passwordLoginInput.text;

        string savedUsername = PlayerPrefs.GetString("SavedUsername", "");
        string savedPassword = PlayerPrefs.GetString("SavedPassword", "");

        if (username == savedUsername && password == savedPassword)
        {
            successScreen.SetActive(true);
            errorMessage.SetActive(false);
        }
        else
        {
            errorMessage.SetActive(true);
        }
    }
}