namespace Conductor.Dtos
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiResult<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        public ApiResult(bool result, int code, string message, T data)
        {
            Result = result;
            Code = code;
            Message = message;
            Data = data;
        }

        /// <summary>
        /// 是否成功
        /// </summary>
        /// <returns></returns>
        public bool Result { get; }

        /// <summary>
        /// 错误代码
        /// </summary>
        /// <returns></returns>
        public int Code { get; }

        /// <summary>
        /// 信息
        /// </summary>
        /// <returns></returns>
        public string Message { get; }

        /// <summary>
        /// 数据
        /// </summary>
        /// <returns></returns>
        public T Data { get; }

        /// <summary>
        /// 操作成功
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ApiResult<T> True(T data)
        {
            return new ApiResult<T>(true, -1, "操作成功", data);
        }

        /// <summary>
        /// 操作成功-数据为空
        /// </summary>
        /// <returns></returns>
        public static ApiResult<T> Empty()
        {
            return new ApiResult<T>(true, -1, "操作成功", default(T));
        }

        /// <summary>
        /// 操作失败
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ApiResult<T> False(string message)
        {
            return new ApiResult<T>(false, 1, message, default(T));
        }

        /// <summary>
        /// 操作失败
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ApiResult<T> False(int code, string message)
        {
            return new ApiResult<T>(false, code, message, default(T));
        }
    }
}