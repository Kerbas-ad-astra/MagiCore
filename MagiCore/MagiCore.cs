﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MagiCore
{
    public class MagiCore
    {
        public static Version GetVersion()
        {
            return new Version(1, 2, 0, 0);
        }
    }

    public class Utilities
    {
        /// <summary>
        /// Takes a time and splits it into an integer array describing the number of years, days, hours, minutes, seconds
        /// </summary>
        /// <param name="UT">The time to convert</param>
        /// <returns>An integer array of each time part</returns>
        public static int[] ConvertUT(double UT)
        {
            double time = UT;
            int[] ret = { 0, 0, 0, 0, 0 };
            ret[0] = (int)Math.Floor(time / (KSPUtil.dateTimeFormatter.Year)) + 1; //year
            time %= (KSPUtil.dateTimeFormatter.Year);
            ret[1] = (int)Math.Floor(time / KSPUtil.dateTimeFormatter.Day) + 1; //days
            time %= (KSPUtil.dateTimeFormatter.Day);
            ret[2] = (int)Math.Floor(time / (3600)); //hours
            time %= (3600);
            ret[3] = (int)Math.Floor(time / (60)); //minutes
            time %= (60);
            ret[4] = (int)Math.Floor(time); //seconds

            return ret;
        }

        /// <summary>
        /// Formats a string from a time value into X days, X hours, X minutes, and X seconds.
        /// </summary>
        /// <param name="time">Time in seconds</param>
        /// <param name="skipZeroes">If true, skips any zero values. Default false.</param>
        /// <returns>The formatted time</returns>
        public static string GetFormattedTime(double time, bool skipZeroes=false)
        {
            if (time > 0)
            {
                double t;
                StringBuilder formatedTime = new StringBuilder();
                if (GameSettings.KERBIN_TIME)
                {
                    t = Math.Floor(time / 21600);
                    if (!skipZeroes || t > 0)
                        formatedTime.AppendFormat("{0,2:0} days ", t);
                    time = time % 21600;
                }
                else
                {
                    t = Math.Floor(time / 86400);
                    if (!skipZeroes || t > 0)
                        formatedTime.AppendFormat("{0,2:0} days ", t);
                    time = time % 86400;
                }
                t = Math.Floor(time / 3600);
                if (!skipZeroes || t > 0)
                    formatedTime.AppendFormat("{0,2:0} hours ", t);
                time = time % 3600;
                t = Math.Floor(time / 60);
                if (!skipZeroes || t > 0)
                    formatedTime.AppendFormat("{0,2:0} minutes ", t);
                time = time % 60;

                if (!skipZeroes || time > 0)
                    formatedTime.AppendFormat("{0,2:0} seconds", time);

                return formatedTime.ToString();
            }
            else
            {
                return "0 days,  0 hours,  0 minutes,  0 seconds";
            }

        }

        /// <summary>
        /// Formats a time in "colon format" such as "1:23:45:54"
        /// </summary>
        /// <param name="time">The time to format</param>
        /// <returns>The formatted time</returns>
        public static string GetColonFormattedTime(double time)
        {
            if (time > 0)
            {
                StringBuilder formatedTime = new StringBuilder();
                if (GameSettings.KERBIN_TIME)
                {
                    formatedTime.AppendFormat("{0,2:00}:", Math.Floor(time / 21600));
                    time = time % 21600;
                }
                else
                {
                    formatedTime.AppendFormat("{0,2:00}:", Math.Floor(time / 86400));
                    time = time % 86400;
                }
                formatedTime.AppendFormat("{0,2:00}:", Math.Floor(time / 3600));
                time = time % 3600;
                formatedTime.AppendFormat("{0,2:00}:", Math.Floor(time / 60));
                time = time % 60;
                formatedTime.AppendFormat("{0,2:00}", time);

                return formatedTime.ToString();
            }
            else
            {
                return "00:00:00:00";
            }
        }


        /// <summary>
        /// Converts a string containing time elements to a UT or timespan
        /// </summary>
        /// <param name="timeString">The string to parse</param>
        /// <param name="toUT">If true, converts to a UT rather than a timespan. Default true.</param>
        /// <returns>Time in seconds</returns>
        public static double ParseTimeString(string timeString, bool toUT = true)
        {
            //if it doesn't contain colons, we assume it's not colon formatted
            timeString = timeString.ToLower();
            try
            {
                if (timeString.Contains(":"))
                {
                    return ParseColonFormattedTime(timeString, toUT);
                }
                else if (timeString.Contains("s") || timeString.Contains("m") || timeString.Contains("h") || timeString.Contains("d") || timeString.Contains("y"))
                {
                    return ParseCommonFormattedTime(timeString, toUT);
                }
                else
                {
                    double time;
                    if (double.TryParse(timeString, out time))
                    {
                        return time;
                    }
                    return 0;
                }
            }
            catch
            {
                return 0;
            }

        }

        /// <summary>
        /// Takes a string with qualifiers like y, d, h, m, s and converts it to seconds
        /// </summary>
        /// <param name="timeString">The string to parse</param>
        /// <param name="toUT">If true, converts to a UT rather than a timespan. Default true.</param>
        /// <returns>Time in seconds</returns>
        public static double ParseCommonFormattedTime(string timeString, bool toUT = true)
        {
            //parses strings like "12d 14h 32m" or "3y8d"
            double time = -1;
            timeString = timeString.ToLower(); //make sure everything is lowercase
            string[] parts = Regex.Split(timeString, "([a-z])");//split on characters (should also include the character as the next element of the array)
            int len = parts.Length;
            double sPerDay = GameSettings.KERBIN_TIME ? 6 * 3600 : 24 * 3600;
            double sPerYear = GameSettings.KERBIN_TIME ? 426 * sPerDay : 365 * sPerDay;

            //loop over all the elements, if it's y,d,h,m,s then take the previous element as the number
            if (len > 1)
            {
                for (int i = 1; i < len; i++)
                {
                    double multiplier = 1;
                    double value = 0;

                    string s = parts[i].Trim();
                    if (s == "s")
                    {
                        //seconds
                        multiplier = 1;
                        double.TryParse(parts[i - 1], out value);
                    }
                    else if (s == "m")
                    {
                        //minutes
                        multiplier = 60;
                        double.TryParse(parts[i - 1], out value);
                    }
                    else if (s == "h")
                    {
                        //hours
                        multiplier = 3600;
                        double.TryParse(parts[i - 1], out value);
                    }
                    else if (s == "d")
                    {
                        //days
                        multiplier = sPerDay;
                        double.TryParse(parts[i - 1], out value);
                        if (toUT)
                            value -= 1;
                    }
                    else if (s == "y")
                    {
                        //years
                        multiplier = sPerYear;
                        double.TryParse(parts[i - 1], out value);
                        if (toUT)
                            value -= 1;
                    }

                    time += multiplier * value;
                }
            }
            return time;
        }

        /// <summary>
        /// Takes a colon formatted time string ("1:23:45:54") and converts it to seconds
        /// </summary>
        /// <param name="timeString">The string to parse</param>
        /// <param name="toUT">If true, converts to a UT rather than a timespan. Default true.</param>
        /// <returns>Time in seconds</returns>
        public static double ParseColonFormattedTime(string timeString, bool toUT = true)
        {
            //toUT is for converting a string that is given as a formatted UT (Starting with Y1, D1)
            double time = -1;
            string[] parts = timeString.Split(':');
            int len = parts.Length;
            double sPerDay = GameSettings.KERBIN_TIME ? 6 * 3600 : 24 * 3600;
            double sPerYear = GameSettings.KERBIN_TIME ? 426 * sPerDay : 365 * sPerDay;
            try
            {
                time = double.Parse(parts[len - 1]);
                if (len > 1)
                    time += 60 * double.Parse(parts[len - 2]); //minutes
                if (len > 2)
                    time += 3600 * double.Parse(parts[len - 3]); //hours
                if (len > 3)
                {
                    time += sPerDay * double.Parse(parts[len - 4]); //days
                    if (toUT)
                        time -= sPerDay;
                }
                if (len > 4)
                {
                    time += sPerYear * double.Parse(parts[len - 5]); //years
                    if (toUT)
                        time -= sPerYear;
                }
            }
            catch
            {
                time = -1;
            }
            return time;
        }
    }
}
    