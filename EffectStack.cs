using System;
using System.Collections.Generic;

namespace OpenTKGUI
{
    /// <summary>
    /// Represents a reversible manipulation performed on a system.
    /// </summary>
    public interface IEffect<TBaseEffect>
        where TBaseEffect : IEffect<TBaseEffect>
    {
        /// <summary>
        /// Applies this effect to the top of the specified effect stack.
        /// </summary>
        void Apply(IEffectStack<TBaseEffect> Stack);

        /// <summary>
        /// Removes this effect.
        /// </summary>
        void Remove();
    }

    /// <summary>
    /// A class form of an effect. Also contains methods for the manipulation of effect stacks.
    /// </summary>
    public abstract class Effect<TSelf> : IEffect<TSelf>
        where TSelf : Effect<TSelf>
    {
        public abstract void Apply(IEffectStack<TSelf> Stack);
        public abstract void Remove();

        /// <summary>
        /// Creates an IDisposable object that will pop an item from the effect stack when disposed. This can be used
        /// with the "using" statement to insure that an item pushed to the stack eventually gets popped. This may be
        /// used multiple times with multiple calls to dispose.
        /// </summary>
        public static IDisposable PopOnDispose(IEffectStack<TSelf> Stack)
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
                this._Stack.Pop();
            }

            public IEffectStack<TSelf> Stack;
        }
    }

    /// <summary>
    /// A collection of effects that may only be removed in the order they are applied.
    /// </summary>
    public interface IEffectStack<TEffect>
        where TEffect : IEffect<TEffect>
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
        /// Gets the effects in this stack, in reverse order than they were applied.
        /// </summary>
        IEnumerable<TEffect> Effects { get; }
    }

    /// <summary>
    /// A simple concrete implementation of an effect stack.
    /// </summary>
    public class EffectStack<TEffect> : IEffectStack<TEffect>
        where TEffect : IEffect<TEffect>
    {
        public EffectStack()
        {
            this._Stack = new Stack<TEffect>();
        }

        public void Push(TEffect Effect)
        {
            Effect.Apply(this);
            this._Stack.Push(Effect);
        }

        public void Pop()
        {
            this._Stack.Pop().Remove();
        }

        public IEnumerable<TEffect> Effects
        {
            get
            {
                return this._Stack;
            }
        }

        private Stack<TEffect> _Stack;
    }
}