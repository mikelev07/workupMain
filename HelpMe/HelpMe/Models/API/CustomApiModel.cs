using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HelpMe.Models.API
{
    public class CustomApiModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public CustomStatus Status { get; set; }
        public bool? DoneInTime { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EndingDate { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ExecutorStartDate { get; set; }
        public string TypeTaskName { get; set; }
        public string CategoryTaskName { get; set; }
        public string SkillName { get; set; }
        public int Price { get; set; }
      
        public string UserName { get; set; }

        public string ExecutorName { get; set; }
       
        public int ExecutorPrice { get; set; }

        public bool IsRevision { get; set; }
    }
}