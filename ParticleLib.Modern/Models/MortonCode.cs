using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ParticleLib.Modern.Models;

/// <summary>
/// Provides efficient Morton code (Z-order curve) operations for 3D spatial indexing.
/// </summary>
public static class MortonCode
{
    // Pre-computed lookup tables for faster encoding
    private static readonly byte[] _mortonTable256X;
    private static readonly byte[] _mortonTable256Y;
    private static readonly byte[] _mortonTable256Z;

    static MortonCode()
    {
        // Initialize lookup tables
        _mortonTable256X = new byte[256];
        _mortonTable256Y = new byte[256];
        _mortonTable256Z = new byte[256];

        for (int i = 0; i < 256; i++)
        {
            _mortonTable256X[i] = (byte)ExpandBits((uint)i);
            _mortonTable256Y[i] = (byte)(ExpandBits((uint)i) << 1);
            _mortonTable256Z[i] = (byte)(ExpandBits((uint)i) << 2);
        }
    }

    /// <summary>
    /// Encodes three coordinates into a 64-bit Morton code.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Encode(float x, float y, float z)
    {
        // Normalize to 21-bit unsigned integers (3 coordinates * 21 bits = 63 bits)
        uint ux = (uint)(x * 2097151.0f); // 2^21 - 1
        uint uy = (uint)(y * 2097151.0f);
        uint uz = (uint)(z * 2097151.0f);
        
        return Encode(ux, uy, uz);
    }

    /// <summary>
    /// Encodes three 21-bit unsigned integers into a 64-bit Morton code.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Encode(uint x, uint y, uint z)
    {
        // Ensure values don't exceed 21 bits
        x &= 0x1FFFFF;
        y &= 0x1FFFFF;
        z &= 0x1FFFFF;

        // Use hardware acceleration if available
        if (BitOperations.PopCount(0) == 0) // Check if BitOperations is hardware accelerated
        {
            return EncodeHardwareAccelerated(x, y, z);
        }
        
        // Fall back to lookup table implementation
        return EncodeLookupTable(x, y, z);
    }

    /// <summary>
    /// Encodes a 3D point into a 64-bit Morton code.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Encode(Point3D point)
    {
        return Encode(point.X, point.Y, point.Z);
    }

    /// <summary>
    /// Decodes a 64-bit Morton code into three coordinates.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point3D Decode(ulong code)
    {
        uint x = 0, y = 0, z = 0;
        
        // Use hardware acceleration if available
        if (BitOperations.PopCount(0) == 0) // Check if BitOperations is hardware accelerated
        {
            DecodeHardwareAccelerated(code, out x, out y, out z);
        }
        else
        {
            // Fall back to compact bit manipulation
            for (int i = 0; i < 21; i++)
            {
                x |= (uint)(((code >> (3 * i + 0)) & 1) << i);
                y |= (uint)(((code >> (3 * i + 1)) & 1) << i);
                z |= (uint)(((code >> (3 * i + 2)) & 1) << i);
            }
        }
        
        // Convert back to float coordinates
        const float invScale = 1.0f / 2097151.0f;
        return new Point3D(
            x * invScale,
            y * invScale,
            z * invScale
        );
    }

    /// <summary>
    /// Gets the depth of a Morton code (number of levels in the octree).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetDepth(ulong code)
    {
        if (BitOperations.LeadingZeroCount(code) == 64)
        {
            return 0;
        }
        
        return (63 - BitOperations.LeadingZeroCount(code)) / 3 + 1;
    }

    /// <summary>
    /// Gets the quadrant (octant) of a Morton code at the specified level.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte GetQuadrant(ulong code, int level)
    {
        int shift = (level - 1) * 3;
        return (byte)((code >> shift) & 0b111);
    }

    /// <summary>
    /// Gets the quadrant (octant) of a Morton code at its deepest level.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte GetQuadrant(ulong code)
    {
        return (byte)(code & 0b111);
    }

    /// <summary>
    /// Creates a Morton code for a child node at the specified quadrant and level.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong CreateChildCode(ulong parentCode, byte quadrant, int level)
    {
        int shift = (level - 1) * 3;
        return parentCode | ((ulong)quadrant << shift);
    }

    /// <summary>
    /// Creates a child Morton code by combining a parent code with a quadrant at a specific depth.
    /// </summary>
    public static ulong CreateChildCode(byte quadrant, int depth)
    {
        return (ulong)quadrant << (3 * depth);
    }
    
