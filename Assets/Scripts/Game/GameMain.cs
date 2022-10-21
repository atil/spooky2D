using JamKit;
using UnityEngine;

namespace Game
{
    public class GameMain : MonoBehaviour
    {
        public enum PlayerState
        {
            Idle, Run
        }

        [Header("World")]
        [SerializeField] private Camera _camera;

        [Header("Player")]
        [SerializeField] private float _playerSpeed = 2;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private SpriteRenderer _playerSpriteRenderer;
        [SerializeField] private SpriteFx _playerIdleFx;
        [SerializeField] private SpriteFx _playerRunFx;

        private PlayerState _playerState = PlayerState.Idle;
        private Coroutine _playerFxCoroutine;

        private void Start()
        {
            _playerFxCoroutine = CoroutineStarter.Run(SpriteFx.Play(_playerIdleFx, _playerSpriteRenderer));
        }

        private void Update()
        {
            Vector2 moveDir = Vector2.zero;
            if (Input.GetKey(KeyCode.W))
            {
                moveDir += Vector2.up;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                moveDir -= Vector2.up;
            }
            if (Input.GetKey(KeyCode.A))
            {
                moveDir -= Vector2.right;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                moveDir += Vector2.right;
            }
            moveDir.Normalize();

            _playerTransform.position += (Vector3)moveDir * _playerSpeed * Time.deltaTime;
            Vector3 cursorWorldPos = _camera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 toCursor = cursorWorldPos.WithZ(_playerTransform.position.z) - _playerTransform.position;
            _playerTransform.right = toCursor;

            _camera.transform.position = Vector3.Lerp(_playerTransform.position, cursorWorldPos, 0.2f).WithZ(_camera.transform.position.z);

            if (moveDir.sqrMagnitude > 0.1f)
            {
                if (_playerState == PlayerState.Idle)
                {
                    CoroutineStarter.Stop(_playerFxCoroutine);
                    _playerFxCoroutine = CoroutineStarter.Run(SpriteFx.Play(_playerRunFx, _playerSpriteRenderer));
                    _playerState = PlayerState.Run;
                }
            }
            else
            {
                if (_playerState == PlayerState.Run)
                {
                    CoroutineStarter.Stop(_playerFxCoroutine);
                    _playerFxCoroutine = CoroutineStarter.Run(SpriteFx.Play(_playerIdleFx, _playerSpriteRenderer));
                    _playerState = PlayerState.Idle;
                }
            }
        }
    }
}