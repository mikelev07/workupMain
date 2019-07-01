using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HelpMe.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string UserId { get; set; }
        public virtual User User { get; set; }
        public string OwnerId { get; set; }
        [ForeignKey("OwnerId")]
        public virtual User Owner { get; set; }
    }

    public class ReviewDashModel
    {
        public IEnumerable<Review> MyReviews { get; set; }
        public IEnumerable<Review> Reviews { get; set; }
    }

    public class ReviewIndexViewModel
    {
        public IEnumerable<Review> Reviews { get; set; }
        public PageInfo PageInfo { get; set; }
    }
}