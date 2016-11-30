using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace ZSubtitle
{
	internal class ZSettings
	{
		[DllImport("kernel32")]
		private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
		[DllImport("kernel32")]
		private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

		public static void Set(string name, object value)
		{
			try
			{
				WritePrivateProfileString("Z-Subtitle", name, value.ToString(), @".\Z-Subtitle.ini");
				// Properties.Settings.Default[name] = value;
				// Properties.Settings.Default.Save();
			}
			catch { }
		}
		public static object Get(string name, object DefaultValue = null)
		{
			try
			{
				StringBuilder temp = new StringBuilder(255);
				GetPrivateProfileString("Z-Subtitle", name, "", temp, 255, @".\Z-Subtitle.ini");
				string output = temp.ToString();
				if (output == null || output.Length == 0) return null;
				return output;
				// return Properties.Settings.Default[name];
			}
			catch
			{
				return null;
			}
		}
		public static T? Get<T>(string name, T? DefaultValue = null) where T : struct
		{
			try
			{
				return (T)Get(name, DefaultValue);
			}
			catch
			{
				return null;
			}
		}
	}
}
