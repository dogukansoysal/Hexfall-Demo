using UnityEngine;

namespace ScriptableObject
{
    [CreateAssetMenu(menuName = "ScriptableObject/New Level", order = 0)]
    public class Level : UnityEngine.ScriptableObject
    {
        [Range(3,6)] public int ColorCount = 6;
        [Range(2, 11)] public int GridWidth = 8;
        [Range(2, 11)] public int GridHeight = 9;
        [Range(0f, 1f)] public float Gap = 0f;
    }
}
