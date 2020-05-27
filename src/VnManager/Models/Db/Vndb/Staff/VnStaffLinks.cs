﻿using LiteDB;

namespace VnManager.Models.Db.Vndb.Staff
{
    public class VnStaffLinks
    {
        [BsonId]
        public int Index { get; set; }
        public int? StaffId { get; set; }
        public string Homepage { get; set; }
        public string Wikidata { get; set; }
        public string Twitter { get; set; }
        public string AniDb { get; set; }
        public int? Pixiv { get; set; }
    }
}