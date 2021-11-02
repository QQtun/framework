using UnityEngine;

public class Example : MonoBehaviour
{
    public bool _localPosition = true;
    public bool _useGizmo = true;

    //vector3
    [Header("Arrow")]
    public bool _debugArrow;
    public Vector3 _debugArrowPosition;
    public Color _debugArrowColor = Color.red;
    public Vector3 _debugArrowDirection;
    // TODO to from

    //vector3
    [Header( "Bounds" )]
    public bool _debugBounds;
    public Vector3 _debugBoundsCenterPosition;
    public Color _debugBoundsColor = Color.blue;
    public Vector3 _debugBoundsSize;
    // TODO align

    //vector3, float
    [Header( "Capsule" )]
    public bool _debugCapsule;
    public Vector3 _debugCapsulePosition;
    public Color _debugCapsuleColor = Color.cyan;
    public Vector3 _debugCapsuleEnd;
    public float _debugCapsuleRadius;

    //float, vector3
    [Header( "Circle" )]
    public bool _debugCircle;
    public Vector3 _debugCirclePosition;
    public Color _debugCircleColor = Color.green;
    public float _debugCircleRadius;
    public Vector3 _debugCircleUp;

    //vector3, float
    [Header( "Cone" )]
    public bool _debugCone;
    public Vector3 _debungConePosition;
    public Color _debugConeColor = Color.magenta;
    public float _debugConeAngle;
    public Vector3 _debugConeDirection;

    //vector3, float
    [Header( "Cylinder" )]
    public bool _debugCylinder;
    public Vector3 _debugCylinderPosition;
    public Color _debugCylinderColor = Color.white;
    public Vector3 _debugCylinderEnd; // ¦³¿ù
    public float _debugCylinderRadius;

    //float
    [Header( "Point" )]
    public bool _debugPoint;
    public Vector3 _debugPointPosition;
    public Color _debugPointColor = Color.yellow;
    public float _debugPointScale;

    //float
    [Header( "Sphere" )]
    public bool _debugWireSphere;
    public Vector3 _debugWireSpherePosition;
    public Color _debugWireSphereColor = Color.gray;
    public float _debugWireSphereRadius;

    void OnDrawGizmos()
    {
        if( !_useGizmo )
        {
            return;
        }

        Vector3 debugPointPosition = _debugPointPosition;
        Vector3 debugBoundsPosition = _debugBoundsCenterPosition;
        Vector3 debugCirclePosition = _debugCirclePosition;
        Vector3 debugWireSpherePosition = _debugWireSpherePosition;
        Vector3 debugCylinderPosition = _debugCylinderPosition;
        Vector3 debungConePosition = _debungConePosition;
        Vector3 debugArrowPosition = _debugArrowPosition;
        Vector3 debugCapsulePosition = _debugCapsulePosition;

        if( _localPosition )
        {
            Vector3 wPosition = transform.position;
            debugPointPosition += wPosition;
            debugBoundsPosition += wPosition;
            debugCirclePosition += wPosition;
            debugWireSpherePosition += wPosition;
            debugCylinderPosition += wPosition;
            debungConePosition += wPosition;
            debugArrowPosition += wPosition;
            debugCapsulePosition += wPosition;
        }

        if( _debugPoint )
        {
            DebugExtension.DrawPoint( debugPointPosition, _debugPointColor, _debugPointScale );
        }
        if( _debugBounds )
        {
            DebugExtension.DrawBounds( new Bounds( debugBoundsPosition, _debugBoundsSize ), _debugBoundsColor );
        }
        if( _debugCircle )
        {
            DebugExtension.DrawCircle( debugCirclePosition, _debugCircleUp, _debugCircleColor, _debugCircleRadius );
        }
        if( _debugWireSphere )
        {
            DebugExtension.DrawSphere( debugWireSpherePosition, _debugWireSphereColor, _debugWireSphereRadius );
        }
        if( _debugCylinder )
        {
            DebugExtension.DrawCylinder( debugCylinderPosition, debugCylinderPosition + _debugCylinderEnd, _debugCylinderColor,
                _debugCylinderRadius );
        }
        if( _debugCone )
        {
            DebugExtension.DrawCone( debungConePosition, _debugConeDirection, _debugConeColor, _debugConeAngle );
        }
        if( _debugArrow )
        {
            DebugExtension.DrawArrow( debugArrowPosition, _debugArrowDirection, _debugArrowColor );
        }
        if( _debugCapsule )
        {
            DebugExtension.DrawCapsule( debugCapsulePosition, debugCapsulePosition + _debugCapsuleEnd, _debugCapsuleColor,
                _debugCapsuleRadius );
        }
    }

    // Update is called once per frame
    void Update()
    {
        if( _useGizmo )
        {
            return;
        }

        Vector3 debugPointPosition = _debugPointPosition;
        Vector3 debugBoundsPosition = _debugBoundsCenterPosition;
        Vector3 debugCirclePosition = _debugCirclePosition;
        Vector3 debugWireSpherePosition = _debugWireSpherePosition;
        Vector3 debugCylinderPosition = _debugCylinderPosition;
        Vector3 debungConePosition = _debungConePosition;
        Vector3 debugArrowPosition = _debugArrowPosition;
        Vector3 debugCapsulePosition = _debugCapsulePosition;

        if( _localPosition )
        {
            Vector3 wPosition = transform.position;
            debugPointPosition += wPosition;
            debugBoundsPosition += wPosition;
            debugCirclePosition += wPosition;
            debugWireSpherePosition += wPosition;
            debugCylinderPosition += wPosition;
            debungConePosition += wPosition;
            debugArrowPosition += wPosition;
            debugCapsulePosition += wPosition;
        }

        if( _debugPoint )
        {
            DebugExtension.DebugPoint( debugPointPosition, _debugPointColor, _debugPointScale );
        }
        if( _debugBounds )
        {
            DebugExtension.DebugBounds( new Bounds( debugBoundsPosition, _debugBoundsSize ), _debugBoundsColor );
        }
        if( _debugCircle )
        {
            DebugExtension.DebugCircle( debugCirclePosition, _debugCircleUp, _debugCircleColor, _debugCircleRadius );
        }
        if( _debugWireSphere )
        {
            DebugExtension.DebugWireSphere( debugWireSpherePosition, _debugWireSphereColor, _debugWireSphereRadius );
        }
        if( _debugCylinder )
        {
            DebugExtension.DebugCylinder( debugCylinderPosition, debugCylinderPosition + _debugCylinderEnd, _debugCylinderColor,
                _debugCylinderRadius );
        }
        if( _debugCone )
        {
            DebugExtension.DebugCone( debungConePosition, _debugConeDirection, _debugConeColor, _debugConeAngle );
        }
        if( _debugArrow )
        {
            DebugExtension.DebugArrow( debugArrowPosition, _debugArrowDirection, _debugArrowColor );
        }
        if( _debugCapsule )
        {
            DebugExtension.DebugCapsule( debugCapsulePosition, debugCapsulePosition + _debugCapsuleEnd, _debugCapsuleColor,
                _debugCapsuleRadius );
        }
    }
}
