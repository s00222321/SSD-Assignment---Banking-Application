﻿using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.DirectoryServices.AccountManagement;
using SSD_Assignment___Banking_Application;
using System.Security.Cryptography;
using System.Text;

namespace Banking_Application
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //string text = "Hello World";
            //byte[] plaintextData = Encoding.ASCII.GetBytes(text);

            //Console.WriteLine("Plaintext: " + text);

            //// Generate random key and IV
            //byte[] key = new byte[16];
            //byte[] iv = new byte[16];
            //RandomNumberGenerator.Fill(key);
            //RandomNumberGenerator.Fill(iv);

            //Console.WriteLine("Key (Base64): " + Convert.ToBase64String(key));
            //Console.WriteLine("IV (Base64): " + Convert.ToBase64String(iv));

            //// Create CryptoUtils object
            //Cryptography_Utilities cryptoUtils = new Cryptography_Utilities(key, iv);

            //// Encrypt and display ciphertext
            //byte[] encryptedData = cryptoUtils.Encrypt(plaintextData);
            //Console.WriteLine("Encrypted Data (Base64): " + Convert.ToBase64String(encryptedData));

            //// Decrypt and display plaintext
            //byte[] decryptedData = cryptoUtils.Decrypt(encryptedData);
            //string decryptedText = Encoding.ASCII.GetString(decryptedData);
            //Console.WriteLine("Decrypted Text: " + decryptedText);

            Data_Access_Layer dal = Data_Access_Layer.getInstance();
            //dal.loadBankAccounts(); // REMOVED FROM DAL
            string accNo;
            bool running = true;
            bool isGroupMember = false;
            bool isAdminGroupMember = false;
            int loginCount = 0;
            loginCount = 4; isGroupMember = true; isAdminGroupMember = true; // REMOVE BEFORE SUBMITTING!!!!!!!!!!!!!!

            string domainName = "ITSLIGO.LAN"; // hide somehow
            string groupName = "Bank Teller"; //User Group Name HIDE?
            string adminGroupName = "Bank Teller Administrator";

            String username = null;
            String password = null;

            while ((loginCount < 3) && (!isAdminGroupMember && !isGroupMember))
            {
                loginCount++;
                // get user to log in
                Console.WriteLine("Log in");
                Console.Write("Username: ");
                username = Console.ReadLine();
                Console.Write("Password: ");
                password = Console.ReadLine();
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

                if (validCreds && isGroupMember)
                {
                    Console.WriteLine("User Is Authorized To Perform Access Control Protected Action");
                }
                else
                {
                    Console.WriteLine("User Is Not Authorized To Perform This Action.");
                    if (validCreds == false)
                        Console.WriteLine("Invalid User Credentials Provided.");
                    if (isGroupMember == false)
                        Console.WriteLine("User Is Not A Member Of The Authorized User Group.");

                    if (loginCount < 3)
                    {
                        Console.WriteLine("Please try again");
                    }
                    else
                    {
                        Console.WriteLine("Max number of log in attempts. Program terminating");
                        running = false;
                    }
                }
            }

            //Output

            // then wipe the memory
            username = null;
            password = null;
            domainName = null;

            if (isGroupMember || isAdminGroupMember) // ONLY CONTINUE IF USERS ARE AUTHORISED
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
                    Console.WriteLine("CHOOSE OPTION:");
                    String option = Console.ReadLine(); // ONLY A CERTAIN NUMBER OF TRIES NOT TO CRASH OUT??

                    switch (option)
                    {
                        case "1":
                            String accountType = "";
                            int loopCount = 0;

                            do
                            {

                                if (loopCount > 0)
                                    Console.WriteLine("INVALID OPTION CHOSEN - PLEASE TRY AGAIN");

                                Console.WriteLine("");
                                Console.WriteLine("***Account Types***:");
                                Console.WriteLine("1. Current Account.");
                                Console.WriteLine("2. Savings Account.");
                                Console.WriteLine("CHOOSE OPTION:");
                                accountType = Console.ReadLine();

                                loopCount++;

                            } while (!(accountType.Equals("1") || accountType.Equals("2")));

                            String name = "";
                            loopCount = 0;

                            do
                            {

                                if (loopCount > 0)
                                    Console.WriteLine("INVALID NAME ENTERED - PLEASE TRY AGAIN");

                                Console.WriteLine("Enter Name: ");
                                name = Console.ReadLine();

                                loopCount++;

                            } while (name.Equals(""));

                            String addressLine1 = "";
                            loopCount = 0;

                            do
                            {

                                if (loopCount > 0)
                                    Console.WriteLine("INVALID ÀDDRESS LINE 1 ENTERED - PLEASE TRY AGAIN");

                                Console.WriteLine("Enter Address Line 1: ");
                                addressLine1 = Console.ReadLine();

                                loopCount++;

                            } while (addressLine1.Equals(""));

                            Console.WriteLine("Enter Address Line 2: ");
                            String addressLine2 = Console.ReadLine();

                            Console.WriteLine("Enter Address Line 3: ");
                            String addressLine3 = Console.ReadLine();

                            String town = "";
                            loopCount = 0;

                            do
                            {

                                if (loopCount > 0)
                                    Console.WriteLine("INVALID TOWN ENTERED - PLEASE TRY AGAIN");

                                Console.WriteLine("Enter Town: ");
                                town = Console.ReadLine();

                                loopCount++;

                            } while (town.Equals(""));

                            double balance = -1;
                            loopCount = 0;

                            do
                            {

                                if (loopCount > 0)
                                    Console.WriteLine("INVALID OPENING BALANCE ENTERED - PLEASE TRY AGAIN");

                                Console.WriteLine("Enter Opening Balance: ");
                                String balanceString = Console.ReadLine();

                                try
                                {
                                    balance = Convert.ToDouble(balanceString);
                                }

                                catch
                                {
                                    loopCount++;
                                }

                            } while (balance < 0);

                            Bank_Account ba;

                            if (Convert.ToInt32(accountType) == Account_Type.Current_Account)
                            {
                                double overdraftAmount = -1;
                                loopCount = 0;

                                do
                                {

                                    if (loopCount > 0)
                                        Console.WriteLine("INVALID OVERDRAFT AMOUNT ENTERED - PLEASE TRY AGAIN");

                                    Console.WriteLine("Enter Overdraft Amount: ");
                                    String overdraftAmountString = Console.ReadLine();

                                    try
                                    {
                                        overdraftAmount = Convert.ToDouble(overdraftAmountString);
                                    }

                                    catch
                                    {
                                        loopCount++;
                                    }

                                } while (overdraftAmount < 0);

                                ba = new Current_Account(name, addressLine1, addressLine2, addressLine3, town, balance, overdraftAmount);
                            }

                            else
                            {

                                double interestRate = -1;
                                loopCount = 0;

                                do
                                {

                                    if (loopCount > 0)
                                        Console.WriteLine("INVALID INTEREST RATE ENTERED - PLEASE TRY AGAIN");

                                    Console.WriteLine("Enter Interest Rate: ");
                                    String interestRateString = Console.ReadLine();

                                    try
                                    {
                                        interestRate = Convert.ToDouble(interestRateString);
                                    }

                                    catch
                                    {
                                        loopCount++;
                                    }

                                } while (interestRate < 0);

                                ba = new Savings_Account(name, addressLine1, addressLine2, addressLine3, town, balance, interestRate);
                            }

                            if (dal.addBankAccount(ba))
                                Console.WriteLine("New Account Has Been Added");
                            //Console.WriteLine("New Account Number Is: " + accNo); // DO NOT DO THIS!!!

                            break;
                        case "2":
                            if (isAdminGroupMember) // ONLY ALLOW IF USER IS ADMIN
                            {
                                Console.WriteLine("Enter Account Number: ");
                                accNo = Console.ReadLine();

                                ba = dal.findBankAccountByAccNo(accNo);

                                if (ba is null)
                                {
                                    Console.WriteLine("Account Does Not Exist");
                                }
                                else
                                {
                                    Console.WriteLine(ba.ToString());

                                    String ans = "";

                                    do
                                    {

                                        Console.WriteLine("Proceed With Delection (Y/N)?");
                                        ans = Console.ReadLine();

                                        switch (ans)
                                        {
                                            case "Y":
                                            case "y":
                                                dal.closeBankAccount(accNo);
                                                break;
                                            case "N":
                                            case "n":
                                                break;
                                            default:
                                                Console.WriteLine("INVALID OPTION CHOSEN - PLEASE TRY AGAIN");
                                                break;
                                        }
                                    } while (!(ans.Equals("Y") || ans.Equals("y") || ans.Equals("N") || ans.Equals("n")));
                                }
                            }
                            else
                            {
                                Console.WriteLine("You do not have permissions to carry out this action");
                            }

                            break;
                        case "3":
                            Console.WriteLine("Enter Account Number: ");
                            accNo = Console.ReadLine();

                            ba = dal.findBankAccountByAccNo(accNo);

                            if (ba is null)
                            {
                                Console.WriteLine("Account Does Not Exist"); // SAY SOMETHING LIKE "Unable to process your request. Please try again." SO THEY WON'T TRY LOTS, BUT LOG CORRECT
                            }
                            else
                            {
                                Console.WriteLine(ba.ToString());
                            }

                            break;
                        case "4": //Lodge
                            Console.WriteLine("Enter Account Number: ");
                            accNo = Console.ReadLine();  // SQL ATTACK - MUST BE VALIDATED, IMPLEMENT AUTHORISATION, MAKE SURE SENSISITVE DATA IS NOT LOGGED, RESOURCE MANAGEMENT?, ENCRYPT DATA IN TRANSIT

                            ba = dal.findBankAccountByAccNo(accNo);

                            if (ba is null)
                            {
                                Console.WriteLine("Account Does Not Exist"); // SAY SOMETHING LIKE "Unable to process your request. Please try again." SO THEY WON'T TRY LOTS, BUT LOG CORRECT
                            }
                            else
                            {
                                double amountToLodge = -1;
                                loopCount = 0;

                                do
                                {

                                    if (loopCount > 0)
                                        Console.WriteLine("INVALID AMOUNT ENTERED - PLEASE TRY AGAIN");

                                    Console.WriteLine("Enter Amount To Lodge: ");
                                    String amountToLodgeString = Console.ReadLine();

                                    try
                                    {
                                        amountToLodge = Convert.ToDouble(amountToLodgeString);
                                    }

                                    catch
                                    {
                                        loopCount++;
                                    }

                                } while (amountToLodge < 0);

                                dal.lodge(accNo, amountToLodge);
                            }
                            break;
                        case "5": //Withdraw
                            Console.WriteLine("Enter Account Number: ");
                            accNo = Console.ReadLine();

                            ba = dal.findBankAccountByAccNo(accNo);

                            if (ba is null)
                            {
                                Console.WriteLine("Account Does Not Exist"); // SAY SOMETHING LIKE "Unable to process your request. Please try again." SO THEY WON'T TRY LOTS, BUT LOG CORRECT
                            }
                            else
                            {
                                double amountToWithdraw = -1;
                                loopCount = 0;

                                do
                                {

                                    if (loopCount > 0)
                                        Console.WriteLine("INVALID AMOUNT ENTERED - PLEASE TRY AGAIN");

                                    Console.WriteLine("Enter Amount To Withdraw (€" + ba.getAvailableFunds() + " Available): ");
                                    String amountToWithdrawString = Console.ReadLine();

                                    try
                                    {
                                        amountToWithdraw = Convert.ToDouble(amountToWithdrawString);
                                    }

                                    catch
                                    {
                                        loopCount++;
                                    }

                                } while (amountToWithdraw < 0);

                                bool withdrawalOK = dal.withdraw(accNo, amountToWithdraw);

                                if (withdrawalOK == false)
                                {

                                    Console.WriteLine("Insufficient Funds Available.");
                                }
                            }
                            break;
                        case "6":
                            running = false;
                            break;
                        default:
                            Console.WriteLine("INVALID OPTION CHOSEN - PLEASE TRY AGAIN");
                            break;
                    }

                } while (running != false);
            }
        }

    }
}