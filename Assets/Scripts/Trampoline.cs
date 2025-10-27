using UnityEngine;

public class Trampoline : MonoBehaviour
{
    public void OnTriggerStay(Collider collider)
    {
        if (!collider.GetComponent<PlayerControl>())
        {
            return;
        }
        collider.GetComponent<PlayerControl>().TrampolineHit();
    }
}
