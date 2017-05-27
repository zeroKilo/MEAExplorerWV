using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

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
    public static class DXHelper
    {
        public static Device device;
        public static DeviceContext context;
        public static SwapChain swapChain;
        public static Texture2D backBuffer;
        public static RenderTargetView renderTargetView;
        public static float CamRot = 3.1415f / 180f, CamDis = 5f;
        public static List<RenderObject> objects;

        private static InputElement[] inputElements = new InputElement[] { new InputElement("POSITION", 0, Format.R32G32B32_Float, 0) };
        private static VertexShader vertexShader;
        public static PixelShader pixelShader;
        public static PixelShader pixelShaderSel;
        private static ShaderSignature inputSignature;
        private static InputLayout inputLayout;
        private static SharpDX.Direct3D11.Buffer constantBuffer;
        private static RasterizerState rasterState;
        private static RawViewportF viewport;

        private static RawMatrix world;
        private static RawMatrix view;
        private static RawMatrix proj;
        private static RawVector3 camPos;

        [StructLayout(LayoutKind.Sequential)]
        public struct ConstantBufferData
        {
            public Matrix world;
        }

        public static void Init(PictureBox f)
        {
            InitDevice(f);
            InitShaders();
            objects = new List<RenderObject>();
            RenderObject d = new RenderObject(device, RenderObject.RenderType.TriListWire, pixelShader);
            d.InitGeometry();
            objects.Add(d);
        }

        private static void InitDevice(PictureBox f)
        {
            Device.CreateWithSwapChain(
                SharpDX.Direct3D.DriverType.Hardware,
                DeviceCreationFlags.None,
                new[] {
                    SharpDX.Direct3D.FeatureLevel.Level_11_1,
                    SharpDX.Direct3D.FeatureLevel.Level_11_0,
                    SharpDX.Direct3D.FeatureLevel.Level_10_1,
                    SharpDX.Direct3D.FeatureLevel.Level_10_0,
                },
                new SwapChainDescription()
                {
                    ModeDescription =
                        new ModeDescription(
                            f.ClientSize.Width,
                            f.ClientSize.Height,
                            new Rational(60, 1),
                            Format.R8G8B8A8_UNorm
                        ),
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = SharpDX.DXGI.Usage.BackBuffer | Usage.RenderTargetOutput,
                    BufferCount = 1,
                    Flags = SwapChainFlags.None,
                    IsWindowed = true,
                    OutputHandle = f.Handle,
                    SwapEffect = SwapEffect.Discard,
                },
                out device, out swapChain
            );
            context = device.ImmediateContext;
            backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
            renderTargetView = new RenderTargetView(device, backBuffer);
            Resize(f);
        }

        private static void InitShaders()
        {
            ShaderBytecode vertexShaderByteCode = vertexShaderByteCode = ShaderBytecode.Compile(Properties.Res.vertexShader, "main", "vs_4_0", ShaderFlags.Debug);
            vertexShader = new VertexShader(device, vertexShaderByteCode);
            inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
            ShaderBytecode pixelShaderByteCode = ShaderBytecode.Compile(Properties.Res.pixelShader, "main", "ps_4_0", ShaderFlags.Debug);
            pixelShader = new PixelShader(device, pixelShaderByteCode);
            ShaderBytecode pixelShaderByteCodeSel = ShaderBytecode.Compile(Properties.Res.pixelShaderSel, "main", "ps_4_0", ShaderFlags.Debug);
            pixelShaderSel = new PixelShader(device, pixelShaderByteCodeSel);
            context.VertexShader.Set(vertexShader);
            inputLayout = new InputLayout(device, inputSignature, inputElements);
            context.InputAssembler.InputLayout = inputLayout;
            SharpDX.Direct3D11.BufferDescription buffdesc = new SharpDX.Direct3D11.BufferDescription()
            {
                Usage = ResourceUsage.Dynamic,
                SizeInBytes = Utilities.SizeOf<ConstantBufferData>(),
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            };
            constantBuffer = new SharpDX.Direct3D11.Buffer(device, buffdesc);
            RasterizerStateDescription renderStateDesc = new RasterizerStateDescription
            {
                CullMode = CullMode.None,
                DepthBias = 0,
                DepthBiasClamp = 0,
                FillMode = FillMode.Wireframe,
                IsAntialiasedLineEnabled = false,
                IsDepthClipEnabled = true,
                IsFrontCounterClockwise = false,
                IsMultisampleEnabled = true,
                IsScissorEnabled = false,
                SlopeScaledDepthBias = 0
            };
            rasterState = new RasterizerState(device, renderStateDesc);
            context.Rasterizer.State = rasterState;
        }

        public static void Resize(PictureBox f)
        {
            if (renderTargetView != null) { renderTargetView.Dispose(); }
            backBuffer.Dispose();
            swapChain.ResizeBuffers(1, f.ClientSize.Width, f.ClientSize.Height, SharpDX.DXGI.Format.Unknown, SwapChainFlags.AllowModeSwitch);
            backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
            renderTargetView = new RenderTargetView(device, backBuffer);
            viewport = new RawViewportF();
            viewport.X = 0;
            viewport.Y = 0;
            viewport.Width = f.ClientSize.Width;
            viewport.Height = f.ClientSize.Height;
            context.Rasterizer.SetViewport(viewport);
            proj = Matrix.PerspectiveFovLH((float)Math.PI / 3f, f.ClientSize.Width / (float)f.ClientSize.Height, 0.5f, 100f);
        }

        public static void Render()
        {
            context.OutputMerger.SetRenderTargets(renderTargetView);
            context.ClearRenderTargetView(renderTargetView, new RawColor4(0, 128, 255, 255));
            camPos = new RawVector3((float)Math.Sin(CamRot) * CamDis, 0, (float)Math.Cos(CamRot) * CamDis);
            view = Matrix.LookAtLH(camPos, Vector3.Zero, Vector3.UnitY);
            world = Matrix.Identity * view * proj;
            world = Matrix.Transpose(world);
            DataStream data;
            context.MapSubresource(constantBuffer, 0, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out data);
            data.Write(world);
            context.UnmapSubresource(constantBuffer, 0);
            context.VertexShader.SetConstantBuffer(0, constantBuffer);
            foreach (RenderObject ro in objects)
                ro.Render(context);
            swapChain.Present(0, PresentFlags.None);
        }

        public static void Cleanup()
        {
            renderTargetView.Dispose();
            backBuffer.Dispose();
            device.Dispose();
            swapChain.Dispose();
            foreach (RenderObject ro in objects)
                ro.Dispose();
            inputLayout.Dispose();
            inputSignature.Dispose();
        }
    }
}
