using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace SupportBank
{
    class Program
    {
        private enum CommandType
        {
            ListAll,
            ListOne
        }

        private struct Command
        {
            public CommandType Type { get; set; }
            public string Account { get; set; }
        }

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        
        static void Main()
        {
            var config = new LoggingConfiguration();
            var target = new FileTarget { FileName = @"C:\Users\EmiPat\Code\TechSwitch teaching\SupportBank\SupportBank.log", Layout = @"${longdate} ${level} - ${logger}: ${message}" };
            config.AddTarget("File Logger", target);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, target));
            LogManager.Configuration = config;

            var transactions = ReadCsvFile(@"DodgyTransactions2015.csv");
            var accounts = CreateAccountsFromTransactions(transactions);

            PrintWelcomeScreen();
            var command = PromptForCommand();
            Console.WriteLine();

            switch (command.Type)
            {
                case CommandType.ListAll:
                    ListAllAccounts(accounts);
                    break;
                case CommandType.ListOne:
                    ListOneAccount(accounts[command.Account]);
                    break;
            }
        }

        private static IEnumerable<Transaction> ReadCsvFile(string filename)
        {
            Logger.Info($"Loading transactions from file {filename}");
            var lines = File.ReadAllLines(filename).Skip(1);

            foreach (var line in lines)
            {
                Logger.Debug($"Parsing transaction: {line}");
                var fields = line.Split(',');

                if (fields.Length != 5)
                {
                    ReportSkippedTransaction(line, "Wrong number of fields");
                    continue;
                }

                DateTime date;
                if (!DateTime.TryParse(fields[0], out date))
                {
                    ReportSkippedTransaction(line, "Invalid date");
                    continue;
                }

                decimal amount;
                if (!decimal.TryParse(fields[4], out amount))
                {
                    ReportSkippedTransaction(line, "Invalid transaction amount");
                    continue;
                }

                yield return new Transaction
                {
                    Date = date,
                    From = fields[1],
                    To = fields[2],
                    Narrative = fields[3],
                    Amount = amount
                };
            }
        }

        private static void ReportSkippedTransaction(string transaction, string reason)
        {
            Logger.Error($"Unable to process transaction because {reason}: {transaction}");
            Console.Error.WriteLine($"Skipping invalid transaction: {transaction}");
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

        private static void PrintWelcomeScreen()
        {
            Console.WriteLine("Welcome to SupportBank!");
            Console.WriteLine("=======================");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("  List All - list all account balances");
            Console.WriteLine("  List [Account] - list all transactions for the specified account");
            Console.WriteLine();
        }

        private static Command PromptForCommand()
        {
            while (true)
            {
                Console.Write("Enter a command: ");
                string commandText = Console.ReadLine();

                Command command;

                if (ParseCommand(commandText, out command))
                {
                    return command;
                }

                Console.WriteLine("Sorry, I don't understand.");
                Console.WriteLine();
            }
        }

        private static bool ParseCommand(string commandText, out Command command)
        {
            command = new Command();

            if (!commandText.ToLower().StartsWith("list "))
            {
                return false;
            }

            if (commandText.Substring(5).ToLower() == "all")
            {
                command.Type = CommandType.ListAll;
            }
            else
            {
                command.Type = CommandType.ListOne;
                command.Account = commandText.Substring(5);
            }

            return true;
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

        private static void ListOneAccount(Account account)
        {
            var allTransactions = account.OutgoingTransactions.Union(account.IncomingTransactions);
            int colWidth = allTransactions.Max(x => x.Narrative.Length) + 3;
            string colFormat = $"{{0,-15}} {{1,-{colWidth}}} {{2,-15}} {{3,10}}";

            Console.WriteLine($"Account: {account.Name}");
            foreach (var transaction in allTransactions.OrderBy(tr => tr.Date))
            {
                Console.WriteLine(colFormat,
                                transaction.Date.ToShortDateString(),
                                transaction.Narrative,
                                transaction.To,
                                $"£{String.Format("{0:0.00}", transaction.Amount)}");
            }

            Console.WriteLine();
        }
    }
}
