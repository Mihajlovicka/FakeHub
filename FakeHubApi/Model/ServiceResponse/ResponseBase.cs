namespace FakeHubApi.Model.ServiceResponse;

public class ResponseBase
{
    public object? Result { get; set; }
    public bool Success { get; set; } = true;
    public string ErrorMessage { get; set; } = "";

    public static ResponseBase ErrorResponse(string errorMessage)
    {
        return new ResponseBase { Success = false, ErrorMessage = errorMessage };
    }

    public static ResponseBase SuccessResponse(object? result = null)
    {
        return new ResponseBase { Result = result };
    }
}
