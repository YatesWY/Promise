namespace Yates.Runtime.Promise
{
    using System;
    using System.Collections.Generic;

    using UnityEngine.Assertions;

    using Yates.Runtime.Helper;

    public class Promise<T> : IResolveable<T>, IResolveable<Promise<T>>, IRejectable<Exception>
    {
        private readonly Queue<IExecuteHandler> always = new Queue<IExecuteHandler>();

        private readonly Queue<IExecuteHandler<Exception>> failures = new Queue<IExecuteHandler<Exception>>();

        private readonly Queue<IExecuteHandler<T>> successes = new Queue<IExecuteHandler<T>>();

        private Exception reason;

        private T value;

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
        public static Promise<T> Create(Action<Action<T>, Action<Exception>> excutor)
        {
            var promise = new Promise<T>(PromiseState.Pending);
            Promise.PLog("New T N", promise.ID.ToString());

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
        public static Promise<T> Default()
        {
            var promise = Create(null);
            Promise.PLog("New T SP", promise.ID.ToString());
            return promise;
        }

        public static Promise<T> Succeeded(T val)
        {
            var promise = Create(null);
            promise.State = PromiseState.Succeeded;
            promise.value = val;
            Promise.PLog("New T SS", promise.ID.ToString());
            return promise;
        }

        public static Promise<T> Failed(Exception ex)
        {
            var promise = Create(null);
            promise.State = PromiseState.Failed;
            promise.reason = ex;
            Promise.PLog("New T SF", promise.ID.ToString());
            return promise;
        }

        public void Reject(Exception ex)
        {
            Promise.PLog("Reject", this.ID.ToString());
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

        public void Resolve(T val)
        {
            Promise.PLog("Resolve T", this.ID.ToString());
            Assert.IsTrue(this.State == PromiseState.Pending);
            this.State = PromiseState.Succeeded;
            this.value = val;
            while (this.successes.Count > 0)
            {
                this.successes.Dequeue().Execute(val);
            }

            while (this.always.Count > 0)
            {
                this.always.Dequeue().Execute();
            }
        }

        public void Resolve(Promise<T> promise)
        {
            Promise.PLog("Resolve T Promise", this.ID.ToString(), promise.ID.ToString());
            promise.Then((Action<T>)this.Resolve, this.Reject);
        }

        /// <summary>
        /// Promise<T> -> Promise<T>
        /// </summary>
        /// <param name="onSuccess"></param>
        /// <param name="onFail"></param>
        /// <returns></returns>
        public Promise<T> Then(Action<T> onSuccess, Action<Exception> onFail = null)
        {
            var promise = Default();
            Promise.PLog("Then PtPt", this.ID.ToString(), "<-", promise.ID.ToString());
            var resolveHandler = new ActionResolveHandler<T>(onSuccess, promise, promise);
            var rejectHandler = new RejectHandler(onFail, promise);
            this.AddHandler(resolveHandler, rejectHandler);
            return promise;
        }

        /// <summary>
        /// Promise<T> -> Promise<T> -> Promise<T>
        /// </summary>
        /// <param name="onSuccess"></param>
        /// <param name="onFail"></param>
        /// <returns></returns>
        public Promise<T> Then(Func<T, Promise<T>> onSuccess, Action<Exception> onFail = null)
        {
            var promise = Default();
            Promise.PLog("Then PtPtPt", this.ID.ToString(), "<-", promise.ID.ToString());
            var resolveHandler = new FuncResolveHandler<T, Promise<T>>(onSuccess, promise, promise);
            var rejectHandler = new RejectHandler(onFail, promise);
            this.AddHandler(resolveHandler, rejectHandler);
            return promise;
        }

        private void AddHandler(IExecuteHandler<T> resolveHandler, IExecuteHandler<Exception> rejectHandler)
        {
            if (this.State == PromiseState.Succeeded)
            {
                resolveHandler.Execute(this.value);
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