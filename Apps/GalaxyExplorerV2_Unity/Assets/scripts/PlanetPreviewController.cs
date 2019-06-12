using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanetPreviewController : MonoBehaviour
{
    [SerializeField] private Button[] buttons;

    public void OnButtonSelected(Button selectedButton)
    {
        foreach (var button in buttons)
        {
            button.interactable = button != selectedButton;
        }
    }
}
