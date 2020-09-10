using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using d3d = SharpDX.Direct3D;
using d3d11 = SharpDX.Direct3D11;
using d2d = SharpDX.Direct2D1;
using dfx = SharpDX.Direct2D1.Effects;
using dw = SharpDX.DirectWrite;
using wic = SharpDX.WIC;
using dmi = SharpDX.Mathematics.Interop;
using dx = SharpDX;
using dxgi = SharpDX.DXGI;

using SharpDX.Direct2D1;
using SharpDX.Direct2D1.Effects;

using Vector3 = SharpDX.Mathematics.Interop.RawVector3;
using Vector4 = SharpDX.Mathematics.Interop.RawVector4;
using Vector2 = SharpDX.Mathematics.Interop.RawVector2;
using System.IO;
using System.Windows.Media;
using System.Windows.Forms.Integration;
using System.Windows.Controls.Primitives;
using System.Windows;

namespace Simula.DirectX
{
    public partial class HandleRenderTarget : UserControl
    {
        WindowsFormsHost parent = null;
        public HandleRenderTarget(WindowsFormsHost par) {
            InitializeComponent();
            this.parent = par;
            CompositionTarget.Rendering += TargetRefresh;
            this.Resize += TargetResize;
            Init(); 
        }

        private void TargetResize(object sender, EventArgs e) {
            defaultDevice.Dispose();
            d3dDevice.Dispose();
            dxgiDevice.Dispose();
            d2dDevice.Dispose();
            imagingFactory.Dispose();
            d2dContext.Dispose();
            dwFactory.Dispose();

            defaultDevice = new d3d11.Device(SharpDX.Direct3D.DriverType.Hardware, d3d11.DeviceCreationFlags.BgraSupport);
            d3dDevice = defaultDevice.QueryInterface<d3d11.Device1>();
            dxgiDevice = d3dDevice.QueryInterface<dxgi.Device>();
            d2dDevice = new d2d.Device(dxgiDevice);
            imagingFactory = new wic.ImagingFactory2();
            d2dContext = new d2d.DeviceContext(d2dDevice, d2d.DeviceContextOptions.None);
            dwFactory = new dw.Factory(dw.FactoryType.Shared);

            var hwndProps = new d2d.HwndRenderTargetProperties();
            var pxFormat = new d2d.PixelFormat(dxgi.Format.R8G8B8A8_UNorm, d2d.AlphaMode.Premultiplied);
            hwndProps.Hwnd = this.Handle;
            hwndProps.PixelSize = new dx.Size2(this.Width, this.Height);
            hwndProps.PresentOptions = d2d.PresentOptions.None;

            var rndTargProps = new d2d.RenderTargetProperties(d2d.RenderTargetType.Default, pxFormat, 0, 0,
                 d2d.RenderTargetUsage.None, d2d.FeatureLevel.Level_DEFAULT);
            if (wndRenderTarget != null) {
                wndRenderTarget.Dispose();
                wndRenderTarget = null;
                wndRenderTarget = new d2d.WindowRenderTarget(d2dContext.Factory, rndTargProps, hwndProps);
            } else {
                wndRenderTarget = new d2d.WindowRenderTarget(d2dContext.Factory, rndTargProps, hwndProps);
            }
            Draw();
        }

        private void TargetRefresh(object sender, EventArgs e) {
            Draw();
        }

