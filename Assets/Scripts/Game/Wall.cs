using System.Linq;
using UnityEngine;

namespace Game
{
    [ExecuteInEditMode]
    public class Wall : MonoBehaviour
    {
        [SerializeField] private Transform _pointsParent;
        [SerializeField] private MeshRenderer _rendererVisible;
        [SerializeField] private MeshFilter _filterVisible;
        [SerializeField] private MeshRenderer _rendererDark;
        [SerializeField] private MeshFilter _filterDark;
        [SerializeField] private PolygonCollider2D _collider;

        private Vector3[] _localPoints;
        public Vector3[] LocalPoints => _localPoints;
        
        void Awake()
        {
            if (Application.isPlaying)
            {
                SetPoints();
                _filterDark.mesh = new Mesh();
                _filterVisible.mesh = new Mesh();
                BuildMesh();
            }

        }

        void Update()
        {
            if (!Application.isPlaying)
            {
                SetPoints();
            }
        }

        private void SetPoints()
        {
            if (_localPoints == null)
            {
                _localPoints = new Vector3[_pointsParent.childCount];
            }
            for (int i = 0; i < _pointsParent.childCount; i++)
            {
                _localPoints[i] = _pointsParent.GetChild(i).localPosition;
            }
        }

        private void OnDrawGizmos()
        {
            if (_localPoints == null) { return; }
            for (int i = 0; i < _localPoints.Length; i++)
            {
                Vector3 p1 = transform.position + _localPoints[i];
                Vector3 p2 = transform.position + _localPoints[(i + 1) % _localPoints.Length];
                Gizmos.DrawLine(p1, p2);
            }
        }

        public void BuildMesh()
        {
            int[] tris = new int[6]
            {
                0, 1, 2,
                0, 2, 3,
            };

            Vector2[] uvs = new Vector2[4]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1),
            };

            _filterVisible.sharedMesh.SetVertices(_localPoints);
            _filterVisible.sharedMesh.SetIndices(tris, MeshTopology.Triangles, 0);
            _filterVisible.sharedMesh.SetUVs(0, uvs);
            _filterVisible.sharedMesh.RecalculateBounds();

            Vector3[] shrinkedLocalPoints = new Vector3[_localPoints.Length];
            for (int i = 0; i < _localPoints.Length; i++)
            {
                shrinkedLocalPoints[i] = _localPoints[i] * 0.9f; 
            }

            _filterDark.sharedMesh.SetVertices(shrinkedLocalPoints);
            _filterDark.sharedMesh.SetIndices(tris, MeshTopology.Triangles, 0);
            _filterDark.sharedMesh.SetUVs(0, uvs);
            _filterDark.sharedMesh.RecalculateBounds();

            _collider.points = _localPoints.Select(p => new Vector2(p.x, p.y)).ToArray();
        }

    }
}
