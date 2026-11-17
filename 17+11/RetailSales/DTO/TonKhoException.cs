using System;

namespace RetailSales.DTO
{
    public class TonKhoException : Exception
    {
        public TonKhoException(string message) : base(message) { }
    }
}
