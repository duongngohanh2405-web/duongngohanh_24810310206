using System;
using RetailSales.DAL;

namespace RetailSales.BUS
{
    public static class ErrorHelper
    {
        public static string ToMessage(Exception ex)
        {
            return DBHelper.FriendlyError(ex);
        }
    }
}
