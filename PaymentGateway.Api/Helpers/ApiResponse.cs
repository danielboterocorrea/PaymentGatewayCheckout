using Newtonsoft.Json;
using PaymentGateway.Application.ResponseModels;
using PaymentGateway.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PaymentGateway.Api.Helpers
{
    public class ApiResponse
    {
        [JsonProperty(PropertyName = "result", NullValueHandling = NullValueHandling.Ignore)]
        public object Result { get; set; }
        [JsonProperty(PropertyName = "_links", NullValueHandling = NullValueHandling.Ignore)]
        public List<Links> Links { get; set; }
        [JsonProperty(PropertyName = "error_type", NullValueHandling = NullValueHandling.Ignore)]
        public string ErrorType { get; set; }
        [JsonProperty(PropertyName = "error_codes", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> ErrorCodes { get; set; }
    }

    public class Self
    {
        public string Href { get; set; }
    }
    public class Links
    {
        public Self Self { get; set; }
    }

    public static class ApiReponseActionResult
    {
        public static ApiResponse CreateResponse(Guid id, string controller)
        {
            return new ApiResponse
            {
                Result = new { Id = id },
                Links = new List<Links>
                {
                    new Links
                    {
                        Self = new Self{
                            Href = $"{controller}/{id}"
                        }
                    }
                }
            };
        }

        public static ApiResponse CreateNotFoundResponse(Guid id)
        {
            return new ApiResponse
            {
                ErrorType = StatusCode.Failure.ToString(),
                ErrorCodes = new List<string>()
                {
                    CommonStatusCode.NotFound.ToString()
                }
            };
        }

        public static ApiResponse CreateResponse(PaymentDetailResponse payment, string controller)
        {
            return new ApiResponse
            {
                Result = payment,
                Links = new List<Links>
                {
                    new Links
                    {
                        Self = new Self{
                            Href = $"{controller}/{payment.Id}"
                        }
                    }
                }
            };
        }

        public static ApiResponse CreateInvalid(string[] errors, string controller)
        {
            return new ApiResponse
            {
                ErrorType = "request_invalid",
                ErrorCodes = errors.ToList()
            };
        }
    }

}
