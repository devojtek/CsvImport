using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Data.Entity;
using CsvHelper;
using Pkshetlie.Csv.Import.Interfaces;

namespace Pkshetlie.Csv.Import
{
    public class CsvImport
    {

        /// <summary>
        /// Directory where are csv
        /// </summary>
        private static DirectoryInfo ImportDirectory { get; set; }

        /// <summary>
        /// Directory where to place file when done
        /// </summary>
        private static DirectoryInfo DoneDirectory { get; set; }

        private static bool RmFile = true;
        /// <summary>
        /// Field separator in CSV file
        /// </summary>
        private static string ColumnSeparator = ";";

        /// <summary>
        /// Line separator in CSV file
        /// </summary>
        private static string LineSeparator = Environment.NewLine;

        /// <summary>
        /// Define the line separator
        /// </summary>
        /// <param name="lineSeparator">the line separator</param>
        public static void SetLineSeparator(string lineSeparator)
        {
            LineSeparator = lineSeparator;
        }

        /// <summary>
        /// Define the line separator
        /// </summary>
        /// <param name="columnSeparator">the line separator</param>
        public static void SetColumnSeparator(string columnSeparator)
        {
            ColumnSeparator = columnSeparator;
        }

        /// <summary>
        /// Define the DoneDirectory
        /// </summary>
        /// <param name="path">path to directory</param>
        public static void SetDoneDirectory(string path)
        {
            if (path == null)
            {
                RmFile = true;
            }
            else
            {
                DoneDirectory = Directory.CreateDirectory(path);
                RmFile = false;
            }
        }

        /// <summary>
        /// Define the DoneDirectory
        /// </summary>
        /// <param name="directory">Directoryinfo</param>
        public static void SetDoneDirectory(DirectoryInfo directory)
        {
            DoneDirectory = directory;
        }

        /// <summary>
        /// Define the ImportDirectory
        /// </summary>
        /// <param name="path">path to the directory</param>
        public static void SetImportDirectory(string path)
        {
            ImportDirectory = Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Define the ImportDirectory
        /// </summary>
        /// <param name="directory">DirectoryInfo</param>
        public static void SetImportDirectory(DirectoryInfo directory)
        {
            ImportDirectory = directory;
        }


        /// <summary>
        /// Do import and deplace file when done
        /// </summary>
        /// <typeparam name="TMap">a CsvClassMap </typeparam>
        /// <typeparam name="TModel">A model implementing an ICSVMocel</typeparam>
        /// <typeparam name="TContext"> DbContext</typeparam>
        /// <param name="fileName"> the file name to search in directory</param>
        /// <param name="importPath">import directory</param>
        /// <param name="donePath">Done directory</param>
        /// <returns></returns>
        public static List<TModel> Import<TMap, TModel, TContext>(string fileName, string importPath, string donePath = null) where TMap : CsvHelper.Configuration.CsvClassMap<TModel> where TModel : ICsvModel<TContext> where TContext : DbContext, new()
        {
            SetImportDirectory(importPath);
            SetDoneDirectory(donePath);
            return Import<TMap, TModel, TContext>(fileName);
        }

        /// <summary>
        /// Do import and deplace file when done, think to define import and done directories
        /// </summary>
        /// <typeparam name="TMap">a CsvClassMap </typeparam>
        /// <typeparam name="TModel">A model implementing an ICSVMocel</typeparam>
        /// <typeparam name="TContext"> DbContext</typeparam>
        /// <param name="fileName"> the file name to search in directory</param>
        /// <param name="importPath">import directory</param>
        /// <param name="donePath">Done directory</param>
        /// <returns></returns>
        public static List<TModel> Import<TMap, TModel, TContext>(string fileName) where TMap : CsvHelper.Configuration.CsvClassMap<TModel> where TModel : ICsvModel<TContext> where TContext : DbContext, new()
        {
            // Configuration
            List<TModel> itemsModel = new List<TModel>();
            foreach (FileInfo file in ImportDirectory.EnumerateFiles(fileName))
            {
                // Open the file
                using (StreamReader streamReader = new StreamReader(file.FullName, Encoding.UTF8))
                {
                    using (CsvReader csvReader = new CsvReader(streamReader))
                    {
                        // Configure CsvReader
                        csvReader.Configuration.Delimiter = ColumnSeparator;
                        csvReader.Configuration.TrimFields = true;
                        csvReader.Configuration.TrimHeaders = true;
                        csvReader.Configuration.RegisterClassMap<TMap>();

                        // Get records
                        itemsModel = csvReader.GetRecords<TModel>().ToList();

                        int itemsCount = itemsModel.Count;
                        int index = 0;
                        StringBuilder errorMessages = new StringBuilder();
                        for (int loop = 0; loop < itemsCount; loop++)
                        {
                            using (TContext db = new TContext())
                            {
                                TModel it = itemsModel.ElementAtOrDefault(index);
                                if (it != null)
                                {
                                    it.CsvFileName = file.Name;
                                    it.OnStart(db);
                                    if (it.TestBeforeSave(db))
                                    {
                                        it.Save(db);
                                    }
                                    it.OnFinish(db);
                                }
                                index += 1;
                            }
                        }
                    }
                }
                if (RmFile)
                {
                    File.Delete(file.FullName);
                }
                else
                {
                    File.Move(file.FullName, DoneDirectory.ToString() + file.Name);
                }
            }
            return itemsModel;
        }
    }
}
