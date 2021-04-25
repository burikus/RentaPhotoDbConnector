using System.ComponentModel.DataAnnotations;

namespace RentaPhotoDbConnector.EFModels
{
    public class GoodsEntity
    {
        [Required]
        public sbyte ProductId { get; set; }
        public byte AmountOfProducts { get; set; }
    }
}
