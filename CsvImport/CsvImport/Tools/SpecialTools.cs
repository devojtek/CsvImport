using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pkshetlie.Csv.Import.Tools
{
    public static class SpecialTools
    {
        public static void Merge<TObject>(this TObject target, TObject source)
        {
            Type t = typeof(TObject);
            var properties = t.GetProperties().Where(prop => prop.CanRead && prop.CanWrite);
            foreach (var prop in properties)
            {
                var value = prop.GetValue(source, null);
                if (value != null)
                {
                    prop.SetValue(target, value, null);
                }
            }
        }

    }
}
