using Mango.Presentation.Models.Dtos;

namespace Mango.Presentation.Interfaces
{
    public interface ICouponService
    {
        Task<ResponseDto?> GetAll();
        Task<ResponseDto?> GetById(int id);
        Task<ResponseDto?> AlterAdd(CouponsDto couponsDto);
    }
}
