using System;

namespace SupportBank
{
    class Transaction
    {
        public DateTime Date { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Narrative { get; set; }
        public decimal Amount { get; set; }
    }
}