        private void Init() {
            // 初始化 D3D 设备。
            defaultDevice = new d3d11.Device(SharpDX.Direct3D.DriverType.Hardware, d3d11.DeviceCreationFlags.BgraSupport);
            // 获取 Direct3D 11.1 设备。
            d3dDevice = defaultDevice.QueryInterface<d3d11.Device1>();
            // 获取 DXGI 设备。
            dxgiDevice = d3dDevice.QueryInterface<dxgi.Device>();
            // 初始化 Direct2D 设备。
            d2dDevice = new d2d.Device(dxgiDevice);
            // 初始化 WIC 工厂。
            imagingFactory = new wic.ImagingFactory2();
            // 初始化 Direct2D 设备上下文。
            d2dContext = new d2d.DeviceContext(d2dDevice, d2d.DeviceContextOptions.None);
            // 初始化 DirectWrite 工厂。
            dwFactory = new dw.Factory(dw.FactoryType.Shared);

            var hwndProps = new d2d.HwndRenderTargetProperties();
            var pxFormat = new d2d.PixelFormat(dxgi.Format.R8G8B8A8_UNorm, d2d.AlphaMode.Premultiplied);
            hwndProps.Hwnd = this.Handle;
            hwndProps.PixelSize = new dx.Size2(this.Width, this.Height);
            hwndProps.PresentOptions = d2d.PresentOptions.None;

            var rndTargProps = new d2d.RenderTargetProperties(d2d.RenderTargetType.Default, pxFormat, 0, 0,
                 d2d.RenderTargetUsage.None, d2d.FeatureLevel.Level_DEFAULT);
            if (wndRenderTarget != null) {
                wndRenderTarget.Dispose();
                wndRenderTarget = null;
                wndRenderTarget = new d2d.WindowRenderTarget(d2dContext.Factory, rndTargProps, hwndProps);
            } else {
                wndRenderTarget = new d2d.WindowRenderTarget(d2dContext.Factory, rndTargProps, hwndProps);
            }
        }

        dx.Mathematics.Interop.RawPoint mouse = new dx.Mathematics.Interop.RawPoint(-1, -1);
        d3d11.Device defaultDevice;
        d3d11.Device1 d3dDevice;
        dxgi.Device dxgiDevice;
        d2d.Device d2dDevice;
        d2d.DeviceContext d2dContext;
        dw.Factory dwFactory;
        wic.ImagingFactory2 imagingFactory;
        d2d.WindowRenderTarget wndRenderTarget;

        float elapse;
        private void Draw() {
            if (wndRenderTarget != null) {
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                wndRenderTarget.AntialiasMode = d2d.AntialiasMode.PerPrimitive;
                wndRenderTarget.BeginDraw();
                wndRenderTarget.Clear(new dx.Mathematics.Interop.RawColor4(1, 1, 1, 1));

                // Initialization
                var redBrush = new d2d.SolidColorBrush(wndRenderTarget, new dx.Mathematics.Interop.RawColor4(1, 0, 0, 1));
                var lightGrayBrush = new d2d.SolidColorBrush(wndRenderTarget, new dx.Mathematics.Interop.RawColor4(0.9f, 0.9f, 0.9f, 0.8f));
                var uiShadow = new d2d.SolidColorBrush(wndRenderTarget, new dx.Mathematics.Interop.RawColor4(0.5f, 0.5f, 0.5f, 0.5f));
                var uiLight = new d2d.SolidColorBrush(wndRenderTarget, new dx.Mathematics.Interop.RawColor4(0.5f,0.5f,0.5f, 1));
                var textPFSC = new dw.TextFormat(dwFactory, "PingFang SC", 20);

                Rect rawrect = new Rect();
                try {
                    var rect = LayoutInformation.GetLayoutClip(parent);
                    rawrect = rect.Bounds;
                } catch { }
                for (int i = 0; i < 1; i++) {
                    wndRenderTarget.DrawText("FPS: " + (1 / elapse).ToString() + "\n" +
                        "Display Rectangle: ("+rawrect.Left+","+rawrect.Top+","+rawrect.Width+","+rawrect.Height+")",
                        textPFSC,
                        new dx.Mathematics.Interop.RawRectangleF(10, 10, this.Width, this.Height),
                        redBrush);
                }

                wndRenderTarget.EndDraw();

                // Disposal
                redBrush.Dispose();
                textPFSC.Dispose();
                uiLight.Dispose();
                uiShadow.Dispose();
                lightGrayBrush.Dispose();

                sw.Stop();
                elapse = sw.ElapsedMilliseconds / 1000f;
            }
        }
    }
}
