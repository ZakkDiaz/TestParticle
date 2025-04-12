using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ParticleLib.Modern.Models;

/// <summary>
/// Represents a point in 3D space with optimized vector operations using SIMD when available.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct Point3D : IEquatable<Point3D>
{
    public readonly float X;
    public readonly float Y;
    public readonly float Z;

    /// <summary>
    /// Creates a new Point3D with the specified coordinates.
    /// </summary>
    public Point3D(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>
    /// Creates a Point3D from a Vector3.
    /// </summary>
    public Point3D(Vector3 vector)
    {
        X = vector.X;
        Y = vector.Y;
        Z = vector.Z;
    }

    /// <summary>
    /// Implicitly converts a Point3D to a Vector3 for SIMD operations.
    /// </summary>
    public static implicit operator Vector3(Point3D point) => new(point.X, point.Y, point.Z);

    /// <summary>
    /// Implicitly converts a Vector3 to a Point3D.
    /// </summary>
    public static implicit operator Point3D(Vector3 vector) => new(vector);

    /// <summary>
    /// Subtracts one point from another, returning a new point.
    /// Uses SIMD operations when available.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point3D operator -(Point3D to, Point3D from)
    {
        if (Vector.IsHardwareAccelerated && Vector<float>.Count >= 3)
        {
            return new Point3D(Vector3.Subtract(to, from));
        }
        
        return new Point3D(to.X - from.X, to.Y - from.Y, to.Z - from.Z);
    }

    /// <summary>
    /// Adds two points, returning a new point.
    /// Uses SIMD operations when available.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point3D operator +(Point3D a, Point3D b)
    {
        if (Vector.IsHardwareAccelerated && Vector<float>.Count >= 3)
        {
            return new Point3D(Vector3.Add(a, b));
        }
        
        return new Point3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }

    /// <summary>
    /// Divides a point by a scalar, returning a new point.
    /// Uses SIMD operations when available.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point3D operator /(Point3D point, float scalar)
    {
        if (Vector.IsHardwareAccelerated && Vector<float>.Count >= 3)
        {
            return new Point3D(Vector3.Divide(point, scalar));
        }
        
        return new Point3D(point.X / scalar, point.Y / scalar, point.Z / scalar);
    }

    /// <summary>
    /// Multiplies a point by a scalar, returning a new point.
    /// Uses SIMD operations when available.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point3D operator *(Point3D point, float scalar)
    {
        if (Vector.IsHardwareAccelerated && Vector<float>.Count >= 3)
        {
            return new Point3D(Vector3.Multiply(point, scalar));
        }
        
        return new Point3D(point.X * scalar, point.Y * scalar, point.Z * scalar);
    }

    /// <summary>
    /// Calculates the distance between two points.
    /// Uses SIMD operations when available.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float DistanceTo(Point3D other)
    {
        if (Vector.IsHardwareAccelerated && Vector<float>.Count >= 3)
        {
            return Vector3.Distance(this, other);
        }
        
        float dx = X - other.X;
        float dy = Y - other.Y;
        float dz = Z - other.Z;
        return MathF.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    /// <summary>
    /// Calculates the squared distance between two points.
    /// This is faster than DistanceTo when only comparing distances.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float DistanceSquaredTo(Point3D other)
    {
        if (Vector.IsHardwareAccelerated && Vector<float>.Count >= 3)
        {
            return Vector3.DistanceSquared(this, other);
        }
        
        float dx = X - other.X;
        float dy = Y - other.Y;
        float dz = Z - other.Z;
        return dx * dx + dy * dy + dz * dz;
    }

    /// <summary>
    /// Creates a point at the center of two points.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point3D Center(Point3D a, Point3D b)
    {
        if (Vector.IsHardwareAccelerated && Vector<float>.Count >= 3)
        {
            return new Point3D(Vector3.Lerp(a, b, 0.5f));
        }
        
        return new Point3D(
            (a.X + b.X) * 0.5f,
            (a.Y + b.Y) * 0.5f,
            (a.Z + b.Z) * 0.5f
        );
    }

    /// <summary>
    /// Returns a string representation of the point.
    /// </summary>
    public override string ToString() => $"({X}, {Y}, {Z})";
    
    /// <summary>
    /// Determines whether this point is equal to another point.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is Point3D other)
        {
            return Equals(other);
        }
        return false;
    }
    
    /// <summary>
    /// Determines whether this point is equal to another point.
    /// </summary>
    public bool Equals(Point3D other)
    {
        const float epsilon = 0.0001f;
        return Math.Abs(X - other.X) < epsilon &&
               Math.Abs(Y - other.Y) < epsilon &&
               Math.Abs(Z - other.Z) < epsilon;
    }
    
    /// <summary>
    /// Returns a hash code for this point.
    /// </summary>
    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }
    
    /// <summary>
    /// Determines whether two points are equal.
    /// </summary>
    public static bool operator ==(Point3D left, Point3D right)
    {
        return left.Equals(right);
    }
    
    /// <summary>
    /// Determines whether two points are not equal.
    /// </summary>
    public static bool operator !=(Point3D left, Point3D right)
    {
        return !(left == right);
    }
    
    /// <summary>
    /// Creates a point at the origin (0,0,0).
    /// </summary>
    public static Point3D Origin => new(0, 0, 0);
}
