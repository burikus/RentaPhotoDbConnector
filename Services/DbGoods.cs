using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentaPhotoDbConnector.Abstractions;
using RentaPhotoDbConnector.EFData;
using RentaPhotoDbConnector.EFModels;
using System;
using System.Linq;
using System.Net;

namespace RentaPhotoDbConnector.Services
{
    public class DbGoods : Controller, IDbGoods
    {
        private DataContext dc;

        public DbGoods(DataContext datacontext)
        {
            dc = datacontext;
            InitGoodsTable(); // just init some data to work with
        }

        public ActionResult GetListOfProducts()
        {
            try
            {
                var result = dc.Goods.ToList();

                if (!result.Any())
                {
                    return NotFound(new
                    {
                        SatusCode = $"{StatusCodes.Status404NotFound} {HttpStatusCode.NotFound}",
                        Description = $"Goods not found."
                    });
                }

                return Ok(new
                {
                    SatusCode = $"{StatusCodes.Status200OK} {HttpStatusCode.OK}",
                    Goods = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    SatusCode = $"{StatusCodes.Status400BadRequest} {HttpStatusCode.BadRequest}",
                    Description = $"Executing {nameof(GetListOfProducts)} failure. {ex}"
                });
            }
        }

        public ActionResult GetProductByArticle(sbyte article)
        {
            try
            {
                var result = dc.Goods.FirstOrDefault(p => p.ProductArticle == article);

                if (result == null)
                {
                    return NotFound(new
                    {
                        SatusCode = $"{StatusCodes.Status404NotFound} {HttpStatusCode.NotFound}",
                        Description = $"Product with article {article} not found."
                    });
                }

                return Ok(new
                {
                    SatusCode = $"{StatusCodes.Status200OK} {HttpStatusCode.OK}",
                    Product = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    SatusCode = $"{StatusCodes.Status400BadRequest} {HttpStatusCode.BadRequest}",
                    Description = $"Executing {nameof(GetProductByArticle)} failure. {ex}"
                });
            }
        }

        // For demo purposes
        private void InitGoodsTable()
        {
            for (int i = 1; i < 15; i++)
            {
                var entity = new ProductEntity { ProductArticle = (sbyte)i, ProductName = $"product {i}", ProductPrice = (decimal)i };
                dc.Entry(entity).State = EntityState.Added;
                dc.SaveChanges();
            }
        }
    }
}
