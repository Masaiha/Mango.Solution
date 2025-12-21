namespace Mango.Presentation.Web.Models.Dtos
{
    public class CouponsDto
    {
        public int Id { get; set; }
        public string CouponCode { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; } = 0;
    }
}
