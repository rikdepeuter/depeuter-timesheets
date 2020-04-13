using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;

namespace DePeuter.Shared.Helpers
{
    public class NetbankHelper
    {
        private readonly string _npmDirectory;
        private readonly string _username;
        private readonly string _password;

        private readonly ILog _logger = LogManager.GetLogger(typeof(NetbankHelper));

        public NetbankHelper(string npmDirectory, string username, string password)
        {
            _npmDirectory = npmDirectory;
            _username = username;
            _password = password;
        }

        public Account[] GetAccounts()
        {
            var arguments = new List<string>();
            arguments.AddRange(new[]
            {
                "/C",
                Path.Combine(_npmDirectory, "cba-netbank.cmd"),
                "list",
                "-u",
                _username,
                "-p",
                _password
            });
            
            var strArguments = string.Join(" ", arguments.Select(x => "\"" + x + "\""));
            var p = new Process();
            p.StartInfo = new ProcessStartInfo("cmd", strArguments);
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();
            p.WaitForExit();

            var output = p.StandardOutput.ReadToEnd().Split('\n');

            var header = output[1];
            //Name                    Number           Balance   Available
            var numberIndex = header.IndexOf("Number");
            var balanceIndex = header.IndexOf("Balance");
            var availableIndex = header.IndexOf("Available");
            return output.Skip(3).Where(x => !string.IsNullOrEmpty(x)).Select(x =>
            {
                var name = x.Substring(0, numberIndex).Trim();
                var number = x.Substring(numberIndex, balanceIndex - numberIndex).Trim();
                var balance = double.Parse(x.Substring(balanceIndex, availableIndex - balanceIndex).Trim().Replace("$", ""), CultureInfo.InvariantCulture);
                var available = double.Parse(x.Substring(availableIndex).Trim().Replace("$", ""), CultureInfo.InvariantCulture);

                return new Account() { Name = name, Number = number, Balance = balance, Available = available };
            }).Where(x => x != null).ToArray();
        }

        public Transaction[] GetTransactions(string accountNumber, DateTime? startDate, DateTime? endDate)
        {
            var path = Path.GetTempFileName();
            var arguments = new List<string>();
            arguments.AddRange(new[]
            {
                "/C",
                Path.Combine(_npmDirectory, "cba-netbank.cmd"),
                "download",
                "-u",
                _username,
                "-p",
                _password,
                "-a",
                accountNumber.Replace(" ", ""),
                "--format",
                "json",
                "-o",
                path
            });

            if(startDate != null)
            {
                arguments.Add("-f");
                arguments.Add(startDate.Value.ToString("dd/MM/yyyy"));
            }

            if(endDate != null)
            {
                arguments.Add("-t");
                arguments.Add(endDate.Value.ToString("dd/MM/yyyy"));
            }

            var strArguments = string.Join(" ", arguments.Select(x => "\"" + x + "\""));
            var p = new Process();
            p.StartInfo = new ProcessStartInfo("cmd", strArguments);
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();
            p.WaitForExit();

            var fileOutput = File.ReadAllText(path);

            //if(_logger.IsDebugEnabled)
            //{
            //    _logger.DebugFormat("Process: {0} {1}", p.StartInfo.FileName, p.StartInfo.Arguments);
            //    var output = p.StandardOutput.ReadToEnd();
            //    _logger.Debug("Output:");
            //    _logger.Debug(output);
            //    _logger.DebugFormat("File output:");
            //    _logger.Debug(fileOutput);
            //}

            var result = JsonConvert.DeserializeObject<Transaction[]>(fileOutput);
            File.Delete(path);
            return result ?? new Transaction[0];
        }

        public Transaction[] GetPendingTransactions(string accountNumber)
        {
            var path = Path.GetTempFileName();
            var arguments = new List<string>();
            arguments.AddRange(new[]
            {
                "/C",
                Path.Combine(_npmDirectory, "cba-netbank.cmd"),
                "download-pending",
                "-u",
                _username,
                "-p",
                _password,
                "-a",
                accountNumber.Replace(" ", ""),
                "--format",
                "json",
                "-o",
                path
            });
            
            var strArguments = string.Join(" ", arguments.Select(x => "\"" + x + "\""));
            var p = new Process();
            p.StartInfo = new ProcessStartInfo("cmd", strArguments);
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();
            p.WaitForExit();

            var fileOutput = File.ReadAllText(path);

            //if(_logger.IsDebugEnabled)
            //{
            //    _logger.DebugFormat("Process: {0} {1}", p.StartInfo.FileName, p.StartInfo.Arguments);
            //    var output = p.StandardOutput.ReadToEnd();
            //    _logger.Debug("Output:");
            //    _logger.Debug(output);
            //    _logger.DebugFormat("File output:");
            //    _logger.Debug(fileOutput);
            //}

            var result = JsonConvert.DeserializeObject<Transaction[]>(fileOutput);
            File.Delete(path);
            return result ?? new Transaction[0];
        }

        public class Account
        {
            public string Name { get; set; }
            public string Number { get; set; }
            public double Balance { get; set; }
            public double Available { get; set; }
        }

        public class Transaction
        {
            public DateTime Date { get; set; }
            public string Description { get; set; }
            public double Amount { get; set; }
            public double Balance { get; set; }
            public string Trancode { get; set; }
            public string ReceiptNumber { get; set; }
        }
    }
}