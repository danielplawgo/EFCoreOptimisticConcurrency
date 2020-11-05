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
                EditProduct(productId, p => p.Price = 11, 1000),
                EditProduct(productId, p => p.Name = "new product", 100)
            };

            await Task.WhenAll(tasks);

            var product = await GetProduct(productId);

            Console.WriteLine();
            Console.WriteLine($"Product: {product.Name}, price: {product.Price}");
        }

        private static async Task Setup(Guid productId)
        {
            await using var db = new DataContext();

            var product = await db.Products.FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                product = new Product()
                {
                    Id = productId
                };

                await db.Products.AddAsync(product);
            }

            product.Name = "product";
            product.Price = 10;

            await db.SaveChangesAsync();
        }

        private static async Task EditProduct(Guid productId, Action<Product> editAction, int delay)
        {
            await using var db = new DataContext();

            try
            {
                var product = await db.Products.FirstOrDefaultAsync(p => p.Id == productId);

                await Task.Delay(delay);

                editAction(product);

                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                foreach (var entry in ex.Entries)
                {
                    var databaseValues = await entry.GetDatabaseValuesAsync();

                    foreach (var property in entry.CurrentValues.Properties)
                    {
                        var proposedValue = entry.CurrentValues[property];
                        var databaseValue = databaseValues[property];
                        var originalValue = entry.OriginalValues[property];

                        if (proposedValue.Equals(databaseValue) || originalValue.Equals(databaseValue) || property.Name == "RowVersion")
                        {
                            continue;
                        }

                        Console.WriteLine($"ProposedValue: {proposedValue}, DatabaseValue: {databaseValue}, OriginalValue: {originalValue}.");

                        //proposedValues[property] = wartość do zapisu;
                    }

                    entry.OriginalValues.SetValues(databaseValues);
                }
            }
        }

        private static async Task<Product> GetProduct(Guid productId)
        {
            await using var db = new DataContext();

            return await db.Products
                .Where(p => p.Id == productId)
                .FirstOrDefaultAsync();
        }
    }
}
