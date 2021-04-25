using RentaPhotoDbConnector.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RentaPhotoDbConnector.EFModels
{
    public class OrderEntity
    {
        [Required]
        public Int16 OrderId { get; set; }
        public string ClientName { get; set; }
        public OrderStatus? OrderStatus { get; set; }
        public DateTime? OrderDate { get; set; }
        public List<GoodsEntity> Goods { get; set; }
    }
}
