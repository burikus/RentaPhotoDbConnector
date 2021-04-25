using Microsoft.AspNetCore.Mvc;

namespace RentaPhotoDbConnector.Abstractions
{
    public interface IDbGoods
    {
        ActionResult GetListOfProducts();
        ActionResult GetProductByArticle(sbyte article);
    }
}
