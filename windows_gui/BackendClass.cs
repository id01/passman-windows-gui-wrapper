using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Net.Http;

namespace WindowsFormsApp1
{
    class BackendClass
    {
        // Import DLL functions from main.dll
        [DllImport("windows.dll")]
        static extern bool parseGetResultWrapper(string userhash, string httpresult, string password, StringBuilder result, int result_len);
        [DllImport("windows.dll")]
        static extern bool respondToAddWrapper(string userhash, string httpresult, string password, string accountName, int passLength, StringBuilder result, int result_len);
        [DllImport("windows.dll")]
        static extern bool hashuserhexWrapper(string user, int user_len, StringBuilder result, int result_len);
        [DllImport("windows.dll")]
        static extern bool hashaccounthexWrapper(string account, int account_len, string userhash, int userhash_len, StringBuilder result, int result_len);

        // Wrappers around functions from main.dll
        static public string parseGetResult(string userhash, string httpresult, string password) {
            StringBuilder sb = new StringBuilder(8192);
            bool exception_thrown = parseGetResultWrapper(userhash, httpresult, password, sb, sb.Capacity);
            if (exception_thrown) {
                throw new Exception(sb.ToString());
            } else {
                return sb.ToString();
            }
        }

        static public string respondToAdd(string userhash, string httpresult, string password, string accountName, int passLength) {
            StringBuilder sb = new StringBuilder(8192);
            bool exception_thrown = respondToAddWrapper(userhash, httpresult, password, accountName, passLength, sb, sb.Capacity);
            if (exception_thrown) {
                throw new Exception(sb.ToString());
            } else {
                return sb.ToString();
            }
        }

        static public string hashuserhex(string plaintext)
        {
            StringBuilder sb = new StringBuilder(17);
            bool exception_thrown = hashuserhexWrapper(plaintext, plaintext.Length, sb, sb.Capacity);
            if (exception_thrown) {
                throw new Exception("Unknown error in hashuserhex");
            } else {
                return sb.ToString();
            }
        }

        static public string hashaccounthex(string plaintext, string userhash)
        {
            StringBuilder sb = new StringBuilder(9);
            bool exception_thrown = hashaccounthexWrapper(plaintext, plaintext.Length, userhash, userhash.Length, sb, sb.Capacity);
            if (exception_thrown) {
                throw new Exception("Unknown error in hashaccounthex");
            } else {
                return sb.ToString();
            }
        }

        // Replacements for simpler functions from main.dll
        static public FormUrlEncodedContent generateFirstPost(string userhash, string accountName)
        {
            string accounthash = hashaccounthex(accountName, userhash);
            return new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("userhash", userhash),
                new KeyValuePair<string, string>("account", accounthash)
            });
        }
    }
}
