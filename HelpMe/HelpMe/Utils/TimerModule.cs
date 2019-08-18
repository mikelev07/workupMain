using HelpMe.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Web;

namespace HelpMe.Utils
{
   
    public class TimerModule : IHttpModule
    {
        private static ApplicationDbContext db = new ApplicationDbContext();
       
        static Timer timer;
        long interval = 10000; //10 секунд
        static object synclock = new object();
        static bool sent = false;

        public void Init(HttpApplication app)
        {
            timer = new Timer(new TimerCallback(CloseBlockedTransaction), null, 0, interval);
        }

        private void CloseBlockedTransaction(object obj)
        {
            lock (synclock)
            {
                DateTime dd = DateTime.Now;
                if (dd.Hour == 20 && dd.Minute == 16 && sent == false)
                {
                    List<Transaction> listNoBlock = db.Transactions.Where(t => t.Status == TransactionStatus.Waiting).ToList();

                    foreach (Transaction t in listNoBlock)
                    {
                        bool checkDays = t.DateBlockEnd.Subtract(t.Date).TotalDays >= 0;
                        if (checkDays)
                        {
                            var wallet = t.ToUser.Wallets.FirstOrDefault().Summ;

                            t.Status = TransactionStatus.Success;
                            wallet += t.Price;
                            db.Entry(t).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }

                    sent = true;
                }
                else if (dd.Hour != 16 && dd.Minute != 53)
                {
                    sent = false;
                }
            }
        }
        public void Dispose()
        { }
    }
}