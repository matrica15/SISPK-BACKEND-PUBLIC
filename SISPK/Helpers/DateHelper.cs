using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace SISPK.Helpers
{
    public class DateHelper
    {
        public static string convertDateFromDb(string _tanggal, string newPattern = "", string defaultPattern = "")
        {
            DateTime parsedDate;
            var bulan = "";
            if (DateTime.TryParseExact(_tanggal, defaultPattern, null, DateTimeStyles.None, out parsedDate))
            {
                var culture = new CultureInfo("id-ID");
                bulan = parsedDate.ToString(newPattern, culture);
            }
            return bulan;
        }
    }
}