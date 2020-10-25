﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AdysTech.CredentialManager;
using FluentValidation;
using LiteDB;
using Stylet;
using StyletIoC;
using VnManager.Helpers;
using VnManager.Models.Settings;
using VnManager.ViewModels.UserControls;

namespace VnManager.ViewModels.Windows
{
    public class SetEnterPasswordViewModel: Screen
    {
        public string Title { get; set; }
        /// <summary>
        /// Manual password creation is required, shows the 2 password fields
        /// </summary>
        public bool RequirePasswordChecked { get; set; } //yes checked for creating a new password
        public SecureString Password { get; set; }
        public SecureString ConfirmPassword { get; set; }
        /// <summary>
        /// If true, shows the dialog for unlocking the database (password entry)
        /// </summary>
        public bool IsUnlockPasswordVisible { get; set; }

        private bool _isCreatePasswordVisible = true;
        /// <summary>
        /// If true, shows the dialog for creating a new password
        /// </summary>
        public bool IsCreatePasswordVisible
        {
            get => _isCreatePasswordVisible;
            set
            {
                _isCreatePasswordVisible = value;
                IsUnlockPasswordVisible = !value;
                SetAndNotify(ref _isCreatePasswordVisible, value);
            }
        }

        /// <summary>
        /// Is the button for unlocking the Db enabled, or is it locked out by the wait timer
        /// </summary>
        public bool IsUnlockPasswordButtonEnabled { get; set; } = true;
        /// <summary>
        /// Indicates if you have clicked submit/unlock to create a new password, or to unlock the database
        /// </summary>
        internal bool ShouldCheckPassword = false;
        /// <summary>
        /// Attempts to unlock the database.
        /// </summary>
        private int _attemptCounter = 0;


        public SetEnterPasswordViewModel(IModelValidator<SetEnterPasswordViewModel> validator) : base(validator)
        {
            ValidateFiles();
            Title = App.ResMan.GetString("CreatePassTitle");
            if (App.UserSettings.RequirePasswordEntry)
            {
                IsCreatePasswordVisible = false; //sets form to unlock mode
                Title = App.ResMan.GetString("UnlockDbTitle");
                if (CredentialManager.GetCredentials(App.CredDb) == null && File.Exists(Path.Combine(App.ConfigDirPath, @"config\config.json")))
                {
                    ClearPreviousCredentials();
                    File.Delete(Path.Combine(App.ConfigDirPath, @"config\config.json"));
                    Environment.Exit(0);
                }
            }
            
        }

        private static void ClearPreviousCredentials()
        {
            //remove any previous credentials
            string[] credStrings = new[] { App.CredDb, App.CredFile };
            foreach (var cred in credStrings)
            {
                var value = CredentialManager.GetCredentials(cred);
                if (value != null)
                {
                    CredentialManager.RemoveCredentials(cred);
                }
            }
        }

