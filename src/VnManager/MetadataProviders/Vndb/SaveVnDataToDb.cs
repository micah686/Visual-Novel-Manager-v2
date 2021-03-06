﻿// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AdysTech.CredentialManager;
using LiteDB;
using Sentry;
using VndbSharp.Models.Character;
using VndbSharp.Models.VisualNovel;
using VnManager.Converters;
using VnManager.Helpers;
using VnManager.Models.Db;
using VnManager.Models.Db.Vndb.Character;
using VnManager.Models.Db.Vndb.Main;
using VnManager.ViewModels;
using VnManager.ViewModels.UserControls;

namespace VnManager.MetadataProviders.Vndb
{
    public static class SaveVnDataToDb
    {
        /// <summary>
        /// Sort all of the VnInfo and save it to the database
        /// </summary>
        /// <param name="vn"></param>
        /// <param name="character"></param>
        /// <param name="increment"></param>
        /// <param name="currentProgress"></param>
        /// <param name="isRepairing"></param>
        /// <returns></returns>
        public static async Task SortVnInfoAsync(VisualNovel vn,  ICollection<Character> character, double increment, double currentProgress, bool isRepairing)
        {
            if (vn == null)
            {
                StatusBarViewModel.ResetValues();
                return;
            }

            try
            {
                var currentValue = currentProgress;

                SaveVnInfo(vn);
                currentValue += increment;
                RootViewModel.StatusBarPage.ProgressBarValue = currentValue;

                SaveVnCharacters(character, vn.Id);
                currentValue += increment;
                RootViewModel.StatusBarPage.ProgressBarValue = currentValue;

                await DownloadVndbContent.DownloadCoverImageAsync(vn.Id);
                currentValue += increment;
                RootViewModel.StatusBarPage.ProgressBarValue = currentValue;

                await DownloadVndbContent.DownloadCharacterImagesAsync(vn.Id);
                currentValue += increment;
                RootViewModel.StatusBarPage.ProgressBarValue = currentValue;
                await DownloadVndbContent.DownloadScreenshotsAsync(vn.Id);
                currentValue += increment;

                RootViewModel.StatusBarPage.ProgressBarValue = currentValue;
                RootViewModel.StatusBarPage.IsFileDownloading = false;

                if (isRepairing || !App.DidDownloadTagTraitDump)
                {
                    RootViewModel.StatusBarPage.IsDatabaseProcessing = true;
                    await DownloadVndbContent.GetAndSaveTagDumpAsync();
                    currentValue += increment;
                    RootViewModel.StatusBarPage.ProgressBarValue = currentValue;

                    await DownloadVndbContent.GetAndSaveTraitDumpAsync();
                    currentValue += increment;
                    RootViewModel.StatusBarPage.ProgressBarValue = currentValue;
                }
            }
            catch (Exception e)
            {
                App.Logger.Error(e, "Failed to Sort VnInfo");
                SentryHelper.SendException(e, null, SentryLevel.Warning);
                StatusBarViewModel.ResetValues();
            }
            finally
            {
                StatusBarViewModel.ResetValues();
            }
            
            
            
        }


