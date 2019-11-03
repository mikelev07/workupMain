using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HelpMe.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string DescriptionFrom { get; set; }
        public string DescriptionTo { get; set; }
        public string UserToId { get; set; }
        public string UserFromId { get; set; }
        public NotificationStatus Status { get; set; }
        public string UserId { get; set; }
        public virtual User User { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }
    }

    public class NotificationHubModel
    {
        public string DescriptionFrom { get; set; }
        public string DescriptionTo { get; set; }
        public string UserToId { get; set; }
        public string UserFromId { get; set; }
    }

    public enum NotificationStatus
    {
        Unreading,
        Reading
    }
}