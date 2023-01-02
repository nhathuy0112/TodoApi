namespace TodoApi.Errors;

public class ApiValidationErrorResponse : ApiResponse
{
    public ApiValidationErrorResponse() : base(400, null)
    {
    }
    public IEnumerable<string> Errors { get; set; }

}