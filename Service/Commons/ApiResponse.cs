using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Commons
{
    public class ApiResponse<T>
    {
        public string? Message { get; set; }
        public string? Error { get; set; }
        public T? Result { get; set; }

        public ApiResponse(string? message, string? error = null, T? result = default) {
            Message = message;
            Error = error;
            Result = result;
        }
    }
}