        #region VnInfo
        /// <summary>
        /// Save the Main VnInfo to the database
        /// </summary>
        /// <param name="visualNovel"></param>
        public static void SaveVnInfo(VisualNovel visualNovel)
        {
            if (visualNovel == null)
            {
                return;
            }
            try
            {
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1)
                {
                    return;
                }
                using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}'{cred.Password}'"))
                {
                    var dbVnInfo = db.GetCollection<VnInfo>(DbVnInfo.VnInfo.ToString());
                    var dbVnInfoLinks = db.GetCollection<VnInfoLinks>(DbVnInfo.VnInfo_Links.ToString());
                    ILiteCollection<VnInfoScreens> dbVnInfoScreens = db.GetCollection<VnInfoScreens>(DbVnInfo.VnInfo_Screens.ToString());
                    ILiteCollection<VnInfoRelations> dbVnInfoRelations = db.GetCollection<VnInfoRelations>(DbVnInfo.VnInfo_Relations.ToString());
                    ILiteCollection<VnInfoTags> dbVnInfoTags = db.GetCollection<VnInfoTags>(DbVnInfo.VnInfo_Tags.ToString());

                    var prevVnInfo = dbVnInfo.Query().Where(x => x.VnId == visualNovel.Id).FirstOrDefault();
                    var prevVnInfoLinks = dbVnInfoLinks.Query().Where(x => x.VnId == visualNovel.Id).FirstOrDefault();

                    List<VnInfoTags> vnTags = new List<VnInfoTags>();
                    List<VnInfoRelations> vnRelations = new List<VnInfoRelations>();
                    List<VnInfoScreens> vnScreenshot = new List<VnInfoScreens>();

                    var vn = prevVnInfo ?? new VnInfo();

                    vn.VnId = visualNovel.Id;
                    vn.Title = visualNovel.Name;
                    vn.Original = visualNovel.OriginalName;
                    vn.Released = visualNovel.Released?.ToString();
                    vn.Languages = CsvConverter.ConvertToCsv(visualNovel.Languages);
                    vn.OriginalLanguages = CsvConverter.ConvertToCsv(visualNovel.OriginalLanguages);
                    vn.Platforms = CsvConverter.ConvertToCsv(visualNovel.Platforms);
                    vn.Aliases = CsvConverter.ConvertToCsv(visualNovel.Aliases);
                    vn.Length = visualNovel.Length?.ToString();
                    vn.Description = visualNovel.Description;
                    vn.ImageLink = !string.IsNullOrEmpty(visualNovel.Image) ? visualNovel.Image : string.Empty;
                    vn.ImageRating = visualNovel.ImageRating;
                    vn.Popularity = visualNovel.Popularity;
                    vn.Rating = visualNovel.Rating;


                    //links
                    VnInfoLinks vnLinks = prevVnInfoLinks ?? new VnInfoLinks();
                    vnLinks.VnId = visualNovel.Id;
                    vnLinks.Wikidata = visualNovel.VisualNovelLinks.Wikidata;

                    //screenshot
                    vnScreenshot.AddRange(FormatVnInfoScreens(visualNovel, dbVnInfoScreens));

                    //relations
                    vnRelations.AddRange(FormatVnInfoRelations(visualNovel, dbVnInfoRelations));


                    //tags
                    vnTags.AddRange(FormatVnInfoTags(visualNovel, dbVnInfoTags));

                    dbVnInfo.Upsert(vn);
                    dbVnInfoLinks.Upsert(vnLinks);
                    dbVnInfoScreens.Upsert(vnScreenshot);
                    dbVnInfoRelations.Upsert(vnRelations);
                    dbVnInfoTags.Upsert(vnTags);
                }
            }
            catch (IOException e)
            {
                App.Logger.Error(e, "an I/O exception occurred");
                SentryHelper.SendException(e, null, SentryLevel.Error);
            }
            catch (Exception e)
            {
                App.Logger.Error(e, "Failed to save VnInfo");
                SentryHelper.SendException(e, null, SentryLevel.Error);
            }
        }

        /// <summary>
        /// Save the Screenshot info of VnInfo to db
        /// </summary>
        /// <param name="visualNovel"></param>
        /// <param name="dbVnInfoScreens"></param>
        /// <returns></returns>
        private static List<VnInfoScreens> FormatVnInfoScreens(VisualNovel visualNovel, ILiteCollection<VnInfoScreens> dbVnInfoScreens)
        {
            try
            {
                List<VnInfoScreens> vnScreenshot = new List<VnInfoScreens>();
                if (visualNovel.Screenshots.Count > 0)
                {
                    var prevVnInfoScreens = dbVnInfoScreens.Query().Where(x => x.VnId == visualNovel.Id).ToList();
                    foreach (var screenshot in visualNovel.Screenshots)
                    {
                        var entry = prevVnInfoScreens.FirstOrDefault(x => x.ImageLink == screenshot.Url) ??
                                    new VnInfoScreens();
                        entry.VnId = visualNovel.Id;
                        entry.ImageLink = !string.IsNullOrEmpty(screenshot.Url) ? screenshot.Url : string.Empty;
                        entry.ReleaseId = screenshot.ReleaseId;
                        entry.Height = screenshot.Height;
                        entry.Width = screenshot.Width;
                        entry.ImageRating = screenshot.ImageRating;
                        vnScreenshot.Add(entry);
                    }
                }
            
                return vnScreenshot;
            }
            catch (Exception e)
            {
                App.Logger.Error(e, "Failed to save VnInfo screens");
                SentryHelper.SendException(e, null, SentryLevel.Warning);
                return new List<VnInfoScreens>();
            }
        }

        /// <summary>
        /// Save list of VnInfo Relations
        /// </summary>
        /// <param name="visualNovel"></param>
        /// <param name="dbVnInfoRelations"></param>
        /// <returns></returns>
        private static List<VnInfoRelations> FormatVnInfoRelations(VisualNovel visualNovel, ILiteCollection<VnInfoRelations> dbVnInfoRelations)
        {
            try
            {
                List<VnInfoRelations> vnRelations = new List<VnInfoRelations>();
                if (visualNovel.Relations.Count > 0)
                {
                    var prevVnInfoRelations = dbVnInfoRelations.Query().Where(x => x.VnId == visualNovel.Id).ToList();
                    foreach (var relation in visualNovel.Relations)
                    {
                        var entry = prevVnInfoRelations.FirstOrDefault(x => x.RelationId == relation.Id) ??
                                    new VnInfoRelations();
                        entry.VnId = visualNovel.Id;
                        entry.RelationId = relation.Id;
                        entry.Relation = relation.Type.ToString();
                        entry.Title = relation.Title;
                        entry.Original = relation.Original;
                        entry.Official = relation.Official ? "Yes" : "No";
                        vnRelations.Add(entry);
                    }
                }
            
                return vnRelations;
            }
            catch (Exception e)
            {
                App.Logger.Error(e, "Failed to save VnInfo Relations");
                SentryHelper.SendException(e, null, SentryLevel.Warning);
                return new List<VnInfoRelations>();
            }
        }

        /// <summary>
        /// Save VnInfo Tags
        /// </summary>
        /// <param name="visualNovel"></param>
        /// <param name="dbVnInfoTags"></param>
        /// <returns></returns>
        private static List<VnInfoTags> FormatVnInfoTags(VisualNovel visualNovel, ILiteCollection<VnInfoTags> dbVnInfoTags)
        {
            try
            {
                List<VnInfoTags> vnTags = new List<VnInfoTags>();
                if (visualNovel.Tags.Count > 0)
                {
                    List<VnInfoTags> prevVnInfoTags = dbVnInfoTags.Query().Where(x => x.VnId == visualNovel.Id).ToList();
                    foreach (var tag in visualNovel.Tags)
                    {
                        var entry = prevVnInfoTags.FirstOrDefault(x => x.TagId == tag.Id) ?? new VnInfoTags();
                        entry.VnId = visualNovel.Id;
                        entry.TagId = tag.Id;
                        entry.Score = tag.Score;
                        entry.Spoiler = tag.SpoilerLevel;
                        vnTags.Add(entry);
                    }
                }

                return vnTags;
            }
            catch (Exception e)
            {
                App.Logger.Error(e, "Failed to save VnInfo Tags");
                SentryHelper.SendException(e, null, SentryLevel.Warning);
                return new List<VnInfoTags>();
            }
        }
        #endregion

        #region VnCharacters
        /// <summary>
        /// Save Vn Characters to db
        /// </summary>
        /// <param name="characters"></param>
        /// <param name="vnid"></param>
        public static void SaveVnCharacters(ICollection<Character> characters, uint vnid)
        {
            try
            {
                if (characters== null || characters.Count < 1)
                {
                    return;
                }
                var cred = CredentialManager.GetCredentials(App.CredDb);
                if (cred == null || cred.UserName.Length < 1)
                {
                    return;
                }
                using (var db = new LiteDatabase($"{App.GetDbStringWithoutPass}'{cred.Password}'"))
                {
                    var dbCharInfo = db.GetCollection<VnCharacterInfo>(DbVnCharacter.VnCharacter.ToString());
                    ILiteCollection<VnCharacterTraits> dbCharTraits = db.GetCollection<VnCharacterTraits>(DbVnCharacter.VnCharacter_Traits.ToString());

                    if (characters.Count > 0)
                    {
                        List<VnCharacterInfo> vnCharactersList = new List<VnCharacterInfo>();
                        List<VnCharacterTraits> vnCharacterTraitsList = new List<VnCharacterTraits>();
                        foreach (Character vnCharacter in characters)
                        {
                            var prevVnCharacter = dbCharInfo.Query().Where(x => x.CharacterId == vnCharacter.Id);
                            var character = prevVnCharacter.FirstOrDefault() ?? new VnCharacterInfo();

                            character.VnId = vnid;
                            character.CharacterId = vnCharacter.Id;
                            character.Name = vnCharacter.Name;
                            character.Original = vnCharacter.OriginalName;
                            character.Gender = vnCharacter.Gender.ToString();
                            character.BloodType = vnCharacter.BloodType.ToString();
                            character.Age = vnCharacter.Age.ToString();
                            character.Birthday = SimpleDateConverter.ConvertSimpleDate(vnCharacter.Birthday);
                            character.Aliases = CsvConverter.ConvertToCsv(vnCharacter.Aliases);
                            character.Description = vnCharacter.Description;
                            character.ImageLink = !string.IsNullOrEmpty(vnCharacter.Image) ? vnCharacter.Image : string.Empty;
                            character.ImageRating = vnCharacter.ImageRating;
                            character.Bust = Convert.ToInt32(vnCharacter.Bust, CultureInfo.InvariantCulture);
                            character.Waist = Convert.ToInt32(vnCharacter.Waist, CultureInfo.InvariantCulture);
                            character.Hip = Convert.ToInt32(vnCharacter.Hip, CultureInfo.InvariantCulture);
                            character.Height = Convert.ToInt32(vnCharacter.Height, CultureInfo.InvariantCulture);
                            character.Weight = Convert.ToInt32(vnCharacter.Weight, CultureInfo.InvariantCulture);
                            vnCharactersList.Add(character);

                            vnCharacterTraitsList.AddRange(FormatVnCharacterTraits(vnCharacter, dbCharTraits));
                        }

                        dbCharInfo.Upsert(vnCharactersList);
                        dbCharTraits.Upsert(vnCharacterTraitsList);
                    }
                }
            }
            catch (Exception e)
            {
                App.Logger.Error(e, "Failed to save VnCharacters");
                SentryHelper.SendException(e, null, SentryLevel.Error);
            }

        }

        /// <summary>
        /// Format Traits for usage
        /// </summary>
        /// <param name="vnCharacter"></param>
        /// <param name="dbCharTraits"></param>
        /// <returns></returns>
        private static List<VnCharacterTraits> FormatVnCharacterTraits(Character vnCharacter, ILiteCollection<VnCharacterTraits> dbCharTraits)
        {
            try
            {
                List<VnCharacterTraits> vnCharacterTraitsList = new List<VnCharacterTraits>();

                if (vnCharacter.Traits.Count <= 0)
                {
                    return vnCharacterTraitsList;
                }
                var prevVnCharacterTraits = dbCharTraits.Query().Where(x => x.CharacterId == vnCharacter.Id);
            
                foreach (var traits in vnCharacter.Traits)
                {
                    var entry = prevVnCharacterTraits.FirstOrDefault() ?? new VnCharacterTraits();
                    entry.CharacterId = vnCharacter.Id;
                    entry.TraitId = traits.Id;
                    entry.SpoilerLevel = traits.SpoilerLevel;
                    vnCharacterTraitsList.Add(entry);
                }

                return vnCharacterTraitsList;
            }
            catch (Exception e)
            {
                App.Logger.Error(e, "Failed to format VnTraits");
                SentryHelper.SendException(e, null, SentryLevel.Warning);
                return new List<VnCharacterTraits>();
            }
        }
        #endregion



    }
}
