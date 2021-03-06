﻿// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AdysTech.CredentialManager;
using FluentValidation;
using LiteDB;
using MvvmDialogs;
using Sentry;
using Stylet;
using VndbSharp;
using VndbSharp.Models;
using VndbSharp.Models.Errors;
using VndbSharp.Models.VisualNovel;
using VnManager.Events;
using VnManager.Helpers;
using VnManager.Helpers.Vndb;
using VnManager.Models.Db;
using VnManager.Models.Db.User;

namespace VnManager.ViewModels.Dialogs.AddGameSources
{
    public class AddGameVndbViewModel: Screen
    {
        #region Properties

        internal readonly List<MultiExeGamePaths> ExeCollection = new List<MultiExeGamePaths>();
        internal VndbResponse<VisualNovel> VnNameList;
        public BindableCollection<string> SuggestedNamesCollection { get; private set; }

        public bool IsLockDown { get; set; } = false;

        public int VnId { get; set; }
        public string VnName { get; set; }
        private bool _isNameChecked;
        public bool IsNameChecked
        {
            get => _isNameChecked;
            set
            {
                VnIdOrName = value == false ? App.ResMan.GetString("Id") : App.ResMan.GetString("Name");
                SetAndNotify(ref _isNameChecked, value);
            }
        }

        public string VnIdOrName { get; set; }
        public string ExePath { get; set; }
        public string IconPath { get; set; }
        public string ExeArguments { get; set; }

        public bool CanChangeVnName { get; set; }
        public bool IsNameDropDownOpen { get; set; } = false;
        public int DropDownIndex { get; set; }
        public string SelectedName { get; set; }
        public bool IsSearchNameButtonEnabled { get; set; } = true;
        public bool IsResetNameButtonEnabled { get; set; } = true;
        public bool IsSearchingForNames { get; set; } = false;



        private ExeType _exeType;
        public ExeType ExeType
        {
            get => _exeType;
            set
            {
                SetAndNotify(ref _exeType, value);
                IsNotExeCollection = _exeType != ExeType.Collection;
            }
        }

        private bool _isNotExeCollection;
        public bool IsNotExeCollection
        {
            get { return _isNotExeCollection; }
            set
            {
                ExeArguments = string.Empty;
                IconPath = string.Empty;
                IsArgsChecked = false;
                IsIconChecked = false;
                SetAndNotify(ref _isNotExeCollection, value);
            }
        }


        private bool _isIconChecked;
        public bool IsIconChecked
        {
            get => _isIconChecked;
            set
            {
                SetAndNotify(ref _isIconChecked, value);
                if (_isIconChecked == false)
                {
                    IconPath = string.Empty;
                    ValidateAsync();
                }
            }
        }

        private bool _isArgsChecked;

        public bool IsArgsChecked
        {
            get => _isArgsChecked;
            set
            {
                SetAndNotify(ref _isArgsChecked, value);
                if (_isArgsChecked == false)
                {
                    ExeArguments = string.Empty;
                    ValidateAsync();
                }
            }
        }

        #endregion

        


        private readonly IWindowManager _windowManager;
        private readonly IDialogService _dialogService;
        private readonly IEventAggregator _events;
        private readonly Func<AddGameMultiViewModel> _addGameMultiFactory;
        public AddGameVndbViewModel(Func<AddGameMultiViewModel> addGameMulti, IWindowManager windowManager, IModelValidator<AddGameVndbViewModel> validator, 
            IDialogService dialogService, IEventAggregator events) : base(validator)
        {
            _addGameMultiFactory = addGameMulti;
            _windowManager = windowManager;
            _dialogService = dialogService;
            _events = events;
            IsNotExeCollection = true;
            CanChangeVnName = true;
            SuggestedNamesCollection = new BindableCollection<string>();
        }




