using JamKit;
using UnityEngine;

namespace Game
{
    public class LookTrigger : MonoBehaviour
    {
        private bool _triggered = false;


        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (!gameObject.activeSelf) { return; }
            if (collider.name != "VisibilityCone") { return; }
            
            if (_triggered) { return; }
            _triggered = true;

            FindObjectOfType<GameMain>().OnGhoulInitiated();
        }
    }
}
