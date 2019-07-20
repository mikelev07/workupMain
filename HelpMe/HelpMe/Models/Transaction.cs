using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HelpMe.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        public string FromUserId { get; set; }
        public virtual User FromUser { get; set; }

        public string ToUserId { get; set; }
        public virtual User ToUser { get; set; }

        public int Price { get; set; }
        
        public int AttachId { get; set; }

        public string Status { get; set; }

    }
}