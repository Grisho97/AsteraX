using UnityEngine;

public static class Vector3Extensions
{
    /// <summary>
    /// <para>
    /// Divides the individual components of v0 by those of v1.
    /// </para><para>
    /// e.g. v0.ComponentDivide(v1) returns [ v0.x/v1.x, v0.y/v1.y, v0.z/v1.z ]
    /// </para><para>
    /// If any of the components of v1 are 0, then that component of v0 will
    /// remain unchanged to avoid divide by zero errors.
    /// </para>
    /// </summary>
    /// <returns>The Vector3 result of the ComponentDivide.</returns>
    /// <param name="v0">The numerator Vector3</param>
    /// <param name="v1">The denominator Vector3</param>
    static public Vector3 ComponentDivide(this Vector3 v0, Vector3 v1)
    {
        Vector3 vRes = v0;
        
        if (v1.x != 0)  
        {
            vRes.x = v0.x / v1.x;
        }
        if (v1.y != 0)
        {
            vRes.y = v0.y / v1.y;
        }
        if (v1.z != 0)
        {
            vRes.z = v0.z / v1.z;
        }

        return vRes;
    }
}