    /// <summary>
    /// Gets the parent Morton code of the specified code.
    /// </summary>
    public static ulong GetParentCode(ulong code)
    {
        // Find the highest 3 bits that are set
        int highestBit = 0;
        ulong temp = code;
        
        while (temp > 0)
        {
            temp >>= 1;
            highestBit++;
        }
        
        // Round up to the nearest multiple of 3
        int depth = (highestBit + 2) / 3;
        
        // Clear the 3 bits at that depth
        ulong mask = ~(7UL << (3 * (depth - 1)));
        return code & mask;
    }

    /// <summary>
    /// Hardware-accelerated Morton code encoding using bit operations.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong EncodeHardwareAccelerated(uint x, uint y, uint z)
    {
        x = (uint)((x | (x << 32)) & 0x1f00000000ffff);
        x = (uint)((x | (x << 16)) & 0x1f0000ff0000ff);
        x = (uint)((x | (x << 8)) & 0x100f00f00f00f00f);
        x = (uint)((x | (x << 4)) & 0x10c30c30c30c30c3);
        x = (uint)((x | (x << 2)) & 0x1249249249249249);

        y = (uint)((y | (y << 32)) & 0x1f00000000ffff);
        y = (uint)((y | (y << 16)) & 0x1f0000ff0000ff);
        y = (uint)((y | (y << 8)) & 0x100f00f00f00f00f);
        y = (uint)((y | (y << 4)) & 0x10c30c30c30c30c3);
        y = (uint)((y | (y << 2)) & 0x1249249249249249);

        z = (uint)((z | (z << 32)) & 0x1f00000000ffff);
        z = (uint)((z | (z << 16)) & 0x1f0000ff0000ff);
        z = (uint)((z | (z << 8)) & 0x100f00f00f00f00f);
        z = (uint)((z | (z << 4)) & 0x10c30c30c30c30c3);
        z = (uint)((z | (z << 2)) & 0x1249249249249249);

        return x | (y << 1) | (z << 2);
    }

    /// <summary>
    /// Lookup table-based Morton code encoding for better performance.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong EncodeLookupTable(uint x, uint y, uint z)
    {
        ulong answer = 0;
        
        // Process all bytes of the input coordinates
        for (int i = 0; i < 3; i++)
        {
            answer |= (ulong)_mortonTable256X[(x >> (i * 8)) & 0xFF] << (3 * i);
            answer |= (ulong)_mortonTable256Y[(y >> (i * 8)) & 0xFF] << (3 * i);
            answer |= (ulong)_mortonTable256Z[(z >> (i * 8)) & 0xFF] << (3 * i);
        }
        
        return answer;
    }

    /// <summary>
    /// Hardware-accelerated Morton code decoding.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void DecodeHardwareAccelerated(ulong code, out uint x, out uint y, out uint z)
    {
        x = CompactBits(code);
        y = CompactBits(code >> 1);
        z = CompactBits(code >> 2);
    }

    /// <summary>
    /// Expands a 10-bit integer into 30 bits by inserting 2 zeros after each bit.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint ExpandBits(uint v)
    {
        v &= 0x000003ff;                  // x = ---- ---- ---- ---- ---- --98 7654 3210
        v = (v ^ (v << 16)) & 0xff0000ff; // x = ---- --98 ---- ---- ---- ---- 7654 3210
        v = (uint)((v ^ (v << 8)) & 0x100f00f00f00f00f);  // x = ---- --98 ---- ---- 7654 ---- ---- 3210
        v = (uint)((v ^ (v << 4)) & 0x10c30c30c30c30c3);  // x = ---- --98 ---- 76-- --54 ---- 32-- --10
        v = (uint)((v ^ (v << 2)) & 0x1249249249249249);  // x = ---- 9--8 --7- -6-- 5--4 --3- -2-- 1--0
        return v;
    }

    /// <summary>
    /// Compacts a 30-bit integer into 10 bits by removing 2 bits after each bit.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint CompactBits(ulong v)
    {
        v &= 0x9249249249249249;                  // x = ---- 9--8 --7- -6-- 5--4 --3- -2-- 1--0
        v = (v ^ (v >> 2)) & 0x030c30c30c30c30c3; // x = ---- --98 ---- 76-- --54 ---- 32-- --10
        v = (v ^ (v >> 4)) & 0x0300f00f00f00f00f; // x = ---- --98 ---- ---- 7654 ---- ---- 3210
        v = (v ^ (v >> 8)) & 0xff0000ff0000ff;    // x = ---- --98 ---- ---- ---- ---- 7654 3210
        v = (v ^ (v >> 16)) & 0x00000000000003ff; // x = ---- ---- ---- ---- ---- --98 7654 3210
        return (uint)v;
    }
}
