﻿using System;
using System.IO;
using System.IO.Abstractions;
using VnManager.Utilities;
using VnManager.Helpers;
using System.Globalization;
using System.Linq;
using LiteDB;

namespace VnManager.Initializers
{
    public static class Startup
    {
        private static readonly IFileSystem fs = new FileSystem();
        public static void SetDirectories()
        {            
            if(App.ExecutableDirPath.Contains(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)) || App.ExecutableDirPath.Contains(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86))
                || App.ExecutableDirPath.Contains(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)))
            {
                //prog files or appdata local
                bool canReadWrite = (CheckWriteAccess.CheckWrite(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)) 
                    && CheckWriteAccess.CheckWrite(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)));
                if (canReadWrite)
                {
                    App.AssetDirPath = fs.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VnManager");
                    App.ConfigDirPath = fs.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VnManager");
                    App.IsPortable = false;
                }
                else
                {
                    App.AssetDirPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    App.Logger.Error("Could not read/write to asset/config directories");
                    AdonisUI.Controls.MessageBox.Show($"Could not read/write to user data directories!\nExiting Application", "Failed to start!", AdonisUI.Controls.MessageBoxButton.OK, AdonisUI.Controls.MessageBoxImage.Exclamation);
                    Environment.Exit(1);
                }
            }            
            else
            {
                //instance
                bool canReadWrite = CheckWriteAccess.CheckWrite(App.ExecutableDirPath);
                if (canReadWrite)
                {
                    App.AssetDirPath = fs.Path.Combine(App.ExecutableDirPath, "Data");
                    App.ConfigDirPath = fs.Path.Combine(App.ExecutableDirPath, "Data");
                    App.IsPortable = true;
                }
                else
                {
                    App.AssetDirPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    App.Logger.Error("Could not read/write to asset/config directories");
                    AdonisUI.Controls.MessageBox.Show($"Could not read/write to user data directories!\nExiting Application", "Failed to start!", AdonisUI.Controls.MessageBoxButton.OK, AdonisUI.Controls.MessageBoxImage.Exclamation);
                    Environment.Exit(1);
                }
            }

            CreateFolders();            
        }
        
        private static void CreateFolders()
        {
            //Assets folder ( images, logs,...)
            fs.Directory.CreateDirectory(Path.Combine(App.AssetDirPath, @"res\icons\countryFlags"));
            fs.Directory.CreateDirectory(Path.Combine(App.AssetDirPath, @"sources\vndb\images\cover"));
            fs.Directory.CreateDirectory(Path.Combine(App.AssetDirPath, @"sources\vndb\images\cover"));
            fs.Directory.CreateDirectory(Path.Combine(App.AssetDirPath, @"sources\vndb\images\screenshots"));
            fs.Directory.CreateDirectory(Path.Combine(App.AssetDirPath, @"sources\vndb\images\characters"));

            fs.Directory.CreateDirectory(Path.Combine(App.AssetDirPath, @"logs"));

            //Config
            fs.Directory.CreateDirectory(Path.Combine(App.ConfigDirPath, @"database"));
            fs.Directory.CreateDirectory(Path.Combine(App.ConfigDirPath, @"secure"));
            fs.Directory.CreateDirectory(Path.Combine(App.ConfigDirPath, @"config"));
        }

        internal static void DeleteOldLogs()
        {
            //doesn't delete logs out of User Profile directory
            var minDate = (DateTime.Today - new TimeSpan(30, 0, 0, 0));
            if (!Directory.Exists(App.ConfigDirPath + @"\logs")) return;
            foreach (var file in Directory.GetFiles(App.ConfigDirPath + @"\logs"))
            {
                var name = Path.GetFileName(file);
                int charLoc = name.IndexOf("_", StringComparison.Ordinal);
                var subst = name.Substring(0, charLoc);

                var didParse = DateTime.TryParseExact(subst.ToCharArray(), "dd-MM-yyyy".ToCharArray(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var date);
                if (didParse == false) continue;
                if (date >= minDate) continue;
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    App.Logger.Warning(ex, "Couldn't delete log file");
                }
            }

        }

        internal static void ValidateFiles()
        {
            var configFile = Path.Combine(App.ConfigDirPath, @"config\config.xml");
            var secretStore = Path.Combine(App.ConfigDirPath, @"secure\secrets.store");
            var secretKey = Path.Combine(App.ConfigDirPath, @"secure\secrets.key");

            if (File.Exists(configFile))
            {
                if (!ValidateXml.IsValidXml(configFile))
                {
                    File.Delete(configFile);
                }
            }
            

            if (!File.Exists(secretStore) || !File.Exists(Path.Combine(secretKey)))
            {
                Directory.Delete(Path.Combine(App.ConfigDirPath, @"secure"), true);
                Directory.CreateDirectory(Path.Combine(App.ConfigDirPath, @"secure"));
                return;
            }

            string[] secKeys = new[] {"FileEnc", "ConnStr"};
            var encSt = new Secure();
            if (secKeys.Select(key => encSt.TestSecret(key)).Any(output => output == false))
            {
                Directory.Delete(Path.Combine(App.ConfigDirPath, @"secure"), true);
                Directory.CreateDirectory(Path.Combine(App.ConfigDirPath, @"secure"));
            }
        }
        
    }
}
