# CsvImport
[![Latest version](https://img.shields.io/nuget/v/csvimport.svg)](https://www.nuget.org/packages?q=csvimport) [![Build Status](https://travis-ci.org/pkshetlie/CsvImport.svg?branch=master)](https://travis-ci.org/pkshetlie/CsvImport) [![MIT  License](https://img.shields.io/badge/license-MIT-blue.svg)](http://www.gnu.org/licenses/lgpl-3.0.html)

## Introduction 
---
This project aims to easily import a csv to an entity through a model that contains the csv conversion into specific entity, see [Product Model](https://github.com/pkshetlie/CsvImport/blob/master/CsvImport/CsvImportTest/Examples/CsvModels/Product.cs) and [Product Entity](https://github.com/pkshetlie/CsvImport/blob/master/CsvImport/CsvImportTest/Examples/EF/Entities/Product.cs) for example

## How to install
---
CsvImport is available as a NuGet package. So, you can install it using the NuGet Package Console window:
```cmd 
PM> Install-Package CsvImport
```
## How to use
---
You can find example in [CsvImportTest](https://github.com/pkshetlie/CsvImport/tree/master/CsvImport/CsvImportTest) else you can try to understand the following example

__Program.cs__ (your program)
```C#
DirectoryInfo import = Directory.CreateDirectory("C:/");
//do Import
CsvImport csv = new CsvImport(import);
// do import with spécific configuration

// CsvConfiguration baseConfig = new CsvConfiguration()
//            {
//                Delimiter = ";",
//                TrimFields = true,
//                TrimHeaders = true,
//                HasHeaderRecord = true,
//                WillThrowOnMissingField = false
//            };
// CsvImport csv = new CsvImport(import,baseConfig);

csv.Import<ProductMap, ProductModel, MyDbContext>("products-*.csv");

```
__Product.cs__ (Csv Model & Mapper)
```C#
//the model
public class ProductModel : ICsvModel<MyContext>
{
  public string Name { get; set; }
  public string Reference { get; set; }
  public string Quantity { get; set; }
  public string Price { get; set; }
  private string _filename { get; set; }

  // required by the interface
  public int Index { get; set; }
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
    Console.WriteLine(string.Format("{0} ... ",Reference);
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
  }

  public void OnFinish(MyContext db)
  {
    db.SaveChanges();
    Console.WriteLine("saved !");
  }
}

// the mapper
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
```

__Product.cs__ (Entity From EntityFramework)
```C#
public class Product 
{
  public int Id { get; set; }
  public string Label { get; set; }
  public int Quantity { get; set; }
  public string Reference { get; set; }
}
```

__product-001.csv__ (a csv file)
```
Name;Reference;Quantity;Price
Fromage;FROMAGE;15;1.25
Baguette;Baguette;85;0.90
```

