using JamKit;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public static class Vector2Extension
    {
        public static Vector2 Rotate(this Vector2 v, float degrees)
        {
            float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

            float tx = v.x;
            float ty = v.y;
            v.x = (cos * tx) - (sin * ty);
            v.y = (sin * tx) + (cos * ty);
            return v;
        }

        public static bool IsDirectionBetween(this Vector2 v, Vector2 dir1, Vector2 dir2)
        {
            if (Vector2.Dot(v, dir1) < 0 || Vector2.Dot(v, dir2) < 0) { return false; } // Behind

            float cross1 = v.x * dir1.y - v.y * dir1.x;
            float cross2 = v.x * dir2.y - v.y * dir2.x;
            return Mathf.Sign(cross1) != Mathf.Sign(cross2);
        }
    }

    public class GameMain : MonoBehaviour
    {
        public enum PlayerState
        {
            Idle, Run
        }

        [Header("World")]
        [SerializeField] private GameUi _ui;
        [SerializeField] private Camera _gameCamera;
        [SerializeField] private Camera _renderCamera;

        [Header("Player")]
        [SerializeField] private float _playerSpeed = 2;
        [SerializeField] private float _playerVisibilityRange = 500;
        [SerializeField] private float _playerVisibilityAngle = 45;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private CircleCollider2D _playerCollider;
        [SerializeField] private SpriteRenderer _playerSpriteRenderer;
        [SerializeField] private SpriteFx _playerIdleFx;
        [SerializeField] private SpriteFx _playerRunFx;
        [SerializeField] private VisibilityConeSetup _visibilityCone;
        [SerializeField] private float _playerStepDistance;

        [Header("Ghoul")]
        [SerializeField] private Transform _ghoulTransform;
        [SerializeField] private SpriteFx _ghoulFx;
        [SerializeField] private SpriteRenderer _ghoulRenderer;
        [SerializeField] private float _ghoulRunSpeed = 5;
        [SerializeField] private AudioSource _ghoulAudioSource;
        [SerializeField] private Vector2 _ghoulAudioVolumeRange;

        private PlayerState _playerState = PlayerState.Idle;
        private Coroutine _playerFxCoroutine;
        private float _playerTravelledDistance;

        private List<Vector2> _wallCorners = new List<Vector2>();

        private int _physicsLayerMask;

        private bool _isGhoulSequence;

        private void Start()
        {
            Sfx.Instance.StartMusic("Ambience", true);

            _physicsLayerMask = ~(1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Trigger"));
            _playerFxCoroutine = CoroutineStarter.Run(SpriteFx.Play(_playerIdleFx, _playerSpriteRenderer));

            foreach (Wall wall in FindObjectsOfType<Wall>())
            {
                foreach (Vector3 p in wall.LocalPoints)
                {
                    _wallCorners.Add(wall.transform.position + p);
                }
            }
        }

        private void Update()
        {
            UpdateVisibilityShape();

            UpdatePlayer();

            UpdateGhoulSequence();
        }


        private void UpdateVisibilityShape()
        {
            const float MaxRayDist = 40;

            List<Vector2> visibilityZoneCorners = new List<Vector2>();

            Vector2 forward = _playerTransform.right * _playerVisibilityRange;
            Vector2 coneP1 = forward.Rotate(_playerVisibilityAngle / 2.0f);
            Vector2 coneP2 = forward.Rotate(-_playerVisibilityAngle / 2.0f);

            Vector2 coneDir1 = (coneP1 - (Vector2)_playerTransform.position).normalized;
            Vector2 coneDir2 = (coneP2 - (Vector2)_playerTransform.position).normalized;

            RaycastHit2D coneHit1 = Physics2D.Raycast(_playerTransform.position, coneDir1, MaxRayDist, _physicsLayerMask);
            Vector2 conePoint1 = coneHit1.collider != null ? coneHit1.point : new Ray2D(_playerTransform.position, coneDir1).GetPoint(MaxRayDist);
            visibilityZoneCorners.Add(conePoint1);

            RaycastHit2D coneHit2 = Physics2D.Raycast(_playerTransform.position, coneDir2, MaxRayDist, _physicsLayerMask);
            Vector2 conePoint2 = coneHit2.collider != null ? coneHit2.point : new Ray2D(_playerTransform.position, coneDir2).GetPoint(MaxRayDist);
            visibilityZoneCorners.Add(conePoint2);

            foreach (Vector2 corner in _wallCorners)
            {
                Vector2 dir = (corner - (Vector2)_playerTransform.position).normalized;
                if (!dir.IsDirectionBetween(coneDir1, coneDir2)) { continue; }

                Vector2 nudgedCorner = corner - (dir * 0.1f);
                RaycastHit2D hit = Physics2D.Linecast(_playerTransform.position, nudgedCorner, _physicsLayerMask);
                if (hit.collider == null)
                {
                    visibilityZoneCorners.Add(nudgedCorner);

                    Vector2 dir1 = dir.Rotate(1f);
                    RaycastHit2D rayHit1 = Physics2D.Raycast(_playerTransform.position, dir1, MaxRayDist, _physicsLayerMask);
                    Vector2 rotatedPoint1 = rayHit1.collider != null ? rayHit1.point : new Ray2D(_playerTransform.position, dir1).GetPoint(MaxRayDist);
                    visibilityZoneCorners.Add(rotatedPoint1);

                    Vector2 dir2 = dir.Rotate(-1f);
                    RaycastHit2D rayHit2 = Physics2D.Raycast(_playerTransform.position, dir2, MaxRayDist, _physicsLayerMask);
                    Vector2 rotatedPoint2 = rayHit2.collider != null ? rayHit2.point : new Ray2D(_playerTransform.position, dir2).GetPoint(MaxRayDist);
                    visibilityZoneCorners.Add(rotatedPoint2);
                }
            }

            visibilityZoneCorners.Sort((p1, p2) => // Clockwise sort
            {
                Vector2 p1Local = p1 - (Vector2)_playerTransform.position;
                Vector2 p2Local = p2 - (Vector2)_playerTransform.position;

                Vector2 reference = (Vector2)(-_playerTransform.right);
                float angle1 = Vector2.SignedAngle(reference, p1Local);
                angle1 = (angle1 + 360) % 360;
                float angle2 = Vector2.SignedAngle(reference, p2Local);
                angle2 = (angle2 + 360) % 360;

                return angle2.CompareTo(angle1);
            });

            _visibilityCone.SetMesh(visibilityZoneCorners);

            foreach (Vector2 p in visibilityZoneCorners)
            {
                Debug.DrawLine(_playerTransform.position, p, Color.blue);
            }
        }

        private void UpdatePlayer()
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

            float speed = _playerSpeed;
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.LeftShift)) speed *= 3;
#endif
            Vector2 displacement = moveDir * (speed * Time.deltaTime);

            bool skipCollision = false;
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.LeftAlt)) skipCollision = true;
#endif
            RaycastHit2D moveCastResult = Physics2D.CircleCast(_playerTransform.position, _playerCollider.radius, moveDir, _playerSpeed * Time.deltaTime, _physicsLayerMask);
            if (!skipCollision && moveCastResult.collider != null)
            {
                // Subtract the component that's along the normal's direction
                displacement -= moveCastResult.normal * Vector2.Dot(displacement, moveCastResult.normal);
            }

            _playerTransform.position += (Vector3)displacement;

            Vector3 cursorWorldPos = _renderCamera.ScreenToWorldPoint(Input.mousePosition);
            Debug.DrawLine(_playerTransform.position, cursorWorldPos, Color.yellow);
            Vector3 toCursor = cursorWorldPos.WithZ(_playerTransform.position.z) - _playerTransform.position;
            _playerTransform.right = toCursor;

            _gameCamera.transform.position = Vector3.Lerp(_playerTransform.position, cursorWorldPos, 0.2f).WithZ(_gameCamera.transform.position.z);
            _renderCamera.transform.position = _gameCamera.transform.position;

            if (moveDir.sqrMagnitude > 0.1f)
            {
                if (_playerState == PlayerState.Idle)
                {
                    CoroutineStarter.Stop(_playerFxCoroutine);
                    _playerFxCoroutine = CoroutineStarter.Run(SpriteFx.Play(_playerRunFx, _playerSpriteRenderer));
                    _playerState = PlayerState.Run;
                    Sfx.Instance.PlayRandom("Footstep");

                    _playerTravelledDistance = 0;
                }

                _playerTravelledDistance += displacement.magnitude;
                if (_playerTravelledDistance > _playerStepDistance)
                {
                    Sfx.Instance.PlayRandom("Footstep");
                    _playerTravelledDistance = 0;

                }
            }
            else
            {
                if (_playerState == PlayerState.Run)
                {
                    CoroutineStarter.Stop(_playerFxCoroutine);
                    _playerFxCoroutine = CoroutineStarter.Run(SpriteFx.Play(_playerIdleFx, _playerSpriteRenderer));
                    _playerState = PlayerState.Idle;

                    if (_playerTravelledDistance / _playerStepDistance > 0.3f) Sfx.Instance.PlayRandom("Footstep");
                }
            }
        }

        public void OnGhoulInitiated()
        {
            _isGhoulSequence = true;
            _ghoulTransform.gameObject.SetActive(true);
            CoroutineStarter.Run(SpriteFx.Play(_ghoulFx, _ghoulRenderer));
            _ghoulAudioSource.Play();
        }

        private void UpdateGhoulSequence()
        {
            if (!_isGhoulSequence) { return; }

            float dist = Vector2.Distance(_ghoulTransform.position, _playerTransform.position);

            float volumeT = Mathf.Lerp(1, 0, dist / 10.0f);
            _ghoulAudioSource.volume = Mathf.Lerp(_ghoulAudioVolumeRange.x, _ghoulAudioVolumeRange.y, volumeT);
            
            Vector3 dir = (_playerTransform.position - _ghoulTransform.position).normalized;
            _ghoulTransform.position += dir * (_ghoulRunSpeed * Time.deltaTime);

            if (dist < 1f)
            {
                // End
                _ghoulAudioSource.Stop();
                _ui.ActivateCover();
                Sfx.Instance.FadeOutMusic(0.01f);
                Sfx.Instance.Play("Deep");
                SceneManager.LoadScene("End");
            }
        }
    }
}