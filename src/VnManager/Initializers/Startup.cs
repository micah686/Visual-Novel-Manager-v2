﻿// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Stylet;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using AdysTech.CredentialManager;
using LiteDB;
using Mayerch1.GithubUpdateCheck;
using Sentry;
using VnManager.Helpers;
using VnManager.ViewModels.Dialogs;
using VnManager.ViewModels.Windows;

namespace VnManager.Initializers
{
    public class Startup
    {
        private readonly Func<ImportViewModel> _importVm;
        private readonly Func<SetEnterPasswordViewModel> _enterPassVm;
        private readonly IWindowManager _windowManager;
        public Startup(IWindowManager windowManager, Func<ImportViewModel> import, Func<SetEnterPasswordViewModel> enterPass)
        {
            _windowManager = windowManager;
            _importVm = import;
            _enterPassVm = enterPass;
            StartupCheck();
        }

        /// <summary>
        /// Do some final checks before main window is allowed to load
        /// </summary>
        private void StartupCheck()
        {
            CheckForUpdates();
            
            if (!IsNormalStart())
            {
                App.UserSettings = UserSettingsHelper.ReadUserSettings();
                var auth = _enterPassVm();
                var isAuth = _windowManager.ShowDialog(auth);

                if (isAuth == true)
                {
                    App.UserSettings = UserSettingsHelper.ReadUserSettings();//read for any changed user settings
                    CheckForImportDb();
                }
                else
                {
                    Environment.Exit(0);//closed auth window
                }
            }
            else
            {
                CheckDbError();
            }
        }
        /// <summary>
        /// Checks to see if the user has been asked to import the database yet. If not, prompts for importing data into the Db
        /// </summary>
        private void CheckForImportDb()
        {
            if (App.UserSettings.DidAskImportDb != false)
            {
                return;
            }
            var result = _windowManager.ShowMessageBox(App.ResMan.GetString("AskImportDb"),
                App.ResMan.GetString("ImportDataTitle"), MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                var vm = _importVm();
                _windowManager.ShowDialog(vm);
            }


            App.UserSettings.DidAskImportDb = true;
            UserSettingsHelper.SaveUserSettings(App.UserSettings);
        }


        //should exit if it can't read the database
        /// <summary>
        /// Checks if the program can successfully open the database.
        /// If it encounters an error, the program will exit, as the program can't function if it can't read the database
        /// </summary>
        private void CheckDbError()
        {
            string appExit = App.ResMan.GetString("AppExit");
            string dbError = App.ResMan.GetString("DbError");

            string errorStr;
            try
            {
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1)
                {
                    errorStr = $"{App.ResMan.GetString("PasswordNoEmpty")}\n{appExit}";
                    _windowManager.ShowMessageBox(errorStr, dbError);
                    Environment.Exit(1);
                }
                else
                {
                    using (var db = new LiteDatabase($"Filename={Path.Combine(App.ConfigDirPath, App.DbPath)};Password='{cred.Password}'"))
                    {
                        //do nothing. This is checking if the database can be opened
                    }
                }
            }
            catch (IOException)
            {
                errorStr = $"{App.ResMan.GetString("DbIsLockedProc")}\n{appExit}";
                _windowManager.ShowMessageBox(errorStr, dbError);
                Environment.Exit(1);
            }
            catch (LiteException ex)
            {
                if (ex.Message == "Invalid password")
                {
                    errorStr = $"{App.ResMan.GetString("PassIncorrect")}\n{appExit}";
                    _windowManager.ShowMessageBox(errorStr, dbError);
                    Environment.Exit(1);
                }
                else
                {
                    errorStr = $"{ex.Message}\n{appExit}";
                    _windowManager.ShowMessageBox(errorStr, dbError);
                    App.Logger.Error(ex, "CheckDb LiteException error");
                    SentryHelper.SendException(ex, null, SentryLevel.Error);
                    Environment.Exit(1);
                }
            }
            catch (Exception e)
            {
                errorStr = $"{App.ResMan.GetString("UnknownException")}\n{appExit}";
                _windowManager.ShowMessageBox(errorStr, dbError);
                App.Logger.Error(e, "CheckDbError failed");
                SentryHelper.SendException(e, null, SentryLevel.Error);
                Environment.Exit(1);
            }
        }


        private static bool IsNormalStart()
        {
            var configFile = Path.Combine(App.ConfigDirPath, @"config\config.json");
            if (!File.Exists(configFile))
            {
                return false;
            }
            if (!UserSettingsHelper.ValidateConfigFile())
            {
                return false;
            }
            if (CredentialManager.GetCredentials(App.CredDb) == null)
            {
                return false;
            }
            App.UserSettings = UserSettingsHelper.ReadUserSettings();
            var useEncryption = App.UserSettings.RequirePasswordEntry;
            return !useEncryption;
        }

        private void CheckForUpdates()
        {
            try
            {
                var updateCheck = new GithubUpdateCheck("micah686", "VnManager");
                bool updateAvailable = updateCheck.IsUpdateAvailable(App.VersionString, VersionChange.Build);
                if (updateAvailable == false)
                {
                    return;
                }
                var result = _windowManager.ShowMessageBox(App.ResMan.GetString("UpdateMsg"), App.ResMan.GetString("UpdateAvailable"), MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    const int twoMegaBytes = 2000000;
                    var releasePath = new Uri(@"https://github.com/micah686/VnManager/releases/latest/download/VnManagerInstaller.exe");
                    var tempFolder = Path.GetTempPath();
                    var filePath = $@"{tempFolder}VnManager-{Guid.NewGuid()}.exe";
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(releasePath, filePath);
                    }

                    var totalBytes = new FileInfo(filePath).Length;
                    if (totalBytes > twoMegaBytes)
                    {
                        var ps = new ProcessStartInfo(filePath)
                        {
                            UseShellExecute = true,
                            Verb = "open"
                        };
                        Process.Start(ps);
                        Environment.Exit(0);
                    }
                }
            }
            catch (Exception e)
            {
                App.Logger.Warning(e, "Failed to check for Updates");
                SentryHelper.SendException(e, null, SentryLevel.Warning);
            }
        }
    }
}
