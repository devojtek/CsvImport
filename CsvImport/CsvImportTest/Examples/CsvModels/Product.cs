using CsvHelper.Configuration;
using System.Data.Entity;
using System;
using CsvImportTest.Examples.EF;
using CsvImportTest.Examples.EF.Entity;
using System.Linq;
using Pkshetlie.Csv.Import.Interfaces;

namespace CsvImportTest.Example.CsvModels
{
    public class ProductModel : ICsvModel<MyContext>
    {
        public string Name { get; set; }
        public string Reference { get; set; }
        public string Quantity { get; set; }
        public string Price { get; set; }
        private string _filename { get; set; }
        public string CsvFileName
        {
            get
            {
                return _filename;
            }
            set
            {
                if (_filename != value)
                {
                    _filename = value;
                    OnNameChange(_filename);
                }
            }
        }

        public void OnNameChange(string filename)
        {
            Console.WriteLine("Ouverture du fichier : " + filename);
        }
        public void OnStart(MyContext db)
        {
            Console.WriteLine("Start !");

        }

        public bool TestBeforeSave(MyContext db)
        {
            Console.WriteLine("Do some test, if not passed don't do Save() !");
            return true;
        }

        public void Save(MyContext db)
        {
            Product p = db.Products.Where(x => x.Reference.ToLower() == Reference.ToLower()).FirstOrDefault();

            if (p == null)
            {
                db.Products.Add(new Product()
                {
                    Label = Name,
                    Reference = Reference,
                    Quantity = int.Parse(Quantity)

                });
            }
            else
            {
                p.Quantity = int.Parse(Quantity);
                p.Label = Name;
                db.Entry(p).State = EntityState.Modified;
            }

            db.SaveChanges();
        }

        public void OnFinish(MyContext db)
        {
            Console.WriteLine("Finish !");
        }
    }

    public class ProductMap : CsvClassMap<ProductModel>
    {
        public ProductMap()
        {
            Map(x => x.Name).Index(0);

            Map(x => x.Reference).Index(1);
            Map(x => x.Quantity).Index(2);
            Map(x => x.Price).Index(3);

        }
    }
}
