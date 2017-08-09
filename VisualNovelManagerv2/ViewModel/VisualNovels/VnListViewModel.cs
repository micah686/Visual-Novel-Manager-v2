﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using FirstFloor.ModernUI.Presentation;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using VisualNovelManagerv2.CustomClasses;
using VisualNovelManagerv2.CustomClasses.Vndb;
using VisualNovelManagerv2.Design.VisualNovel;
using VndbSharp;
using VndbSharp.Models;

namespace VisualNovelManagerv2.ViewModel.VisualNovels
{
    public class VnListViewModel: ViewModelBase
    {
        #region Properties

        #region VotelistCollection
        private ObservableCollection<string> _userListCollection= new ObservableCollection<string>();
        public ObservableCollection<string> UserListCollection
        {
            get { return _userListCollection; }
            set
            {
                _userListCollection = value;
                RaisePropertyChanged(nameof(UserListCollection));
            }
        }        
        #endregion

        #region Username
        private string _username;
        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                RaisePropertyChanged(nameof(Username));
            }
        }
        #endregion

        #region Password
        private SecureString _password = new SecureString();
        public SecureString Password
        {
            get { return _password; }
            set
            {
                _password = value;
                RaisePropertyChanged(nameof(Password));
            }
        }
        #endregion

        #region SelectedIndex
        private int _selectedIndex = 0;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;
                RaisePropertyChanged(nameof(SelectedIndex));
            }
        }
        #endregion

        #region SelectedItem
        private string _selectedItem;
        public string SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                RaisePropertyChanged(nameof(SelectedItem));
                if (value != null)
                {
                    BindImage();
                }
            }
        }
        #endregion

        #region IsVoteListSelected
        private bool _isVoteListSelected;
        public bool IsVoteListSelected
        {
            get { return _isVoteListSelected; }
            set
            {
                _isVoteListSelected = value;
                RaisePropertyChanged(nameof(IsVoteListSelected));
            }
        }
        #endregion

        #region IsWishlistSelected
        private bool _isWishlistSelected;
        public bool IsWishlistSelected
        {
            get { return _isWishlistSelected; }
            set
            {
                _isWishlistSelected = value;
                RaisePropertyChanged(nameof(IsWishlistSelected));
            }
        }
        #endregion

        #region IsVnListSelected
        private bool _isVnListSelected;
        public bool IsVnListSelected
        {
            get { return _isVnListSelected; }
            set
            {
                _isVnListSelected = value;
                RaisePropertyChanged(nameof(IsVnListSelected));
            }
        }
        #endregion

        #region VnLinksModel
        private VnLinksModel _vnLinksModel;
        public VnLinksModel VnLinksModel
        {
            get { return _vnLinksModel; }
            set
            {
                _vnLinksModel = value;
                RaisePropertyChanged(nameof(VnLinksModel));
            }
        }
        #endregion

        private uint _userId = 0;

        #endregion

        public ICommand LoginCommand => new GalaSoft.MvvmLight.Command.RelayCommand(Login);

        public VnListViewModel()
        {
            _vnLinksModel = new VnLinksModel();
        }

        private async void Login()
        {
            using (Vndb client = new Vndb(Username, Password))
            {
                var users = await client.GetUserAsync(VndbFilters.Username.Equals(Username));
                if (users != null)
                {
                    _userId = users.Items[0].Id;
                }
            }
            _userListCollection.Clear();
            //_userId = 7887;

            if(IsVoteListSelected)
                GetVoteList();
            else if(IsVnListSelected)
                GetVisualNovelList();
            else if(IsWishlistSelected)
                GetWishlist();
        }

        private async void GetVoteList()
        {
            if (string.IsNullOrEmpty(_username) || _password.Length <= 0) return;
            using (Vndb client = new Vndb(Username, Password))
            {
                bool hasMore = true;
                RequestOptions ro = new RequestOptions();
                int page = 1;
                List<UInt32> idList = new List<uint>();
                //get the list of all ids on the votelist
                while (hasMore)
                {
                    ro.Page = page;
                    var voteList = await client.GetVoteListAsync(VndbFilters.UserId.Equals(_userId), VndbFlags.FullVotelist, ro);
                    hasMore = voteList.HasMore;
                    idList.AddRange(voteList.Select(wish => wish.Id));
                    page++;
                }
                //get names from ids on votelist, and add them to ObservableCollection
                hasMore = true;
                page = 1;
                while (hasMore)
                {
                    ro.Count = 25;
                    ro.Page = page;
                    var data = await client.GetVisualNovelAsync(VndbFilters.Id.Equals(idList.ToArray()), VndbFlags.Basic, ro);
                    hasMore = data.HasMore;
                    foreach (var item in data)
                    {
                        _userListCollection.Add(item.Name);
                    }
                    page++;
                }
                client.Dispose();
            }
        }

        private async void GetWishlist()
        {
            if (!string.IsNullOrEmpty(_username) && _password.Length > 0)
            {
                using (Vndb client = new Vndb(Username,Password))
                {
                    bool hasMore = true;
                    RequestOptions ro = new RequestOptions();
                    int page = 1;
                    List<UInt32> idList= new List<uint>();
                    //get the list of all ids on the wishlist
                    while (hasMore)
                    {
                        ro.Page = page;
                        var wishlist = await client.GetWishlistAsync(VndbFilters.UserId.Equals(_userId), VndbFlags.FullWishlist, ro);
                        hasMore = wishlist.HasMore;
                        idList.AddRange(wishlist.Select(wish => wish.Id));
                        page++;
                    }
                    //get names from ids on wishlist, and add them to ObservableCollection
                    hasMore = true;
                    page = 1;
                    while (hasMore)
                    {
                        ro.Count = 25;
                        ro.Page = page;
                        var data = await client.GetVisualNovelAsync(VndbFilters.Id.Equals(idList.ToArray()), VndbFlags.Basic, ro);
                        hasMore = data.HasMore;
                        foreach (var item in data)
                        {
                            _userListCollection.Add(item.Name);
                        }
                        page++;
                    }
                    client.Dispose();
                }
            }
        }

        private async void GetVisualNovelList()
        {
            //if (string.IsNullOrEmpty(_username) || _password.Length <= 0) return;
            using (Vndb client = new Vndb(true))
            {
                bool hasMore = true;
                RequestOptions ro = new RequestOptions();
                int page = 1;
                List<UInt32> idList = new List<uint>();
                //get the list of all ids on the vnList
                int errorCounter = 0;
                while (hasMore)
                {
                    ro.Page = page;
                    ro.Count = 10;
                    var vnList = await client.GetVisualNovelListAsync(VndbFilters.UserId.Equals(_userId), VndbFlags.FullVisualNovelList, ro);
                    if (vnList != null)
                    {
                        hasMore = vnList.HasMore;
                        idList.AddRange(vnList.Select(wish => wish.Id));
                        page++;
                    }
                    else
                    {
                        HandleError.HandleErrors(client.GetLastError(), errorCounter);
                        errorCounter++;
                    }
                    
                }
                //get names from ids on vnlist, and add them to ObservableCollection
                hasMore = true;
                page = 1;
                while (hasMore)
                {
                    ro.Count = 25;
                    ro.Page = page;
                    var data = await client.GetVisualNovelAsync(VndbFilters.Id.Equals(idList.ToArray()), VndbFlags.Basic, ro);
                    hasMore = data.HasMore;
                    foreach (var item in data)
                    {
                        _userListCollection.Add(item.Name);
                    }
                    page++;
                }
                client.Dispose();
            }
        }

        private async void BindImage()
        {
            try
            {
                using (Vndb client = new Vndb())
                {
                    var data = await client.GetVisualNovelAsync(VndbFilters.Title.Equals(SelectedItem), VndbFlags.Details);
                    if (data != null)
                    {
                        var id = data.Items[0].Id;
                        if (!File.Exists($@"{Globals.DirectoryPath}\Data\images\userlist\{id}.jpg"))
                        {
                            Globals.StatusBar.IsDownloading = true;
                            Thread.Sleep(1500);
                            WebClient webclient = new WebClient();
                            webclient.DownloadFile(new Uri(data.Items[0].Image), $@"{Globals.DirectoryPath}\Data\images\userlist\{id}.jpg");
                            webclient.Dispose();
                            VnLinksModel.Image = new BitmapImage(new Uri($@"{Globals.DirectoryPath}\Data\images\userlist\{id}.jpg"));
                        }
                        else if (File.Exists($@"{Globals.DirectoryPath}\Data\images\userlist\{id}.jpg"))
                        {
                            VnLinksModel.Image = new BitmapImage(new Uri($@"{Globals.DirectoryPath}\Data\images\userlist\{id}.jpg"));
                        }
                    }                    
                    client.Dispose();
                }
            }
            catch (Exception e)
            {
                DebugLogging.WriteDebugLog(e);
                throw;
            }
        }
    }
}
