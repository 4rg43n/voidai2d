using TMPro;
using UnityEngine;

public class BattleMenuUI : MonoBehaviour
{
    public TextMeshProUGUI modeText;

    private void Update()
    {
        modeText.text = "" + GameManager.Singleton.GameSubState.ToString();
    }

    public void SetGameSubState(int subState)
    {
        GameManager.Singleton.SetGameSubState((GameSubState)subState);
    }
}
