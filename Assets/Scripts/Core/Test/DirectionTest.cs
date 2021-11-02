using Core.Game.Utility;
using UnityEngine;

public class DirectionTest : MonoBehaviour
{
    public Transform a;
    public Transform b;

    private int tempDir = -1;
    private double tempAngle = double.MaxValue;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var dir = DirectionUtil.GetDir16(a.position - b.position);
        if (tempDir != dir)
        {
            Debug.LogError("dir=" + dir);
            tempDir = dir;

        }
        var angle = Vector3.SignedAngle(Vector3.forward, a.position - b.position, Vector3.up);
        if (angle != tempAngle)
        {
            Debug.LogError("angle=" + angle);
            tempAngle = angle;
        }
    }
}
