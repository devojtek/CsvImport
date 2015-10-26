﻿namespace Pkshetlie.Csv.Import.Interfaces
{
    public interface ICsvModel<TContext> 
    {
        string CsvFileName { get; set; }
        int Index { get; set; }
        
        //void OnFileNameChange(TContext db, string fileName);//not for now
        void OnStart(TContext db );
        bool TestBeforeSave(TContext db );
        void Save(TContext db);
        void OnFinish(TContext db);
    }
}
