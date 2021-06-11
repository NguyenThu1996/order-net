using OPS.Utility.Extensions;
using System;
using System.Globalization;
using System.Linq;
using static OPS.Utility.Constants;

namespace OPS.Utility
{
    public class Commons
    {
        // Summary:
        //     Returns the era in the specified System.DateTime.
        //
        // Parameters:
        //   date:
        //     The System.DateTime to read.
        //
        // Returns:
        //     The name of EraYear
        public static string GetEraYear(DateTime? date)
        {
            if (date.HasValue)
            {
                JapaneseCalendar japaneseCalendar = new JapaneseCalendar();
                return ((EraName)japaneseCalendar.GetEra(date.Value)).GetEnumDescription();
            }
            return string.Empty;
        }

        // Summary:
        //     Returns the day of week Japanese in the specified System.DateTime.
        //
        // Parameters:
        //   date:
        //     The System.DateTime to read.
        //
        // Returns:
        //     The day of week Japanese
        public static string GetDayOfWeekJP(DateTime? date)
        {
            if (date.HasValue)
            {
                return ((DateOfWeekJP)date.Value.DayOfWeek).GetEnumDescription();
            }
            return string.Empty;
        }

        // Summary:
        //     Returns a string after rounding  
        //
        // Parameters:
        //   Revenue:
        //     The decimal? to read. Number want to round
        //   Unit:
        //     The int to read. How many unit want to round 
        //
        // Returns:
        //     The day of week Japanese
        public static long RoundRevenue(decimal? Revenue, int Unit)
        {
            var unitRound = (decimal)Math.Pow(10, Unit);
            if (Revenue.HasValue && Revenue >= unitRound)
            {
                var revenueStr = Decimal.ToInt64(Revenue.Value).ToString();
                return Convert.ToInt64(revenueStr.Substring(0, revenueStr.Length - Unit));
            }
            return 0;
        }

        // Summary:
        //     Returns a string DateJapan after convert 
        //
        // Parameters:
        //   date:
        //     The DateTime to read. Date want to convert 
        //
        // Returns:
        //    string DateJapan 
        public static string GetTextDateJapan(DateTime? date)
        {
            string result = string.Empty;
            if (date.HasValue)
            {
                JapaneseCalendar calendarJp = new System.Globalization.JapaneseCalendar();
                CultureInfo cultureJp = new System.Globalization.CultureInfo("ja-JP", false);
                cultureJp.DateTimeFormat.Calendar = calendarJp;
                result = date.Value.ToString(ExactDateJapanEraFormat, cultureJp);
            }
            return result;
        }

        public static long TryParseLong(string number)
        {
            try
            {
                return long.Parse(number);
            }
            catch
            {
                return 0;
            }
        }

        public static string JoinNumbersToString(string seperator = ",", params int[] nums)
        {
            if(nums == null || nums.Length < 1)
            {
                return "";
            }

            return string.Join(seperator, nums);
        }

        public static int[] SplitStringToNumbers(string str, char seperator = ',')
        {
            try
            {
                return str.Split(seperator).Select(item => int.Parse(item)).ToArray();
            }
            catch
            {
                return new int[4];
            }
        }
    }
}
