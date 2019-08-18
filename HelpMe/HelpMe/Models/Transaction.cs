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

        public int CustomId { get; set; }

        public string FromUserId { get; set; }
        public User FromUser { get; set; }

        public string ToUserId { get; set; }
        public User ToUser { get; set; }

        public int Price { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        public int AttachId { get; set; }
        public int TimeBlock { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DateBlockEnd { get; set; }

        public TransactionStatus Status { get; set; }

    }

    public enum TransactionStatus
    {
        [Display(Name = "В ожидании")]
        Waiting,
        [Display(Name = "Успешная транзакция")]
        Success,
        [Display(Name = "Ошибка транзакции")]
        Error
    }
}