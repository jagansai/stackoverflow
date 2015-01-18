using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ConsoleApplicationCS
{




    public class Utils
    {
       
        internal static DateTime ConvertFromUnix(int unixtime)
        {
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return dt.AddSeconds(unixtime);
        }

        internal static string Elapsed(DateTime dt)
        {
            TimeSpan ts = DateTime.UtcNow.Subtract(dt);
            return string.Format("{0:dd\\.hh\\:mm\\:ss}", ts);                        
        }


        public static void TimeTaken(Action action, string message)
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                action();
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                sw.Stop();
                TimeSpan ts = sw.Elapsed;
                Console.WriteLine("{0} took:{1:F3} secs", message, ts.TotalSeconds);
            }
        }
    }
}
