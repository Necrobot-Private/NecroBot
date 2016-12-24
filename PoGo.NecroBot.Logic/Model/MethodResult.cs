using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.Model
{
    public class MethodResult
    {
        public Exception Error { get; set; }
        public bool Success { get; set; }
        public string CaptchaId { get; set; }
        public string CaptchaResponse { get; internal set; }
    }

    public class MethodResult<T> : MethodResult
    {
        public T Value { get; set; }

    }
}
