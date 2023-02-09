using System;
using UnityEngine;

public class VirusBaseMenu : MonoBehaviour
{
    // Need to manually match number of choices with number of available viruses. Can be made dynamic.
    public VirusBase[] allVirusBases;
    public VirusBaseChoice[] virusBaseChoices;
    private VirusBase selectedChoice;
    public Action<VirusBase> onChoiceChanged;

    void Awake() {
        for (int i = 0; i < allVirusBases.Length; i++) {
            virusBaseChoices[i].virusChoice = allVirusBases[i];
            virusBaseChoices[i].colorIndicator.color = allVirusBases[i].color;
        }
    }

    void Start() {
        gameObject.GetComponentInParent<Canvas>().worldCamera = HackGameManager.instance.gameCamera;
        transform.localPosition = Quaternion.Euler(0, 0, -transform.rotation.eulerAngles.z) * transform.localPosition;
        transform.rotation = Quaternion.identity;
    }

    void OnEnable() {
        foreach (VirusBaseChoice choice in virusBaseChoices) {
            choice.onChoiceSelected += SetSelectedChoice;
        }
    }

    void OnDisable() {
        foreach (VirusBaseChoice choice in virusBaseChoices) {
            choice.onChoiceSelected -= SetSelectedChoice;
        }
    }

    void SetSelectedChoice(VirusBase selectedVirus) {
        if (selectedChoice == selectedVirus) return;

        selectedChoice = selectedVirus;
        onChoiceChanged?.Invoke(selectedChoice);
        gameObject.SetActive(false);
    }
}
