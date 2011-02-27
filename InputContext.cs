using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace OpenTKGUI
{
    /// <summary>
    /// Context given to a control to receive user input.
    /// </summary>
    public class InputContext : Context<InputContext, InputEffect>
    {
        public InputContext(double Time, Stack<Scope> FocusStack, MouseState MouseState, KeyboardState KeyboardState)
        {
            this._Time = Time;
            this._FocusStack = FocusStack;
            this._MousePos = MouseState.Position;
            this._MouseVisible = true;
            this._ScopeStack = new Stack<Scope>();

            this.EffectStack = new EffectStack<InputEffect, InputContext>(this);
            
            // Calculate fixed scope states by focusing
            if (FocusStack != null)
            {
                this._ScopeStates = new Dictionary<Scope, _State>();
                foreach (Scope s in FocusStack)
                {
                    this._ScopeStates[s] = new _State()
                    {
                        KeyboardState = KeyboardState,
                        MouseState = MouseState
                    };
                    s.AlterExternalControl(ref MouseState, ref KeyboardState);
                }
            }

            this._MouseState = MouseState;
            this._KeyboardState = KeyboardState;
        }

        /// <summary>
        /// Gets the current focus stack for the context. The top of the stack contains the
        /// scope with the highest-priority focus (the scope that requested the focus) while the rest
        /// of the stack contains the containining scopes of that scope.
        /// </summary>
        public Stack<Scope> FocusStack
        {
            get
            {
                return this._FocusStack;
            }
        }

        /// <summary>
        /// Gets the focus stack to be used for the next update.
        /// </summary>
        public Stack<Scope> NextFocusStack
        {
            get
            {
                return this._NextFocusStack;
            }
        }

        /// <summary>
        /// Gets the mouse state for the current scope, or null if the mouse can't be accessed.
        /// </summary>
        public MouseState MouseState
        {
            get
            {
                MouseState ms = this._MouseState;
                if (ms != null)
                {
                    return ms.SetPosition(this._MousePos);
                }
                return null;
            }
        }

        /// <summary>
        /// Gets if the mouse is visible, and not occluded by stencil operations.
        /// </summary>
        public bool MouseVisible
        {
            get
            {
                return this._MouseVisible;
            }
        }

        /// <summary>
        /// Gets the keyboard state for the current scope, or null if the keyboard can't be accessed.
        /// </summary>
        public KeyboardState KeyboardState
        {
            get
            {
                return this._KeyboardState;
            }
        }

        /// <summary>
        /// Gets the amount of time that passed while this input was recording. (How much time the corresponding update is for).
        /// </summary>
        public double Time
        {
            get
            {
                return this._Time;
            }
        }

        /// <summary>
        /// Sets which input scope enclosed operations are in. Note that all child scopes should be declared before
        /// querying inputs.
        /// </summary>
        public IDisposable Scope(Scope Scope)
        {
            return this.With(new ScopeInputEffect(Scope));
        }

        /// <summary>
        /// Translates the area of the context and the mouse.
        /// </summary>
        public IDisposable Translate(Point Offset)
        {
            return this.With(new TranslateInputEffect(Offset));
        }

        /// <summary>
        /// An effect that causes all stencil operations (which determine visibility of an area) to be restored after leaving the scope.
        /// </summary>
        public IDisposable Stencil
        {
            get
            {
                return this.With(new StencilInputEffect());
            }
        }

        /// <summary>
        /// Applies a clip operation to the stencil, which marks everything outside of the given area as occluded.
        /// </summary>
        public void StencilClip(Rectangle Area)
        {
            this._MouseVisible = this._MouseVisible && Area.In(this._MousePos);
        }

        /// <summary>
        /// Applies an occlude operation to the stencil, which marks everthing inside the given area as occluded.
        /// </summary>
        public void StencilOcclude(Rectangle Area)
        {
            this._MouseVisible = this._MouseVisible && !Area.In(this._MousePos);
        }

        /// <summary>
        /// Focuses the current scope.
        /// </summary>
        public void Focus()
        {
            this._NextFocusStack = new Stack<Scope>(new Stack<Scope>(this._ScopeStack));
        }

        /// <summary>
        /// Releases focus on the current scope if it is focused.
        /// </summary>
        public void Release()
        {
            if (this._NextFocusStack != null && this._ScopeStack != null && this._NextFocusStack.Peek() == this._ScopeStack.Peek())
            {
                this._NextFocusStack = null;
            }
        }

        /// <summary>
        /// Occludes all areas of the stencil.
        /// </summary>
        public void StencilFill()
        {
            this._MouseVisible = false;
        }

        internal struct _State
        {
            public MouseState MouseState;
            public KeyboardState KeyboardState;
        }

        private double _Time;
        internal bool _MouseVisible;
        internal Point _MousePos;
        internal Stack<Scope> _ScopeStack;
        internal Stack<Scope> _FocusStack;
        internal Stack<Scope> _NextFocusStack;
        internal Dictionary<Scope, _State> _ScopeStates;

        internal MouseState _MouseState;
        internal KeyboardState _KeyboardState;
    }

    /// <summary>
    /// An effect on the input context.
    /// </summary>
    public abstract class InputEffect : Effect<InputContext>
    {

    }

    /// <summary>
    /// An effect that sets which input scope enclosed operations are in.
    /// </summary>
    public class ScopeInputEffect : InputEffect
    {
        public ScopeInputEffect(Scope Scope)
        {
            this._Scope = Scope;
        }

        public override void Apply(InputContext Environment)
        {
            Environment._ScopeStack.Push(this._Scope);
            this._RestoreMouseState = Environment._MouseState;
            this._RestoreKeyboardState = Environment._KeyboardState;

            if (Environment._FocusStack != null)
            {
                // Make sure the top of the focus stack reregisters as the next focused scope (if none is already set)
                if (Environment._FocusStack.Peek() == this._Scope)
                {
                    if (Environment._NextFocusStack == null)
                    {
                        // This is the quickest way of copying a stack I could find.
                        Environment._NextFocusStack = new Stack<Scope>(new Stack<Scope>(Environment._FocusStack));
                    }
                }

                // Fixed scope states supersede current state
                InputContext._State s;
                if (Environment._ScopeStates.TryGetValue(this._Scope, out s))
                {
                    Environment._MouseState = s.MouseState;
                    Environment._KeyboardState = s.KeyboardState;
                }
            }

            this._Scope.AlterInternalControl(Environment, ref Environment._MouseState, ref Environment._KeyboardState);
        }

        public override void Remove(InputContext Environment)
        {
            Environment._ScopeStack.Pop();
            Environment._MouseState = this._RestoreMouseState;
            Environment._KeyboardState = this._RestoreKeyboardState;
        }

        private MouseState _RestoreMouseState;
        private KeyboardState _RestoreKeyboardState;
        private Scope _Scope;
    }

    /// <summary>
    /// An input effect that translates the context and mouse.
    /// </summary>
    public class TranslateInputEffect : InputEffect
    {
        public TranslateInputEffect(Point Offset)
        {
            this._Offset = Offset;
        }

        public override void Apply(InputContext Environment)
        {
            Environment._MousePos -= this._Offset;
        }

        public override void Remove(InputContext Environment)
        {
            Environment._MousePos += this._Offset;
        }

        private Point _Offset;
    }

    /// <summary>
    /// An input effect that stores and restores the stencil state.
    /// </summary>
    public class StencilInputEffect : InputEffect
    {
        public override void Apply(InputContext Environment)
        {
            this._MouseVisible = Environment._MouseVisible;
        }

        public override void Remove(InputContext Environment)
        {
            Environment._MouseVisible = this._MouseVisible;
        }

        private bool _MouseVisible;
    }

    /// <summary>
    /// Represents a focusable object that affects what input is given to enclosed objects. A scope with focus can be regarded as an alternative control mode 
    /// that can be activated by the program or the user. Note that scopes's act as unique identifiers to the InputContext and should be saved
    /// between methods calls involving the context.
    /// </summary>
    public class Scope
    {
        /// <summary>
        /// Gets the MouseState and KeyboardState for input queries outside of the scope when focus of it or a descandant is acquired.
        /// </summary>
        public virtual void AlterExternalControl(ref MouseState MouseState, ref KeyboardState KeyboardState)
        {

        }

        /// <summary>
        /// Gets the MouseState and KeyboardState for input queries inside the scope.
        /// </summary>
        public virtual void AlterInternalControl(InputContext Context, ref MouseState MouseState, ref KeyboardState KeyboardState)
        {

        }
    }

    /// <summary>
    /// A scope used for dragging an object.
    /// </summary>
    public class DragScope : Scope
    {
        public DragScope(MouseButton Button)
        {
            this._Button = Button;
        }

        /// <summary>
        /// Gets if the scope is currently being dragged.
        /// </summary>
        public bool Dragging
        {
            get
            {
                return this._DragPoint != null;
            }
        }

        public override void AlterExternalControl(ref MouseState MouseState, ref KeyboardState KeyboardState)
        {
            MouseState = null;
            KeyboardState = null;
        }

        /// <summary>
        /// Updates the dragging state of the scope.
        /// </summary>
        public void Update(InputContext Context)
        {
            MouseState ms = Context.MouseState;
            if (this.Dragging)
            {
                if (ms == null || !ms.IsButtonDown(this._Button))
                {
                    Context.Release();
                    this._DragPoint = null;
                }
            }
            else
            {
                if (ms != null && ms.HasPushedButton(this._Button) && Context.MouseVisible)
                {
                    Context.Focus();
                    this._DragPoint = ms.Position;
                }
            }
        }

        private Point? _DragPoint;
        private MouseButton _Button;
    }
}