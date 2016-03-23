using CsvHelper;
using CsvHelper.Configuration;
using Pkshetlie.Csv.Import.Interfaces;
using Pkshetlie.Csv.Import.Tools;
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
        /// Directory where is csv
        /// </summary>
        public DirectoryInfo ImportDirectory { get; set; }
        /// <summary>
        /// Directory where goes csv
        /// </summary>
        private DirectoryInfo _doneDirectory;

        /// <summary>
        /// Ask to Delete File
        /// </summary>
        private bool _removeFile = false;

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
            }
        }

        /// <summary>
        ///  Remove File true : yes / false : no
        /// </summary>
        public bool RemoveFile
        {
            get
            {
                return _removeFile;
            }
            set
            {
                _removeFile = true;
            }
        }


        private CsvConfiguration _configuration { get; set; }

        /// <summary>
        /// initialize CsvImport
        /// </summary>
        /// <param name="importDirectory"></param>
        /// <param name="doneDirectory"></param>
        public CsvImport(DirectoryInfo importDirectory, DirectoryInfo doneDirectory = null)
        {
            ImportDirectory = importDirectory;
            DoneDirectory = doneDirectory;
            _configuration = _configuration = new CsvConfiguration();
        }

        /// <summary>
        /// Do import and deplace file when done, think to define import and done directories
        /// </summary>
        /// <typeparam name="TMap">a CsvClassMap </typeparam>
        /// <typeparam name="TModel">A model implementing an ICSVMocel</typeparam>
        /// <typeparam name="TContext"> DbContext</typeparam>
        /// <param name="fileName">the name of the csv to load</param>
        /// <returns></returns>
        public List<TModel> Import<TMap, TModel, TContext>(string fileName, CsvConfiguration configuration = null) where TMap : CsvHelper.Configuration.CsvClassMap<TModel> where TModel : ICsvModel<TContext> where TContext : DbContext, new()
        {
            if (configuration != null)
            {
                _configuration.Merge(configuration);
            }
            // Configuration
            List<TModel> itemsModel = new List<TModel>();
            foreach (FileInfo file in ImportDirectory.EnumerateFiles(fileName).OrderBy(x => x.Name))
            {
                try
                {
                    
                    using (StreamReader streamReader = new StreamReader(file.FullName, true))
                    {
                        if (Encoding.UTF8 != streamReader.CurrentEncoding)
                        {
                            //need to convert to utf8
                            ConvertAnsiToUTF8(file.FullName);
                        }

                    }      // Open the file
                    using (StreamReader streamReader = new StreamReader(file.FullName, Encoding.UTF8))
                    {
                        using (CsvReader csvReader = new CsvReader(streamReader, _configuration))
                        {
                            // Configure CsvReader
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
                                            it.Index = index;
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
                    if (RemoveFile)
                    {
                        File.Delete(file.FullName);
                    }
                    else if (DoneDirectory != null)
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
