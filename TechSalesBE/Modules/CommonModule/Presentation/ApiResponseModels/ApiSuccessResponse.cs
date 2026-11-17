using CommonModule.Application.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommonModule.Presentation.ApiResponseModels
{
    public class ApiSuccessResponse<T> : ApiResponse<T>
    {
        public override bool Success => true;
        public ApiSuccessResponse(T? data, string message = ResponseMessage.SuccessDefault)
        {
            Message = message;
            Data = data;
        }
    }
}
