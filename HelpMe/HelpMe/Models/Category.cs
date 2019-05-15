using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace HelpMe.Models
{
    public class TaskCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<CustomViewModel> Categories { get; set; }
        public ICollection<User> Users { get; set; }
        public ICollection<Skill> Skills { get; set; }

        public TaskCategory()
        {
            Categories = new List<CustomViewModel>();
            Users = new List<User>();
            Skills = new List<Skill>();
        }
    }

    ///<summary>
    ///Предметы в конкретной дисциплине
    ///</summary>
    public class Skill
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<User> Users { get; set; }
        public int? TaskCategoryId { get; set; }
        public TaskCategory TaskCategory { get; set; }
    }
}