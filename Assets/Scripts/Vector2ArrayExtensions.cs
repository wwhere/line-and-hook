using UnityEngine;

public static class Vector2ArrayExtensions
{
    /// <summary>
    /// Returns a deep clone of the Vecto3 array
    /// </summary>
    /// <param name="original"></param>
    /// <returns></returns>
    public static Vector2[] ClonePositions(this Vector2[] original)
    {
        var clone = new Vector2[original.Length];
        for (var i = 0; i < original.Length; i++)
        {
            clone[i] = new Vector2(original[i].x, original[i].y);
        }
        return clone;
    }
}
