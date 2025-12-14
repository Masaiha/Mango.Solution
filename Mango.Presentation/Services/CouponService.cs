using Mango.Presentation.Interfaces;
using Mango.Presentation.Models.Dtos;
using Mango.Presentation.Utils;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Mango.Presentation.Services
{
    public class CouponService : ICouponService
    {
        private readonly IBaseService _baseService;

        public CouponService(IBaseService baseService)
        {
            _baseService = baseService;
        }


        public async Task<ResponseDto?> AlterAdd(CouponsDto couponsDto)
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseDto?> GetAll()
        {
            var teste = await _baseService.SendAsync(new RequestDto()
            {
                ApiType = RequestType.GET,
                Url = SD.CouponApiBase + "api/coupons/GetAll"
            });

            return teste;
        }

        public async Task<ResponseDto?> GetById(int id)
        {
            throw new NotImplementedException();
        }
    }
}
