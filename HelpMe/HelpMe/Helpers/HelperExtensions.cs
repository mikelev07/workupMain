﻿using HelpMe.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;

namespace HelpMe.Helpers
{
    public static class HtmlExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()?
                            .GetMember(enumValue.ToString())?
                            .First()?
                            .GetCustomAttribute<DisplayAttribute>()?
                            .Name;
        }


        public static MvcHtmlString RawActionLink(this AjaxHelper ajaxHelper, string linkText, string actionName, string controllerName, object routeValues, AjaxOptions ajaxOptions, object htmlAttributes)
        {
            var repID = Guid.NewGuid().ToString();
            var lnk = ajaxHelper.ActionLink(repID, actionName, controllerName, routeValues, ajaxOptions, htmlAttributes);
            return MvcHtmlString.Create(lnk.ToString().Replace(repID, linkText));
        }

        //returns max price for customer's order
        public static int GetMaxCustomPrice()
        {
            return 50000;
        }


        public static int DoneInTimePercentage(int customsNumber, int doneInTimeNumber)
        {
            if (customsNumber > 0)
            {
                return (int)Math.Round((double)doneInTimeNumber / customsNumber * 100, 0) ;
            }
            return 0;
        }

        //5-star rating of user based on the number of likes and dislikes
        public static double FindUserRating(int likes, int dislikes)
        {
            var total = likes + dislikes;
            double rating = 0.0;

            if (total > 0)
            {
                rating = Math.Round((double)likes / total * 5, 1);
            }

            return rating;
        }

        //return 5-star rating in string format
        public static string FindUserRatingString(int likes, int dislikes)
        {
            return FindUserRating(likes, dislikes).ToString("F1").Replace(',', '.');
        }

        //percent rating of user
        public static int FindUserPercentRating(int likes, int dislikes)
        {
            int total = likes + dislikes;
            int rating = 0;
            if (total > 0)
            {
                rating = (int)Math.Round((double)likes / total * 100, 0);
            }

            return rating;
        }

        public static MvcHtmlString Nl2Br(this HtmlHelper htmlHelper, string text)
        {
            if (string.IsNullOrEmpty(text))
                return MvcHtmlString.Create(text);
            else
            {
                StringBuilder builder = new StringBuilder();
                string[] lines = text.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    if (i > 30)
                        builder.Append("<br/>");
                    builder.Append(HttpUtility.HtmlEncode(lines[i]));
                }
                return MvcHtmlString.Create(builder.ToString());
            }
        }

        public static MvcHtmlString EnumDisplayNameFor(this Enum item)
        {
            var type = item.GetType();
            var member = type.GetMember(item.ToString());
            DisplayAttribute displayName = (DisplayAttribute)member[0].GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault();

            if (displayName != null)
            {
                return new MvcHtmlString(displayName.Name);
            }

            return new MvcHtmlString(item.ToString());
        }

        public static MvcHtmlString PageLinks(this HtmlHelper html,
        PageInfo pageInfo, Func<int, string> pageUrl)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 1; i <= pageInfo.TotalPages; i++)
            {
                TagBuilder tag = new TagBuilder("a");
                TagBuilder tagLi = new TagBuilder("li");
                tag.MergeAttribute("href", pageUrl(i));
                tag.InnerHtml = i.ToString();
                // если текущая страница, то выделяем ее,
                // например, добавляя класс
                if (i == pageInfo.PageNumber)
                {
                    //tag.AddCssClass("ripple-effect current-page");
                    tag.AddCssClass("current-page");
                }
                tag.AddCssClass("ripple-effect");
                tagLi.InnerHtml = tag.ToString();
                result.Append(tagLi.ToString());
            }
            return MvcHtmlString.Create(result.ToString());
        }

        private static readonly long DatetimeMinTimeTicks = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Ticks;
        public static long ToJavaScriptMilliseconds(this DateTime dateTime)
        {
            return (long)((dateTime.ToUniversalTime().Ticks - DatetimeMinTimeTicks) / 10000);
        }
    }
}