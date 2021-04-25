using System.ComponentModel.DataAnnotations;

namespace RentaPhotoDbConnector.EFModels
{
    public class ProductEntity
    {
        [Required]
        public sbyte ProductArticle { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
    }
}
