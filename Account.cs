using System;
using System.Collections.Generic;

namespace SupportBank
{
    class Account
    {
        public string Name { get; set; }
        public List<Transaction> IncomingTransactions { get; set; }
        public List<Transaction> OutgoingTransactions { get; set; }

        public Account(string name)
        {
            Name = name;
            IncomingTransactions = new List<Transaction>();
            OutgoingTransactions = new List<Transaction>();
        }
    }
}