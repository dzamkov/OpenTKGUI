using System;
using System.Collections.Generic;

namespace OpenTKGUI
{
    /// <summary>
    /// Context given to a control when an event occurs. This allows the control to interact with the gui system as a whole.
    /// </summary>
    public abstract class GUIContext
    {
        /// <summary>
        /// Gets the control this context is intended for.
        /// </summary>
        public abstract Control Control { get; }

        /// <summary>
        /// Gets the state of the mouse as seen by this control, or null if the mouse is not in the control.
        /// </summary>
        public abstract MouseState MouseState { get; }

        /// <summary>
        /// Creates a subcontext for a child of the intended control. The child can use the subcontext to interact with the GUI system through its parent.
        /// </summary>
        public GUIContext CreateSubcontext(Control Child, Point Offset)
        {
            return new _SubGUIContext(Child, Offset, this);
        }

        private class _SubGUIContext : GUIContext
        {
            public _SubGUIContext(Control Child, Point Offset, GUIContext ParentContext)
            {
                this._Child = Child;
                this._Offset = Offset;
                this._ParentContext = ParentContext;
            }

            public override Control Control
            {
                get
                {
                    return this._Child;
                }
            }

            public override MouseState MouseState
            {
                get
                {
                    MouseState ms = this._ParentContext.MouseState;
                    if (ms != null)
                    {
                        Point npos = ms.Position - this._Offset;
                        Point size = this._Child.Size;
                        if (npos.X >= 0.0 && npos.Y >= 0.0 && npos.X < size.X && npos.Y < size.Y)
                        {
                            return new _SubMouseState(npos, ms);
                        }
                    }
                    return null;
                }
            }

            private Control _Child;
            private Point _Offset;
            private GUIContext _ParentContext;
        }

        private class _SubMouseState : MouseState
        {
            public _SubMouseState(Point Position, MouseState Parent)
            {
                this._Position = Position;
                this._Parent = Parent;
            }

            public override Point Position
            {
                get
                {
                    return this._Position;
                }
            }

            private Point _Position;
            private MouseState _Parent;
        }
    }

    /// <summary>
    /// The state of the mouse at one time, in relation to a control.
    /// </summary>
    public abstract class MouseState
    {
        /// <summary>
        /// Gets the current position of the mouse in relation to the control.
        /// </summary>
        public abstract Point Position { get; }
    }
}