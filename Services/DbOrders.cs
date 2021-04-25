using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentaPhotoDbConnector.Abstractions;
using RentaPhotoDbConnector.EFData;
using RentaPhotoDbConnector.EFModels;
using RentaPhotoDbConnector.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace RentaPhotoDbConnector.Services
{
    public class DbOrders : Controller, IDbOrders
    {
        private DataContext dc;

        public DbOrders(DataContext datacontext)
        {
            dc = datacontext;
        }

        public ActionResult GetRegisteredOrders()
        {
            try
            {
                var result = dc.Orders.Where(o => o.OrderStatus == OrderStatus.Registered).ToList();

                if (!result.Any())
                {
                    return NotFound(new
                    {
                        SatusCode = $"{StatusCodes.Status404NotFound} {HttpStatusCode.NotFound}",
                        Description = $"Registered orders not found."
                    });
                }

                return Ok(new
                {
                    SatusCode = $"{StatusCodes.Status200OK} {HttpStatusCode.OK}",
                    Orders = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    SatusCode = $"{StatusCodes.Status400BadRequest} {HttpStatusCode.BadRequest}",
                    Description = $"Executing {nameof(GetRegisteredOrders)} failure. {ex}"
                });
            }
        }

        public ActionResult GetOrderDetailsById(Int16 orderId)
        {
            try
            {
                var result = dc.Orders.FirstOrDefault(o => o.OrderId == orderId);

                if (result == null)
                {
                    return NotFound(new
                    {
                        SatusCode = $"{StatusCodes.Status404NotFound} {HttpStatusCode.NotFound}",
                        Description = $"Order with id {orderId} not found."
                    });
                }

                return Ok(new
                {
                    SatusCode = $"{StatusCodes.Status200OK} {HttpStatusCode.OK}",
                    Order = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    SatusCode = $"{StatusCodes.Status400BadRequest} {HttpStatusCode.BadRequest}",
                    Description = $"Executing {nameof(GetOrderDetailsById)} failure. {ex}"
                });
            }
        }

        public ActionResult GetRegisteredOrdersByDate(DateTime date)
        {
            try
            {
                var result = dc.Orders.Where(o => o.OrderDate.Value.Date == date.Date);

                if (result == null)
                {
                    return NotFound(new
                    {
                        SatusCode = $"{StatusCodes.Status404NotFound} {HttpStatusCode.NotFound}",
                        Description = $"Orders by date {date.Date} not found."
                    });
                }

                return Ok(new
                {
                    SatusCode = $"{StatusCodes.Status200OK} {HttpStatusCode.OK}",
                    Orders = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    SatusCode = $"{StatusCodes.Status400BadRequest} {HttpStatusCode.BadRequest}",
                    Description = $"Executing {nameof(GetRegisteredOrdersByDate)} failure. {ex}"
                });
            }
        }

        public ActionResult AddOrder(OrderEntity order, int maxOrderSum)
        {
            try
            {
                if (dc.Orders.Any(o => o.OrderId == order.OrderId))
                {
                    return BadRequest(new
                    {
                        SatusCode = $"{StatusCodes.Status400BadRequest} {HttpStatusCode.BadRequest}",
                        Description = $"Order {order.OrderId} wasn't added cause already exists."
                    });
                }

                var orderSum = order.Goods.Join(dc.Goods,
                    o => o.ProductId,
                    g => g.ProductArticle,
                    (o, g) => new { o, g }
                    ).Select(m => new { sum = m.o.AmountOfProducts * m.g.ProductPrice }).ToList();

                if (orderSum.Sum(o => o.sum) > maxOrderSum)
                {
                    return BadRequest(new
                    {
                        SatusCode = $"{StatusCodes.Status400BadRequest} {HttpStatusCode.BadRequest}",
                        Description = $"Order {order.OrderId} wasn't added cause total sum exceeds {maxOrderSum}."
                    });
                }

                order.OrderDate = DateTime.Now;
                order.OrderStatus = OrderStatus.Registered;
                dc.Entry(order).State = EntityState.Added;
                dc.SaveChanges();

                return Ok(new
                {
                    SatusCode = $"{StatusCodes.Status200OK} {HttpStatusCode.OK}",
                    Description = $"Order {order.OrderId} added successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    SatusCode = $"{StatusCodes.Status400BadRequest} {HttpStatusCode.BadRequest}",
                    Description = $"Order {order.OrderId} registration failed. {ex}"
                });
            }
        }

        public ActionResult UpdateOrder(OrderEntity order)
        {
            try
            {
                var _order = dc.Orders.FirstOrDefault(o => o.OrderId == order.OrderId && o.OrderStatus == OrderStatus.Registered);

                if (_order == null)
                {
                    return NotFound(new
                    {
                        SatusCode = $"{StatusCodes.Status404NotFound} {HttpStatusCode.NotFound}",
                        Description = $"Registered order id {order.OrderId} not found."
                    });
                }

                var modifiedData = new List<string>();

                if (!string.IsNullOrEmpty(_order.ClientName) && !_order.ClientName.Equals(order.ClientName, StringComparison.OrdinalIgnoreCase))
                {
                    modifiedData.Add(order.ClientName);
                }
                if (_order.OrderStatus != null && _order.OrderStatus != order.OrderStatus)
                {
                    modifiedData.Add(Enum.GetName(typeof(OrderStatus), (int)order.OrderStatus));
                }
                if (_order.OrderDate != null && DateTime.Compare(_order.OrderDate.Value, order.OrderDate.Value) != 0)
                {
                    modifiedData.Add(order.OrderDate.Value.ToString());
                }

                if (order.Goods.Count > 0)
                {
                    var goodsUpdated = _order.Goods.Join(order.Goods,
                        o => o.ProductId,
                        _o => _o.ProductId,
                        (o, _o) => new { o, _o }
                        ).Where(u => u.o.ProductId == u._o.ProductId && u.o.AmountOfProducts != u._o.AmountOfProducts)
                        .Select(m => $"order = {m.o}, newAmount = {m.o.AmountOfProducts}, prevAmount = {m._o.AmountOfProducts}").ToList();
                    if (goodsUpdated.Count > 0)
                    {
                        goodsUpdated.ForEach(el => modifiedData.Add(el));
                    }
                }

                _order = order;
                dc.Entry(_order).State = EntityState.Modified;
                dc.SaveChanges();

                return Ok(new
                {
                    SatusCode = $"{StatusCodes.Status200OK} {HttpStatusCode.OK}",
                    Description = $"Order {order.OrderId} data {modifiedData} updated successfully. "
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    SatusCode = $"{StatusCodes.Status400BadRequest} {HttpStatusCode.BadRequest}",
                    Description = $"Order {order.OrderId} update failed. {ex}"
                });
            }
        }

        public ActionResult PatchOrder(OrderEntity order)
        {
            try
            {
                var _order = dc.Orders.FirstOrDefault(o => o.OrderId == order.OrderId && o.OrderStatus == OrderStatus.Registered);

                if (_order == null)
                {
                    return NotFound(new
                    {
                        SatusCode = $"{StatusCodes.Status404NotFound} {HttpStatusCode.NotFound}",
                        Description = $"Registered order id {order.OrderId} not found."
                    });
                }

                var modifiedData = new List<string>();

                if (!string.IsNullOrEmpty(_order.ClientName) && !_order.ClientName.Equals(order.ClientName, StringComparison.OrdinalIgnoreCase))
                {
                    _order.ClientName = order.ClientName;
                    modifiedData.Add(order.ClientName);
                }
                if (_order.OrderStatus != null && _order.OrderStatus != order.OrderStatus)
                {
                    _order.OrderStatus = order.OrderStatus;
                    modifiedData.Add(Enum.GetName(typeof(OrderStatus), (int)order.OrderStatus));
                }
                if (_order.OrderDate != null && DateTime.Compare(_order.OrderDate.Value, order.OrderDate.Value) != 0)
                {
                    _order.OrderDate = order.OrderDate;
                    modifiedData.Add(order.OrderDate.Value.ToString());
                }

                if (order.Goods.Count > 0)
                {
                    var goodsUpdated = _order.Goods.Join(order.Goods,
                    o => o.ProductId,
                    _o => _o.ProductId,
                    (o, _o) => new { o, _o }
                    ).Where(u => u.o.ProductId == u._o.ProductId && u.o.AmountOfProducts != u._o.AmountOfProducts)
                    .Select(m => $"order = {m.o}, newAmount = {m.o.AmountOfProducts}, prevAmount = {m._o.AmountOfProducts}").ToList();
                    if (goodsUpdated.Count > 0)
                    {
                        _order.Goods = order.Goods;
                        goodsUpdated.ForEach(el => modifiedData.Add(el));
                    }
                }

                dc.Entry(_order).State = EntityState.Modified;
                dc.SaveChanges();

                return Ok(new
                {
                    SatusCode = $"{StatusCodes.Status200OK} {HttpStatusCode.OK}",
                    Description = $"Order {order.OrderId} data {modifiedData} updated successfully. "
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    SatusCode = $"{StatusCodes.Status400BadRequest} {HttpStatusCode.BadRequest}",
                    Description = $"Order {order.OrderId} update failed. {ex}"
                });
            }
        }

        public ActionResult CancelOrder(Int16 orderId)
        {
            try
            {
                var result = dc.Orders.FirstOrDefault(o => o.OrderId == orderId && o.OrderStatus == OrderStatus.Registered);

                if (result == null)
                {
                    return NotFound(new
                    {
                        SatusCode = $"{StatusCodes.Status404NotFound} {HttpStatusCode.NotFound}",
                        Description = $"Registered order with id {orderId} not found."
                    });
                }

                dc.Entry(result).State = EntityState.Deleted;
                dc.SaveChanges();

                return Ok(new
                {
                    SatusCode = $"{StatusCodes.Status200OK} {HttpStatusCode.OK}",
                    Description = $"Registered order with id {orderId} was cancelled."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    SatusCode = $"{StatusCodes.Status400BadRequest} {HttpStatusCode.BadRequest}",
                    Description = $"Executing {nameof(CancelOrder)} failure. {ex}"
                });
            }
        }
    }
}
