using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;


class CNIiniControl
{
    private static readonly string iniPath = Application.StartupPath + @"\Config\Config.ini";

    [DllImport("kernel32")]
    private static extern uint GetPrivateProfileString(

        string lpAppName, // points to section name 
        string lpKeyName, // points to key name 
        string lpDefault, // points to default string 
        byte[] lpReturnedString, // points to destination buffer 
        uint nSize, // size of destination buffer 
        string lpFileName  // points to initialization filename 
    );

    [DllImport("kernel32.dll")]
    private static extern int GetPrivateProfileString(   // INI Read
        string section,
        string key,
        string def,
        StringBuilder retVal,
        int size,
        string filePath);

    [DllImport("kernel32.dll")]
    private static extern long WritePrivateProfileString( // INI Write
        string section,
        string key,
        string val,
        string filePath);

    // INI File Read
    public static string IniReadValue(string Section, string Key)
    {
        StringBuilder temp = new StringBuilder(255);
        int i = GetPrivateProfileString(Section, Key, "", temp, 255, iniPath);
        return temp.ToString();
    }

    public static string IniReadValue(string Section, string Key, string sPath)
    {

        StringBuilder temp = new StringBuilder(255);
        int i = GetPrivateProfileString(Section, Key, "", temp, 255, sPath);
        return temp.ToString();
    }

    // INI File Write
    public static void IniWriteValue(string Section, string Key, string Value)
    {
        WritePrivateProfileString(Section, Key, Value, iniPath);
    }

    public static void IniWriteValue(string Section, string Key, string Value, string sPath)
    {
        WritePrivateProfileString(Section, Key, Value, sPath);
    }

    //Section Count
    public static List<string> ReadSection(string sPath)
    {
        List<string> result = new List<string>();

        byte[] buf = new byte[65536];
        uint len = GetPrivateProfileString(null, null, null, buf, (uint)buf.Length, sPath);
        int j = 0;

        for (int i = 0; i < len; i++)
        {
            if (buf[i] == 0)
            {
                result.Add(Encoding.Default.GetString(buf, j, i - j));
                j = i + 1;
            }
        }
        return result;
    }
}

