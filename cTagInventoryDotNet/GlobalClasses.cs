using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cTagInventoryDotNet
{
    public class GlobalClasses
    {
        public DateTime? DateFromString(string dtIn)
        {
            DateTime? dtOut = null;
            try
            {
                dtOut = Convert.ToDateTime(dtIn);
            }
            catch { }
            return dtOut;
        }

        public int? IntFromString(string intIn)
        {
            int? intOut = null;
            try
            {
                if (intIn != "")
                {
                    intOut = Convert.ToInt32(intIn);
                }
            }
            catch { }
            return intOut;
        }

        public decimal? DecFromString(string intIn)
        {
            decimal? intOut = null;
            try
            {
                if (intIn != "")
                {
                    intOut = Convert.ToDecimal(intIn);
                }
            }
            catch { }
            return intOut;
        }

        public string FormatPhone(string varIn)
        {
            string varOut = varIn;
            try
            {
                varOut = varIn.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");
            }
            catch { }

            return varOut;
        }
        public string StringFromDate(DateTime? varIn)
        {
            string varOut = "";
            try
            {
                varOut = varIn.Value.ToString("MM/dd/yyyy");
            }
            catch { }
            return varOut;
        }

        public string GetLegalDate()
        {
            string dayend = "th";
            switch (DateTime.Now.Day)
            {
                case 1:
                case 21:
                case 31:
                    dayend = "st";
                    break;
                case 2:
                case 22:
                    dayend = "nd";
                    break;
                case 3:
                case 23:
                    dayend = "rd";
                    break;
                default:
                    dayend = "th";
                    break;
            }
            string dated = DateTime.Now.Day.ToString("D") + dayend;
            string datem = DateTime.Now.ToString("MMMM");
            string datey = (DateTime.Now.Year.ToString()).Substring(2);

            return "This " + dated + dayend + " of " + datem + ", " + datey;
        }

    }
}