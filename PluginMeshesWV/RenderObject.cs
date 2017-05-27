using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Windows;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using SharpDX.Mathematics.Interop;

using Device = SharpDX.Direct3D11.Device;

namespace PluginMeshesWV
{
    public class RenderObject
    {
        public enum RenderType
        {
            Lines,
            TriListWire
        }

        public Device device;
        public RenderType type;
        public RawVector3[] vertices = new RawVector3[] { new RawVector3(-0.5f, 0.5f, 0.0f), new RawVector3(0.5f, 0.5f, 0.0f), new RawVector3(0.0f, -0.5f, 0.0f) };
        public SharpDX.Direct3D11.Buffer triangleVertexBuffer;
        public PixelShader pixelShader;

        public RenderObject(Device d, RenderType t, PixelShader p)
        {
            device = d;
            type = t;
            pixelShader = p;
        }

        public void InitGeometry()
        {
            triangleVertexBuffer = SharpDX.Direct3D11.Buffer.Create<RawVector3>(device, BindFlags.VertexBuffer, vertices);
        }

        public void Render(DeviceContext context)
        {
            switch (type)
            {
                case RenderType.Lines:
                    context.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
                    break;
                case RenderType.TriListWire:
                    context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                    break;
            }
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(triangleVertexBuffer, Utilities.SizeOf<RawVector3>(), 0));
            context.PixelShader.Set(pixelShader);
            context.Draw(vertices.Count(), 0);
        }

        public void Dispose()
        {
            triangleVertexBuffer.Dispose();
        }
    }
}
