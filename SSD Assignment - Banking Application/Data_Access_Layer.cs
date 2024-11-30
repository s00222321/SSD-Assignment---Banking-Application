using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace Banking_Application
{
    public class Data_Access_Layer
    {

        private List<Bank_Account> accounts;
        public static String databaseName = "Banking Database.db"; // HARD CODED!!!! WTF PUT AS ENV OR SOMETHING
        private static Data_Access_Layer instance = new Data_Access_Layer(); // USE REFLECTION OR SMTH??

        private Data_Access_Layer()//Singleton Design Pattern (For Concurrency Control) - Use getInstance() Method Instead.
        {
            accounts = new List<Bank_Account>(); // BAD FOR MEMORY AND SECURITY
        }

        public static Data_Access_Layer getInstance()
        {
            return instance;
        }

        private SqliteConnection getDatabaseConnection()
        {

            String databaseConnectionString = new SqliteConnectionStringBuilder()
            {
                DataSource = Data_Access_Layer.databaseName, // LACK OF ENCRYPTION??, 
                Mode = SqliteOpenMode.ReadWriteCreate // STRICTER MODE?
            }.ToString();

            return new SqliteConnection(databaseConnectionString); // MAKE SURE NOT TO LOG THE CONNECTION STRING
            // PUT ERROR HANDLING??
            // ADD LOGGING
        }

        private void initialiseDatabase()
        {
            using (var connection = getDatabaseConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = // ENCRYPT ACCOUNTNO, ADDRESS??
                @"
                    CREATE TABLE IF NOT EXISTS Bank_Accounts(    
                        accountNo TEXT PRIMARY KEY,
                        name TEXT NOT NULL,
                        address_line_1 TEXT,
                        address_line_2 TEXT,
                        address_line_3 TEXT,
                        town TEXT NOT NULL,
                        balance REAL NOT NULL,
                        accountType INTEGER NOT NULL,
                        overdraftAmount REAL,
                        interestRate REAL
                    ) WITHOUT ROWID
                ";

                command.ExecuteNonQuery();
                // ADD ERROR HANDLING??
                // ADD LOGGING
            }
        }

        public void loadBankAccounts()
        {
            if (!File.Exists(Data_Access_Layer.databaseName))
                initialiseDatabase();
            else
            {

                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT * FROM Bank_Accounts";
                    SqliteDataReader dr = command.ExecuteReader();
                    
                    // VALIDATE THAT NOTHING HAS BEEN CHANGED
                    // ADD LOGGING AND ERROR HANDLING

                    while(dr.Read())
                    {

                        int accountType = dr.GetInt16(7);

                        if(accountType == Account_Type.Current_Account) // HASH/ENCYPT PII VALUES
                        {
                            Current_Account ca = new Current_Account();
                            ca.accountNo = dr.GetString(0); // ADD COLUMN NAMES INSTEAD OF INDEXES
                            ca.name = dr.GetString(1);
                            ca.address_line_1 = dr.GetString(2);
                            ca.address_line_2 = dr.GetString(3);
                            ca.address_line_3 = dr.GetString(4);
                            ca.town = dr.GetString(5);
                            ca.balance = dr.GetDouble(6);
                            ca.overdraftAmount = dr.GetDouble(8);
                            accounts.Add(ca);
                        }
                        else
                        {
                            Savings_Account sa = new Savings_Account();
                            sa.accountNo = dr.GetString(0);
                            sa.name = dr.GetString(1);
                            sa.address_line_1 = dr.GetString(2);
                            sa.address_line_2 = dr.GetString(3);
                            sa.address_line_3 = dr.GetString(4);
                            sa.town = dr.GetString(5);
                            sa.balance = dr.GetDouble(6);
                            sa.interestRate = dr.GetDouble(9);
                            accounts.Add(sa);
                        }


                    }

                }

            }
        }

        public String addBankAccount(Bank_Account ba)
        {

            if (ba.GetType() == typeof(Current_Account))  // typeof vs is
                ba = (Current_Account)ba;
            else
                ba = (Savings_Account)ba;

            accounts.Add(ba);

            using (var connection = getDatabaseConnection())
            {
                // CHECK FOR NULL VALUES IN BANK ACCOUNT OBJECT
                // ADD LOGGING AND ERROR HANDLING
                connection.Open();
                var command = connection.CreateCommand();
                // USED PREPARED STATEMENTS TO PREVENT SQL INJECTION
                //command.CommandText =
                //@"
                //    INSERT INTO Bank_Accounts VALUES(" +
                //    "'" + ba.accountNo + "', " +
                //    "'" + ba.name + "', " +
                //    "'" + ba.address_line_1 + "', " +
                //    "'" + ba.address_line_2 + "', " +
                //    "'" + ba.address_line_3 + "', " +
                //    "'" + ba.town + "', " +
                //    ba.balance + ", " +
                //    (ba.GetType() == typeof(Current_Account) ? 1 : 2) + ", ";

                //if (ba.GetType() == typeof(Current_Account))
                //{
                //    Current_Account ca = (Current_Account)ba;
                //    command.CommandText += ca.overdraftAmount + ", NULL)";
                //}

                //else
                //{
                //    Savings_Account sa = (Savings_Account)ba;
                //    command.CommandText += "NULL," + sa.interestRate + ")";
                //}
                command.CommandText = @"
                    INSERT INTO Bank_Accounts 
                    (accountNo, name, address_line_1, address_line_2, address_line_3, town, balance, accountType, overdraftAmount, interestRate)
                    VALUES (@accountNo, @name, @address_line_1, @address_line_2, @address_line_3, @town, @balance, @accountType, @overdraftAmount, @interestRate)";

                // Add common parameters
                command.Parameters.AddWithValue("@accountNo", ba.accountNo);
                command.Parameters.AddWithValue("@name", ba.name);
                command.Parameters.AddWithValue("@address_line_1", ba.address_line_1);
                command.Parameters.AddWithValue("@address_line_2", ba.address_line_2);
                command.Parameters.AddWithValue("@address_line_3", ba.address_line_3);
                command.Parameters.AddWithValue("@town", ba.town);
                command.Parameters.AddWithValue("@balance", ba.balance);
                command.Parameters.AddWithValue("@accountType", ba.GetType() == typeof(Current_Account) ? 1 : 2);

                // Add type-specific parameters
                if (ba.GetType() == typeof(Current_Account))
                {
                    Current_Account ca = (Current_Account)ba;
                    command.Parameters.AddWithValue("@overdraftAmount", ca.overdraftAmount);
                    command.Parameters.AddWithValue("@interestRate", DBNull.Value);
                }
                else
                {
                    Savings_Account sa = (Savings_Account)ba;
                    command.Parameters.AddWithValue("@overdraftAmount", DBNull.Value);
                    command.Parameters.AddWithValue("@interestRate", sa.interestRate);
                }

                command.ExecuteNonQuery();

            }

            return ba.accountNo;

        }

        public Bank_Account findBankAccountByAccNo(String accNo) 
        {
            if(!File.Exists(Data_Access_Layer.databaseName))
                initialiseDatabase();
            else
            {
                // UPDATED THIS SO IT ONLY GETS THE INDIVIDUAL RECORD WHEN REQUIRED
                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT * FROM Bank_Accounts WHERE accountNo = @accountNo";
                    command.Parameters.AddWithValue("@accountNo", accNo);
                    SqliteDataReader dr = command.ExecuteReader();

                    while (dr.Read())
                    {

                        int accountType = dr.GetInt16(7);

                        if (accountType == Account_Type.Current_Account) // HASH/ENCYPT PII VALUES
                        {
                            Current_Account ca = new Current_Account();
                            ca.accountNo = dr.GetString(0); // ADD COLUMN NAMES INSTEAD OF INDEXES
                            ca.name = dr.GetString(1);
                            ca.address_line_1 = dr.GetString(2);
                            ca.address_line_2 = dr.GetString(3);
                            ca.address_line_3 = dr.GetString(4);
                            ca.town = dr.GetString(5);
                            ca.balance = dr.GetDouble(6);
                            ca.overdraftAmount = dr.GetDouble(8);
                            return ca;
                        }
                        else
                        {
                            Savings_Account sa = new Savings_Account();
                            sa.accountNo = dr.GetString(0);
                            sa.name = dr.GetString(1);
                            sa.address_line_1 = dr.GetString(2);
                            sa.address_line_2 = dr.GetString(3);
                            sa.address_line_3 = dr.GetString(4);
                            sa.town = dr.GetString(5);
                            sa.balance = dr.GetDouble(6);
                            sa.interestRate = dr.GetDouble(9);
                            return sa;
                        }
                    }
                }
            }

            //foreach (Bank_Account ba in accounts)
            //{
            //    if (ba.accountNo.Equals(accNo))
            //    {
            //        return ba;
            //    }

            //}

            return null; 
        }

        // MUST HAVE AUTHORISATION
        public bool closeBankAccount(String accNo) 
        {
            // ADD LOGGING AND ERROR HANDLING
            Bank_Account toRemove = null;
            
            foreach (Bank_Account ba in accounts)
            {

                if (ba.accountNo.Equals(accNo)) // CASE SENSITIVE
                {
                    toRemove = ba;
                    break;
                }

            }

            if (toRemove == null)
                return false;
            else
            {
                accounts.Remove(toRemove);

                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    //command.CommandText = "DELETE FROM Bank_Accounts WHERE accountNo = '" + toRemove.accountNo + "'";
                    // PREPARED STATEMENT FOR SQL INJECTION PREVENTION
                    command.CommandText = "DELETE FROM Bank_Accounts WHERE accountNo = @accountNo";
                    command.Parameters.AddWithValue("@accountNo", toRemove.accountNo);
                    command.ExecuteNonQuery();

                }

                return true;
            }

        }

        public bool lodge(String accNo, double amountToLodge)
        {
            // ADD LOGGING AND ERROR HANDLING
            Bank_Account toLodgeTo = null;

            foreach (Bank_Account ba in accounts)
            {

                if (ba.accountNo.Equals(accNo)) // CASE SENSITIVE
                {
                    ba.lodge(amountToLodge);
                    toLodgeTo = ba;
                    break;
                }

            }

            if (toLodgeTo == null)
                return false;
            else
            {

                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    // SQL INJECTION, USE PREPARED STATEMENT
                    //command.CommandText = "UPDATE Bank_Accounts SET balance = " + toLodgeTo.balance + " WHERE accountNo = '" + toLodgeTo.accountNo + "'";
                    command.CommandText = "UPDATE Bank_Accounts SET balance = @balance WHERE accountNo = @accountNo";
                    command.Parameters.AddWithValue("@balance", toLodgeTo.balance);
                    command.Parameters.AddWithValue("@accountNo", toLodgeTo.accountNo);

                    command.ExecuteNonQuery();

                }

                return true;
            }

        }

        public bool withdraw(String accNo, double amountToWithdraw)
        {
            // ADD LOGGING AND ERROR HANDLING

            Bank_Account toWithdrawFrom = null;
            bool result = false;

            foreach (Bank_Account ba in accounts)
            {

                if (ba.accountNo.Equals(accNo))
                {
                    result = ba.withdraw(amountToWithdraw);
                    toWithdrawFrom = ba;
                    break;
                }

            }

            if (toWithdrawFrom == null || result == false)
                return false;
            else
            {

                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    // SQL INJECTION USE PREPARED STATEMENT
                    //command.CommandText = "UPDATE Bank_Accounts SET balance = " + toWithdrawFrom.balance + " WHERE accountNo = '" + toWithdrawFrom.accountNo + "'";
                    command.CommandText = "UPDATE Bank_Accounts SET balance = @balance WHERE accountNo = @accountNo";
                    command.Parameters.AddWithValue("@balance", toWithdrawFrom.balance);
                    command.Parameters.AddWithValue("@accountNo", toWithdrawFrom.accountNo);

                    command.ExecuteNonQuery();

                }

                return true;
            }

        }

    }
}
