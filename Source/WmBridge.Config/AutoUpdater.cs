//
//  Copyright (c) 2015 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System;
using System.Diagnostics;
using System.IO;

namespace WmBridge.Config
{
    public static class AutoUpdater
    {
        static bool hasChecked = false;

        public static void CheckForUpdates()
        {
            if (hasChecked)
                return;

            try
            {
                Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "updater.exe"), "/silent").Dispose();
            }
            catch (Exception) 
            { 
            
            }

            hasChecked = true;
        }
    }
}
