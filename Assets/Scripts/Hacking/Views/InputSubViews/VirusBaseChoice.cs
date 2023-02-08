using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class VirusBaseChoice : MonoBehaviour
{
    public VirusBase virusChoice;
    public TextMeshProUGUI virusText;
    public Button button;
    public Action<VirusBase> onChoiceSelected;

    void Start() {
        virusText.text = virusChoice.ToString();
        button.onClick.AddListener(ReactToClick);
    }

    void ReactToClick() {
        onChoiceSelected?.Invoke(virusChoice);
    }
}
