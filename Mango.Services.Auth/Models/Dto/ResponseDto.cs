namespace Mango.Services.Auth.Models.Dto
{
    public class ResponseDto
    {
        public object? Response { get; set; }
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;
    }
}
