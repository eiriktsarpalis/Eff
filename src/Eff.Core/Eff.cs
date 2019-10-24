using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Eff.Core
{

    public class Unit
    {
        private Unit() { }
        public static Unit Value { get; } = new Unit();
    }

    [AsyncMethodBuilder(typeof(EffMethodBuilder))]
    public abstract class Eff
    {
        internal abstract Eff<Unit> Ignore();
    }

    [AsyncMethodBuilder(typeof(EffMethodBuilder<>))]
    public abstract class Eff<TResult> : Eff
    {
        internal override Eff<Unit> Ignore()
        {
            switch((Eff)this)
            {
                case Eff<Unit> _eff : return _eff;
                default: return Ignored();

                async Eff<Unit> Ignored() { await this.AsEffect() ; return Unit.Value; }
            }
        }
    }

    public class Await<TResult> : Eff<TResult>
    {
        private readonly IEffect effect;
        private readonly Func<object, Eff<TResult>> continuation;
        private readonly object state;

        public Await(IEffect effect, Func<object, Eff<TResult>> continuation, object state)
        {
            this.effect = effect;
            this.continuation = continuation;
            this.state = state;
        }

        public IEffect Effect => effect;
        public Func<object, Eff<TResult>> Continuation => continuation;
        public object State => state;

    }

    public class SetResult<TResult> : Eff<TResult>
    {
        private readonly TResult result;
        private readonly object state;

        public SetResult(TResult result, object state)
        {
            this.result = result;
            this.state = state;
        }

        public TResult Result => result;
        public object State => state;

    }

    public class SetException<TResult> : Eff<TResult>
    {
        private readonly Exception exception;
        private readonly object state;

        public SetException(Exception exception, object state)
        {
            this.exception = exception;
            this.state = state;
        }

        public Exception Exception => exception;
        public object State => state;

    }

    public class Delay<TResult> : Eff<TResult>
    {
        private readonly Func<object, Eff<TResult>> func;
        private readonly object state;

        public Delay(Func<object, Eff<TResult>> func, object state)
        {
            this.func = func;
            this.state = state;
        }

        public Func<object, Eff<TResult>> Func => func;
        public object State => state;
    }


}
