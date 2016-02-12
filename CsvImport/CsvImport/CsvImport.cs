﻿using CsvHelper;
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
        /// Directory where is csv
        /// </summary>
        public DirectoryInfo ImportDirectory { get; set; }
        /// <summary>
        /// Directory where goes csv
        /// </summary>
        private DirectoryInfo _doneDirectory;

        /// <summary>
        /// Don't do conversion if utf8
        /// </summary>
        private bool _convertToUtf8 = false;

        /// <summary>
        /// Ask to Delete File
        /// </summary>
        private bool _removeFile = false;

        /// <summary>
        /// Ask to Delete File
        /// </summary>
        private bool _hasFistLine = true;
        /// <summary>
        /// Field separator in CSV file
        /// </summary>
        public string ColumnSeparator { get; set; }

        /// <summary>
        /// Line separator in CSV file
        /// </summary>
        public string LineSeparator { get; set; }



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
        /// Do conversion frome ANSI to UTF8 : true : yes / false : no
        /// </summary>
        public bool ConvertToUtf8
        {
            get
            {
                return _convertToUtf8;
            }
            set
            {
                _convertToUtf8 = value;
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
        /// <summary>
        ///  Remove File true : yes / false : no
        /// </summary>
        public bool HasFirstLine
        {
            get
            {
                return _hasFistLine;
            }
            set
            {
                _hasFistLine = value;
            }
        }
        /// <summary>
        /// initialize CsvImport
        /// </summary>
        /// <param name="importDirectory"></param>
        /// <param name="doneDirectory"></param>
        public CsvImport(DirectoryInfo importDirectory, DirectoryInfo doneDirectory = null, string columnSeparator = ";")
        {
            ImportDirectory = importDirectory;
            DoneDirectory = doneDirectory;
            ColumnSeparator = columnSeparator;
            LineSeparator = Environment.NewLine;
        }

        /// <summary>
        /// Do import and deplace file when done, think to define import and done directories
        /// </summary>
        /// <typeparam name="TMap">a CsvClassMap </typeparam>
        /// <typeparam name="TModel">A model implementing an ICSVMocel</typeparam>
        /// <typeparam name="TContext"> DbContext</typeparam>
        /// <param name="fileName">the name of the csv to load</param>
        /// <returns></returns>
        public List<TModel> Import<TMap, TModel, TContext>(string fileName) where TMap : CsvHelper.Configuration.CsvClassMap<TModel> where TModel : ICsvModel<TContext> where TContext : DbContext, new()
        {
            // Configuration
            List<TModel> itemsModel = new List<TModel>();
            foreach (FileInfo file in ImportDirectory.EnumerateFiles(fileName).OrderBy(x=>x.Name))
            {
                try
                {
                    if (ConvertToUtf8)
                    {
                        //need to convert to utf8
                        ConvertAnsiToUTF8(file.FullName);
                    }
                    // Open the file
                    using (StreamReader streamReader = new StreamReader(file.FullName, Encoding.UTF8))
                    {
                        using (CsvReader csvReader = new CsvReader(streamReader))
                        {
                            // Configure CsvReader
                            csvReader.Configuration.Delimiter = ColumnSeparator;
                            csvReader.Configuration.TrimFields = true;
                            csvReader.Configuration.TrimHeaders = true;
                            csvReader.Configuration.HasHeaderRecord = HasFirstLine;
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
