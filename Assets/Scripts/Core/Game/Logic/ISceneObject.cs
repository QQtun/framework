using UnityEngine;

namespace Core.Game.Logic
{
    public interface ISceneObject
    {
        int Id { get; }
        string Name { get;}
        GameObject GameObject { get; }
        Vector2Int OrigCoordinate { get; }
        Vector2Int Coordinate { get; set; }
        int Direction { get; }
        Quaternion Rotation { get; set; }
        int MoveSpeed { get; }
        ActionType Action { get; }

        SceneObjectType Type { get; }
        SpriteType SpriteType { get; }
        bool IsReady { get; }

        void OnUpdate();
    }
}