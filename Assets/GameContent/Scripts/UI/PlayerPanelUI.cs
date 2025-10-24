using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPanelUI : MonoBehaviour
{
    public Image playerIcon;
    public TextMeshProUGUI playerName;

    private void Update()
    {
        //if (CharacterGameManager.Singleton.currentStory.playerData == null)
        //{
        //    playerIcon.color = Color.gray;
        //    playerName.text = "";

        //    return;
        //}

        //playerIcon.color = Color.blue;
        //playerName.text = CharacterGameManager.Singleton.currentStory.playerData.DisplayName;
    }
}



