using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class s_Captcha : MonoBehaviour
{
    [System.Serializable]
    public class Category
    {
        public string name;        // "traffic lights", "buses", "crosswalks"...
        public Sprite[] images;
    }

    public s_InsuranceWebsite website;
    public Category[] categories;     // need at least 2
    public Button[] tiles;            // the 9 grid buttons
    public Text promptText;           // "Select all images with..."
    public Text statusText;

    public int roundsRequired = 2;
    public Color selectedColor = new Color(0.5f, 0.7f, 1f);

    bool[] isTarget;
    bool[] isSelected;
    int passed;

    void Awake()
    {
        isTarget = new bool[tiles.Length];
        isSelected = new bool[tiles.Length];
        for (int i = 0; i < tiles.Length; i++)
        {
            int index = i;   // copy for the click listener
            tiles[i].onClick.AddListener(() => Toggle(index));
        }
    }

    void OnEnable()
    {
        passed = 0;
        NewRound("");
    }

    void Toggle(int index)
    {
        isSelected[index] = !isSelected[index];
        tiles[index].image.color = isSelected[index] ? selectedColor : Color.white;
    }

    public void Verify()
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            if (isSelected[i] != isTarget[i])
            {
                NewRound("Please try again.");
                return;
            }
        }

        passed++;
        if (passed >= roundsRequired)
            website.NextPage();
        else
            NewRound("Please verify again (" + passed + "/" + roundsRequired + ").");
    }

    void NewRound(string status)
    {
        int target = Random.Range(0, categories.Length);
        promptText.text = "Select all images with <b>" + categories[target].name + "</b>";
        statusText.text = status;

        int targetCount = Random.Range(3, 5);   // 3 or 4 correct tiles
        for (int i = 0; i < tiles.Length; i++)
        {
            isTarget[i] = i < targetCount;
            isSelected[i] = false;
        }
        Shuffle(isTarget);

        for (int i = 0; i < tiles.Length; i++)
        {
            int cat = target;
            if (!isTarget[i])
                while (cat == target) cat = Random.Range(0, categories.Length);

            Sprite[] pool = categories[cat].images;
            tiles[i].image.sprite = pool[Random.Range(0, pool.Length)];
            tiles[i].image.color = Color.white;
        }
    }

    void Shuffle(bool[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
}
