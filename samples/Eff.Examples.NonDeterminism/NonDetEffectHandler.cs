﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nessos.Effects.Handlers;

namespace Nessos.Effects.Examples.NonDeterminism
{
    public static class NonDetEffectHandler
    {
        public static Task<List<TResult>> Run<TResult>(Eff<TResult> eff) => Run(eff.GetStateMachine());

        private static async Task<List<TResult>> Run<TResult>(EffStateMachine<TResult> stateMachine)
        {
            while (true)
            {
                stateMachine.MoveNext();

                switch (stateMachine.Position)
                {
                    case StateMachinePosition.Result:
                        return new List<TResult> { stateMachine.Result };
                    case StateMachinePosition.Exception:
                        throw stateMachine.Exception!;

                    case StateMachinePosition.TaskAwaiter:
                        await stateMachine.TaskAwaiter!.Value;
                        break;

                    case StateMachinePosition.EffAwaiter:
                        var awaiter = stateMachine.EffAwaiter!;
                        var handler = new NonDetEffectHandlerImpl<TResult>(stateMachine);
                        await awaiter.Accept(handler);
                        return handler.Results;

                    default:
                        throw new Exception($"Invalid state machine position {stateMachine.Position}.");
                }
            }
        }

        private class NonDetEffectHandlerImpl<TResult> : IEffectHandler
        {
            private readonly EffStateMachine<TResult> _stateMachine;

            public NonDetEffectHandlerImpl(EffStateMachine<TResult> stateMachine)
            {
                _stateMachine = stateMachine;
            }

            public List<TResult> Results { get; } = new List<TResult>();

            public async Task Handle<TValue>(EffectAwaiter<TValue> awaiter)
            {
                switch (awaiter.Effect)
                {
                    case NonDetEffect<TValue> nde:
                        foreach (var result in nde.Choices)
                        {
                            awaiter.SetResult(result);
                            await ContinueStateMachine(clone: true);
                        }

                        break;
                }
            }

            public async Task Handle<TValue>(EffStateMachine<TValue> stateMachine)
            {
                List<TValue>? values = null;
                Exception? error = null;
                try
                {
                    values = await NonDetEffectHandler.Run(stateMachine);
                }
                catch (Exception e)
                {
                    error = e;
                }

                if (values != null)
                {
                    foreach (var result in values)
                    {
                        stateMachine.SetResult(result);
                        await ContinueStateMachine(clone: true);
                    }
                }
                else
                {
                    stateMachine.SetException(error!);
                    await ContinueStateMachine();
                }
            }

            /// <summary>
            ///   Executes the state machine to completion, using non-deterministic semantics,
            ///   appending any results to the handler state.
            /// </summary>
            private async Task ContinueStateMachine(bool clone = false)
            {
                var stateMachine = clone ? _stateMachine.Clone() : _stateMachine;
                var results = await NonDetEffectHandler.Run(stateMachine);
                Results.AddRange(results);
            }
        }
    }
}
