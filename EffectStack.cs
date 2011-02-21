using System;
using System.Collections.Generic;

namespace OpenTKGUI
{
    /// <summary>
    /// Represents a reversible manipulation performed on a system. Each effect may only be used once.
    /// </summary>
    /// <typeparam name="TEnvironment">The environment the effect is appliable to.</typeparam>
    public interface IEffect<TEnvironment>
    {
        /// <summary>
        /// Applies this effect to the specified environment.
        /// </summary>
        void Apply(TEnvironment Environment);

        /// <summary>
        /// Removes this effect.
        /// </summary>
        void Remove(TEnvironment Environment);
    }

    /// <summary>
    /// A class form of an effect. 
    /// </summary>
    public abstract class Effect<TEnvironment> : IEffect<TEnvironment>
    {
        public abstract void Apply(TEnvironment Environment);
        public abstract void Remove(TEnvironment Environment);
    }

    /// <summary>
    /// A collection of effects on an environment that may only be removed in the order they are applied.
    /// </summary>
    public interface IEffectStack<TEffect, TEnvironment>
        where TEffect : IEffect<TEnvironment>
    {
        /// <summary>
        /// Applies and effect and pushes it on the stack.
        /// </summary>
        void Push(TEffect Effect);

        /// <summary>
        /// Removes the effect that was last pushed on the stack.
        /// </summary>
        void Pop();

        /// <summary>
        /// Gets the environment this effect stack is for.
        /// </summary>
        TEnvironment Environment { get; }

        /// <summary>
        /// Gets the effects in this stack, in reverse order than they were applied.
        /// </summary>
        IEnumerable<TEffect> Effects { get; }
    }

    /// <summary>
    /// A simple concrete implementation of an effect stack. Also contains methods for the manipulation of effect stacks.
    /// </summary>
    public class EffectStack<TEffect, TEnvironment> : IEffectStack<TEffect, TEnvironment>
        where TEffect : IEffect<TEnvironment>
    {
        public EffectStack(TEnvironment Environment)
        {
            this._Environment = Environment;
            this._Stack = new Stack<TEffect>();
        }

        public void Push(TEffect Effect)
        {
            Effect.Apply(this._Environment);
            this._Stack.Push(Effect);
        }

        public void Pop()
        {
            this._Stack.Pop().Remove(this._Environment);
        }

        public TEnvironment Environment
        {
            get
            {
                return this._Environment;
            }
        }

        public IEnumerable<TEffect> Effects
        {
            get
            {
                return this._Stack;
            }
        }

        /// <summary>
        /// Creates an IDisposable object that will pop an item from the effect stack when disposed. This can be used
        /// with the "using" statement to insure that an item pushed to the stack eventually gets popped. This may be
        /// used multiple times with multiple calls to dispose.
        /// </summary>
        public static IDisposable PopOnDispose(IEffectStack<TEffect, TEnvironment> Stack)
        {
            return new _PopOnDispose()
            {
                Stack = Stack
            };
        }

        private class _PopOnDispose : IDisposable
        {
            public void Dispose()
            {
                this.Stack.Pop();
            }

            public IEffectStack<TEffect, TEnvironment> Stack;
        }

        private TEnvironment _Environment;
        private Stack<TEffect> _Stack;
    }
}