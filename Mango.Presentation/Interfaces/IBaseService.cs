using Mango.Presentation.Models.Dtos;

namespace Mango.Presentation.Interfaces
{
    public interface IBaseService
    {
        Task<ResponseDto?> SendAsync(RequestDto requestDto);
    }
}
