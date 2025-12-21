using Mango.Presentation.Web.Models.Dtos;

namespace Mango.Presentation.Web.Interfaces
{
    public interface ICouponService
    {
        Task<ResponseDto?> GetAll();
        Task<ResponseDto?> GetById(int id);
        Task<ResponseDto?> AlterAdd(CouponsDto couponsDto);
    }
}
