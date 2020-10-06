using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4API.Controllers
{
    [Route("products")]
    public class ProductsController : Controller
    {
        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Authorize("AuthorizedAccess")]
        public IEnumerable<Product> GetProducts()
        {
            yield return new Product
            {
                Id = 1,
                SerialNo = "ABC123",
            };
        }

        [HttpGet("{id}")]
        public Product GetProduct(int id)
        {
            return new Product
            {
                Id = 1,
                SerialNo = "ABC123",
            };

        }

        [HttpPost]
        public void CreateProduct([FromBody] Product product)
        {
        }

        [HttpDelete("{id}")]
        public void DeleteProduct(int id)
        {
        }
    }

    public class Product
    {
        public int Id { get; internal set; }
        public string SerialNo { get; set; }
        public ProductStatus Status { get; set; }
    }

    public enum ProductStatus
    {
        InStock, ComingSoon
    }
}
