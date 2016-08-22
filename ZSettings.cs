using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZSubtitle
{
    internal class ZSettings
    {
        public static void Set(string name, object value)
        {
            try
            {
                Properties.Settings.Default[name] = value;
                Properties.Settings.Default.Save();
            }
            catch { }
        }
        public static object Get(string name)
        {
            try
            {
                return Properties.Settings.Default[name];
            }
            catch
            {
                return null;
            }
        }
        public static T? Get<T>(string name) where T : struct
        {
            try
            {
                return (T)Properties.Settings.Default[name];
            }
            catch
            {
                return null;
            }
        }
    }
}
