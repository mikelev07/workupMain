using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HelpMe.Models
{
    public class Portfolios
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public virtual User User { get; set; }
        public bool IsDownloaded { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DownloadDate { get; set; }
        public string AttachFileName { get; set; }
        public string AttachFileExtens { get; set; }
        public string AttachFilePath { get; set; }
        [NotMapped]
        public HttpPostedFileBase AttachFile { get; set; }
        public AttachStatus AttachStatus { get; set; }
    }
}