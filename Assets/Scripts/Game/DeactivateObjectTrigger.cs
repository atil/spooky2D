using System.Collections;
using UnityEngine;

namespace Game
{
    public class DeactivateObjectTrigger : MonoBehaviour
    {
        [SerializeField] private GameObject _objectToDeactivate;
        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.name != "ColliderCircle") { return; }

            _objectToDeactivate.SetActive(false);
        }
    }
}