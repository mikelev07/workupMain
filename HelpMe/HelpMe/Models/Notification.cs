using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HelpMe.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string UserName { get; set; }
        public string ExUserName { get; set; }
        public string Description { get; set; }
        public NotificationStatus Status { get; set; }
        public string UserId { get; set; }
        public virtual User User { get; set; }
    }

    public enum NotificationStatus
    {
        Unreading,
        Reading
    }
}