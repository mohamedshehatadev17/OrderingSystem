namespace OrderingSystem.API.DTOs
{
    public class ApiErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();
    }
}
