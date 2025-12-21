using Mango.Presentation.Web.Models.Dtos;

namespace Mango.Presentation.Web.Interfaces
{
    public interface IBaseService
    {
        Task<ResponseDto?> SendAsync(RequestDto requestDto);
    }
}
