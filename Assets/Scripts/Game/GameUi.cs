using JamKit;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class GameUi : UiBase
    {
        [SerializeField] private FlashInfo _openFlashInfo;
        [SerializeField] private Image _cover;

        void Start()
        {
            Flash(_openFlashInfo);
        }

        public void ActivateCover()
        {
            _cover.gameObject.SetActive(true);
        }

    }
}