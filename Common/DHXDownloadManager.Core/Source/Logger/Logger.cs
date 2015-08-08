using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DHXDownloadManager
{
    public static class Logger
    {
        public static System.Action<string> OnLogged;

        public static void Log(object e)
        {
            if (OnLogged != null)
                OnLogged(e.ToString());
        }
    }
}
