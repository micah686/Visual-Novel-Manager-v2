﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using VisualNovelManagerv2.EF.Context;

namespace VisualNovelManagerv2.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.0-rtm-26452");

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnCharacter.VnCharacter", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Aliases");

                    b.Property<string>("Birthday");

                    b.Property<string>("BloodType");

                    b.Property<int?>("Bust");

                    b.Property<int?>("CharacterId");

                    b.Property<string>("Description");

                    b.Property<string>("Gender");

                    b.Property<int?>("Height");

                    b.Property<int?>("Hip");

                    b.Property<string>("ImageLink");

                    b.Property<string>("Name");

                    b.Property<string>("Original");

                    b.Property<int?>("VnCharacterVnsId");

                    b.Property<uint?>("VnId");

                    b.Property<int?>("VnInfoId");

                    b.Property<int?>("Waist");

                    b.Property<int?>("Weight");

                    b.HasKey("Id");

                    b.HasIndex("VnCharacterVnsId");

                    b.HasIndex("VnInfoId");

                    b.ToTable("VnCharacter");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnCharacter.VnCharacterTraits", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("CharacterId");

                    b.Property<string>("SpoilerLevel");

                    b.Property<int?>("TraitId");

                    b.Property<string>("TraitName");

                    b.Property<int?>("VnCharacterId");

                    b.HasKey("Id");

                    b.HasIndex("VnCharacterId");

                    b.ToTable("VnCharacterTraits");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnCharacter.VnCharacterVns", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("CharacterId");

                    b.Property<int?>("ReleaseId");

                    b.Property<string>("Role");

                    b.Property<string>("SpoilerLevel");

                    b.Property<int?>("VnId");

                    b.HasKey("Id");

                    b.ToTable("VnCharacterVns");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnInfo.VnInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Aliases");

                    b.Property<string>("Description");

                    b.Property<string>("ImageLink");

                    b.Property<string>("ImageNsfw");

                    b.Property<string>("Languages");

                    b.Property<string>("Length");

                    b.Property<string>("Original");

                    b.Property<string>("OriginalLanguage");

                    b.Property<string>("Platforms");

                    b.Property<double?>("Popularity");

                    b.Property<int?>("Rating");

                    b.Property<string>("Released");

                    b.Property<string>("Title");

                    b.Property<uint>("VnId");

                    b.Property<int?>("VnInfoAnimeId");

                    b.Property<int?>("VnInfoLinksId");

                    b.Property<int?>("VnInfoRelationsId");

                    b.Property<int?>("VnInfoScreensId");

                    b.Property<int?>("VnInfoStaffId");

                    b.HasKey("Id");

                    b.HasIndex("VnInfoAnimeId");

                    b.HasIndex("VnInfoLinksId");

                    b.HasIndex("VnInfoRelationsId");

                    b.HasIndex("VnInfoScreensId");

                    b.HasIndex("VnInfoStaffId");

                    b.ToTable("VnInfo");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnInfo.VnInfoAnime", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("AniDbId");

                    b.Property<string>("AniNfoId");

                    b.Property<string>("AnimeType");

                    b.Property<int?>("AnnId");

                    b.Property<string>("TitleEng");

                    b.Property<string>("TitleJpn");

                    b.Property<int?>("VnId");

                    b.Property<string>("Year");

                    b.HasKey("Id");

                    b.ToTable("VnInfoAnime");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnInfo.VnInfoLinks", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Encubed");

                    b.Property<string>("Renai");

                    b.Property<int?>("VnId");

                    b.Property<string>("Wikipedia");

                    b.HasKey("Id");

                    b.ToTable("VnInfoLinks");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnInfo.VnInfoRelations", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Official");

                    b.Property<string>("Original");

                    b.Property<string>("Relation");

                    b.Property<int?>("RelationId");

                    b.Property<string>("Title");

                    b.Property<int?>("VnId");

                    b.HasKey("Id");

                    b.ToTable("VnInfoRelations");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnInfo.VnInfoScreens", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("Height");

                    b.Property<string>("ImageUrl");

                    b.Property<string>("Nsfw");

                    b.Property<string>("ReleaseId");

                    b.Property<int?>("VnId");

                    b.Property<int?>("VnInfoId");

                    b.Property<int?>("Width");

                    b.HasKey("Id");

                    b.HasIndex("VnInfoId");

                    b.ToTable("VnInfoScreens");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnInfo.VnInfoStaff", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("AliasId");

                    b.Property<string>("Name");

                    b.Property<string>("Note");

                    b.Property<string>("Original");

                    b.Property<string>("Role");

                    b.Property<int?>("StaffId");

                    b.Property<int?>("VnId");

                    b.HasKey("Id");

                    b.ToTable("VnInfoStaff");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnInfo.VnInfoTags", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<double>("Score");

                    b.Property<string>("Spoiler");

                    b.Property<int?>("TagId");

                    b.Property<string>("TagName");

                    b.Property<int?>("VnId");

                    b.Property<int?>("VnInfoId");

                    b.HasKey("Id");

                    b.HasIndex("VnInfoId");

                    b.ToTable("VnInfoTags");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnOther.Categories", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Category");

                    b.Property<string>("Created");

                    b.HasKey("Id");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnOther.VnIdList", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Title");

                    b.Property<uint>("VnId");

                    b.HasKey("Id");

                    b.ToTable("VnIdList");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnOther.VnUserData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ExePath");

                    b.Property<string>("IconPath");

                    b.Property<string>("LastPlayed");

                    b.Property<string>("PlayTime");

                    b.Property<uint?>("VnId");

                    b.HasKey("Id");

                    b.ToTable("VnUserData");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnOther.VnUserDataCategories", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("Category");

                    b.Property<int?>("VnId");

                    b.HasKey("Id");

                    b.ToTable("VnUserDataCategories");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnProducer.VnProducer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Aliases");

                    b.Property<string>("Description");

                    b.Property<string>("Language");

                    b.Property<string>("Name");

                    b.Property<string>("Original");

                    b.Property<int?>("ProducerId");

                    b.Property<string>("ProducerType");

                    b.Property<int?>("VnProducerLinksId");

                    b.Property<int?>("VnProducerRelationsId");

                    b.HasKey("Id");

                    b.HasIndex("VnProducerLinksId");

                    b.HasIndex("VnProducerRelationsId");

                    b.ToTable("VnProducer");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnProducer.VnProducerLinks", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Homepage");

                    b.Property<int?>("ProducerId");

                    b.Property<string>("Wikipedia");

                    b.HasKey("Id");

                    b.ToTable("VnProducerLinks");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnProducer.VnProducerRelations", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<string>("Original");

                    b.Property<int?>("ProducerId");

                    b.Property<string>("Relation");

                    b.Property<int?>("RelationId");

                    b.HasKey("Id");

                    b.ToTable("VnProducerRelations");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnRelease.VnRelease", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Animation");

                    b.Property<string>("Catalog");

                    b.Property<string>("Doujin");

                    b.Property<string>("Freeware");

                    b.Property<string>("Gtin");

                    b.Property<string>("Languages");

                    b.Property<int?>("MinAge");

                    b.Property<string>("Notes");

                    b.Property<string>("Original");

                    b.Property<string>("Patch");

                    b.Property<string>("Platforms");

                    b.Property<int?>("ReleaseId");

                    b.Property<string>("ReleaseType");

                    b.Property<string>("Released");

                    b.Property<string>("Resolution");

                    b.Property<string>("Title");

                    b.Property<uint?>("VnId");

                    b.Property<int?>("VnInfoId");

                    b.Property<int?>("VnReleaseMediaId");

                    b.Property<int?>("VnReleaseProducersId");

                    b.Property<int?>("VnReleaseVnId");

                    b.Property<string>("Voiced");

                    b.Property<string>("Website");

                    b.HasKey("Id");

                    b.HasIndex("VnInfoId");

                    b.HasIndex("VnReleaseMediaId");

                    b.HasIndex("VnReleaseProducersId");

                    b.HasIndex("VnReleaseVnId");

                    b.ToTable("VnRelease");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnRelease.VnReleaseMedia", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Medium");

                    b.Property<int?>("Quantity");

                    b.Property<int?>("ReleaseId");

                    b.HasKey("Id");

                    b.ToTable("VnReleaseMedia");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnRelease.VnReleaseProducers", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Developer");

                    b.Property<string>("Name");

                    b.Property<string>("Original");

                    b.Property<int?>("ProducerId");

                    b.Property<string>("ProducerType");

                    b.Property<string>("Publisher");

                    b.Property<int?>("ReleaseId");

                    b.HasKey("Id");

                    b.ToTable("VnReleaseProducers");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnRelease.VnReleaseVn", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<string>("Original");

                    b.Property<int?>("ReleaseId");

                    b.Property<uint?>("VnId");

                    b.HasKey("Id");

                    b.ToTable("VnReleaseVn");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnStaff.VnStaff", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<string>("Gender");

                    b.Property<string>("Language");

                    b.Property<int?>("MainAlias");

                    b.Property<string>("Name");

                    b.Property<string>("Original");

                    b.Property<int?>("StaffId");

                    b.Property<int?>("VnStaffAliasesId");

                    b.Property<int?>("VnStaffLinksId");

                    b.HasKey("Id");

                    b.HasIndex("VnStaffAliasesId");

                    b.HasIndex("VnStaffLinksId");

                    b.ToTable("VnStaff");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnStaff.VnStaffAliases", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("AliasId");

                    b.Property<string>("Name");

                    b.Property<string>("Original");

                    b.Property<int?>("StaffId");

                    b.HasKey("Id");

                    b.ToTable("VnStaffAliases");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnStaff.VnStaffLinks", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AniDb");

                    b.Property<string>("Homepage");

                    b.Property<int?>("StaffId");

                    b.Property<string>("Twitter");

                    b.Property<string>("Wikipedia");

                    b.HasKey("Id");

                    b.ToTable("VnStaffLinks");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnTagTrait.VnTagData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Aliases");

                    b.Property<string>("Cat");

                    b.Property<string>("Description");

                    b.Property<string>("Meta");

                    b.Property<string>("Name");

                    b.Property<string>("Parents");

                    b.Property<int?>("TagId");

                    b.Property<int?>("Vns");

                    b.HasKey("Id");

                    b.ToTable("VnTagData");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnTagTrait.VnTraitData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Aliases");

                    b.Property<int?>("Chars");

                    b.Property<string>("Description");

                    b.Property<string>("Meta");

                    b.Property<string>("Name");

                    b.Property<string>("Parents");

                    b.Property<int?>("TraitId");

                    b.HasKey("Id");

                    b.ToTable("VnTraitData");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnUserList.VnVisualNovelList", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Added");

                    b.Property<string>("Notes");

                    b.Property<string>("Status");

                    b.Property<uint>("UserId");

                    b.Property<uint>("VnId");

                    b.HasKey("Id");

                    b.ToTable("VnVisualNovelList");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnUserList.VnVoteList", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Added");

                    b.Property<uint>("UserId");

                    b.Property<uint>("VnId");

                    b.Property<int>("Vote");

                    b.HasKey("Id");

                    b.ToTable("VnVoteList");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnUserList.VnWishList", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Added");

                    b.Property<string>("Priority");

                    b.Property<uint>("UserId");

                    b.Property<uint>("VnId");

                    b.HasKey("Id");

                    b.ToTable("VnWishList");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnCharacter.VnCharacter", b =>
                {
                    b.HasOne("VisualNovelManagerv2.EF.Entity.VnCharacter.VnCharacterVns", "VnCharacterVns")
                        .WithMany()
                        .HasForeignKey("VnCharacterVnsId");

                    b.HasOne("VisualNovelManagerv2.EF.Entity.VnInfo.VnInfo")
                        .WithMany("VnCharacters")
                        .HasForeignKey("VnInfoId");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnCharacter.VnCharacterTraits", b =>
                {
                    b.HasOne("VisualNovelManagerv2.EF.Entity.VnCharacter.VnCharacter")
                        .WithMany("VnCharacterTraits")
                        .HasForeignKey("VnCharacterId");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnInfo.VnInfo", b =>
                {
                    b.HasOne("VisualNovelManagerv2.EF.Entity.VnInfo.VnInfoAnime", "VnInfoAnime")
                        .WithMany()
                        .HasForeignKey("VnInfoAnimeId");

                    b.HasOne("VisualNovelManagerv2.EF.Entity.VnInfo.VnInfoLinks", "VnInfoLinks")
                        .WithMany()
                        .HasForeignKey("VnInfoLinksId");

                    b.HasOne("VisualNovelManagerv2.EF.Entity.VnInfo.VnInfoRelations", "VnInfoRelations")
                        .WithMany()
                        .HasForeignKey("VnInfoRelationsId");

                    b.HasOne("VisualNovelManagerv2.EF.Entity.VnInfo.VnInfoScreens", "VnInfoScreens")
                        .WithMany()
                        .HasForeignKey("VnInfoScreensId");

                    b.HasOne("VisualNovelManagerv2.EF.Entity.VnInfo.VnInfoStaff", "VnInfoStaff")
                        .WithMany()
                        .HasForeignKey("VnInfoStaffId");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnInfo.VnInfoScreens", b =>
                {
                    b.HasOne("VisualNovelManagerv2.EF.Entity.VnInfo.VnInfo")
                        .WithMany("VnInfoScreensCollection")
                        .HasForeignKey("VnInfoId");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnInfo.VnInfoTags", b =>
                {
                    b.HasOne("VisualNovelManagerv2.EF.Entity.VnInfo.VnInfo")
                        .WithMany("VnInfoTags")
                        .HasForeignKey("VnInfoId");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnProducer.VnProducer", b =>
                {
                    b.HasOne("VisualNovelManagerv2.EF.Entity.VnProducer.VnProducerLinks", "VnProducerLinks")
                        .WithMany()
                        .HasForeignKey("VnProducerLinksId");

                    b.HasOne("VisualNovelManagerv2.EF.Entity.VnProducer.VnProducerRelations", "VnProducerRelations")
                        .WithMany()
                        .HasForeignKey("VnProducerRelationsId");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnRelease.VnRelease", b =>
                {
                    b.HasOne("VisualNovelManagerv2.EF.Entity.VnInfo.VnInfo")
                        .WithMany("VnReleases")
                        .HasForeignKey("VnInfoId");

                    b.HasOne("VisualNovelManagerv2.EF.Entity.VnRelease.VnReleaseMedia", "VnReleaseMedia")
                        .WithMany()
                        .HasForeignKey("VnReleaseMediaId");

                    b.HasOne("VisualNovelManagerv2.EF.Entity.VnRelease.VnReleaseProducers", "VnReleaseProducers")
                        .WithMany()
                        .HasForeignKey("VnReleaseProducersId");

                    b.HasOne("VisualNovelManagerv2.EF.Entity.VnRelease.VnReleaseVn", "VnReleaseVn")
                        .WithMany()
                        .HasForeignKey("VnReleaseVnId");
                });

            modelBuilder.Entity("VisualNovelManagerv2.EF.Entity.VnStaff.VnStaff", b =>
                {
                    b.HasOne("VisualNovelManagerv2.EF.Entity.VnStaff.VnStaffAliases", "VnStaffAliases")
                        .WithMany()
                        .HasForeignKey("VnStaffAliasesId");

                    b.HasOne("VisualNovelManagerv2.EF.Entity.VnStaff.VnStaffLinks", "VnStaffLinks")
                        .WithMany()
                        .HasForeignKey("VnStaffLinksId");
                });
#pragma warning restore 612, 618
        }
    }
}
