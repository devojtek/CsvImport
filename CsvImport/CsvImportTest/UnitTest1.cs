using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CsvImportTest.Examples.EF;
using System.Linq;
using CsvImportTest.Examples.EF.Entity;
using System.IO;
using System.Reflection;
using Pkshetlie.Csv.Import;
using CsvImportTest.Example.CsvModels;

namespace CsvImportTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void BasicTest()
        {
            try
            {
                CsvImport.Import<ProductMap, ProductModel, MyContext>("products-*.csv", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\CsvFolder\\");

            }
            catch { }
            MyContext db = new MyContext();

            Product p = db.Products.Where(x => x.Reference == "FROMAGE").FirstOrDefault();
            Assert.AreNotEqual(null, p);
        }

        [TestMethod]
        public void CountProduct()
        {
            try
            {
                CsvImport.Import<ProductMap, ProductModel, MyContext>("products-*.csv", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\CsvFolder\\");

            }
            catch { }
            MyContext db = new MyContext();
            Assert.AreEqual(2, db.Products.Count());
        }

        [TestMethod]
        public void BaguetteRightQuantity()
        {
            try
            {
                CsvImport.Import<ProductMap, ProductModel, MyContext>("products-*.csv", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\CsvFolder\\");

            }
            catch { }
            MyContext db = new MyContext();
            Assert.AreEqual(20, db.Products.Where(x=>x.Reference == "BAGUETTE").First().Quantity);
        }
    }
}