        /// <summary>
        /// Search for name of entered text
        /// <see cref="SearchAsync"/>
        /// </summary>
        /// <returns></returns>
        public async Task SearchAsync()
        {
            if(await PreSearchCheckAsync() == false)
            {
                return;
            }

            try
            {
                using (Vndb client = new Vndb(true))
                {
                    var stopwatch = new Stopwatch();
                    bool shouldContinue = true;
                    var maxTime = TimeSpan.FromSeconds(45);
                    SuggestedNamesCollection.Clear();
                    IsSearchingForNames = true;

                    stopwatch.Start();
                    while (shouldContinue)
                    {
                        if (stopwatch.Elapsed > maxTime)
                        {
                            return;
                        }
                        shouldContinue = false;
                        VnNameList = await client.GetVisualNovelAsync(VndbFilters.Search.Fuzzy(VnName));
                        if (VnNameList.Count < 1 && client.GetLastError() == null)
                        {
                            //do nothing, needs to check for null
                        }
                        else if (VnNameList.Count < 1 && client.GetLastError().Type == ErrorType.Throttled)
                        {
                            await HandleVndbErrors.ThrottledWaitAsync((ThrottledError)client.GetLastError(), 0);
                            shouldContinue = true;
                        }
                        else if (VnNameList.Count < 1)
                        {
                            HandleVndbErrors.HandleErrors(client.GetLastError());
                            return;
                        }
                        else
                        {
                            List<string> nameList = IsJapaneseText(VnName) == true ? VnNameList.Select(item => item.OriginalName).ToList() : VnNameList.Select(item => item.Name).ToList();
                            SuggestedNamesCollection.AddRange(nameList.Where(x => !string.IsNullOrEmpty(x)).ToList());
                            IsNameDropDownOpen = true;
                            SelectedName = SuggestedNamesCollection.FirstOrDefault();
                        }
                    }
                }
                IsResetNameButtonEnabled = true;
                IsSearchingForNames = false;
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "Failed to search VnName");
                SentryHelper.SendException(ex, null, SentryLevel.Warning);
                IsSearchingForNames = false;
            }

        }

        /// <summary>
        /// Do a quick check of if it can connect to Vndb
        /// </summary>
        /// <returns></returns>
        private async Task<bool> PreSearchCheckAsync()
        {
            
            if (string.IsNullOrEmpty(VnName) || string.IsNullOrWhiteSpace(VnName))
            {
                return false;
            }
            if (VnName.Length <= 1)
            {
                return false;
            }
            CanChangeVnName = false;
            IsResetNameButtonEnabled = false;
            IsResetNameButtonEnabled = false;
            const int retryCount = 5;
            for (int i = 0; i < retryCount; i++)
            {
                if (VndbConnectionTest.VndbTcpSocketTest() == false)
                {
                    const int delay = 3500;
                    App.Logger.Warning("Could not connect to the Vndb API over SSL");
                    await Task.Delay(delay);
                }
                else
                {
                    return true;
                }

            }
            return false;
        }

        /// <summary>
        /// Resets the entered name
        /// <see cref="ResetName"/>
        /// </summary>
        public void ResetName()
        {
            IsNameDropDownOpen = false;
            SuggestedNamesCollection.Clear();
            CanChangeVnName = true;
            IsSearchNameButtonEnabled = true;
        }

        /// <summary>
        /// Browse for Exe
        /// <see cref="BrowseExe"/>
        /// </summary>
        public void BrowseExe()
        {
            ExePath = FileDialogHelper.BrowseExe(_dialogService, this);
        }

        /// <summary>
        /// Loads the form to have a collection of Exes
        /// <see cref="ManageExes"/>
        /// </summary>
        public void ManageExes()
        {
            try
            {
                var multiVm = _addGameMultiFactory();
                var result = _windowManager.ShowDialog(_addGameMultiFactory());
                if (result != null && (result == true && multiVm.GameCollection != null))
                {
                    ExeCollection.Clear();
                    ExeCollection.AddRange(from item in multiVm.GameCollection
                        select new MultiExeGamePaths { ExePath = item.ExePath, IconPath = item.IconPath, ArgumentsString = item.ArgumentsString });
                }
                multiVm.Remove();
            }
            catch (Exception e)
            {
                App.Logger.Error(e, "Failed to manage Exes");
                SentryHelper.SendException(e, null, SentryLevel.Warning);
            }
        }

