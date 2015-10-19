using CsvImportTest.Examples.EF.Entity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvImportTest.Examples.EF
{
    public class MyContext : DbContext
    {
        public MyContext() : base("MyDatabase") { }
        public DbSet<Product> Products { get; set; }
    }
}
