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
        public NotificationStatus Status { get; set; } 
    }

    public enum NotificationStatus
    {
        Unreading,
        Reading
    }
}