        /// <summary>
        /// Browse Icon
        /// <see cref="BrowseIcon"/>
        /// </summary>
        public void BrowseIcon()
        {
            IconPath = FileDialogHelper.BrowseIcon(_dialogService, this);
        }

        /// <summary>
        /// Submit entry to add to the database
        /// <see cref="SubmitAsync"/>
        /// </summary>
        /// <returns></returns>
        public async Task SubmitAsync()
        {
            IsLockDown = true;
            var parent = (AddGameMainViewModel)Parent;
            parent.CanChangeSource = false;
            bool result = await ValidateAsync();
            if (result == true)
            {
                await SetGameDataEntryAsync();

                IsLockDown = false;
                parent.CanChangeSource = true;
                parent.RequestClose(true);
                _events.PublishOnUIThread(new UpdateEvent { ShouldUpdate = true }, EventChannels.RefreshGameGrid.ToString());
            }

            IsLockDown = false;
            parent.CanChangeSource = true;
        }

        private async Task SetGameDataEntryAsync()
        {
            await VndbAddGameHelper.SetGameDataEntryAsync(this);
        }
        

        
        
        public void Cancel()
        {
            ClearData();
            var parent = (AddGameMainViewModel)Parent;
            parent.RequestClose();
        }



        private static bool IsJapaneseText(string text)
        {
            Regex regex = new Regex(@"/[\u3000-\u303F]|[\u3040-\u309F]|[\u30A0-\u30FF]|[\uFF00-\uFFEF]|[\u4E00-\u9FAF]|[\u2605-\u2606]|[\u2190-\u2195]|\u203B/g");
            return regex.IsMatch(text);
        }

