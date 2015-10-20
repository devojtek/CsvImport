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

            string str = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CsvFolder\\");
            DirectoryInfo import = Directory.CreateDirectory(str);
            try
            {
                CsvImport csv = new CsvImport(import);
                csv.Import<ProductMap, ProductModel, MyContext>("products-*.csv");
            }
            catch { }
            MyContext db = new MyContext();

            Product p = db.Products.Where(x => x.Reference == "FROMAGE").FirstOrDefault();
            Assert.AreNotEqual(null, p);
        }

        [TestMethod]
        public void CountProduct()
        {
            string str = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CsvFolder\\");
            DirectoryInfo import = Directory.CreateDirectory(str);
            try
            {
                CsvImport csv = new CsvImport(import);
                csv.Import<ProductMap, ProductModel, MyContext>("products-*.csv");
            }
            catch { }
            MyContext db = new MyContext();
            Assert.AreEqual(2, db.Products.Count());
        }

        [TestMethod]
        public void BaguetteRightQuantity()
        {

            string str = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CsvFolder\\");
            DirectoryInfo import = Directory.CreateDirectory(str);
            try
            {
                CsvImport csv = new CsvImport(import);
                csv.Import<ProductMap, ProductModel, MyContext>("products-*.csv");
            }
            catch { }
            MyContext db = new MyContext();
            Assert.AreEqual(20, db.Products.Where(x => x.Reference == "BAGUETTE").First().Quantity);
        }
    }
}
