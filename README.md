# ParticleLib - Octree with Morton Codes

This project is a proof of concept implementation of a static octree with Morton codes for efficient O(1) sector lookups. The implementation demonstrates several advanced concepts:

1. Spatial indexing using Morton codes (Z-order curves)
2. Parallel processing for particle simulation
3. Particle reflow across octree boundaries
4. Memory-efficient spatial partitioning

## Project Structure

The solution contains several projects:

- **ParticleLib**: Original implementation using .NET Framework 4.7.1
- **ParticleLib.Modern**: Modernized implementation using .NET 8 and modern C# features
- **PerformanceTests**: Benchmarking suite to compare the performance of both implementations
- **StaticTree**: Console application demonstrating the original implementation
- **TestParticle**: Test application for the original implementation
- **Ocdisplay**: Visualization tool for the octree

## Modernization Effort

The modernized implementation in `ParticleLib.Modern` leverages .NET 8 and modern C# features to improve performance and maintainability:

- **SIMD Operations**: Uses `System.Numerics.Vector` for hardware-accelerated vector operations
- **Memory Pooling**: Uses `ArrayPool<T>` to reduce GC pressure
- **Modern Concurrency**: Improved parallel processing with modern concurrency primitives
- **Improved Type Safety**: Uses nullable reference types and readonly structs
- **Hardware Intrinsics**: Leverages hardware-specific optimizations when available

For more details, see the [Architectural Decision Record](docs/adr/001-modernize-particlelib.md).

## Performance Testing

The `PerformanceTests` project contains benchmarks to compare the performance of the original and modernized implementations. The benchmarks measure:

- Octree construction performance
- Particle addition throughput
- Lookup performance
- Processing performance
- Scalability with different numbers of particles

### Running the Benchmarks

To run the benchmarks:

```powershell
cd PerformanceTests
dotnet run -c Release
```

For a quick test during development:

```powershell
dotnet run -c Release -- --quick
```

## Key Concepts

### Morton Codes (Z-order curves)

Morton codes map multidimensional data to one dimension while preserving locality. This allows for efficient spatial indexing and O(1) lookups based on the depth of the tree.

### Octree Structure

The octree recursively divides 3D space into eight octants. Each node in the tree represents a cubic region of space, with child nodes representing the eight subdivisions of that region.

### Parallel Processing

The implementation supports parallel processing of particles, with each node potentially having its own processor. This allows for efficient utilization of multi-core systems.

### Particle Reflow

When particles move outside their current node's boundary, they are automatically reassigned to the appropriate node. This dynamic restructuring ensures that particles are always in the correct spatial partition.

## Future Improvements

Potential areas for further optimization:

1. GPU acceleration for particle processing
2. Dynamic load balancing for better parallel performance
3. Adaptive subdivision based on particle density
4. Compressed octree representation for memory efficiency
