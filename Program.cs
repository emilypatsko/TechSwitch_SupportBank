using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace SupportBank
{
    class Program
    {
        static void Main()
        {
            var transactions = ReadCsvFile(@"Transactions2014.csv");
            var accounts = CreateAccountsFromTransactions(transactions);
            
            ListAllAccounts(accounts);
        }
        
        private static IEnumerable<Transaction> ReadCsvFile(string filename)
        {
            var lines = File.ReadAllLines(filename).Skip(1);

            foreach (var line in lines)
            {
                var fields = line.Split(',');

                yield return new Transaction
                {
                    Date = DateTime.Parse(fields[0]),
                    From = fields[1],
                    To = fields[2],
                    Narrative = fields[3],
                    Amount = decimal.Parse(fields[4])
                };
            }
        }       
    
        private static Dictionary<string, Account> CreateAccountsFromTransactions(IEnumerable<Transaction> transactions)
        {
            var accounts = new Dictionary<string, Account>();

            foreach (var transaction in transactions)
            {
                GetOrCreateAccount(accounts, transaction.From).OutgoingTransactions.Add(transaction);
                GetOrCreateAccount(accounts, transaction.To).IncomingTransactions.Add(transaction);
            }

            return accounts;
        }

        private static Account GetOrCreateAccount(Dictionary<string, Account> accounts, string name)
        {
            if (accounts.ContainsKey(name))
            {
                return accounts[name];
            }

            var newAccount = new Account(name);
            accounts[name] = newAccount;
            return newAccount;
        }
   
        private static void ListAllAccounts(Dictionary<string, Account> accounts)
        {
            foreach (var account in accounts.Values)
            {
                var netBalance = account.IncomingTransactions.Sum(tr => tr.Amount) -
                                 account.OutgoingTransactions.Sum(tr => tr.Amount);

                Console.WriteLine("{0,-11}{1,-10}{2,-10}",
                                account.Name,
                                (netBalance < 0 ? "owes" : "is owed"),
                                $"{Math.Abs(netBalance):C}");
            }

            Console.WriteLine();
        }
    }
}
