using UnityEngine;
using VoidAI.GenAI.Text;

public class WaitingPanelUI : MonoBehaviour
{
    public Transform spinnerImage;

    private void Update()
    {
        if (spinnerImage == null)
            return;

        spinnerImage.gameObject.SetActive(TextGenBridge.IsSending);
    }
}
