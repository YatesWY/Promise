namespace Yates.Runtime.Promise
{
    public enum PromiseState
    {
        /// <summary>
        /// 待定
        /// </summary>
        Pending = 0,

        /// <summary>
        /// 失败
        /// </summary>
        Failed = 1,

        /// <summary>
        /// 成功
        /// </summary>
        Succeeded = 2,
    }
}