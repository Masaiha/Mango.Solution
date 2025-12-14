namespace Mango.Presentation.Models.Dtos
{
    public class RequestDto
    {
        public RequestType ApiType { get; set; } = RequestType.GET;
        public string Url { get; set; }
        public Object Data { get; set; }
        public string AccessToken { get; set; }

    }

    public enum RequestType
    {
        GET,
        POST,
        PUT,
        DELETE
    }
}
