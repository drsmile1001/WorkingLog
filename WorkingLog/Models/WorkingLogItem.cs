using System;
using LiteDB;

namespace WorkingLog.Models
{
    public class WorkingLogItem
    {
        [BsonId]
        public string Id { get; set; }

        public string UserId { get; set; }

        public DateTime Date { get; set; }

        public string Project { get; set; }

        public string WorkItem { get; set; }

        public string Type { get; set; }

        public double Hours { get; set; }

        public string Detail { get; set; }
    }
}
