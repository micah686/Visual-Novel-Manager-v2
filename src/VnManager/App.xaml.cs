﻿using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using System.Windows;
using LiteDB;
using VnManager.Helpers;
using VnManager.Models.Settings;
using VnManager.Utilities;
using VnManager.ViewModels.UserControls;

namespace VnManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Assembly Assembly { get; } = typeof(App).Assembly;
        public static string VersionString { get; } = Assembly.GetName().Version?.ToString(3);

        public static string ExecutableDirPath { get; } = AppDomain.CurrentDomain.BaseDirectory!;

        #region StartupLockout
        private static bool _wasSetStartupLockout = false;
        private static bool _startupLockout;
        /// <summary>
        /// Lockout variable used to prevent changing certain configurations after the application has started up
        /// </summary>
        public static bool StartupLockout
        {
            get { return _startupLockout; }
            set
            {
                if (!_wasSetStartupLockout)
                {
                    _startupLockout = value;
                    _wasSetStartupLockout = true;
                }
                else
                {
                    throw new InvalidOperationException("Value cannot be set after Startup has finished");
                }
            }
        }
        #endregion

        #region AssetDirPath 
        private static string _assetDirPath;
        /// <summary>
        /// Directory for saving assets like images, logs,...
        /// </summary>
        public static string AssetDirPath
        {
            get { return _assetDirPath; }
            set
            {
                if (!StartupLockout)
                {
                    _assetDirPath = value;
                }
                else
                {
                    throw new InvalidOperationException("AssetDirPath can only be set once!");
                }
            }
        }
        #endregion

        #region ConfigDirPath
        private static string _configDirPath;
        /// <summary>
        /// Configuration directory path, where the userSettings, database,... are stored
        /// </summary>
        public static string ConfigDirPath
        {
            get { return _configDirPath; }
            set
            {
                if (!StartupLockout)
                {
                    _configDirPath = value;
                }
                else
                {
                    throw new InvalidOperationException("ConfigDirPath can only be set once!");
                }
            }
        }
        #endregion


        #region Logger
        private static ILogger _logger= LogManager.Logger;
        /// <summary>
        /// Logger instance used for writing log files
        /// </summary>
        public static ILogger Logger
        {
            get => _logger ?? LogManager.Logger;
            set
            {
                if (!StartupLockout)
                {
                    _logger = value;
                }
                else
                {
                    throw new InvalidOperationException("Cannot set Logger after Startup");
                }
            }
        }
        #endregion

        /// <summary>
        /// Current user settings for the application
        /// </summary>
        public static UserSettings UserSettings { get; set; }

        /// <summary>
        /// Resource Manager used to retrieve strings from the .resx files
        /// </summary>
        public static readonly ResourceManager ResMan = new ResourceManager("VnManager.Strings.Resources", Assembly.GetExecutingAssembly());

        /// <summary>
        /// Database connection string without the password. Use CredentialManager to get the password
        /// </summary>
        public static string GetDbStringWithoutPass => $"Filename={Path.Combine(App.ConfigDirPath, @"database\Data.db")};Password=";

        /// <summary>
        /// Static instance of the Statusbar, so any method can write values to it
        /// </summary>
        public static StatusBarViewModel StatusBar { get; set; }

        /// <summary>
        /// Credential name for the database encryption
        /// </summary>
        internal const string CredDb = "VnManager.DbEnc";
        /// <summary>
        /// Credential name for the file encryption
        /// </summary>
        internal const string CredFile = "VnManager.FileEnc";

    }
}
