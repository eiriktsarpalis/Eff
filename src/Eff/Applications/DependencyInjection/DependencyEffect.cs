﻿using System;
using System.Threading.Tasks;

namespace Nessos.Effects.DependencyInjection
{
    /// <summary>
    ///   Represents an effect that performs an operation against a supplied dependency container.
    /// </summary>
    public abstract class DependencyEffect<TResult> : Effect<TResult>
    {
        /// <summary>
        ///   Callback to be run against the provider contaner instance.
        /// </summary>
        /// <param name="container">Container to be supplied by the effect handler.</param>
        public abstract ValueTask<TResult> Handle(IContainer container);
    }

    internal class FuncDependencyEffect<TDependency, TResult> : DependencyEffect<TResult>
    {
        public Func<TDependency, ValueTask<TResult>> Func { get; }

        public FuncDependencyEffect(Func<TDependency, ValueTask<TResult>> func)
        {
            Func = func;
        }

        public override ValueTask<TResult> Handle(IContainer container)
        {
            var dependency = container.Resolve<TDependency>();
            return Func(dependency);
        }
    }

    internal class ContainerFuncDependencyEffect<TResult> : DependencyEffect<TResult>
    {
        public Func<IContainer, ValueTask<TResult>> Func { get; }

        public ContainerFuncDependencyEffect(Func<IContainer, ValueTask<TResult>> func)
        {
            Func = func;
        }

        public override ValueTask<TResult> Handle(IContainer container) => Func(container);
    }
}
