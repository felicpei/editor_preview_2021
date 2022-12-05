using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class CameraHelper
{
    public static Camera Camera { private set; get; }

    public static void Init(Camera camera)
    {
        Camera = camera;
    }

    public static void SetCameraPosition(Vector2 pos)
    {
        Camera.transform.position = pos;
    }

    public static void OnEnterMission()
    {
        
    }

    public static void OnExitMission()
    {
        
    }
    
    public static void ClearShake()
    {
        
    }

    public static Vector3 GetMouseWorldPos(Vector3 touchPos)
    {
        var transform = Camera.transform;
        var targetPos = transform.position;
        targetPos += transform.forward * Camera.orthographicSize;
        
        var screenPoint = Camera.WorldToScreenPoint(targetPos);
        var mousePos = touchPos;
        mousePos.z = screenPoint.z;

        return Camera.ScreenToWorldPoint(mousePos);
    }

    public static void SetRenderType(this Camera camera, CameraRenderType type)
    {
        var cameraData = camera.GetUniversalAdditionalCameraData();
        cameraData.renderType = type;
    }
}