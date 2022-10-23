using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{
    public class VisibilityConeSetup : MonoBehaviour
    {
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private PolygonCollider2D _collider;

        void Start()
        {
            if (_meshFilter.mesh == null)
            {
                _meshFilter.mesh = new Mesh();
            }
        }

        public void SetMesh(List<Vector2> visibleCorners)
        {
            _meshFilter.mesh.Clear();

            List<Vector2> visibilityMeshVertices = new List<Vector2>(visibleCorners);
            visibilityMeshVertices.Insert(0, (Vector2)_playerTransform.position);
            _meshFilter.mesh.SetVertices(visibilityMeshVertices.Select(p => new Vector3(p.x, p.y, 0)).ToList());

            List<int> indices = new List<int>();
            
            for (int i = 0; i < visibleCorners.Count - 1; i++)
            {
                indices.Add(0);
                indices.Add(i + 1);
                indices.Add(i + 2);
            }

            _meshFilter.mesh.SetIndices(indices, MeshTopology.Triangles, 0);
            _meshFilter.mesh.RecalculateBounds();

            _collider.points = visibilityMeshVertices.ToArray();
        }
    }
}
