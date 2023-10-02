using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Nebula.Editor
{
    public static class EditorMath
    {
        private static MethodInfo _gridSizeGetMethod;
        public static Vector3 RoundToNearestGrid(Vector3 position, Vector3? gridSize = null)
        {
            gridSize ??= (Vector3)_gridSizeGetMethod.Invoke(null, null);

            var x = RoundToNearestGridValue(position.x, gridSize.Value.x);
            var y = RoundToNearestGridValue(position.y, gridSize.Value.y);
            var z = RoundToNearestGridValue(position.z, gridSize.Value.z);

            return new Vector3(x, y, z);
        }

        public static float RoundToNearestGridValue(float pos, float gridValue)
        {
            float xDiff = pos % gridValue;
            bool isPositive = pos > 0 ? true : false;
            pos -= xDiff;
            if (Mathf.Abs(xDiff) > (gridValue / 2))
            {
                if (isPositive)
                {
                    pos += gridValue;
                }
                else
                {
                    pos -= gridValue;
                }
            }
            return pos;
        }

        static EditorMath()
        {
            _gridSizeGetMethod = Assembly.Load("UnityEditor.dll").GetType("UnityEditor.GridSettings").GetProperty("size").GetGetMethod();
        }
    }
}