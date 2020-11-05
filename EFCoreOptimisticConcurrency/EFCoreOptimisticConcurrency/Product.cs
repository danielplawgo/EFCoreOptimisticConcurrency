using System;
using System.Collections.Generic;
using System.Text;

namespace EFCoreOptimisticConcurrency
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; }

        public decimal Price { get; set; }

        public byte[] RowVersion { get; set; }
    }
}