        /// <summary>
        /// If the user has checked enable custom password, create a database with the custom password
        /// If the user is not generating a custom password, create a database with a 64 character password with 16 special characters
        /// Note: The password for Debug is 123456, in order to assist with debugging.
        /// </summary>
        /// <returns></returns>
        public async Task CreatePasswordClickAsync()
        {
            File.Delete(Path.Combine(App.ConfigDirPath, App.DbPath));
            try
            {
                ShouldCheckPassword = true;
                if (RequirePasswordChecked)
                {
                    if (Password == null || Password.Length < 1 || ConfirmPassword == null || ConfirmPassword.Length <1)
                    {
                        await ValidateAsync();
                        return;
                    }

                    
                    var hashStruct = Secure.GenerateHash(Password, Secure.GenerateRandomSalt());
                    string username = $"{hashStruct.Hash}|{hashStruct.Salt}";
                    
                    var cred = new NetworkCredential(username, Password);
                    CredentialManager.SaveCredentials(App.CredDb, cred);
                    var validPasswords = await ValidateAsync();
                    if(validPasswords != true) return;
                    using (_ = new LiteDatabase(
                        $"Filename={Path.Combine(App.ConfigDirPath, App.DbPath)};Password={cred.Password}"))
                    {
                        //create initial empty database
                    }

                    var settings = new UserSettings
                    {
                        RequirePasswordEntry = true
                    };
                    UserSettingsHelper.SaveUserSettings(settings);
                    bool result = await ValidateAsync();
                    if (result)
                    {
                        RequestClose(true);
                    }
                }
                else
                {
                    
                    var password = new SecureString();

#if DEBUG
                    foreach (var character in "123456") //TODO:Debug only password
                    {
                        password.AppendChar(character);
                    }
#else
                    foreach (var character in Secure.GenerateSecurePassword(64,16))
                    {
                        password.AppendChar(character);
                    }
#endif
                    var hashStruct = Secure.GenerateHash(password, Secure.GenerateRandomSalt());
                    string username = $"{hashStruct.Hash}|{hashStruct.Salt}";

                    var cred = new NetworkCredential(username, password);
                    CredentialManager.SaveCredentials(App.CredDb, cred);

                    using (_ = new LiteDatabase(
                        $"Filename={Path.Combine(App.ConfigDirPath, App.DbPath)};Password={cred.Password}"))
                    {
                        //create initial empty database
                    }

                    bool result = await ValidateAsync();
                    if (result)
                    {
                        RequestClose(true);
                    }
                }
                ShouldCheckPassword = false;
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "Couldn't Create Password");
                ShouldCheckPassword = false;
                RequestClose(false);
            }
        }

        /// <summary>
        /// Attempts to unlock the database with the entered password.
        /// If the password is incorrect, it will create a validation error on the TextBox
        /// </summary>
        /// <returns></returns>
        public async Task UnlockPasswordClickAsync()
        {
            ShouldCheckPassword = true;
            bool result = await ValidateAsync();
            if (result)
            {
                RequestClose(true);
            }
            _attemptCounter += 1;
            await PasswordAttemptCheckerAsync();
            ShouldCheckPassword = false;
        }

