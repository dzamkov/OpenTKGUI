using System;
using System.Collections.Generic;

namespace OpenTKGUI
{
    /// <summary>
    /// A generalization of a context, an interface to a logic object that can be accessed through sequential
    /// calls on the context. Effects can be applied to Context's to somehow alter the enclosed operations.
    /// </summary>
    public class Context<TSelf, TEffect> 
        where TSelf : Context<TSelf, TEffect>
        where TEffect : IEffect<TSelf>
    {

        /// <summary>
        /// Gets the effect stack for the context.
        /// </summary>
        public IEffectStack<TEffect, TSelf> EffectStack
        {
            get
            {
                return this._EffectStack;
            }
            protected set
            {
                this._EffectStack = value;
            }
        }

        /// <summary>
        /// Adds an effect to the effect stack of the context until the provided object is disposed.
        /// </summary>
        /// <remarks>This is best used inside a "using" statement.</remarks>
        public IDisposable With(TEffect Effect)
        {
            this._EffectStack.Push(Effect);
            return EffectStack<TEffect, TSelf>.PopOnDispose(this._EffectStack);
        }

        /// <summary>
        /// Pushes an effect to the context.
        /// </summary>
        public void Push(TEffect Effect)
        {
            this._EffectStack.Push(Effect);
        }

        /// <summary>
        /// Pops the last effect from the context.
        /// </summary>
        public void Pop()
        {
            this._EffectStack.Pop();
        }

        private IEffectStack<TEffect, TSelf> _EffectStack;
    }
}