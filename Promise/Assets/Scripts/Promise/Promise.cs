namespace Yates.Runtime.Promise
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    using UnityEngine.Assertions;

    using Yates.Runtime.Helper;

    /// <summary>
    /// 无参数Promise
    /// </summary>
    public class Promise : IResolveable, IResolveable<Promise>, IRejectable<Exception>
    {
        private readonly Queue<IExecuteHandler> always = new Queue<IExecuteHandler>();

        private readonly Queue<IExecuteHandler<Exception>> failures = new Queue<IExecuteHandler<Exception>>();

        private readonly Queue<IExecuteHandler> successes = new Queue<IExecuteHandler>();

        private Exception reason;

        private Promise(PromiseState state)
        {
            this.ID = IDHelper.Get();
            this.State = state;
        }

        public long ID { get; }

        public PromiseState State { get; private set; }

        /// <summary>
        /// 创建一个promise
        /// </summary>
        /// <param name="excutor"></param>
        /// <returns></returns>
        public static Promise Create(Action<Action, Action<Exception>> excutor)
        {
            var promise = new Promise(PromiseState.Pending);
            PLog("New N", promise.ID.ToString());
            try
            {
                excutor?.Invoke(promise.Resolve, promise.Reject);
            }
            catch (Exception e)
            {
                promise.Reject(e);
            }

            return promise;
        }

        /// <summary>
        /// 返回指定状态的默认promise
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static Promise Default()
        {
            var promise = Create(null);
            PLog("New SD", promise.ID.ToString());
            return promise;
        }

        public static Promise Succeeded()
        {
            var promise = Create(null);
            promise.State = PromiseState.Succeeded;
            PLog("New SS", promise.ID.ToString());
            return promise;
        }

        public static Promise Failed(Exception ex)
        {
            var promise = Create(null);
            promise.State = PromiseState.Failed;
            promise.reason = ex;
            PLog("New SF", promise.ID.ToString());
            return promise;
        }

        public static void PLog(params string[] logs)
        {
#if PROMISE_DEBUG
            builder.Length = 0;
            builder.Append("[Promise]");
            for (int i = 0; i < logs.Length; i++)
            {
                builder.Append(logs[i]);
                builder.Append(" ");
            }

            Debug.LogError(builder.ToString());
#endif
        }

        public Promise Catch(Action<Exception> onFail)
        {
            return this.Then(null, onFail);
        }

        /// <summary>
        /// 无论成功失败都会执行
        /// </summary>
        /// <param name="onFinally"></param>
        /// <returns></returns>
        public Promise Finally(Action onFinally)
        {
            var finallyHandler = new FinallyHandler(onFinally);
            if (this.State == PromiseState.Pending)
            {
                this.always.Enqueue(finallyHandler);
            }
            else
            {
                finallyHandler.Execute();
            }

            return this.Then(null);
        }

        public void Reject(Exception ex)
        {
            PLog("Reject", this.ID.ToString());
            Assert.IsTrue(this.State == PromiseState.Pending);
            this.State = PromiseState.Failed;
            this.reason = ex;
            UnityEngine.Debug.Log("[Promise] Reject" + ex);
            while (this.failures.Count > 0)
            {
                this.failures.Dequeue().Execute(ex);
            }

            while (this.always.Count > 0)
            {
                this.always.Dequeue().Execute();
            }
        }

        public void Resolve()
        {
            PLog("Resolve", this.ID.ToString());
            Assert.IsTrue(this.State == PromiseState.Pending);
            this.State = PromiseState.Succeeded;
            while (this.successes.Count > 0)
            {
                this.successes.Dequeue().Execute();
            }

            while (this.always.Count > 0)
            {
                this.always.Dequeue().Execute();
            }
        }

        public void Resolve(Promise promise)
        {
            PLog("Resolve Promise", this.ID.ToString(), promise.ID.ToString());
            promise.Then((Action)this.Resolve, this.Reject);
        }

        /// <summary>
        /// Promise -> Promise
        /// </summary>
        /// <param name="onSuccess"></param>
        /// <param name="onFail"></param>
        /// <returns></returns>
        public Promise Then(Action onSuccess, Action<Exception> onFail = null)
        {
            var promise = Default();
            PLog("Then PP", this.ID.ToString(), "<-", promise.ID.ToString());
            var resolveHandler = new ActionResolveHandler(onSuccess, promise, promise);
            var rejectHandler = new RejectHandler(onFail, promise);
            this.AddHandler(resolveHandler, rejectHandler);
            return promise;
        }

        /// <summary>
        /// Promise -> Promise -> Promise
        /// </summary>
        /// <param name="onSuccess"></param>
        /// <param name="onFail"></param>
        /// <returns></returns>
        public Promise Then(Func<Promise> onSuccess, Action<Exception> onFail = null)
        {
            var promise = Default();
            PLog("Then PPP", this.ID.ToString(), "<-", promise.ID.ToString());
            var resolveHandler = new FuncResolveHandler<Promise>(onSuccess, promise, promise);
            var rejectHandler = new RejectHandler(onFail, promise);
            this.AddHandler(resolveHandler, rejectHandler);
            return promise;
        }

        /// <summary>
        /// Promise -> Promise<T>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="onSuccess"></param>
        /// <param name="onFail"></param>
        /// <returns></returns>
        public Promise<T> Then<T>(Func<Promise<T>> onSuccess, Action<Exception> onFail = null)
        {
            var promise = Promise<T>.Default();
            PLog("Then PPt", this.ID.ToString(), "<-", promise.ID.ToString());
            var resolveHandler = new FuncResolveHandler<Promise<T>>(onSuccess, promise, promise);
            var rejectHandler = new RejectHandler(onFail, promise);
            this.AddHandler(resolveHandler, rejectHandler);
            return promise;
        }

        private void AddHandler(IExecuteHandler resolveHandler, IExecuteHandler<Exception> rejectHandler)
        {
            if (this.State == PromiseState.Succeeded)
            {
                resolveHandler.Execute();
            }
            else if (this.State == PromiseState.Failed)
            {
                rejectHandler.Execute(this.reason);
            }
            else
            {
                this.successes.Enqueue(resolveHandler);
                this.failures.Enqueue(rejectHandler);
            }
        }
    }
}