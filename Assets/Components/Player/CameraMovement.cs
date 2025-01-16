using System;
using UnityEngine;

public class CameraMovement : MonoBehaviour 
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float keyboardMovementSpeed = 15;
    [SerializeField] private float panningSpeed = 25;
    [SerializeField] private float rotationSpeed = 3;
    [SerializeField] private float mouseRotationSpeed = 100;
    [SerializeField] private int limitX = 100;
    [SerializeField] private int limitY = 100;
    [SerializeField] private float scrollWheelZoomingSensitivity = -25;
    [SerializeField] private float minZoomDistance = 5;
    [SerializeField] private float maxZoomDistance = 30;
    [SerializeField] private float zoomSmoothing = 30;
    [SerializeField] private float zoomPos01;
    [SerializeField] private LayerMask gridLayer;
    [SerializeField] private Camera mainCamera;
    private float zoomPosCached;
    private Vector3 cachedPos;
    

    private Vector2 MouseAxis
    {
        get { return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")); }
    }

    private int RotationDirection
    {
        get
        {
            bool rotateRight = Input.GetKey(KeyCode.E);
            bool rotateLeft = Input.GetKey(KeyCode.Q);
            if(rotateLeft && rotateRight)
                return 0;
            if(rotateLeft)
                return -1;
            if(rotateRight)
                return 1;
            
            return 0;
        }
    }
    private void Update()
    {
        Move();
        Rotation();
        LimitPosition();
        HeightCalculation();
    }

    private void Move()
    {
        Vector3 desiredMove = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        
        desiredMove *= keyboardMovementSpeed;
        desiredMove *= Time.deltaTime;
        desiredMove = Quaternion.Euler(new Vector3(0f, transform.eulerAngles.y, 0f)) * desiredMove;
        desiredMove = transform.InverseTransformDirection(desiredMove);
        
        transform.Translate(desiredMove, Space.Self);

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, gridLayer))
        {
            return;
        }
        if (Input.GetMouseButtonDown(1))
        {
            cachedPos = hit.point;
        }
        if (Input.GetMouseButton(1))
        {
            desiredMove = (cachedPos - hit.point) * (panningSpeed * Time.deltaTime);
            transform.Translate(desiredMove);
            // cachedPos = hit.point;
        }
    }

    private void Rotation()
    {
        transform.Rotate(Vector3.up, RotationDirection * Time.deltaTime * rotationSpeed, Space.World);

        if (Input.GetKey(KeyCode.Mouse2))
            transform.Rotate(Vector3.up, -MouseAxis.x * Time.deltaTime * mouseRotationSpeed, Space.World);
    }

    private float timeCached;
    private void HeightCalculation()
    {
        zoomPos01 += Input.mouseScrollDelta.y * Time.deltaTime * scrollWheelZoomingSensitivity;
        if (Input.mouseScrollDelta.y != 0)
        {
            timeCached = 0;
        }
        zoomPos01 = Mathf.Clamp01(zoomPos01);
        timeCached = Mathf.Clamp01(timeCached);
        zoomPosCached = Mathf.Lerp(zoomPosCached, zoomPos01, timeCached);

        float targetHeight = Mathf.Lerp(minZoomDistance, maxZoomDistance, zoomPosCached);
        Vector3 cameraDist = Vector3.Normalize(cameraTransform.localPosition) * targetHeight;

        cameraTransform.localPosition = cameraDist;
        timeCached += Time.deltaTime * (Mathf.Abs(zoomPosCached - zoomPos01)) * zoomSmoothing;
    }
    
    private void LimitPosition()
    {
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, -limitX, limitX),
            transform.position.y,
            Mathf.Clamp(transform.position.z, -limitY, limitY));
    }
}
