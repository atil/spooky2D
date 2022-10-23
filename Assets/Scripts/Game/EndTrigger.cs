using UnityEngine;

namespace Game
{
    public class EndTrigger : MonoBehaviour
    {
        [SerializeField] private GameObject _lookTrigger;

        private bool _triggered = false;

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.name != "ColliderCircle") { return; }
            if (_triggered) { return; }
            _triggered = true;

            _lookTrigger.SetActive(true);
        }
    }
}
