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

            foreach (var transaction in transactions)
            {
                Console.WriteLine("Date: {0} From: {1} To: {2} Narrative: {3} Amount: {4}",
                                    transaction.Date.ToShortDateString(),
                                    transaction.From,
                                    transaction.To,
                                    transaction.Narrative,
                                    transaction.Amount);
            }
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
    }
}
