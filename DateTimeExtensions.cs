/*

Copyright (C) 2007-2011 by Gustavo Duarte and Bernardo Vieira.
 
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
 
*/
using System;
using System.Globalization;

namespace Common.Helpers {
    public static class DateTimeExtensions {
        #region Public members

        public static readonly long InitialJavaScriptDateTicks = 621355968000000000;
		public static readonly DateTime MinSqlDateTime = new DateTime(1900, 01, 01, 0, 0, 0, DateTimeKind.Utc);
		public static readonly DateTime MaxSqlDateTime = new DateTime(9999, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        public static string ExtJsIso8601 = "yyyy'-'MM'-'dd'T'HH':'mm':'ssK";
        public static DayOfWeek[] Weekend = new[] {DayOfWeek.Sunday, DayOfWeek.Saturday};
        public static string YearMonthDay = "yyyy'-'MM'-'dd";
        public static string YearMonthDaySpaceMilitaryTime = "yyyy'-'MM'-'dd' 'HH':'mm";
		public static string OracleDateTime = "yyyy'-'MM'-'dd' 'HH':'mm':'ss";

		public static DateTime CloneWithClearedMilliseconds(this DateTime d) {
			return new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second);
		}

        public static DateTime CloneWithClearedSeconds(this DateTime d)
        {
            return new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, 0);
        }

		public static DateTime GetMinuteFloor(this DateTime d, int minuteFloor) {
			var minutes = (d.Minute / minuteFloor);
			return new DateTime(d.Year, d.Month, d.Day, d.Hour, minutes * minuteFloor, 0);
		}

		public static string DescribeRelativeTo(this DateTime d, DateTime other) {
			if (d.Date == other.Date) {
				return "{0} {1}".Fi("Today", d.ToShortTimeString());
			}

			var differenceInDays = (d.Date - other.Date).Days;
			if (Math.Abs(differenceInDays) <= 1) {
				return "{0} {1}".Fi(differenceInDays > 0 ? "Tomorrow" : "Yesterday", d.ToShortTimeString());
			}

			if (Math.Abs(differenceInDays) < 7) {
				return "{0} {1} {2}".Fi(differenceInDays > 0 ? "Next" : "Last", other.ToString("dddd"), d.ToShortTimeString());
			}


			return "{0} {1} {2}".Fi(d.ToString("MMM d"), d.Year == other.Year ? string.Empty : d.Year.ToString(), d.ToShortTimeString());
		}

        public static long GetJavascriptTicks(this DateTime d) {
            return (d.Ticks - InitialJavaScriptDateTicks)/10000;
        }

        public static bool IsTimeWithinRange(this DateTime dt, TimeSpan min, TimeSpan max) {
            ArgumentValidator.ThrowIfNull(dt, "dt");
            ArgumentValidator.ThrowIfNull(min, "min");
            ArgumentValidator.ThrowIfNull(max, "max");
            if (min.TotalDays >= 1) {
                throw new ArgumentOutOfRangeException("min", "Min value must be less than one day.");
            }

            if (max.TotalDays >= 1) {
                throw new ArgumentOutOfRangeException("max", "Max value must be less than one day.");
            }
            if (min > max) {
                throw new ArgumentOutOfRangeException("min", "Min value must be less than Max.");
            }
            return dt.TimeOfDay >= min && dt.TimeOfDay <= max;
        }

        public static bool IsWeekend(this DateTime dt) {
            ArgumentValidator.ThrowIfNull(dt, "dt");
            return Array.IndexOf(Weekend, dt.DayOfWeek) != -1;
        }

        public static bool IsWorkday(this DateTime dt) {
            return !dt.IsWeekend();
        }

        public static DateTime LocalFromExtJs(string extjsIso8601Date) {
            ArgumentValidator.ThrowIfNullOrEmpty(extjsIso8601Date, "extjsIso8601Date");

            return DateTime.ParseExact(extjsIso8601Date, ExtJsIso8601, CultureInfo.InvariantCulture);
        }

        public static DateTime Max(DateTime a, DateTime b) {
            return a > b ? a : b;
        }

        public static DateTime Min(DateTime a, DateTime b) {
            return a > b ? b : a;
        }

        public static DateTime NextDay(this DateTime dt) {
            ArgumentValidator.ThrowIfNull(dt, "dt");
            return dt.AddDays(1);
        }

        public static DateTime NextDayAt(this DateTime dt, TimeSpan time) {
            ArgumentValidator.ThrowIfNull(dt, "dt");
            ArgumentValidator.ThrowIfNull(time, "time");
            return dt.NextDay().SetTime(time);
        }

        public static DateTime NextWorkday(this DateTime dt) {
            ArgumentValidator.ThrowIfNull(dt, "dt");
            var result = new DateTime(dt.Ticks);
            do {
                result = result.AddDays(1);
            } while (result.IsWeekend());
            return result;
        }

        public static DateTime PreviousDay(this DateTime dt) {
            ArgumentValidator.ThrowIfNull(dt, "dt");
            return dt.AddDays(-1);
        }

        public static DateTime PreviousDayAt(this DateTime dt, TimeSpan time) {
            ArgumentValidator.ThrowIfNull(dt, "dt");
            ArgumentValidator.ThrowIfNull(time, "time");
            return dt.PreviousDay().SetTime(time);
        }

        public static DateTime PreviousWorkday(this DateTime dt) {
            ArgumentValidator.ThrowIfNull(dt, "dt");
            var result = new DateTime(dt.Ticks);
            do {
                result = result.AddDays(-1);
            } while (result.IsWeekend());
            return result;
        }

        public static DateTime SetTime(this DateTime dt, TimeSpan time) {
            ArgumentValidator.ThrowIfNull(dt, "dt");
            ArgumentValidator.ThrowIfNull(time, "time");
            if (time.TotalDays >= 1) {
                throw new ArgumentOutOfRangeException("time", "Time value must be less than one day.");
            }

            return new DateTime(dt.Year, dt.Month, dt.Day) + time;
        }

        public static string ToExtjsIso8601(this DateTime dt) {
            return dt.ToString(ExtJsIso8601, CultureInfo.InvariantCulture);
        }

        public static DateTime UtcFromExtJs(string extjsIso8601Date) {
            return LocalFromExtJs(extjsIso8601Date).ToUniversalTime();
        }

		public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek) {
			var diff = dt.DayOfWeek - startOfWeek;
			if (diff < 0) {
				diff += 7;
			}

			return dt.AddDays(-1 * diff).Date;
		}

        #endregion
    }
}