using System;
using UnityEngine;
using UnityEngine.UIElements;

public class GameUIHandler : MonoBehaviour
{
    public PlayerControl PlayerControl;
    private UIDocument UIDoc;
    private VisualElement m_ManaBarMask;
    


    private void Start()
    {
        UIDoc = GetComponent<UIDocument>();
        m_ManaBarMask = UIDoc.rootVisualElement.Q<VisualElement>("ManaBarMask");
        PlayerControl.OnManaChange += ManaChanged;
        ManaChanged();
    }


    void ManaChanged()
    {
        float manaRatio = (float)PlayerControl.currentMana / PlayerControl.maxMana;
        float manaPercent = Mathf.Lerp(3.8f, 97.5f, manaRatio);
        m_ManaBarMask.style.width = Length.Percent(manaPercent);
    }
}