using System.Numerics;
using System.Runtime.CompilerServices;

namespace ParticleLib.Modern.Models;

/// <summary>
/// Represents an Axis-Aligned Bounding Box (AABB) in 3D space.
/// </summary>
public readonly struct AAABBB : IEquatable<AAABBB>
{
    /// <summary>
    /// The minimum point of the bounding box.
    /// </summary>
    public readonly Point3D Min { get; init; }
    
    /// <summary>
    /// The maximum point of the bounding box.
    /// </summary>
    public readonly Point3D Max { get; init; }

    /// <summary>
    /// Creates a new AABB with the specified minimum and maximum points.
    /// </summary>
    public AAABBB(Point3D min, Point3D max)
    {
        Min = min;
        Max = max;
    }

    /// <summary>
    /// Gets the center point of the bounding box.
    /// </summary>
    public Point3D Center => Point3D.Center(Min, Max);

    /// <summary>
    /// Gets the size of the bounding box in each dimension.
    /// </summary>
    public Point3D Size => Max - Min;

    /// <summary>
    /// Checks if this bounding box contains the specified point.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(Point3D point)
    {
        if (Vector.IsHardwareAccelerated && Vector<float>.Count >= 3)
        {
            // Use SIMD operations for the comparison when available
            Vector3 p = point;
            Vector3 min = Min;
            Vector3 max = Max;
            
            // Check if min <= p <= max for all components
            return p.X >= min.X && p.X <= max.X &&
                   p.Y >= min.Y && p.Y <= max.Y &&
                   p.Z >= min.Z && p.Z <= max.Z;
        }
        
        return point.X >= Min.X && point.X <= Max.X &&
               point.Y >= Min.Y && point.Y <= Max.Y &&
               point.Z >= Min.Z && point.Z <= Max.Z;
    }

    /// <summary>
    /// Checks if this bounding box intersects with another bounding box.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Intersects(AAABBB other)
    {
        if (Vector.IsHardwareAccelerated && Vector<float>.Count >= 3)
        {
            // Use SIMD operations for the comparison when available
            Vector3 min = Min;
            Vector3 max = Max;
            Vector3 otherMin = other.Min;
            Vector3 otherMax = other.Max;
            
            // Check if the boxes overlap in all dimensions
            return min.X <= otherMax.X && max.X >= otherMin.X &&
                   min.Y <= otherMax.Y && max.Y >= otherMin.Y &&
                   min.Z <= otherMax.Z && max.Z >= otherMin.Z;
        }
        
        return Min.X <= other.Max.X && Max.X >= other.Min.X &&
               Min.Y <= other.Max.Y && Max.Y >= other.Min.Y &&
               Min.Z <= other.Max.Z && Max.Z >= other.Min.Z;
    }

    /// <summary>
    /// Creates a new bounding box that contains both this box and the specified box.
    /// </summary>
    public AAABBB Union(AAABBB other)
    {
        if (Vector.IsHardwareAccelerated && Vector<float>.Count >= 3)
        {
            // Use SIMD operations when available
            Vector3 min = Vector3.Min(Min, other.Min);
            Vector3 max = Vector3.Max(Max, other.Max);
            
            return new AAABBB(new Point3D(min), new Point3D(max));
        }
        
        return new AAABBB(
            new Point3D(
                Math.Min(Min.X, other.Min.X),
                Math.Min(Min.Y, other.Min.Y),
                Math.Min(Min.Z, other.Min.Z)
            ),
            new Point3D(
                Math.Max(Max.X, other.Max.X),
                Math.Max(Max.Y, other.Max.Y),
                Math.Max(Max.Z, other.Max.Z)
            )
        );
    }

    /// <summary>
    /// Creates a new bounding box that is the intersection of this box and the specified box.
    /// </summary>
    public AAABBB? Intersection(AAABBB other)
    {
        if (!Intersects(other))
        {
            return null;
        }
        
        if (Vector.IsHardwareAccelerated && Vector<float>.Count >= 3)
        {
            // Use SIMD operations when available
            Vector3 min = Vector3.Max(Min, other.Min);
            Vector3 max = Vector3.Min(Max, other.Max);
            
            return new AAABBB(new Point3D(min), new Point3D(max));
        }
        
        return new AAABBB(
            new Point3D(
                Math.Max(Min.X, other.Min.X),
                Math.Max(Min.Y, other.Min.Y),
                Math.Max(Min.Z, other.Min.Z)
            ),
            new Point3D(
                Math.Min(Max.X, other.Max.X),
                Math.Min(Max.Y, other.Max.Y),
                Math.Min(Max.Z, other.Max.Z)
            )
        );
    }

    /// <summary>
    /// Splits this bounding box into eight equal octants.
    /// </summary>
    public AAABBB[] Split()
    {
        Point3D center = Center;
        
        return new[]
        {
            // Bottom octants (z < center.Z)
            new AAABBB(new Point3D(Min.X, Min.Y, Min.Z), new Point3D(center.X, center.Y, center.Z)),
            new AAABBB(new Point3D(center.X, Min.Y, Min.Z), new Point3D(Max.X, center.Y, center.Z)),
            new AAABBB(new Point3D(Min.X, center.Y, Min.Z), new Point3D(center.X, Max.Y, center.Z)),
            new AAABBB(new Point3D(center.X, center.Y, Min.Z), new Point3D(Max.X, Max.Y, center.Z)),
            
            // Top octants (z >= center.Z)
            new AAABBB(new Point3D(Min.X, Min.Y, center.Z), new Point3D(center.X, center.Y, Max.Z)),
            new AAABBB(new Point3D(center.X, Min.Y, center.Z), new Point3D(Max.X, center.Y, Max.Z)),
            new AAABBB(new Point3D(Min.X, center.Y, center.Z), new Point3D(center.X, Max.Y, Max.Z)),
            new AAABBB(new Point3D(center.X, center.Y, center.Z), new Point3D(Max.X, Max.Y, Max.Z))
        };
    }

    public override bool Equals(object? obj) => obj is AAABBB box && Equals(box);

    public bool Equals(AAABBB other) => Min.Equals(other.Min) && Max.Equals(other.Max);

    public override int GetHashCode() => HashCode.Combine(Min, Max);

    public override string ToString() => $"AABB[{Min} -> {Max}]";
}
