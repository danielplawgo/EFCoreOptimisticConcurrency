using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EFCoreOptimisticConcurrency
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var productId = Guid.Parse("E440B325-AD1F-4B4D-9162-36DFB0F3A357");

            await Setup(productId);

            var tasks = new[]
            {
                ChangePrice(productId, 11, 1000),
                ChangePrice(productId, 12, 100)
            };

            await Task.WhenAll(tasks);

            var price = await GetPrice(productId);

            Console.WriteLine($"Product price: {price}");
        }

        private static async Task Setup(Guid productId)
        {
            await using var db = new DataContext();

            var product = await db.Products.FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                product = new Product()
                {
                    Id = productId,
                    Name = "Test"
                };

                await db.Products.AddAsync(product);
            }

            product.Price = 10;

            await db.SaveChangesAsync();
        }

        private static async Task ChangePrice(Guid productId, decimal price, int delay)
        {
            await using var db = new DataContext();

            var product = await db.Products.FirstOrDefaultAsync(p => p.Id == productId);

            await Task.Delay(delay);

            product.Price = price;

            await db.SaveChangesAsync();
        }

        private static async Task<decimal> GetPrice(Guid productId)
        {
            await using var db = new DataContext();

            return await db.Products
                .Where(p => p.Id == productId)
                .Select(p => p.Price)
                .FirstOrDefaultAsync();
        }
    }
}
