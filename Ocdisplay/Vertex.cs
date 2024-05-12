using System.Numerics;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;

namespace Ocdisplay
{
    public struct Vertex
    {
        public Vector3 Position;
        public Color4 Color; 

        public Vertex(Vector3 position, Color4? color = null)
        {
            Position = position;
            Color = color ?? Color4.Black;
        }

        public static InputElement[] InputElements => new[]
        {
        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
        new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 12, 0)
    };
    }
}
