namespace EasyGroceries.Common.Dto;

public class StandardResponse
{
    protected StandardResponse()
    {
    }

    public bool IsSuccess { get; set; }
    public string[]? Errors { get; set; }


    public static StandardResponse Failure(params string[]? errors)
    {
        return new StandardResponse
        {
            IsSuccess = false,
            Errors = errors
        };
    }

    public static StandardResponse Success()
    {
        return new StandardResponse
        {
            IsSuccess = true,
            Errors = null
        };
    }

    public static StandardResponse<T> Success<T>(T payload)
    {
        return new StandardResponse<T>
        {
            Payload = payload,
            IsSuccess = true,
            Errors = null
        };
    }
}

public class StandardResponse<T> : StandardResponse
{
    public T? Payload { get; set; }
}