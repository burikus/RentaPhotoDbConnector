using Microsoft.AspNetCore.Mvc;
using RentaPhotoDbConnector.EFModels;
using System;

namespace RentaPhotoDbConnector.Abstractions
{
    public interface IDbOrders
    {
        ActionResult GetRegisteredOrders();
        ActionResult GetOrderDetailsById(Int16 orderId);
        ActionResult GetRegisteredOrdersByDate(DateTime date);
        ActionResult AddOrder(OrderEntity order, int maxOrderSum);
        ActionResult UpdateOrder(OrderEntity order);
        ActionResult CancelOrder(Int16 orderId);
    }
}
