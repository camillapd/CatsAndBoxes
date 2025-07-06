using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    public TMP_Text levelText;
    public TMP_Text boxCounterText;
    public GameObject catButtonOn;
    public GameObject catButtonOff;

    public void SetLevelNumber(int index)
    {
        levelText.text = "" + (index + 1);
    }

    public void SetHoldingCat(bool holding)
    {
        catButtonOn.SetActive(holding);
        catButtonOff.SetActive(!holding);
    }

    public void UpdateBoxCounter(int placedCats, int totalCats)
    {
        boxCounterText.text = placedCats + "\n-\n" + totalCats;
    }

    public void ResetBoxCounter(int totalCats)
    {
        UpdateBoxCounter(0, totalCats);
    }

}