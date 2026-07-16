using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public sealed class MapCameraController : MonoBehaviour
{
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Camera controlledCamera;
    [SerializeField] private Renderer mapRenderer;

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float zoomSpeed = 4f;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float tiltSpeed = 60f;
    [SerializeField] private float minZoomDistance = 6f;
    [SerializeField] private float maxZoomDistance = 16f;
    [SerializeField] private float minTiltAngle = 38f;
    [SerializeField] private float maxTiltAngle = 62f;
    [SerializeField] private float mapBoundsPadding = 0.35f;
    [SerializeField] private float smoothTime = 0.08f;

    private Vector3 targetRigPosition;
    private Vector3 rigMoveVelocity;
    private float targetZoomDistance;
    private float zoomVelocity;
    private float currentYaw;
    private float currentTiltAngle;
    private float initialRigY;

    private void Awake()
    {
        CacheInitialState();
        ApplyCameraDistance(0f);
        ClampRoll();
    }

    private void OnValidate()
    {
        moveSpeed = Mathf.Max(0f, moveSpeed);
        zoomSpeed = Mathf.Max(0f, zoomSpeed);
        rotationSpeed = Mathf.Max(0f, rotationSpeed);
        tiltSpeed = Mathf.Max(0f, tiltSpeed);
        minZoomDistance = Mathf.Max(0.1f, minZoomDistance);
        maxZoomDistance = Mathf.Max(minZoomDistance + 0.1f, maxZoomDistance);
        minTiltAngle = Mathf.Clamp(minTiltAngle, 1f, 85f);
        maxTiltAngle = Mathf.Clamp(maxTiltAngle, minTiltAngle + 0.1f, 85f);
        mapBoundsPadding = Mathf.Max(0f, mapBoundsPadding);
        smoothTime = Mathf.Max(0.001f, smoothTime);
    }

    private void Update()
    {
        if (cameraPivot == null || controlledCamera == null)
        {
            return;
        }

        float deltaTime = Time.unscaledDeltaTime;

        Vector2 moveInput = ReadMoveInput();
        if (moveInput.sqrMagnitude > 0f)
        {
            Pan(moveInput, deltaTime);
        }

        float scrollInput = ReadScrollInput();
        if (Mathf.Abs(scrollInput) > Mathf.Epsilon)
        {
            Zoom(scrollInput);
        }

        if (IsMiddleMousePressed())
        {
            Vector2 mouseDelta = ReadMouseDelta();
            if (mouseDelta.sqrMagnitude > 0f)
            {
                RotateAndTilt(mouseDelta, deltaTime);
            }
        }

        SmoothRigPosition(deltaTime);
        ApplyCameraDistance(deltaTime);
        ClampRoll();
    }

    private void CacheInitialState()
    {
        initialRigY = transform.position.y;
        targetRigPosition = transform.position;

        if (cameraPivot != null)
        {
            Vector3 pivotEuler = cameraPivot.localEulerAngles;
            currentYaw = NormalizeAngle(pivotEuler.y);
            currentTiltAngle = Mathf.Clamp(NormalizeAngle(pivotEuler.x), minTiltAngle, maxTiltAngle);
            cameraPivot.localRotation = Quaternion.Euler(currentTiltAngle, currentYaw, 0f);
        }

        if (controlledCamera != null)
        {
            float distance = Mathf.Abs(controlledCamera.transform.localPosition.z);
            targetZoomDistance = Mathf.Clamp(distance, minZoomDistance, maxZoomDistance);
            controlledCamera.orthographic = false;
            controlledCamera.transform.localRotation = Quaternion.identity;
        }
        else
        {
            targetZoomDistance = Mathf.Clamp((minZoomDistance + maxZoomDistance) * 0.5f, minZoomDistance, maxZoomDistance);
        }
    }

    private Vector2 ReadMoveInput()
    {
        Vector2 input = Vector2.zero;

        if (IsKeyPressed(CameraKey.Forward))
        {
            input.y += 1f;
        }

        if (IsKeyPressed(CameraKey.Backward))
        {
            input.y -= 1f;
        }

        if (IsKeyPressed(CameraKey.Right))
        {
            input.x += 1f;
        }

        if (IsKeyPressed(CameraKey.Left))
        {
            input.x -= 1f;
        }

        return input.sqrMagnitude > 1f ? input.normalized : input;
    }

    private static bool IsKeyPressed(CameraKey key)
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return false;
        }

        return key switch
        {
            CameraKey.Forward => keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed,
            CameraKey.Backward => keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed,
            CameraKey.Left => keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed,
            CameraKey.Right => keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed,
            _ => false
        };
#else
        return key switch
        {
            CameraKey.Forward => Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow),
            CameraKey.Backward => Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow),
            CameraKey.Left => Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow),
            CameraKey.Right => Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow),
            _ => false
        };
