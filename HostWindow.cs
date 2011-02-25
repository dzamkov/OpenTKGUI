using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace OpenTKGUI
{
    /// <summary>
    /// A window hosting a TKGUI Panel.
    /// </summary>
    public class HostWindow : GameWindow
    {
        public HostWindow(BuildControlHandler BuildControl, string Title)
            : this(BuildControl, Title, 640, 480)
        {
            
        }

        public HostWindow(BuildControlHandler BuildControl, string Title, int Width, int Height)
            : this(Title, Width, Height)
        {
            this._Control = BuildControl();
        }

        public HostWindow(string Title, int Width, int Height)
            : base(Width, Height, GraphicsMode.Default, Title, GameWindowFlags.Default)
        {
            this._KeyboardState = new WindowKeyboardState(this);
            this._MouseState = new WindowMouseState(this);
        }

        /// <summary>
        /// Gets or sets the control displayed by the host window. This can not be null during an update or render.
        /// </summary>
        public Control Control
        {
            get
            {
                return this._Control;
            }
            set
            {
                this._Control = value;
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(Color.RGB(1.0, 1.0, 1.0));
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
			
            RenderContext rc = new RenderContext(this.ViewSize);
            rc.Setup();
            this._Control.Render(rc);

            this.SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            this._MouseState.Update();

            InputContext ic = new InputContext(e.Time, this._FocusStack, this._MouseState, this._KeyboardState);
            this._Control.Update(ic);
            this._FocusStack = ic.NextFocusStack;

            this._KeyboardState.PostUpdate();
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
                this._Control.ForceResize(this.ViewSize);
            }
        }

        /// <summary>
        /// Gets the keyboard state for the window.
        /// </summary>
        public KeyboardState KeyboardState
        {
            get
            {
                return this._KeyboardState;
            }
        }

        /// <summary>
        /// Gets the mouse state for the window.
        /// </summary>
        public MouseState MouseState
        {
            get
            {
                return this._MouseState;
            }
        }

        private WindowKeyboardState _KeyboardState;
        private WindowMouseState _MouseState;
        private Stack<Scope> _FocusStack;
        private Control _Control;
    }

    /// <summary>
    /// A function that creates a control hierarchy when controls are needed. This allows the construction of controls to be delayed until
    /// the graphics context is set up.
    /// </summary>
    public delegate Control BuildControlHandler();
}