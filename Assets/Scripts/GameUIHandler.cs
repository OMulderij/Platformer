using System;
using UnityEngine;
using UnityEngine.UIElements;

public class GameUIHandler : MonoBehaviour
{
    public PlayerControl PlayerControl;
    private UIDocument UIDoc;
    private Label m_HealthLabel;


    private void Start()
    {
        UIDoc = GetComponent<UIDocument>();
        m_HealthLabel = UIDoc.rootVisualElement.Q<Label>("ManaLabel");
        PlayerControl.OnHealthChange += HealthChanged;
        HealthChanged();
    }


    void HealthChanged()
    {
        m_HealthLabel.text = $"{Math.Round(PlayerControl.currentHealth)}/{PlayerControl.maxHealth}";
    }
}