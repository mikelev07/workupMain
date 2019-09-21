using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HelpMe.Models
{
    public class MessageStoreViewModel
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public DateTime DateSend { get; set; }
        public MessageStatus Status { get; set; }

        public string UserFromId { get; set; }
        public virtual User UserFrom { get; set; }

        public string UserToId { get; set; }
        [ForeignKey("UserToId")]
        public virtual User UserTo { get; set; }

        public int? ChatDialogId { get; set; }
        public virtual ChatDialog ChatDialog { get; set; }

        public ICollection<MessageAttach> MessageAttaches { get; set; }

        public MessageStoreViewModel()
        {
            MessageAttaches = new List<MessageAttach>();
        }

        public class Comparer : IEqualityComparer<MessageStoreViewModel>
        {
            public bool Equals(MessageStoreViewModel x, MessageStoreViewModel y)
            {
                return x.UserToId == y.UserToId;
            }

            public int GetHashCode(MessageStoreViewModel obj)
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 23 + (obj.UserToId ?? "").GetHashCode();
                    return hash;
                }
            }
        }

    }

    /// <summary>
    /// Для мульти-загрузки файлов
    /// </summary>
    public class MessageAttach
    {
        public int Id { get; set; }
        public string AttachUrl { get; set; }
        public string AttachName { get; set; }
        public int? MessageStoreViewModelId { get; set; }
        public virtual MessageStoreViewModel MessageStoreViewModel { get; set; }
    }

    public class ChatDialog
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DialogStatus Status { get; set; }
        public string UserFromId { get; set; }
        public virtual User UserFrom { get; set; }

        public string UserToId { get; set; }
        [ForeignKey("UserToId")]
        public virtual User UserTo { get; set; }

        public ICollection<MessageStoreViewModel> Messages { get; set; }

        public ChatDialog()
        {
            Messages = new List<MessageStoreViewModel>();
        }
    }

    public enum DialogStatus
    {
        [Display(Name = "Закрыт")]
        Close,
        [Display(Name = "Открыт")]
        Open

    }

    public enum MessageStatus
    {
        [Display(Name = "Прочитано")]
        Reading,
        [Display(Name = "Не прочитано")]
        Undreading
    }

}