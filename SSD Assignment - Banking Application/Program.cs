using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.DirectoryServices.AccountManagement;
using SSD_Assignment___Banking_Application;
using System.Security.Cryptography;
using System.Text;
using System.Collections;
using DotNetEnv;
using System.Runtime.CompilerServices;

[assembly: DisablePrivateReflection] // Disable private reflection for the entire assembly

namespace Banking_Application
{
    public class Program
    {
        private static readonly int maxAttempts = 3; // Max number of attempts before closing the application

        public static void Main(string[] args)
        {
            // Get the relative path to the .env file
            string envFilePath = Path.Combine(Environment.CurrentDirectory, "..", "..", "..", ".env");

            // Load the .env file
            Env.Load(envFilePath);

            Logger.SetupEventSource();
            Cryptography_Utilities cryptography = new Cryptography_Utilities();

            Data_Access_Layer dal = Data_Access_Layer.getInstance();
            //dal.loadBankAccounts(); // removed as it is not used anymore

            string accNo;
            bool running = true;
            bool isGroupMember = false;
            bool isAdminGroupMember = false;
            int loginCount = 0;

            loginCount = 4; isGroupMember = true; isAdminGroupMember = true; // REMOVE BEFORE SUBMITTING!!!!!!!!!!!!!!

            string domainName = Environment.GetEnvironmentVariable("DOMAIN_NAME");
            string groupName = Environment.GetEnvironmentVariable("GROUP_NAME");
            string adminGroupName = Environment.GetEnvironmentVariable("ADMIN_GROUP_NAME");

            String username = null;
            String password = null;

            while ((loginCount < 3) && (!isAdminGroupMember && !isGroupMember))
            {
                loginCount++;
                // get user to log in
                Console.WriteLine("Log in");
                username = GetValidInput("Username: ", "INVALID USERNAME ENTERED - PLEASE TRY AGAIN");
                password = GetValidInput("Password: ", "INVALID PASSWORD ENTERED - PLEASE TRY AGAIN");
                Console.Clear();

                // check if they are authorised

                // Verify Validity Of User Credentials
                PrincipalContext domainContext = new PrincipalContext(ContextType.Domain, domainName);
                bool validCreds = domainContext.ValidateCredentials(username, password);

                //Verify Group Membership Of User Account

                UserPrincipal userPrincipal = UserPrincipal.FindByIdentity(domainContext, IdentityType.SamAccountName, username);
                isGroupMember = false;
                isAdminGroupMember = false;

                if (userPrincipal != null)
                {
                    isGroupMember = userPrincipal.IsMemberOf(domainContext, IdentityType.SamAccountName, groupName);//Throws Exception If User Principal Is Null
                    isAdminGroupMember = userPrincipal.IsMemberOf(domainContext, IdentityType.SamAccountName, adminGroupName);//Throws Exception If User Principal Is Null
                }

                // Encrypt username as it will remain in use
                username = BitConverter.ToString(cryptography.Encrypt(username));

                password = null;
                domainName = null;
                GC.Collect(); // call garbage collector

                if (validCreds && isGroupMember)
                {
                    Console.WriteLine("User Is Authorized To Perform Access Control Protected Action");
                    // Log successful log in
                    Logger.LogTransaction(username, "n/a", "n/a", "Login", DateTime.Now, "Successful log in", "SSD Banking Application v1.0.0");
                }
                else
                {
                    Console.WriteLine("User Is Not Authorized To Perform This Action.");
                    Logger.LogTransaction(username, "n/a", "n/a", "Authorization", DateTime.Now, "User Is Not Authorized To Perform This Action", "SSD Banking Application v1.0.0");
                    if (validCreds == false)
                    {
                        Console.WriteLine("Invalid User Credentials Provided.");
                        Logger.LogTransaction(username, "n/a", "n/a", "Authorization", DateTime.Now, "Invalid User Credentials Provided", "SSD Banking Application v1.0.0");
                    }
                    if (isGroupMember == false)
                    {
                        Console.WriteLine("User Is Not A Member Of The Authorized User Group.");
                        Logger.LogTransaction(username, "n/a", "n/a", "Authorization", DateTime.Now, "User Is Not A Member Of The Authorized User Group", "SSD Banking Application v1.0.0");
                    }

                    if (loginCount < 3)
                    {
                        Console.WriteLine("Please try again");
                        Logger.LogTransaction(username, "n/a", "n/a", "Login Attempt", DateTime.Now, "Login attempt failed. Trying again", "SSD Banking Application v1.0.0");
                    }
                    else
                    {
                        Console.WriteLine("Max number of log in attempts. Program terminating");
                        Logger.LogTransaction(username, "n/a", "n/a", "Login Attempt", DateTime.Now, "Max login attempts reached. Program terminating.", "SSD Banking Application v1.0.0");
                        running = false;
                    }
                }  
            }


            if (isGroupMember || isAdminGroupMember) // only continue if users are authorised
            {
                do
                {
                    Console.WriteLine("");
                    Console.WriteLine("***Banking Application Menu***");
                    Console.WriteLine("1. Add Bank Account");
                    Console.WriteLine("2. Close Bank Account");
                    Console.WriteLine("3. View Account Information");
                    Console.WriteLine("4. Make Lodgement");
                    Console.WriteLine("5. Make Withdrawal");
                    Console.WriteLine("6. Exit");
                    //Console.Write("CHOOSE OPTION: ");
                    //String option = Console.ReadLine();

                    string option = GetValidOption("Choose option: ", new string[] { "1", "2", "3", "4", "5", "6" });   // includes attempts checker and sanitization

                    switch (option)
                    {
                        case "1":
                            //String accountType = "";
                            //int loopCount = 0;
                            //do
                            //{
                            //    if (loopCount > 0)
                            //        Console.WriteLine("INVALID OPTION CHOSEN - PLEASE TRY AGAIN");

                            //    Console.WriteLine("");
                            //    Console.WriteLine("***Account Types***:");
                            //    Console.WriteLine("1. Current Account.");
                            //    Console.WriteLine("2. Savings Account.");
                            //    Console.WriteLine("CHOOSE OPTION:");
                            //    accountType = Console.ReadLine();
                            //    loopCount++;
                            //} while (!(accountType.Equals("1") || accountType.Equals("2")));
                            //String name = "";
                            //loopCount = 0;
                            //do
                            //{

                            //    if (loopCount > 0)
                            //        Console.WriteLine("INVALID NAME ENTERED - PLEASE TRY AGAIN");

                            //    Console.WriteLine("Enter Name: ");
                            //    name = Console.ReadLine();

                            //    loopCount++;

                            //} while (name.Equals(""));
                            //String addressLine1 = "";
                            //loopCount = 0;
                            //do
                            //{
                            //    if (loopCount > 0)
                            //        Console.WriteLine("INVALID ADDRESS LINE 1 ENTERED - PLEASE TRY AGAIN");

                            //    Console.WriteLine("Enter Address Line 1: ");
                            //    addressLine1 = Console.ReadLine();
                            //    loopCount++;
                            //} while (addressLine1.Equals(""));
                            //Console.WriteLine("Enter Address Line 2: ");
                            //String addressLine2 = Console.ReadLine();
                            //Console.WriteLine("Enter Address Line 3: ");
                            //String addressLine3 = Console.ReadLine();
                            //String town = "";
                            //loopCount = 0;
                            //do
                            //{
                            //    if (loopCount > 0)
                            //        Console.WriteLine("INVALID TOWN ENTERED - PLEASE TRY AGAIN");
                            //    Console.WriteLine("Enter Town: ");
                            //    town = Console.ReadLine();
                            //    loopCount++;
                            //} while (town.Equals(""));
                            //double balance = -1;
                            //loopCount = 0;
                            //do
                            //{
                            //    if (loopCount > 0)
                            //        Console.WriteLine("INVALID OPENING BALANCE ENTERED - PLEASE TRY AGAIN");

                            //    Console.WriteLine("Enter Opening Balance: ");
                            //    String balanceString = Console.ReadLine();
                            //    try
                            //    {
                            //        balance = Convert.ToDouble(balanceString);
                            //    }
                            //    catch
                            //    {
                            //        loopCount++;
                            //    }
                            //} while (balance < 0);

                            //Bank_Account ba;

                            //if (Convert.ToInt32(accountType) == Account_Type.Current_Account)
                            //{
                            //    double overdraftAmount = -1;
                            //    loopCount = 0;
                            //    do
                            //    {
                            //        if (loopCount > 0)
                            //            Console.WriteLine("INVALID OVERDRAFT AMOUNT ENTERED - PLEASE TRY AGAIN");

                            //        Console.WriteLine("Enter Overdraft Amount: ");
                            //        String overdraftAmountString = Console.ReadLine();
                            //        try
                            //        {
                            //            overdraftAmount = Convert.ToDouble(overdraftAmountString);
                            //        }
                            //        catch
                            //        {
                            //            loopCount++;
                            //        }
                            //    } while (overdraftAmount < 0);
                            //    //ba = new Current_Account(name, addressLine1, addressLine2, addressLine3, town, balance, overdraftAmount);
                            //    ba = new Current_Account
                            //    {
                            //        Name = name,
                            //        AddressLine1 = addressLine1,
                            //        AddressLine2 = addressLine2,
                            //        AddressLine3 = addressLine3,
                            //        Town = town,
                            //        Balance = balance,
                            //        OverdraftAmount = overdraftAmount
                            //    };
                            //}
                            //else
                            //{
                            //    double interestRate = -1;
                            //    loopCount = 0;
                            //    do
                            //    {
                            //        if (loopCount > 0)
                            //            Console.WriteLine("INVALID INTEREST RATE ENTERED - PLEASE TRY AGAIN");
                            //        Console.WriteLine("Enter Interest Rate: ");
                            //        String interestRateString = Console.ReadLine();
                            //        try
                            //        {
                            //            interestRate = Convert.ToDouble(interestRateString);
                            //        }
                            //        catch
                            //        {
                            //            loopCount++;
                            //        }
                            //    } while (interestRate < 0);
                            //    // ba = new Savings_Account(name, addressLine1, addressLine2, addressLine3, town, balance, interestRate);
                            //    ba = new Savings_Account
                            //    {
                            //        Name = name,
                            //        AddressLine1 = addressLine1,
                            //        AddressLine2 = addressLine2,
                            //        AddressLine3 = addressLine3,
                            //        Town = town,
                            //        Balance = balance,
                            //        InterestRate = interestRate
                            //    };
                            //}

                            // Display account type options and get the valid input.
                            Console.WriteLine("");
                            Console.WriteLine("***Account Types***:");
                            Console.WriteLine("1. Current Account.");
                            Console.WriteLine("2. Savings Account.");
                            int accountType = int.Parse(GetValidOption("Choose Account Type", new string[] { "1", "2" }));


                            // Gather user details with validation for mandatory fields.
                            string name = GetValidInput("Enter Name", "INVALID NAME ENTERED - PLEASE TRY AGAIN");
                            string addressLine1 = GetValidInput("Enter Address Line 1", "INVALID ADDRESS LINE 1 ENTERED - PLEASE TRY AGAIN");
                            string addressLine2 = GetOptionalInput("Enter Address Line 2");
                            string addressLine3 = GetOptionalInput("Enter Address Line 3");
                            string town = GetValidInput("Enter Town", "INVALID TOWN ENTERED - PLEASE TRY AGAIN");

                            // Get opening balance with numeric validation.
                            double balance = GetValidDouble("Enter Opening Balance", "INVALID OPENING BALANCE ENTERED - PLEASE TRY AGAIN");

                            // Create the bank account object based on the account type.
                            Bank_Account ba;
                            if (accountType == 1) // Current Account
                            {
                                double overdraftAmount = GetValidDouble("Enter Overdraft Amount", "INVALID OVERDRAFT AMOUNT ENTERED - PLEASE TRY AGAIN");
                                ba = new Current_Account
                                {
                                    Name = name,
                                    AddressLine1 = addressLine1,
                                    AddressLine2 = addressLine2,
                                    AddressLine3 = addressLine3,
                                    Town = town,
                                    Balance = balance,
                                    OverdraftAmount = overdraftAmount
                                };
                            }
                            else // Savings Account
                            {
                                double interestRate = GetValidDouble("Enter Interest Rate", "INVALID INTEREST RATE ENTERED - PLEASE TRY AGAIN");
                                ba = new Savings_Account
                                {
                                    Name = name,
                                    AddressLine1 = addressLine1,
                                    AddressLine2 = addressLine2,
                                    AddressLine3 = addressLine3,
                                    Town = town,
                                    Balance = balance,
                                    InterestRate = interestRate
                                };
                            }

                            if (dal.addBankAccount(ba))
                                Console.WriteLine("New Account Has Been Added");
                            //Console.WriteLine("New Account Number Is: " + accNo);

                            // Log new account creation
                            Logger.LogTransaction(username, BitConverter.ToString(cryptography.Encrypt(ba.AccountNo)), BitConverter.ToString(cryptography.Encrypt(ba.Name)), "Account Creation", DateTime.Now, "Account Successfully created", "SSD Banking Application v1.0.0");

                            // Wait for user to press any key before clearing the console
                            Console.WriteLine("Press any key to continue...");
                            Console.ReadKey();  // Waits for the user to press a key

                            Console.Clear();  // Clears the console after key press
                            break;
                        case "2":
                            if (isAdminGroupMember) // only allow deletion if user is an admin
                            {
                                //Console.WriteLine("Enter Account Number: ");
                                //accNo = Console.ReadLine();

                                //ba = dal.findBankAccountByAccNo(accNo);

                                //if (ba is null)
                                //{
                                //    Console.WriteLine("Account Does Not Exist");
                                //}
                                //else
                                //{
                                //    Console.WriteLine(ba.ToString());

                                //    String ans = "";

                                //    do
                                //    {

                                //        Console.WriteLine("Proceed With Delection (Y/N)?");
                                //        ans = Console.ReadLine();

                                //        switch (ans)
                                //        {
                                //            case "Y":
                                //            case "y":
                                //                dal.closeBankAccount(accNo);
                                //                // LOG ACCOUNT CLOSURE
                                //                break;
                                //            case "N":
                                //            case "n":
                                //                break;
                                //            default:
                                //                Console.WriteLine("INVALID OPTION CHOSEN - PLEASE TRY AGAIN");
                                //                break;
                                //        }
                                //    } while (!(ans.Equals("Y") || ans.Equals("y") || ans.Equals("N") || ans.Equals("n")));
                                //}

                                // Prompt for account number with sanitization and validation
                                accNo = GetValidAccountNumber("Enter Account Number", "ACCOUNT NUMBER CANNOT BE EMPTY");

                                // Find bank account by account number
                                ba = dal.findBankAccountByAccNo(accNo);

                                if (ba is null)
                                {
                                    Console.WriteLine("UNABLE TO PROCESS YOUR REQUEST. PLEASE TRY AGAIN."); // can help to prevent enumeration attacks by not providing specific information
                                    // Console.WriteLine("ACCOUNT DOES NOT EXIST");
                                    Logger.LogTransaction(username, "n/a", "n/a", "Account Deletion Attempt", DateTime.Now, "Account does not exist", "SSD Banking Application v1.0.0");
                                }
                                else
                                {
                                    Console.WriteLine(ba.ToString());

                                    string ans = GetValidOption("PROCEED WITH DELETION? (Y/N)", new string[] { "Y", "N", "y", "n" });

                                    switch (ans.ToUpper())
                                    {
                                        case "Y":
                                            dal.closeBankAccount(accNo);
                                            Console.WriteLine("ACCOUNT CLOSED SUCCESSFULLY");
                                            // Log account closure
                                            Logger.LogTransaction(username, BitConverter.ToString(cryptography.Encrypt(ba.AccountNo)), BitConverter.ToString(cryptography.Encrypt(ba.Name)), "Account Deletion", DateTime.Now, "Account does not exist", "SSD Banking Application v1.0.0");
                                            break;
                                        case "N":
                                            Console.WriteLine("ACCOUNT CLOSURE ABORTED");
                                            Logger.LogTransaction(username, BitConverter.ToString(cryptography.Encrypt(ba.AccountNo)), BitConverter.ToString(cryptography.Encrypt(ba.Name)), "Account Deletion Attempt", DateTime.Now, "Deletion Aborted", "SSD Banking Application v1.0.0");
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("YOU DO NOT HAVE PERMISSIONS TO CARRY OUT THIS ACTION");
                                // Log unauthorised account deletion attempt
                                Logger.LogTransaction(username, "n/a", "n/a", "Account Deletion Attempt", DateTime.Now, "User is not authorised", "SSD Banking Application v1.0.0");

                            }
                            // Wait for user to press any key before clearing the console
                            Console.WriteLine("Press any key to continue...");
                            Console.ReadKey();  // Waits for the user to press a key

                            Console.Clear();  // Clears the console after key press
                            break;
                        case "3":
                            //Console.WriteLine("Enter Account Number: ");
                            //accNo = Console.ReadLine();
                            accNo = GetValidAccountNumber("Enter Account Number", "ACCOUNT NUMBER CANNOT BE EMPTY");

                            ba = dal.findBankAccountByAccNo(accNo);

                            if (ba is null)
                            {
                                Console.WriteLine("UNABLE TO PROCESS YOUR REQUEST. PLEASE TRY AGAIN."); // can help to prevent enumeration attacks by not providing specific information
                                // Console.WriteLine("Account Does Not Exist");
                                Logger.LogTransaction(username, "n/a", "n/a", "Account Viewing Attempt", DateTime.Now, "Account does not exist", "SSD Banking Application v1.0.0");

                            }
                            else
                            {
                                Console.WriteLine(ba.ToString());
                                // Log account viewing
                                Logger.LogTransaction(username, BitConverter.ToString(cryptography.Encrypt(ba.AccountNo)), BitConverter.ToString(cryptography.Encrypt(ba.Name)), "Account Viewing", DateTime.Now, "Account Information has been viewed", "SSD Banking Application v1.0.0");
                            }
                            // Wait for user to press any key before clearing the console
                            Console.WriteLine("Press any key to continue...");
                            Console.ReadKey();  // Waits for the user to press a key

                            Console.Clear();  // Clears the console after key press
                            break;
                        case "4": //Lodge
                            //Console.WriteLine("Enter Account Number: ");
                            //accNo = Console.ReadLine();

                            accNo = GetValidAccountNumber("Enter Account Number", "ACCOUNT NUMBER CANNOT BE EMPTY");
                            ba = dal.findBankAccountByAccNo(accNo);

                            if (ba is null)
                            {
                                Console.WriteLine("UNABLE TO PROCESS YOUR REQUEST. PLEASE TRY AGAIN."); // can help to prevent enumeration attacks by not providing specific information
                                // Console.WriteLine("Account Does Not Exist");
                                Logger.LogTransaction(username, "n/a", "n/a", "Lodgement Attempt", DateTime.Now, "Account does not exist", "SSD Banking Application v1.0.0");
                            }
                            else
                            {
                                //double amountToLodge = -1;
                                //loopCount = 0;

                                //do
                                //{

                                //    if (loopCount > 0)
                                //        Console.WriteLine("INVALID AMOUNT ENTERED - PLEASE TRY AGAIN");

                                //    Console.WriteLine("Enter Amount To Lodge: ");
                                //    String amountToLodgeString = Console.ReadLine();

                                //    try
                                //    {
                                //        amountToLodge = Convert.ToDouble(amountToLodgeString);
                                //    }

                                //    catch
                                //    {
                                //        loopCount++;
                                //    }

                                //} while (amountToLodge < 0);

                                double amountToLodge = GetValidDouble("Enter amount to lodge", "INVALID AMOUNT ENTERED - PLEASE TRY AGAIN");
                                dal.lodge(accNo, amountToLodge);
                                Console.WriteLine($"Amount of {amountToLodge} successfully lodged");
                                // Log lodgement
                                Logger.LogTransaction(username, BitConverter.ToString(cryptography.Encrypt(ba.AccountNo)), BitConverter.ToString(cryptography.Encrypt(ba.Name)), "Lodgement", DateTime.Now, $"Amount of {amountToLodge} successfully lodged", "SSD Banking Application v1.0.0");
                            }
                            // Wait for user to press any key before clearing the console
                            Console.WriteLine("Press any key to continue...");
                            Console.ReadKey();  // Waits for the user to press a key

                            Console.Clear();  // Clears the console after key press
                            break;
                        case "5": //Withdraw
                            //Console.WriteLine("Enter Account Number: ");
                            //accNo = Console.ReadLine();

                            accNo = GetValidAccountNumber("Enter Account Number", "ACCOUNT NUMBER CANNOT BE EMPTY");
                            ba = dal.findBankAccountByAccNo(accNo);

                            if (ba is null)
                            {
                                Console.WriteLine("UNABLE TO PROCESS YOUR REQUEST. PLEASE TRY AGAIN."); // can help to prevent enumeration attacks by not providing specific information
                                // Console.WriteLine("Account Does Not Exist");
                                Logger.LogTransaction(username, "n/a", "n/a", "Withdraw Attempt", DateTime.Now, "Account does not exist", "SSD Banking Application v1.0.0");
                            }
                            else
                            {
                                //double amountToWithdraw = -1;
                                //loopCount = 0;

                                //do
                                //{

                                //    if (loopCount > 0)
                                //        Console.WriteLine("INVALID AMOUNT ENTERED - PLEASE TRY AGAIN");

                                //    Console.WriteLine("Enter Amount To Withdraw (€" + ba.getAvailableFunds() + " Available): ");
                                //    String amountToWithdrawString = Console.ReadLine();

                                //    try
                                //    {
                                //        amountToWithdraw = Convert.ToDouble(amountToWithdrawString);
                                //    }

                                //    catch
                                //    {
                                //        loopCount++;
                                //    }

                                //} while (amountToWithdraw < 0);

                                double amountToWithdraw = GetValidDouble($"Enter Amount To Withdraw (€{ba.getAvailableFunds()} Available)", "INVALID AMOUNT ENTERED - PLEASE TRY AGAIN");

                                // Check if the amount is greater than available funds
                                if (amountToWithdraw > ba.getAvailableFunds())
                                {
                                    Console.WriteLine("Insufficient Funds Available");
                                    Logger.LogTransaction(username, BitConverter.ToString(cryptography.Encrypt(ba.AccountNo)), BitConverter.ToString(cryptography.Encrypt(ba.Name)), "Withdraw Attempt", DateTime.Now, "Insufficient Funds Available", "SSD Banking Application v1.0.0");
                                }
                                else
                                {
                                    bool withdrawalOK = dal.withdraw(accNo, amountToWithdraw);

                                    //if (withdrawalOK == false)
                                    if (withdrawalOK)
                                    {
                                        Console.WriteLine($"Amount of {amountToWithdraw} successfully withdrawn");
                                        // Log withdrawal
                                        Logger.LogTransaction(username, BitConverter.ToString(cryptography.Encrypt(ba.AccountNo)), BitConverter.ToString(cryptography.Encrypt(ba.Name)), "Withdraw", DateTime.Now, $"Amount of {amountToWithdraw} successfully withdrawn", "SSD Banking Application v1.0.0");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Unexpected Error");
                                        Logger.LogError("Unexpected error when trying to withdraw");
                                    }
                                }
                            }
                            // Wait for user to press any key before clearing the console
                            Console.WriteLine("Press any key to continue...");
                            Console.ReadKey();  // Waits for the user to press a key

                            Console.Clear();  // Clears the console after key press
                            break;
                        case "6":
                            running = false;
                            username = null;
                            dal = null;
                            cryptography = null;
                            ba = null;
                            accNo = null;
                            GC.Collect(); //call garbage collector
                            break;
                        default:
                            Console.WriteLine("INVALID OPTION CHOSEN - PLEASE TRY AGAIN");
                            break;
                    }

                } while (running != false);
            }
        }
        private static string GetValidInput(string prompt, string errorMessage)
        {
            const int maxLength = 150; // Hardcoded maximum length
            string input;
            int attemptCount = 0;

            do
            {
                if (attemptCount >= maxAttempts)
                {
                    Console.WriteLine("You have exceeded the maximum number of attempts. Try again later. Application exiting.");
                    Environment.Exit(0); // Close the application if max attempts are reached
                }

                Console.Write($"{prompt}: ");
                input = Console.ReadLine();

                // Trim leading/trailing whitespace
                input = input?.Trim();

                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine(errorMessage);
                }
                else if (input.Length > maxLength)
                {
                    Console.WriteLine($"INPUT EXCEEDS THE MAXIMUM ALLOWED LENGTH OF {maxLength} CHARACTERS. PLEASE TRY AGAIN.");
                    input = null; // Reset input to ensure the loop continues
                }
                else
                {
                    // Sanitise input by removing unwanted characters
                    input = SanitizeInput(input);
                }
                // Increment attempt count after each failed attempt
                attemptCount++;
            } while (string.IsNullOrEmpty(input));

            return input;
        }


        // Method to allow optional input
        private static string GetOptionalInput(string prompt)
        {
            const int maxLength = 150; // Hardcoded maximum length
            string input;

            Console.Write($"{prompt}: ");
            input = Console.ReadLine();

            if (!string.IsNullOrEmpty(input))
            {
                // Trim leading/trailing whitespace
                input = input.Trim();

                if (input.Length > maxLength)
                {
                    Console.WriteLine($"INPUT EXCEEDS THE MAXIMUM ALLOWED LENGTH OF {maxLength} CHARACTERS. IT WILL BE TRUNCATED.");
                    input = input.Substring(0, maxLength); // Truncate to the maximum length
                }

                // Sanitise input
                input = SanitizeInput(input);
            }

            return input;
        }


        // Method to validate numeric input
        private static double GetValidDouble(string prompt, string errorMessage)
        {
            const int maxLength = 20; // Reasonable maximum length for numeric input
            double value;
            int attemptCount = 0;

            do
            {
                if (attemptCount >= maxAttempts)
                {
                    Console.WriteLine("You have exceeded the maximum number of attempts. Try again later. Application exiting.");
                    Environment.Exit(0); // Close the application if max attempts are reached
                }

                Console.Write($"{prompt}: ");
                string input = Console.ReadLine();

                // Sanitise the input
                input = SanitizeInput(input);

                // Check if the input length exceeds the maximum allowed
                if (input.Length > maxLength)
                {
                    Console.WriteLine($"INPUT EXCEEDS THE MAXIMUM ALLOWED LENGTH OF {maxLength} CHARACTERS. PLEASE TRY AGAIN.");
                    continue;
                }

                // Validate that the input can be parsed as a double and is >= 0
                if (double.TryParse(input, out value) && value >= 0)
                {
                    return value;
                }

                Console.WriteLine(errorMessage);
                attemptCount++;
            } while (true);
        }


        // Method to validate selection from a list of options
        private static string GetValidOption(string prompt, string[] options)
        {
            string input;
            int attemptCount = 0;

            do
            {
                if (attemptCount >= maxAttempts)
                {
                    Console.WriteLine("You have exceeded the maximum number of attempts. Try again later. Application exiting.");
                    Environment.Exit(0); // Close the application if max attempts are reached
                }

                Console.Write($"{prompt}: ");
                // Read and sanitize the input
                input = Console.ReadLine()?.Trim().ToLower(); // Trim and make the input lowercase for case-insensitive comparison

                // Check if the input is valid by ensuring it matches one of the options
                if (input != null && options.Contains(input))
                {
                    return input; // Valid input within the options
                }

                // Show error message if the input is invalid
                Console.WriteLine("INVALID OPTION CHOSEN - PLEASE TRY AGAIN");
                attemptCount++;
            } while (true);
        }


        private static string GetValidAccountNumber(string prompt, string errorMessage)
        {
            string accountNumber;
            int attemptCount = 0;

            do
            {
                if (attemptCount >= maxAttempts)
                {
                    Console.WriteLine("You have exceeded the maximum number of attempts. Try again later. Application exiting.");
                    Environment.Exit(0); // Close the application if max attempts are reached
                }

                Console.Write($"{prompt}: ");
                accountNumber = Console.ReadLine()?.Trim(); // Trim any extra spaces

                // Ensure input is not null or empty
                if (string.IsNullOrEmpty(accountNumber))
                {
                    Console.WriteLine(errorMessage);
                    continue;
                }

                // Try to parse the input as a GUID
                if (Guid.TryParse(accountNumber, out _))
                {
                    return accountNumber; // Return the valid GUID as a string
                }
                else
                {
                    Console.WriteLine("INVALID GUID FORMAT. PLEASE ENTER A VALID ACCOUNT NUMBER.");
                }
                attemptCount++;
            } while (true);
        }

        private static string SanitizeInput(string input)
        {
            // Remove potentially harmful characters
            string sanitizedInput = input.Replace("<", "")
                                         .Replace(">", "")
                                         .Replace("'", "")
                                         .Replace("\"", "")
                                         .Replace(";", "")
                                         .Replace("--", "")
                                         .Replace("/*", "")
                                         .Replace("*/", "");
            return sanitizedInput;
        }

    }
}