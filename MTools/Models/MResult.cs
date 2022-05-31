using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTools.Models
{
    public class MResult<T>
    {
        /// <summary>
        /// 返回值
        /// </summary>
        public T? Content { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// HTTP返回状态
        /// </summary>
        public System.Net.HttpStatusCode StatusCode { get; set; }
    }
}
