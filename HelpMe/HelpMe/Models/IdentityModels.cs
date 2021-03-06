﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace HelpMe.Models
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string Username { get; set; }
        [NotMapped]
        public HttpPostedFileBase ImageFile { get; set; }
        public DateTime RegistrationDate { get; set; }
        public bool IsOnline { get; set; }
        public bool IsNotBusy { get; set; }
        public string ImagePath { get; set; } = "~/Content/Custom/images/user-avatar-big-01.jpg";
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public int PositiveThumbs { get; set; }
        public int NegativeThumbs { get; set; }
        public ICollection<TaskCategory> TaskCategories { get; set; }
        public ICollection<Skill> Skills { get; set; }
        public ICollection<CustomViewModel> Customs { get; set; }
        public ICollection<Review> Reviews { get; set; }
    }

    //public class UserProfile
    //{
    //    [Key]
    //    [ForeignKey("ApplicationUser")]
    //    public string Id { get; set; }
    //    public string Description { get; set; }
    //    public ApplicationUser User { get; set; }
    //}

    [System.Runtime.Serialization.KnownType(typeof(User))]
    [System.Runtime.Serialization.KnownType(typeof(IdentityUser))]
    public class User : IdentityUser
    {
        public string ImagePath { get; set; }
        [NotMapped]
        public HttpPostedFileBase ImageFile { get; set; }
        public DateTime RegistrationDate { get; set; }
        public bool IsOnline { get; set; }
        public bool IsNotBusy { get; set; }
        public string Status { get; set; }
        public int Age { get; set; } // добавляем свойство Age
        public string Description { get; set; }
        public string ConnectionId { get; set; } // для SignalR
        public int PositiveThumbs { get; set; }
        public int NegativeThumbs { get; set; }
        //public UserProfile Profile { get; set; }
        public ICollection<CustomViewModel> Customs { get; set; }
        public ICollection<TaskCategory> TaskCategories { get; set; }
        public ICollection<Skill> Skills { get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public ICollection<CommentViewModel> Comments { get; set; }
        public ICollection<MessageStoreViewModel> Messages { get; set; }
        public ICollection<ChatDialog> ChatDialogs { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Note> Notes { get; set; }
        public ICollection<Wallet> Wallets { get; set; } // создаем универсальную коллекцию
        public ICollection<AttachModel> Attachments { get; set; }
        public ICollection<MyAttachModel> MyAttachments { get; set; }
        public ICollection<MainAttachModel> MainAttachments { get; set; }
        public ICollection<Portfolios> Portfolios { get; set; }
        public User()
        {
            Notifications = new List<Notification>();
            Comments = new List<CommentViewModel>();
            Customs = new List<CustomViewModel>();
            TaskCategories = new List<TaskCategory>();
            Skills = new List<Skill>();
            Messages = new List<MessageStoreViewModel>();
            Reviews = new List<Review>();
            Portfolios = new List<Portfolios>();
            ChatDialogs = new List<ChatDialog>();
            Notes = new List<Note>();
            Attachments = new List<AttachModel>();
            MyAttachments = new List<MyAttachModel>();
            MainAttachments = new List<MainAttachModel>();
            Wallets = new List<Wallet>(); // добавляем список кошельков
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User> manager, string authenticationType)
        {
            // Обратите внимание, что authenticationType должен совпадать с типом, определенным в CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Здесь добавьте утверждения пользователя
            userIdentity.AddClaim(new Claim("avatar", this.ImagePath ?? "~/Content/Custom/images/user-avatar-big-02.jpg"));
            userIdentity.AddClaim(new Claim("status", this.IsNotBusy.ToString()));
            return userIdentity;
        }

    }

    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext()
            : base("DBConnection")
        {
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasMany(m => m.Customs).WithRequired(m => m.Executor);
            modelBuilder.Entity<User>().HasMany(m => m.Messages).WithRequired(m => m.UserTo);
            modelBuilder.Entity<User>().HasMany(m => m.ChatDialogs).WithRequired(m => m.UserTo);
            modelBuilder.Entity<User>().HasMany(m => m.Reviews).WithRequired(m => m.Owner);
            modelBuilder.Entity<User>().HasMany(m => m.TaskCategories).WithMany(m => m.Users)
                .Map(m => m.MapLeftKey("UserId")
                .MapRightKey("TaskCategoryId")
                .ToTable("UseCateg"));
            modelBuilder.Entity<User>().HasMany(m => m.Skills).WithMany(m => m.Users)
                .Map(m => m.MapLeftKey("UserId")
                .MapRightKey("SkillId")
                .ToTable("UserSkill"));

            //modelBuilder.ProxyCreationEnabled = false;

        }

        public DbSet<MessageAttach> MessageAttaches { get; set; }
        public DbSet<CustomViewModel> Customs { get; set; }
        public DbSet<AttachModel> Attachments { get; set; }
        public DbSet<Portfolios> Portfolioses { get; set; }
        public DbSet<MyAttachModel> MyAttachments { get; set; }
        public DbSet<MainAttachModel> MainAttachments { get; set; }
        public DbSet<CommentViewModel> Comments { get; set; }
        public DbSet<MessageStoreViewModel> Messages { get; set; }
        public DbSet<TypeCustomViewModel> CustomTypes { get; set; }
        public DbSet<TaskCategory> TaskCategories { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ChatDialog> ChatDialogs { get; set; }
        public DbSet<Wallet> Wallets { get; set; } // коллекция сущностей типа Wallet
        public DbSet<Transaction> Transactions { get; set; } 

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public System.Data.Entity.DbSet<HelpMe.Models.RoleViewModel> IdentityRoles { get; set; }

        public System.Data.Entity.DbSet<HelpMe.Models.UserViewModel> UserViewModels { get; set; }
    }
}