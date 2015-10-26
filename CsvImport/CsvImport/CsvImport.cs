using CsvHelper;
using Pkshetlie.Csv.Import.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;

namespace Pkshetlie.Csv.Import
{
    public delegate void OnFileErrorHandler(Exception e, FileInfo file);
    public delegate void OnLineErrorHandler(Exception e, object model);

    public class CsvImport
    {
        public event OnFileErrorHandler OnFileError;
        public event OnLineErrorHandler OnLineError;

        /// <summary>
        /// Directory where are csv
        /// </summary>
        public DirectoryInfo ImportDirectory { get; set; }
        
        /// <summary>
        /// Directory where to place file when done
        /// </summary>
        public DirectoryInfo DoneDirectory
        {
            get
            {
                return _doneDirectory;
            }
            set
            {
                _doneDirectory = value;
                RmFile = _doneDirectory == null;
            }
        }
        private DirectoryInfo _doneDirectory;

        private bool RmFile = true;

        /// <summary>
        /// Field separator in CSV file
        /// </summary>
        public string ColumnSeparator { get; set; }

        /// <summary>
        /// Line separator in CSV file
        /// </summary>
        public string LineSeparator { get; set; }

        public CsvImport(DirectoryInfo importDirectory, DirectoryInfo doneDirectory = null)
        {
            ImportDirectory = importDirectory;
            DoneDirectory = doneDirectory;
            ColumnSeparator = ";";
            LineSeparator = Environment.NewLine;
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
        public List<TModel> Import<TMap, TModel, TContext>(string fileName) where TMap : CsvHelper.Configuration.CsvClassMap<TModel> where TModel : ICsvModel<TContext> where TContext : DbContext, new()
        {
            // Configuration
            List<TModel> itemsModel = new List<TModel>();
            foreach (FileInfo file in ImportDirectory.EnumerateFiles(fileName))
            {
                try
                {
                    ConvertAnsiToUTF8(file.FullName);
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
                                TModel it = itemsModel.ElementAtOrDefault(index);
                                try
                                {
                                    using (TContext db = new TContext())
                                    {
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
                                catch (Exception e)
                                {
                                    if (OnLineError != null)
                                    {
                                        OnLineError(e, it);
                                    }
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
                        File.Move(file.FullName, Path.Combine(DoneDirectory.FullName, file.Name));
                    }
                }
                catch (Exception e)
                {
                    if (OnFileError != null)
                    {
                        OnFileError(e, file);
                    }
                }
            }

            return itemsModel;
        }

        private void ConvertAnsiToUTF8(string inputFilePath)
        {
            string fileContent = File.ReadAllText(inputFilePath, Encoding.Default);
            File.Delete(inputFilePath);
            File.WriteAllText(inputFilePath, fileContent, Encoding.UTF8);
        }

    }
}
