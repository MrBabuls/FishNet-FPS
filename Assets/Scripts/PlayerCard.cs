using TMPro;
using UnityEngine;

public class PlayerCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText, killsText, deathsText;

    public void Initialize(string name)
    {
        nameText.text = name;
        killsText.text = "0";
        deathsText.text = "0";
    }
    
    public void SetKills(int kills)
    {
        killsText.text = kills.ToString();
    }
    
    public void SetDeaths(int deaths)
    {
        deathsText.text = deaths.ToString();
    }
}