        private void ClearData()
        {
            IsLockDown = false;
            ExeCollection.Clear();
            SuggestedNamesCollection.Clear();
            VnId = 0;
            VnName= String.Empty;
            ExePath = String.Empty;
            IconPath = String.Empty;
            ExeArguments= String.Empty;
            ExeType = ExeType.Normal;
            IsIconChecked = false;
            IsArgsChecked = false;
        }
    }


    public class AddGameVndbViewModelValidator : AbstractValidator<AddGameVndbViewModel>
    {
        public AddGameVndbViewModelValidator()
        {

            //VnId
            RuleFor(x => x.VnId).Cascade(CascadeMode.Stop)
                .NotEmpty().When(x => x.IsNameChecked == false)
                .WithMessage(App.ResMan.GetString("ValidationVnIdNotAboveZero")).MustAsync(IsNotAboveMaxIdAsync)
                .When(x => x.IsNameChecked == false).WithMessage(App.ResMan.GetString("ValidationVnIdAboveMax"))
                .MustAsync(IsNotDeletedVnAsync).When(x => x.IsNameChecked == false)
                .WithMessage(App.ResMan.GetString("ValidationVnIdDoesNotExist"))
                .Must((x,y)=> IsNotDuplicateId(x.VnId, x.ExeType)).WithMessage(App.ResMan.GetString("VnIdAlreadyExistsInDb"));


                //Vn name validation
            When(x => x.IsNameChecked == true, () =>
            {
                RuleFor(x => x.CanChangeVnName).NotEqual(true).WithMessage(App.ResMan.GetString("ValidationVnNameSelection"));
                RuleFor(x => x.VnName).Cascade(CascadeMode.Stop).NotEmpty().When(x => x.CanChangeVnName == false)
                    .WithMessage(App.ResMan.GetString("ValidationVnNameEmpty"))
                    .Must(TrySetNameId).Unless(x => x.IsLockDown == false).WithMessage(App.ResMan.GetString("SetIdFromNameFail"));
            });




            //Exe Path Validation
            RuleFor(x => x.ExePath).Cascade(CascadeMode.Stop).ExeValidation();

            //Icon
            When(x => x.IsIconChecked == true, () =>
            {
                RuleFor(x => x.IconPath).Cascade(CascadeMode.Stop).IcoValidation();
            });


            //Arguments
            When(x => x.IsArgsChecked == true, () =>
            {
                RuleFor(x => x.ExeArguments).Cascade(CascadeMode.Stop).ArgsValidation();
            });

        }

        /// <summary>
        /// Checks if the the game is a deleted vn on Vndb.Org (Like The Witcher 3)
        /// </summary>
        /// <param name="inputid"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        private static async Task<bool> IsNotDeletedVnAsync(int inputid, CancellationToken cancellation)
        {
            try
            {
                using (Vndb client = new Vndb(true))
                {
                    uint vnid = Convert.ToUInt32(inputid); //17725 is a deleted vn
                    VndbResponse<VisualNovel> response = await client.GetVisualNovelAsync(VndbFilters.Id.Equals(vnid));
                    if (response != null)
                    {
                        return response.Count > 0;
                    }
                    else
                    {
                        HandleVndbErrors.HandleErrors(client.GetLastError());
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "check for deleted vn failed");
                SentryHelper.SendException(ex, null, SentryLevel.Warning);
                return false;
            }
        }

        /// <summary>
        /// Check if the submitted ID is above the max Vndb ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        private static async Task<bool> IsNotAboveMaxIdAsync(int id, CancellationToken cancellation)
        {
            try
            {
                using (Vndb client = new Vndb(true))
                {
                    RequestOptions ro = new RequestOptions { Reverse = true, Sort = "id", Count = 1 };
                    VndbResponse<VisualNovel> response = await client.GetVisualNovelAsync(VndbFilters.Id.GreaterThan(1), VndbFlags.Basic, ro);
                    if (response != null)
                    {
                        return id < response.Items[0].Id;
                    }

                    HandleVndbErrors.HandleErrors(client.GetLastError());
                    return false;
                }
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "Could not check max vndb id", ex.Data);
                SentryHelper.SendException(ex, null, SentryLevel.Warning);
                return false;
            }
        }

        /// <summary>
        /// Checks if the database already has the selected ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="exeType"></param>
        /// <returns></returns>
        private static bool IsNotDuplicateId(int id, ExeType exeType)
        {
            try
            {
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1)
                {
                    return false;
                }
                using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}'{cred.Password}'"))
                {
                    var dbUserData = db.GetCollection<UserDataGames>(DbUserData.UserData_Games.ToString()).Query()
                        .Where(x => x.SourceType == AddGameSourceType.Vndb).ToEnumerable();
                    switch (exeType)
                    {
                        case ExeType.Normal:
                        {
                            var count = dbUserData.Count(x => x.GameId == id);
                            return count <= 0;
                        }
                        case ExeType.Collection:
                        {
                            var count = dbUserData.Count(x => x.GameId == id);
                            return count <= 0;
                        }
                        case ExeType.Launcher:
                            return true;
                        default:
                            throw new ArgumentOutOfRangeException($"Unknown Enum Source");
                    }
                }
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, $"IsNotDuplicateId encountered an error");
                SentryHelper.SendException(ex, null, SentryLevel.Warning);
                throw;
            }
        }

        private static bool TrySetNameId(AddGameVndbViewModel instance, string name)
        {
            try
            {
                if (instance.VnIdOrName == App.ResMan.GetString("Name"))
                {
                    if (instance.VnNameList == null)
                    {
                        return false;
                    }
                    if (instance.VnNameList.Count < 0)
                    {
                        return false;
                    }
                    instance.VnId = (int)instance.VnNameList.Items[instance.DropDownIndex].Id;
                    if (instance.VnId == 0)
                    {
                        return false;
                    }
                    return true;
                }
                return true;
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex,"Failed to set ID from selected Vndb Name");
                SentryHelper.SendException(ex, null, SentryLevel.Warning);
                throw;
            }
        }
    }


}
