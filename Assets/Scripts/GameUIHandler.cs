using System;
using UnityEngine;
using UnityEngine.UIElements;

public class GameUIHandler : MonoBehaviour
{
    public PlayerControl PlayerControl;
    private UIDocument UIDoc;
    private Label m_HealthLabel;
    private VisualElement m_HealthBarMask;
    


    private void Start()
    {
        UIDoc = GetComponent<UIDocument>();
        m_HealthLabel = UIDoc.rootVisualElement.Q<Label>("ManaLabel");
        m_HealthBarMask = UIDoc.rootVisualElement.Q<VisualElement>("ManaBarMask");
        PlayerControl.OnHealthChange += HealthChanged;
        HealthChanged();
    }


    void HealthChanged()
    {
        m_HealthLabel.text = $"{Math.Round(PlayerControl.currentHealth)}/{PlayerControl.maxHealth}";
        float healthRatio = (float)PlayerControl.currentHealth / PlayerControl.maxHealth;
        float healthPercent = Mathf.Lerp(8, 88, healthRatio);
        m_HealthBarMask.style.width = Length.Percent(healthPercent);
    }
}