using GdbWebApi.Application.Controllers;
using GdbWebApi.Application.Dtos;
using GdbWebApi.Application.Services.Contracts;
using GdbWebApi.Application.Services.Implementations;
using GdbWebApi.Domain.Enums;
using GdbWebApi.Domain.Exceptions;
using GdbWebApi.Domain.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace GdbWebApi.Presentation.UI
{
    public class Home
    {
        int choice;

        private static string FormatRupee(decimal? amount) =>
            amount.HasValue
                ? amount.Value.ToString("C", new CultureInfo("en-IN"))
                : "N/A";

        private static bool IsValidAccountNumberInput(string acc)
        {
            if (string.IsNullOrWhiteSpace(acc))
                return false;

            acc = acc.Trim();

            // Exactly 10 digits
            return Regex.IsMatch(acc, @"^\d{10}$");
        }

        private static bool IsValidPinInput(string pin)
        {
            if (string.IsNullOrWhiteSpace(pin))
                return false;

            pin = pin.Trim();

            // Exactly 4 digits
            return Regex.IsMatch(pin, @"^\d{4}$");
        }

        public async Task Start()
        {
            choice = -1;

            while (choice != 0)
            {
                Console.WriteLine();
                Console.WriteLine("Welcome to GDB");

                Console.WriteLine(
                    "1. Create Account\n" +
                    "2. View Account\n" +
                    "3. View All Accounts\n" +
                    "4. View Balance\n" +
                    "5. View Recent Transactions\n" +
                    "6. Withdraw\n" +
                    "7. Deposit\n" +
                    "8. Transfer Funds\n" +
                    "9. Close Account\n" +
                    "0. Exit");

                Console.WriteLine("Enter Your Choice.");
                choice = int.Parse(Console.ReadLine());

                switch (choice)
                {
                    case 1:
                        CreateAccount();
                        break;

                    case 2:
                        await ViewAccountAsync();
                        break;

                    case 3:
                        ViewAllAccounts();
                        break;

                    case 4:
                        await ViewBalanceAsync();
                        break;

                    case 5:
                        await ViewRecentTransactionsAsync();
                        break;

                    case 6:
                        await WithdrawAsync();
                        break;

                    case 7:
                        await DepositAsync();
                        break;

                    case 8:
                        await TransferFundsAsync();
                        break;

                    case 9:
                        await CloseAccountAsync();
                        break;

                    case 0:
                        Exit();
                        break;

                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }

        // ============================================================
        // CREATE ACCOUNT
        // ============================================================

        public void CreateAccount()
        {
            AccountController controller = new AccountController();

            Console.WriteLine("===== CREATE ACCOUNT =====");

            Console.Write("Enter Account Number: ");
            var accountNumber = Console.ReadLine().Trim();

            while (!IsValidAccountNumberInput(accountNumber))
            {
                Console.WriteLine(
                    "Invalid account number. Please enter a 10-digit numeric account number.");

                Console.Write("Enter Account Number: ");
                accountNumber = Console.ReadLine()!;
            }

            Console.Write("Enter Name: ");
            var name = Console.ReadLine().Trim();

            Console.Write("Enter Age: ");
            var age = Convert.ToInt32(Console.ReadLine().Trim());

            Console.Write("Enter Initial Balance: ");
            var balance = Convert.ToDecimal(Console.ReadLine().Trim());

            Console.Write("Enter PIN: ");
            var pin = Console.ReadLine()!.Trim();

            while (!IsValidPinInput(pin))
            {
                Console.WriteLine(
                    "Invalid PIN. Please enter a 4-digit numeric PIN.");

                Console.Write("Enter PIN: ");
                pin = Console.ReadLine()!.Trim();
            }

            Console.WriteLine("Select Account Type:");
            Console.WriteLine("1. Savings");
            Console.WriteLine("2. Current");
            Console.WriteLine("3. Fixed Deposit");
            Console.WriteLine("4. Salary");

            Console.Write("Enter Choice: ");
            int typeChoice = Convert.ToInt32(Console.ReadLine());

            AccountType accountType;

            switch (typeChoice)
            {
                case 1:
                    accountType = AccountType.Savings;
                    break;

                case 2:
                    accountType = AccountType.Current;
                    break;

                case 3:
                    accountType = AccountType.FixedDeposit;
                    break;

                case 4:
                    accountType = AccountType.Salary;
                    break;

                default:
                    throw new Exception("Invalid account type.");
            }

            Console.Write("Enter Privilege (Premium/Gold/Silver): ");
            string privilegeInput = Console.ReadLine()!;

            AccountPrivilege privilege =
                (AccountPrivilege)Enum.Parse(
                    typeof(AccountPrivilege),
                    privilegeInput,
                    true);

            // Default account-specific values
            decimal overdraftLimit = 25000m;
            int tenureMonths = 12;
            double interestRate = 6.5;
            decimal minimumBalance = 1000m;
            string employerName = "TechCorp";
            AccountStatus status = AccountStatus.Active;

            if (accountType == AccountType.Current)
            {
                Console.Write("Enter Overdraft Limit: ");
                overdraftLimit = Convert.ToDecimal(Console.ReadLine());
            }
            else if (accountType == AccountType.FixedDeposit)
            {
                Console.Write("Enter Tenure Months: ");
                tenureMonths = Convert.ToInt32(Console.ReadLine());

                Console.Write("Enter Interest Rate: ");
                interestRate = Convert.ToDouble(Console.ReadLine());
            }
            else if (accountType == AccountType.Savings)
            {
                Console.Write("Enter Minimum Balance: ");
                minimumBalance = Convert.ToDecimal(Console.ReadLine());

                Console.Write("Enter Interest Rate: ");
                interestRate = Convert.ToDouble(Console.ReadLine());
            }
            else if (accountType == AccountType.Salary)
            {
                Console.Write("Enter Employer Name: ");
                employerName = Console.ReadLine()!;
            }

            // ============================================================
            // CREATE REQUEST DTO
            // ============================================================

            CreateAccountRequestDto request = new CreateAccountRequestDto()
            {
                AccountNumber = accountNumber,
                Name = name,
                Age = age,
                Balance = balance,
                Pin = pin,
                AccountType = accountType,
                Status = status,
                Privilege = privilege,
                OverdraftLimit = overdraftLimit,
                TenureMonths = tenureMonths,
                InterestRate = interestRate,
                MinimumBalance = minimumBalance,
                EmployerName = employerName
            };

            try
            {
                // ========================================================
                // SEND REQUEST DTO TO CONTROLLER
                // CONTROLLER RETURNS IActionResult
                // ========================================================

                IActionResult result =
                    controller.CreateAccount(request);

                // ========================================================
                // EXTRACT DTO FROM IActionResult
                // ========================================================

                if (result is OkObjectResult okResult &&
                    okResult.Value is CreateAccountResponseDto response)
                {
                    Console.WriteLine();
                    Console.WriteLine(
                        "===== ACCOUNT CREATED SUCCESSFULLY =====");

                    Console.WriteLine(
                        $"Account Number : {response.AccountNumber}");

                    Console.WriteLine(
                        $"Name           : {response.Name}");

                    Console.WriteLine(
                        $"Account Type   : {response.AccountType}");

                    Console.WriteLine(
                        $"Balance        : {FormatRupee(response.Balance)}");

                    Console.WriteLine(
                        $"Status         : {response.Status}");

                    Console.WriteLine(
                        $"Privilege      : {response.Privilege}");
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Account creation failed.");

                    if (result is ObjectResult errorResult)
                    {
                        Console.WriteLine(
                            $"Error: {errorResult.Value}");
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine("\n" + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nError: " + ex.Message);
            }
        }

        // ============================================================
        // VIEW ACCOUNT
        // ============================================================

        public async Task ViewAccountAsync()
        {
            Console.WriteLine("Enter the account number.");
            string accNo = Console.ReadLine();

            if (!IsValidAccountNumberInput(accNo))
            {
                Console.WriteLine("Invalid account number.");
                return;
            }

            try
            {
                AccountController controller =
                    new AccountController();

                // Controller returns IActionResult
                IActionResult result =
                    await controller.ViewAccountAsync(accNo);

                if (result is OkObjectResult okResult &&
                    okResult.Value is ViewAccountResponseDto account)
                {
                    Console.WriteLine();
                    Console.WriteLine(
                        "Account Number : " + account.AccountNumber);

                    Console.WriteLine(
                        "Name           : " + account.Name);

                    Console.WriteLine(
                        $"Balance        : {FormatRupee(account.Balance)}");
                }
                else
                {
                    Console.WriteLine("Account not found.");

                    if (result is ObjectResult errorResult &&
                        errorResult.Value != null)
                    {
                        Console.WriteLine(
                            $"Error: {errorResult.Value}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nError: " + ex.Message);
            }
        }

        // ============================================================
        // VIEW ALL ACCOUNTS
        // ============================================================

        // ============================================================
        // VIEW ALL ACCOUNTS
        // ============================================================

        public void ViewAllAccounts()
        {
            AccountController controller =
                new AccountController();

            try
            {
                IActionResult result =
                    controller.GetAllAccounts();

                if (result is OkObjectResult okResult &&
                    okResult.Value is IEnumerable<ViewAccountResponseDto> accounts)
                {
                    Console.WriteLine();
                    Console.WriteLine("===== ALL ACCOUNTS =====");
                    Console.WriteLine("----------------------------");

                    foreach (ViewAccountResponseDto account in accounts)
                    {
                        Console.WriteLine(
                            "Account Number : " +
                            account.AccountNumber);

                        Console.WriteLine(
                            "Name           : " +
                            account.Name);

                        Console.WriteLine(
                            $"Balance        : " +
                            $"{FormatRupee(account.Balance)}");

                        Console.WriteLine(
                            "----------------------------");
                    }
                }
                else
                {
                    Console.WriteLine("No accounts found.");

                    if (result is ObjectResult errorResult &&
                        errorResult.Value != null)
                    {
                        Console.WriteLine(
                            $"Error: {errorResult.Value}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "\nError: " + ex.Message);
            }
        }

        // ============================================================
        // VIEW BALANCE
        // ============================================================

        public async Task ViewBalanceAsync()
        {
            Console.WriteLine("Enter Account Number:");
            string accountNumber = Console.ReadLine();

            if (!IsValidAccountNumberInput(accountNumber))
            {
                Console.WriteLine("Invalid account number.");
                return;
            }

            AccountController controller =
                new AccountController();

            try
            {
                // Controller returns IActionResult
                IActionResult result =
                    await controller.GetBalanceAsync(accountNumber);

                if (result is OkObjectResult okResult &&
                    okResult.Value is ViewBalanceResponseDto account)
                {
                    Console.WriteLine();
                    Console.WriteLine(
                        $"Balance : {FormatRupee(account.Balance)}");
                }
                else
                {
                    Console.WriteLine("Account not found.");

                    if (result is ObjectResult errorResult &&
                        errorResult.Value != null)
                    {
                        Console.WriteLine(
                            $"Error: {errorResult.Value}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nError: " + ex.Message);
            }
        }

        // ============================================================
        // VIEW RECENT TRANSACTIONS
        // ============================================================

        public async Task ViewRecentTransactionsAsync()
        {
            Console.WriteLine();
            Console.WriteLine("===== VIEW RECENT TRANSACTIONS =====");

            Console.Write("Enter Account Number: ");
            string accountNumber = Console.ReadLine()!;

            if (!IsValidAccountNumberInput(accountNumber))
            {
                Console.WriteLine(
                    "Invalid account number. Please enter a 10-digit numeric account number.");

                return;
            }

            try
            {
                TransactionController controller =
                    new TransactionController();

                // Controller returns ActionResult<List<...>>
                ActionResult<List<ViewRecentTransactionsResponseDto>> result =
                    await controller.GetRecentTransactionsAsync(
                        accountNumber);

                List<ViewRecentTransactionsResponseDto> transactions;

                if (result.Result is OkObjectResult okResult &&
                    okResult.Value is List<ViewRecentTransactionsResponseDto> list)
                {
                    transactions = list;
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("No transactions found.");

                    if (result.Result is ObjectResult errorResult &&
                        errorResult.Value != null)
                    {
                        Console.WriteLine(
                            $"Error: {errorResult.Value}");
                    }

                    return;
                }

                if (transactions.Count == 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("No transactions found.");
                    return;
                }

                Console.WriteLine();
                Console.WriteLine("===== RECENT TRANSACTIONS =====");

                foreach (
                    ViewRecentTransactionsResponseDto transaction
                    in transactions)
                {
                    Console.WriteLine();
                    Console.WriteLine("-------------------------------");

                    Console.WriteLine(
                        $"Transaction ID   : {transaction.TransactionId}");

                    Console.WriteLine(
                        $"From Account     : {transaction.FromAccountNumber ?? "N/A"}");

                    Console.WriteLine(
                        $"To Account       : {transaction.ToAccountNumber ?? "N/A"}");

                    Console.WriteLine(
                        $"Transaction Type : {transaction.TransactionType}");

                    Console.WriteLine(
                        $"Amount           : {FormatRupee(transaction.Amount)}");

                    Console.WriteLine(
                        $"Status           : {transaction.TransactionStatus}");

                    Console.WriteLine(
                        $"Timestamp        : {transaction.Timestamp}");

                    if (transaction.BalanceAfterFrom.HasValue)
                    {
                        Console.WriteLine(
                            $"Balance After From : {FormatRupee(transaction.BalanceAfterFrom)}");
                    }

                    if (transaction.BalanceAfterTo.HasValue)
                    {
                        Console.WriteLine(
                            $"Balance After To   : {FormatRupee(transaction.BalanceAfterTo)}");
                    }
                }

                Console.WriteLine();
                Console.WriteLine("-------------------------------");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        // ============================================================
        // WITHDRAW
        // ============================================================

        public async Task WithdrawAsync()
        {
            Console.WriteLine("Enter Account Number:");
            string accountNumber = Console.ReadLine().Trim();

            while (!IsValidAccountNumberInput(accountNumber))
            {
                Console.WriteLine(
                    "Invalid account number. Please enter a 10-digit numeric account number.");

                Console.Write("Enter Account Number: ");
                accountNumber = Console.ReadLine()!;
            }

            Console.WriteLine("Enter PIN:");
            string pin = Console.ReadLine().Trim();

            // FIXED:
            // Previously account number was being validated here.
            // Now PIN is validated correctly.
            while (!IsValidPinInput(pin))
            {
                Console.WriteLine(
                    "Invalid PIN. Please enter a 4-digit numeric PIN.");

                Console.Write("Enter PIN: ");
                pin = Console.ReadLine()!;
            }

            Console.WriteLine("Enter Amount:");
            decimal amount =
                decimal.Parse(Console.ReadLine().Trim());

            try
            {
                TransactionController controller =
                    new TransactionController();

                TransactionDto request = new TransactionDto()
                {
                    AccountNumber = accountNumber,
                    Pin = pin,
                    Amount = amount
                };

                ActionResult<WithdrawResponseDto> result =
                    await controller.WithdrawAsync(request);

                if (result.Result is OkObjectResult okResult &&
                    okResult.Value is WithdrawResponseDto account)
                {
                    Console.WriteLine(
                        $"Balance : {FormatRupee(account.Balance)}");

                    Console.WriteLine(
                        "Status: " + account.TransactionStat);
                }
                else
                {
                    Console.WriteLine("Withdrawal failed.");

                    if (result.Result is ObjectResult errorResult &&
                        errorResult.Value != null)
                    {
                        Console.WriteLine(
                            $"Error: {errorResult.Value}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // ============================================================
        // DEPOSIT
        // ============================================================

        public async Task DepositAsync()
        {
            Console.WriteLine("Enter Account Number:");
            string accountNumber = Console.ReadLine();

            while (!IsValidAccountNumberInput(accountNumber))
            {
                Console.WriteLine(
                    "Invalid account number. Please enter a 10-digit numeric account number.");

                Console.Write("Enter Account Number: ");
                accountNumber = Console.ReadLine()!;
            }

            Console.WriteLine("Enter Amount:");
            decimal amount =
                decimal.Parse(Console.ReadLine());

            try
            {
                TransactionController controller =
                    new TransactionController();

                TransactionDto request = new TransactionDto()
                {
                    AccountNumber = accountNumber,
                    Amount = amount
                };

                ActionResult<DepositResponseDto> result =
                    await controller.DepositAsync(request);

                if (result.Result is OkObjectResult okResult &&
                    okResult.Value is DepositResponseDto account)
                {
                    Console.WriteLine(
                        $"Balance : {FormatRupee(account.Balance)}");

                    Console.WriteLine(
                        "Status: " + account.TransactionStat);
                }
                else
                {
                    Console.WriteLine("Deposit failed.");

                    if (result.Result is ObjectResult errorResult &&
                        errorResult.Value != null)
                    {
                        Console.WriteLine(
                            $"Error: {errorResult.Value}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // ============================================================
        // TRANSFER FUNDS
        // ============================================================

        public async Task TransferFundsAsync()
        {
            Console.WriteLine("Enter From Account Number:");
            string fromAccountNumber = Console.ReadLine();

            while (!IsValidAccountNumberInput(fromAccountNumber))
            {
                Console.WriteLine(
                    "Invalid account number. Please enter a 10-digit numeric account number.");

                Console.Write("Enter Account Number: ");
                fromAccountNumber = Console.ReadLine()!;
            }

            Console.WriteLine("Enter To Account Number:");
            string toAccountNumber = Console.ReadLine();

            while (!IsValidAccountNumberInput(toAccountNumber))
            {
                Console.WriteLine(
                    "Invalid account number. Please enter a 10-digit numeric account number.");

                Console.Write("Enter Account Number: ");
                toAccountNumber = Console.ReadLine()!;
            }

            Console.Write("Enter PIN: ");
            var pin = Console.ReadLine()!.Trim();

            while (!IsValidPinInput(pin))
            {
                Console.WriteLine(
                    "Invalid PIN. Please enter a 4-digit numeric PIN.");

                Console.Write("Enter PIN: ");
                pin = Console.ReadLine()!.Trim();
            }

            Console.WriteLine("Enter Amount:");
            decimal amount =
                decimal.Parse(Console.ReadLine());

            try
            {
                if (fromAccountNumber.Equals(toAccountNumber))
                {
                    throw new AccountException(
                        "\nSource Account and Destination Account cannot be the same");
                }

                TransactionController controller =
                    new TransactionController();

                TransactionDto request = new TransactionDto()
                {
                    FromAccount = fromAccountNumber,
                    ToAccount = toAccountNumber,
                    Pin = pin,
                    Amount = amount
                };

                ActionResult<TranferFundsResponseDto> actionResult =
                    await controller.TransferFundsAsync(request);

                if (actionResult.Result is OkObjectResult okResult &&
                    okResult.Value is TranferFundsResponseDto result)
                {
                    Console.WriteLine();

                    Console.WriteLine(
                        $"Transaction Status: {result.TransactionStat}");
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Transfer failed.");

                    if (actionResult.Result is ObjectResult errorResult &&
                        errorResult.Value != null)
                    {
                        Console.WriteLine(
                            $"Error: {errorResult.Value}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.Message);
            }
        }

        // ============================================================
        // CLOSE ACCOUNT
        // ============================================================

        public async Task CloseAccountAsync()
        {
            Console.WriteLine("===== CLOSE ACCOUNT =====");

            Console.Write("Enter Account Number: ");
            string accountNumber = Console.ReadLine()!;

            while (!IsValidAccountNumberInput(accountNumber))
            {
                Console.WriteLine(
                    "Invalid account number. Please enter a 10-digit numeric account number.");

                Console.Write("Enter Account Number: ");
                accountNumber = Console.ReadLine()!;
            }

            try
            {
                CloseAccountRequestDto request =
                    new CloseAccountRequestDto()
                    {
                        AccountNumber = accountNumber
                    };

                AccountController controller =
                    new AccountController();

                /*
                 * Controller returns IActionResult.
                 * Unwrap the CloseAccountResponseDto from the result.
                 */
                IActionResult result =
                    await controller.CloseAccountAsync(request);

                if (result is OkObjectResult okResult &&
                    okResult.Value is CloseAccountResponseDto response)
                {
                    Console.WriteLine();
                    Console.WriteLine("===== ACCOUNT CLOSED =====");

                    Console.WriteLine(
                        $"Account Number : {response.AccountNumber}");

                    Console.WriteLine(
                        $"Status         : {response.Status}");

                    Console.WriteLine(
                        $"Message        : {response.Message}");
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Account close failed.");

                    if (result is ObjectResult errorResult &&
                        errorResult.Value != null)
                    {
                        Console.WriteLine(
                            $"Error: {errorResult.Value}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
            }
        }

        // ============================================================
        // EXIT
        // ============================================================

        public void Exit()
        {
            Console.WriteLine("\nThank you for using GDB.");
            choice = 0;
        }
    }
}