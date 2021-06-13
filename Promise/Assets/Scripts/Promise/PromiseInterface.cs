namespace Yates.Runtime.Promise
{
    using System;

    public interface IExecuteHandler
    {
        void Execute();
    }

    public interface IExecuteHandler<T>
    {
        void Execute(T value);
    }

    public interface IResolveable
    {
        void Resolve();
    }

    public interface IResolveable<T>
    {
        void Resolve(T value);
    }

    public interface IRejectable<T>
    {
        void Reject(T reason);
    }

    public class ActionResolveHandler : IExecuteHandler
    {
        private readonly Action callback;

        public ActionResolveHandler(
            Action callHandler,
            IResolveable resolveHandler,
            IRejectable<Exception> rejectHandler)
        {
            this.callback = () =>
                {
                    try
                    {
                        callHandler?.Invoke();
                        resolveHandler.Resolve();
                    }
                    catch (Exception e)
                    {
                        rejectHandler.Reject(e);
                    }
                };
        }

        public void Execute()
        {
            this.callback();
        }
    }

    public class ActionResolveHandler<T> : IExecuteHandler<T>
    {
        private readonly Action<T> callback;

        public ActionResolveHandler(
            Action<T> callHandler,
            IResolveable<T> resolveHandler,
            IRejectable<Exception> rejectHandler)
        {
            this.callback = (value) =>
                {
                    try
                    {
                        callHandler?.Invoke(value);
                        resolveHandler.Resolve(value);
                    }
                    catch (Exception e)
                    {
                        rejectHandler.Reject(e);
                    }
                };
        }

        public void Execute(T value)
        {
            this.callback(value);
        }
    }

    public class FuncResolveHandler<T> : IExecuteHandler
    {
        private readonly Action callback;

        public FuncResolveHandler(
            Func<T> callHandler,
            IResolveable<T> resolveHandler,
            IRejectable<Exception> rejectHandler)
        {
            this.callback = () =>
                {
                    try
                    {
                        var result = callHandler.Invoke();
                        resolveHandler.Resolve(result);
                    }
                    catch (Exception e)
                    {
                        rejectHandler.Reject(e);
                    }
                };
        }

        public void Execute()
        {
            this.callback();
        }
    }

    public class FuncResolveHandler<T, K> : IExecuteHandler<T>
    {
        private readonly Action<T> callback;

        public FuncResolveHandler(
            Func<T, K> callHandler,
            IResolveable<K> resolveHandler,
            IRejectable<Exception> rejectHandler)
        {
            this.callback = (value) =>
                {
                    try
                    {
                        var result = callHandler.Invoke(value);
                        resolveHandler.Resolve(result);
                    }
                    catch (Exception e)
                    {
                        rejectHandler.Reject(e);
                    }
                };
        }

        public void Execute(T value)
        {
            this.callback(value);
        }
    }

    public class RejectHandler : IExecuteHandler<Exception>
    {
        private readonly Action<Exception> callback;

        public RejectHandler(Action<Exception> callHandler, IRejectable<Exception> rejectHandler)
        {
            this.callback = (reason) =>
                {
                    try
                    {
                        callHandler.Invoke(reason);
                        rejectHandler.Reject(reason);
                    }
                    catch (Exception e)
                    {
                        rejectHandler.Reject(e);
                    }
                };
        }

        public void Execute(Exception reason)
        {
            this.callback(reason);
        }
    }

    public class FinallyHandler : IExecuteHandler
    {
        private readonly Action callback;

        public FinallyHandler(Action callHandler)
        {
            // 不对finally进行保护
            this.callback = callHandler;
        }

        public void Execute()
        {
            this.callback();
        }
    }
}