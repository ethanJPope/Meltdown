using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class PipeInputManager : MonoBehaviour
{
    private Camera _cam;

    private void Awake()
    {
        _cam = Camera.main;
        if (_cam == null)
            Debug.LogError("No Main Camera found for PipeInputManager.");
    }

    void Update()
    {
        // use the new Input System's Mouse API
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            // read screen-space mouse position
            Vector2 screenPos = Mouse.current.position.ReadValue();

            // convert to world-space (for 2D, z can be distance from camera)
            Vector3 worldPos3D = _cam.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y, -_cam.transform.position.z)
            );
            Vector2 worldPos2D = new Vector2(worldPos3D.x, worldPos3D.y);

            // raycast into 2D scene
            RaycastHit2D hit = Physics2D.Raycast(worldPos2D, Vector2.zero);
            if (hit.collider != null)
            {
                var pipe = hit.collider.GetComponent<Pipe>();
                if (pipe != null)
                    pipe.TryRotate();
            }
        }
    }
}