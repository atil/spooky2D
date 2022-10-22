using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game
{
    [ExecuteInEditMode]
    public class LevelEditor : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private Transform _worldTransform;
        [SerializeField] private bool _forceValidate;

        private void OnValidate()
        {
            if (Application.isPlaying) { return; }

            SceneView.beforeSceneGui += BeforeSceneGui;
        }

        private void BeforeSceneGui(SceneView obj)
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.alt)
            {
                OnClick();
            }
        }

        private void OnClick()
        {
            Vector2 mousePos = (Vector2)HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;

            foreach (Transform worldChild in _worldTransform)
            {
                if (!worldChild.name.StartsWith("Wall")) { continue; }

                Transform pointsParent = worldChild.GetChild(0);
                foreach (Transform pointTransform in pointsParent)
                {
                    Vector2 pointPos = pointTransform.position;
                    if (Vector2.Distance(mousePos, pointPos) < 0.3f)
                    {
                        //Selection.objects = new GameObject[] { pointTransform.gameObject };
                        Selection.activeGameObject = pointTransform.gameObject;
                        Event.current.Use();
                        return;
                    }
                }
            }
        }
#endif
    }
}
