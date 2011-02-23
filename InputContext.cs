using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenTKGUI
{
    /// <summary>
    /// Context given to a control to receive user input.
    /// </summary>
    public class InputContext : Context<InputContext, InputEffect>
    {
        public InputContext(double Time, Scope Focused, MouseState MouseState, KeyboardState KeyboardState)
        {
            this._Time = Time;
            this._FocusedScope = this._NextFocusedScope = Focused;
            this._AbsoluteMouseState = this._MouseState = MouseState;
            this._AbsoluteKeyboardState = this._KeyboardState = KeyboardState;
            this._MousePos = MouseState.Position;
            this._MouseVisible = true;
            this.EffectStack = new EffectStack<InputEffect, InputContext>(this);
        }

        /// <summary>
        /// Gets the scope that is currently focused.
        /// </summary>
        public Scope FocusedScope
        {
            get
            {
                return this._FocusedScope;
            }
        }

        /// <summary>
        /// Gets the scope to be focused on the next update.
        /// </summary>
        public Scope NextFocusedScope
        {
            get
            {
                return this._NextFocusedScope;
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
        /// Occludes all areas of the stencil.
        /// </summary>
        public void StencilFill()
        {
            this._MouseVisible = false;
        }

        private double _Time;
        internal bool _MouseVisible;
        internal Point _MousePos;
        internal Scope _FocusedScope;
        internal Scope _NextFocusedScope;
        internal MouseState _AbsoluteMouseState;
        internal KeyboardState _AbsoluteKeyboardState;
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
            this._Focused = Environment._FocusedScope == this._Scope;
            if (this._Focused)
            {
                this._RestoreMouseState = Environment._MouseState = Environment._AbsoluteMouseState;
                this._RestoreKeyboardState = Environment._KeyboardState = Environment._AbsoluteKeyboardState;
            }
            else
            {
                this._RestoreMouseState = Environment._MouseState;
                this._RestoreKeyboardState = Environment._KeyboardState;
            }
            this._Scope.AlterInternalControl(Environment, ref Environment._MouseState, ref Environment._KeyboardState);
        }

        public override void Remove(InputContext Environment)
        {
            if (this._Focused)
            {
                this._Scope.AlterExternalControl(Environment, ref this._RestoreMouseState, ref this._RestoreKeyboardState);
                foreach (InputEffect ie in Environment.EffectStack.Effects)
                {
                    ScopeInputEffect sie = ie as ScopeInputEffect;
                    if (sie != null)
                    {
                        sie._Focused = true;
                        sie._RestoreMouseState = this._RestoreMouseState;
                        sie._RestoreKeyboardState = this._RestoreKeyboardState;
                        sie._Scope.AlterInternalControl(Environment, ref this._RestoreMouseState, ref this._RestoreKeyboardState);
                        break;
                    }
                }
            }
            Environment._MouseState = this._RestoreMouseState;
            Environment._KeyboardState = this._RestoreKeyboardState;
        }

        private bool _Focused;
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
    public class Scope : IDisposable
    {
        /// <summary>
        /// Gets the MouseState and KeyboardState for input queries outside of the scope when focus of it or a descandant is acquired.
        /// </summary>
        public virtual void AlterExternalControl(InputContext Context, ref MouseState MouseState, ref KeyboardState KeyboardState)
        {

        }

        /// <summary>
        /// Gets the MouseState and KeyboardState for input queries inside the scope.
        /// </summary>
        public virtual void AlterInternalControl(InputContext Context, ref MouseState MouseState, ref KeyboardState KeyboardState)
        {

        }

        public void Dispose()
        {

        }
    }
}