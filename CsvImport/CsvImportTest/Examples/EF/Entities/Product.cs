using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvImportTest.Examples.EF.Entity
{
    public class Product 
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public int Quantity { get; set; }
        public string Reference { get; set; }
    }
}
