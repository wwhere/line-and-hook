using UnityEngine;

public static class Vector3ArrayExtensions
{
    /// <summary>
    /// Returns a deep clone of the Vecto3 array
    /// </summary>
    /// <param name="original"></param>
    /// <returns></returns>
    public static Vector3[] ClonePositions(this Vector3[] original)
    {
        var clone = new Vector3[original.Length];
        for (var i = 0; i < original.Length; i++)
        {
            clone[i] = new Vector3(original[i].x, original[i].y, original[i].z);
        }
        return clone;
    }
}
