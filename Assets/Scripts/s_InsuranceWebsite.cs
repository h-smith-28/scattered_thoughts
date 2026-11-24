using TMPro;
using UnityEngine;

public class s_InsuranceWebsite : MonoBehaviour
{
    public GameObject[] pages;          // 0 = login, 1 = SSN, 2 = security question...
    public TMP_InputField passwordInput;
    public GameObject errorMessage;
    public string password = "SeCr3t_w0rd5!";
    public TMP_InputField ssnInput;
    public string ssn = "123-45-6789";   // must match what Mom texts
    public TMP_InputField securityInput;
    public string[] securityAnswers = { "willow" };


    int currentPage;

    void Start() => ShowPage(0);

    public void ShowPage(int index)
    {
        currentPage = index;
        for (int i = 0; i < pages.Length; i++)
            pages[i].SetActive(i == index);
        if (errorMessage != null) errorMessage.SetActive(false);
    }

    public void NextPage() => ShowPage(currentPage + 1);

    public void SubmitPassword()
    {
        if (passwordInput.text == password) NextPage();
        else errorMessage.SetActive(true);
        
    }
        public void SubmitSSN()
    {
        if (DigitsOnly(ssnInput.text) == DigitsOnly(ssn))
            NextPage();
        else
            errorMessage.SetActive(true);
    }

        public void SubmitSecurity()
    {
        string typed = securityInput.text.Trim().ToLower();
        foreach (string answer in securityAnswers)
        {
            if (typed == answer.Trim().ToLower())
            {
                NextPage();
                return;
            }
        }
        errorMessage.SetActive(true);
    }

        public void ChoosePlan(int planIndex)
    {
        Debug.Log("Chose plan " + planIndex);
        FindAnyObjectByType<s_GameMenus>()?.ShowWin();
    }

    string DigitsOnly(string s)
    {
        string result = "";
        foreach (char c in s)
            if (char.IsDigit(c)) result += c;
        return result;
    }
}