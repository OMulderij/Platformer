using UnityEngine;

public class ManaWell : MonoBehaviour
{
    public void OnTriggerStay(Collider collider)
    {
        if (collider.GetComponent<PlayerControl>())
        {
            PlayerControl player = collider.GetComponent<PlayerControl>();
            player.HealToFull();
        }
    }
}
