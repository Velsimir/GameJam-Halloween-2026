using UnityEngine;

namespace Game.Scripts.Views
{
    [AddComponentMenu("Game/Label Billboard")]
    public class LabelBillboard : MonoBehaviour
    {
        private Camera _camera;

        private void LateUpdate()
        {
            if (_camera == null) _camera = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();
            if (_camera == null) return;

            Vector3 direction = transform.position - _camera.transform.position;
            if (direction.sqrMagnitude < 0.0001f) return;

            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
    }
}
