using System.Collections.Generic;

namespace NPS.Api.Application.Common.Models;

public class BaseResponseDto
{
    public bool Succeeded { get; set; }
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public IEnumerable<string>? Errors { get; set; }

    public static BaseResponseDto Success(string msg = "", int code = 200) 
        => new() { Succeeded = true, Message = msg, StatusCode = code };
        
    public static BaseResponseDto Failure(string msg, int code = 400, IEnumerable<string>? errs = null) 
        => new() { Succeeded = false, Message = msg, StatusCode = code, Errors = errs };
}

public class ResponseDto<T> : BaseResponseDto
{
    public T? Data { get; set; }

    public static ResponseDto<T> Success(T data, string msg = "", int code = 200) 
        => new() { Succeeded = true, Data = data, Message = msg, StatusCode = code };
    
    public new static ResponseDto<T> Failure(string msg, int code = 400, IEnumerable<string>? errs = null) 
        => new() { Succeeded = false, Message = msg, StatusCode = code, Errors = errs };
}
