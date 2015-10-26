namespace Pkshetlie.Csv.Import.Interfaces
{
    public interface ICsvModel<TContext> 
    {
        #region properties
        string CsvFileName { get; set; }
        int Index { get; set; }
        #endregion

        #region methods
        void OnStart(TContext db );
        bool TestBeforeSave(TContext db );
        void Save(TContext db);
        void OnFinish(TContext db);
        #endregion
    }
}