#endif
    }

    private static float ReadScrollInput()
    {
    #if ENABLE_INPUT_SYSTEM
        Mouse mouse = Mouse.current;
        if (mouse == null)
        {
            return 0f;
        }

        float rawScroll = mouse.scroll.ReadValue().y;

        // Windows-мыши могут возвращать ±120,
        // другие устройства и конфигурации — ±1.
        return Mathf.Abs(rawScroll) > 10f
            ? rawScroll / 120f
            : rawScroll;
    #else
        return Input.mouseScrollDelta.y;
    #endif
    }

    private static bool IsMiddleMousePressed()
    {
#if ENABLE_INPUT_SYSTEM
        Mouse mouse = Mouse.current;
        return mouse != null && mouse.middleButton.isPressed;
#else
        return Input.GetMouseButton(2);
#endif
    }

    private static Vector2 ReadMouseDelta()
    {
#if ENABLE_INPUT_SYSTEM
        Mouse mouse = Mouse.current;
        return mouse != null ? mouse.delta.ReadValue() : Vector2.zero;
#else
        return new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
#endif
    }

    private void Pan(Vector2 input, float deltaTime)
    {
        Vector3 forward = Vector3.ProjectOnPlane(controlledCamera.transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(controlledCamera.transform.right, Vector3.up).normalized;

        if (forward.sqrMagnitude < Mathf.Epsilon || right.sqrMagnitude < Mathf.Epsilon)
        {
            return;
        }

        Vector3 delta = (forward * input.y + right * input.x) * (moveSpeed * deltaTime);
        targetRigPosition += delta;
        targetRigPosition.y = initialRigY;
        targetRigPosition = ClampToMapBounds(targetRigPosition);
    }

    private void Zoom(float scrollInput)
    {
        targetZoomDistance = Mathf.Clamp(targetZoomDistance - scrollInput * zoomSpeed, minZoomDistance, maxZoomDistance);
    }

    private void RotateAndTilt(Vector2 mouseDelta, float deltaTime)
    {
        currentYaw += mouseDelta.x * rotationSpeed * deltaTime;
        currentTiltAngle = Mathf.Clamp(currentTiltAngle - mouseDelta.y * tiltSpeed * deltaTime, minTiltAngle, maxTiltAngle);
        cameraPivot.localRotation = Quaternion.Euler(currentTiltAngle, currentYaw, 0f);
    }

    private void SmoothRigPosition(float deltaTime)
    {
        Vector3 currentPosition = transform.position;
        Vector3 nextPosition = Vector3.SmoothDamp(currentPosition, targetRigPosition, ref rigMoveVelocity, smoothTime, Mathf.Infinity, deltaTime);
        nextPosition.y = initialRigY;
        transform.position = ClampToMapBounds(nextPosition);
    }

    private void ApplyCameraDistance(float deltaTime)
    {
        if (controlledCamera == null)
        {
            return;
        }

        float currentDistance = Mathf.Abs(controlledCamera.transform.localPosition.z);
        float distance = deltaTime > 0f
            ? Mathf.SmoothDamp(currentDistance, targetZoomDistance, ref zoomVelocity, smoothTime, Mathf.Infinity, deltaTime)
            : targetZoomDistance;

        distance = Mathf.Clamp(distance, minZoomDistance, maxZoomDistance);
        controlledCamera.transform.localPosition = new Vector3(0f, 0f, -distance);
        controlledCamera.transform.localRotation = Quaternion.identity;
    }

    private Vector3 ClampToMapBounds(Vector3 position)
    {
        if (mapRenderer == null)
        {
            position.y = initialRigY;
            return position;
        }

        Bounds bounds = mapRenderer.bounds;
        float zoomInset = Mathf.Abs(targetZoomDistance) * 0.18f;
        float inset = mapBoundsPadding + zoomInset;

        float minX = bounds.min.x + inset;
        float maxX = bounds.max.x - inset;
        float minZ = bounds.min.z + inset;
        float maxZ = bounds.max.z - inset;

        if (minX > maxX)
        {
            minX = maxX = bounds.center.x;
        }

        if (minZ > maxZ)
        {
            minZ = maxZ = bounds.center.z;
        }

        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = initialRigY;
        position.z = Mathf.Clamp(position.z, minZ, maxZ);
        return position;
    }

    private void ClampRoll()
    {
        if (cameraPivot != null)
        {
            cameraPivot.localRotation = Quaternion.Euler(currentTiltAngle, currentYaw, 0f);
        }

        if (controlledCamera != null)
        {
            controlledCamera.transform.localRotation = Quaternion.identity;
        }
    }

    private static float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f)
        {
            angle -= 360f;
        }

        return angle;
    }

    private enum CameraKey
    {
        Forward,
        Backward,
        Left,
        Right
    }
}
