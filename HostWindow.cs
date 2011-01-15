using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenTKGUI
{
    /// <summary>
    /// A window hosting a TKGUI Panel.
    /// </summary>
    public class HostWindow : GameWindow
    {
        public HostWindow(Control Control, string Title)
            : base(640, 480, GraphicsMode.Default, Title, GameWindowFlags.Default)
        {
            this.WindowState = WindowState.Maximized;
            this._Control = Control;
        }

#if DEBUG
        /// <summary>
        /// Main entry point (for testing).
        /// </summary>
        public static void Main(string[] Args)
        {
            ManualContainer mc = new ManualContainer();
            mc.AddChild(null, new Button("Hello?!?"), new Rectangle(200.0, 200.0, 300.0, 40.0));

            new HostWindow(mc, "Test").Run();
        }
#endif

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(Color.RGB(0.8, 0.8, 0.8));
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            GUIRenderContext rc = new GUIRenderContext(this.ViewSize);
            rc.Setup();
            this._Control.Render(rc);

            this.SwapBuffers();
        }

        /// <summary>
        /// Gets the size of the area rendered on this window.
        /// </summary>
        public Point ViewSize
        {
            get
            {
                return new Point((double)this.Width, (double)this.Height);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            if (this._Control != null)
            {
                GL.Viewport(0, 0, this.Width, this.Height);
                this._Control.Resize(null, this.ViewSize);
            }
        }

        private Control _Control;
    }
}