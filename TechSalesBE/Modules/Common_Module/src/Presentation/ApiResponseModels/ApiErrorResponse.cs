using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommonModule.Presentation.ApiResponseModels
{
    public class ApiErrorResponse : ApiResponse<Dictionary<string, List<string>>>
    {
        public override bool Success => false;


        public ApiErrorResponse(string message, Dictionary<string, List<string>>? errors = null)
        {
            Message = message;
            Data = errors;
        }
    }
}