        /// <summary>
        /// Tries to unlock the database if [Enter] is pressed
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public async Task UnlockPasswordKeyPressedAsync(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await UnlockPasswordClickAsync();
            }
        }

        /// <summary>
        /// Checks how many password attempts were made
        /// If too many attempts are made, the time between attempts is increased
        /// Less than 5 tries is 650 ms, 5-20 is somewhere between 3-10 sec, 20-50 is 30 sec
        /// If there have been 50 attempts, the program exits
        /// </summary>
        /// <returns></returns>
        private async Task PasswordAttemptCheckerAsync()
        {
            switch (_attemptCounter)
            {
                case { } n when (n <= 5):
                    IsUnlockPasswordButtonEnabled = false;
                    await Task.Delay(TimeSpan.FromMilliseconds(650));
                    IsUnlockPasswordButtonEnabled = true;
                    break;
                case { } n when (n >= 5 && n <= 20):
                    IsUnlockPasswordButtonEnabled = false;
                    var wait = new Random().NextDouble() * new Random().Next(2,8) + 2;
                    await Task.Delay(TimeSpan.FromSeconds(wait));
                    IsUnlockPasswordButtonEnabled = true;
                    break;
                case { } n when (n > 20 && n <= 50):
                    IsUnlockPasswordButtonEnabled = false;
                    await Task.Delay(TimeSpan.FromSeconds(30));
                    IsUnlockPasswordButtonEnabled = true;
                    break;
                default:
                    Environment.Exit(0);
                    break;
            }

        }

        /// <summary>
        /// Validates the settings of the config file and the database
        /// </summary>
        private static void ValidateFiles()
        {
            var configFile = Path.Combine(App.ConfigDirPath, @"config\config.json");
            var database = Path.Combine(App.ConfigDirPath, App.DbPath);

            if (!File.Exists(configFile))
            {
                UserSettingsHelper.CreateDefaultConfig();
            }

            if (UserSettingsHelper.ValidateConfigFile()== false)
            {
                File.Delete(configFile);
                UserSettingsHelper.CreateDefaultConfig();
            }

            if (App.UserSettings.RequirePasswordEntry == true && !File.Exists(database))
            {
                File.Delete(configFile);
                App.UserSettings = UserSettingsHelper.ReadUserSettings();
            }

            if (CredentialManager.GetCredentials(App.CredFile) == null)
            {
                var cred = new NetworkCredential(Convert.ToBase64String(Secure.GenerateRandomSalt()), Secure.GenerateSecurePassword(64,16));
                CredentialManager.SaveCredentials(App.CredFile, cred);
            }

        }

    }

    public class SetEnterPasswordViewModelValidator: AbstractValidator<SetEnterPasswordViewModel>
    {
        public SetEnterPasswordViewModelValidator()
        {
            When(x => x.IsCreatePasswordVisible && x.RequirePasswordChecked && x.ShouldCheckPassword, () =>
            {

                RuleFor(x => x.Password).NotNull().WithMessage(App.ResMan.GetString("PasswordNoEmpty"));

                RuleFor(x => x.ConfirmPassword).Cascade(CascadeMode.Stop)
                    .NotNull().WithMessage(App.ResMan.GetString("PasswordNoEmpty"))
                    .Must(DoPasswordsMatch).WithMessage(App.ResMan.GetString("PasswordsNoMatch"));

            });

            When(x => x.IsUnlockPasswordVisible && x.ShouldCheckPassword, () =>
            {
                RuleFor(x => x.Password).Cascade(CascadeMode.Stop)
                    .Must(IsNoDbError).WithMessage(CreateDbErrorMessage())
                    .Must(DoPasswordsMatch).WithMessage(App.ResMan.GetString("PassIncorrect"));

            });
        }


        /// <summary>
        /// Checks if first password and the confirmation password match
        /// </summary>
        /// <param name="securePass"></param>
        /// <returns></returns>
        private bool DoPasswordsMatch(SecureString securePass)
        {
            var cred = CredentialManager.GetCredentials(App.CredDb);
            if (cred == null|| cred.UserName.Length <1) return false;
            var split = cred.UserName.Split('|');
            var hashSalt = new  Secure.PassHash
            {
                Hash = split[0], Salt = split[1]
            };
            bool result = Secure.ValidatePassword(securePass, hashSalt.Salt, hashSalt.Hash);
            return result;
        }

        
        /// <summary>
        /// Checks if trying to open the database generates an error
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        private static bool IsNoDbError(SecureString password)
        {
            try
            {
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1) return false;
                using (_ = new LiteDatabase($"Filename={Path.Combine(App.ConfigDirPath, App.DbPath)};Password={cred.Password}"))
                {
                    //test if the database can be opened
                }

                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (LiteException)
            {
                return false;
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex,"DidEnterWithRightPassword an unknown error occurred");
                return false;
            }
        }

        /// <summary>
        /// Tries to create the database, and generates a validation error if it fails
        /// </summary>
        /// <returns></returns>
        private static string CreateDbErrorMessage()
        {
            try
            {
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1) return App.ResMan.GetString("PasswordNoEmpty");
                using (_ = new LiteDatabase($"Filename={Path.Combine(App.ConfigDirPath, App.DbPath)};Password={cred.Password}"))
                {
                    //test if the database can be opened
                }
                return String.Empty;
            }
            catch (IOException)
            {
                return App.ResMan.GetString("DbIsLockedProc");
            }
            catch (LiteException ex)
            {
                return ex.Message == "Invalid password" ? App.ResMan.GetString("PassIncorrect") : ex.Message;
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "CreateDbErrorMessage an unknown error occurred");
                return App.ResMan.GetString("UnknownException");
            }
        }
    }



}
