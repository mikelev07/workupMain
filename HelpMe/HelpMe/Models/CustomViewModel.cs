﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Web;

namespace HelpMe.Models
{
    /// <summary>
    /// класс заказов
    /// </summary>
    public class CustomViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public CustomStatus Status { get; set; }
        public bool? DoneInTime { get; set; }
        public string FilePath { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EndingDate { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ExecutorStartDate { get; set; }
        public int? TypeTaskId { get; set; }
        public TypeCustomViewModel TypeTask { get; set; }
        public int? CategoryTaskId { get; set; }
        public TaskCategory CategoryTask { get; set; }
        public int? SkillId { get; set; }
        public Skill Skill { get; set; }
        public int Price { get; set; }
        public string AttachFilePath { get; set; }

        [NotMapped]
        public HttpPostedFileBase AttachFile { get; set; }

        public string UserId { get; set; }
        public virtual User User { get; set; }

        public string ExecutorId { get; set; }
        [ForeignKey("ExecutorId")]
        public virtual User Executor { get; set; }

        public int ExecutorPrice { get; set; }

        public bool IsRevision { get; set; }

        public ICollection<CommentViewModel> Comments { get; set; }
        public ICollection<AttachModel> Attachments { get; set; }
        public ICollection<MyAttachModel> MyAttachments { get; set; }

        [NotMapped]
        public TimeSpan TotalHrs
        {
            get
            { 
                return EndingDate - DateTime.Now;
            }
        }

        public CustomViewModel()
        {
            Comments = new List<CommentViewModel>();
            Attachments = new List<AttachModel>();
            MyAttachments = new List<MyAttachModel>();
        } 
    }

    public class MyAttachModel
    {
        public int Id { get; set; }
        public int? CustomViewModelId { get; set; }
        public CustomViewModel CustomViewModel { get; set; }
        public string UserId { get; set; }
        public virtual User User { get; set; }
        public string AttachFilePath { get; set; }
        [NotMapped]
        public HttpPostedFileBase AttachFile { get; set; }
        public AttachStatus AttachStatus { get; set; }
    }

    public class AttachModel
    {
        public int Id { get; set; }
        public int ExecutorPrice { get; set; }
        public int? CustomViewModelId { get; set; }
        public bool IsRevision { get; set; }
        public CustomViewModel CustomViewModel { get; set; }
        public string UserId { get; set; }
        public virtual User User { get; set; }
        public string AttachFileName { get; set; }
        public string AttachFileExtens { get; set; }
        public string AttachFilePath { get; set; }
        [NotMapped]
        public HttpPostedFileBase AttachFile { get; set; }
        public AttachStatus AttachStatus { get; set; }
    }

    public class PageInfo
    {
        public int PageNumber { get; set; } // номер текущей страницы
        public int PageSize { get; set; } // кол-во объектов на странице
        public int TotalItems { get; set; } // всего объектов
        public int TotalPages  // всего страниц
        {
            get { return (int)Math.Ceiling((decimal)TotalItems / PageSize); }
        }

        public bool HasPreviousPage { get { return PageNumber > 1 && TotalPages > 1; } }
        public int PreviousPageIndex { get { return PageNumber - 1; } }
        public bool HasNextPage { get { return PageNumber < TotalPages && TotalPages > 1; } }
        public int NextPageIndex { get { return PageNumber + 1; } }
    }
    public class CustomIndexViewModel
    {
        public IEnumerable<CustomViewModel> Customs { get; set; }
        public PageInfo PageInfo { get; set; }
    }

    public enum AttachStatus
    {
        [Display(Name = "Работа куплена")]
        Purchased,
        [Display(Name = "Не куплена")]
        NotPurchased
    }


    public enum CustomStatus
    {
        [Display(Name = "Открытая заявка")]
        Open,
        [Display(Name = "Закрытая заявка")]
        Close,
        [Display(Name = "Выполняется исполнителем")]
        Check,
        [Display(Name = "Ожидает покупки")]
        NeedBuy,
        [Display(Name = "Проверяется заказчиком")]
        CheckCustom,
        [Display(Name = "На доработке")]
        Revision
    }

  

}