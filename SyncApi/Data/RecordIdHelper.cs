using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using SyncApi.Models;

namespace SyncApi.Data
{
    public class RecordIdHelper
    {
        public static string CreateRecordId(ISyncItem item)
        {
            string uniqueTableColumnId = "";
            string uniqueTableColumnIdValue = "";

            if (item.TableName == "UserPreference")
            {
                UserPreferenceEntity castedItem = (UserPreferenceEntity)item;
                uniqueTableColumnId = "PreferenceName";
                uniqueTableColumnIdValue = castedItem.PreferenceName;
            }
            else if (item.TableName == "UserArticleStatus")
            {
                UserArticleStatusEntity castedItem = (UserArticleStatusEntity)item;
                uniqueTableColumnId = "ArticleCode";
                uniqueTableColumnIdValue = castedItem.ArticleCode;
            }
            else
            {
                //continue on
            }

            return item.CustomerId + "_" + uniqueTableColumnId + "_" + uniqueTableColumnIdValue;
        }
    }
}