using System;
using HRM.DAL;

namespace HRM.BUS
{
    public static class ErrorHelper
    {
        public static string ToMessage(Exception ex)
        {
            return DBHelper.FriendlyError(ex);
        }
    }
}
