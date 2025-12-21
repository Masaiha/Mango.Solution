using Mango.Presentation.Web.Enuns;
using Mango.Presentation.Web.Interfaces;
using Mango.Presentation.Web.Models.Dtos;
using Mango.Presentation.Web.Utils;

namespace Mango.Presentation.Web.Services
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
                ApiType = ApiType.GET,
                Url = SD.CouponApiBase + "api/coupons/GetAll"
            });

            return teste;
        }

        public async Task<ResponseDto?> GetById(int id)
        {
            throw new NotImplementedException();
        }
    }


    public class AuthService : IAuthService
    {
        private readonly IBaseService _baseService;

        public AuthService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDto?> AssignRoleAsync(RegisterationRequestDto registerationRequestDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = ApiType.POST,
                Data = registerationRequestDto,
                Url = SD.AuthAPIBase + "api/auth/AssignRole"
            });
        }

        public async Task<ResponseDto?> LoginAsync(LoginRequestDto loginRequestDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = ApiType.POST,
                Data = loginRequestDto,
                Url = SD.AuthAPIBase + "api/auth/login"
            });
        }

        public async Task<ResponseDto?> RegisterAsync(RegisterationRequestDto registerationRequestDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = ApiType.POST,
                Data = registerationRequestDto,
                Url = SD.AuthAPIBase + "api/auth/register"
            });
        }
    }
}
