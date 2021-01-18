﻿using LiteDB;

namespace VnManager.Models.Db.Vndb.Release
{
    public class VnRelease
    {
        [BsonId]
        public int Index { get; set; }
        public uint? VnId { get; set; }
        public uint ReleaseId { get; set; }
        public string Title { get; set; }
        public string Original { get; set; }
        public string Released { get; set; }
        public string ReleaseType { get; set; }
        public bool Patch { get; set; }
        public bool Freeware { get; set; }
        public bool Doujin { get; set; }
        public string Languages { get; set; }
        public string Website { get; set; }
        public string Notes { get; set; }
        public byte MinAge { get; set; }
        public string Gtin { get; set; }
        public string Catalog { get; set; }
        public string Platforms { get; set; }
        public string Resolution { get; set; }
        public string Voiced { get; set; }
        //using a csv for animation
        public string Animation { get; set; }
    }
}
