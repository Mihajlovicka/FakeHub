namespace FakeHubApi.Model.ServiceResponse;

public class ResponseBase
{
    public object? Result { get; set; }
    public bool Success { get; set; } = true;
    public string ErrorMessage { get; set; } = "";
}