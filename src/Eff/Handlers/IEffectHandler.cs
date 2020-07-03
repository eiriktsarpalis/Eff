using System.Threading.Tasks;

namespace Nessos.Effects.Handlers
{
    /// <summary>
    ///   Abstract effect handler. Implementations provide interpretation semantics for Eff computations.
    /// </summary>
    public interface IEffectHandler
    {
        /// <summary>
        ///   Handles abstract effect awaiters.
        /// </summary>
        Task Handle<TResult>(EffectAwaiter<TResult> awaiter);

        /// <summary>
        ///   Handles an eff state machine awaiter.
        /// </summary>
        Task Handle<TResult>(EffStateMachine<TResult> stateMachine);

        /// <summary>
        ///   Executes an eff instance and returns its result.
        /// </summary>
        Task<TResult> Execute<TResult>(Eff<TResult> eff);
    }
}
