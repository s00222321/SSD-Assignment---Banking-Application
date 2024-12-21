using Microsoft.Data.Sqlite;
using SSD_Assignment___Banking_Application;
using System.Security.Cryptography;
using System.Text;

namespace Banking_Application
{
    public class Data_Access_Layer
    {
        private Cryptography_Utilities cryptoUtils;


        // private List<Bank_Account> accounts;  // NOT NEEDED ANYMORE
        public static String databaseName = "Banking Database.db"; // HARD CODED!!!! WTF PUT AS ENV OR SOMETHING
        private static Data_Access_Layer instance = new Data_Access_Layer(); // USE REFLECTION OR SMTH??

        private Data_Access_Layer()//Singleton Design Pattern (For Concurrency Control) - Use getInstance() Method Instead.
        {
            // accounts = new List<Bank_Account>(); // BAD FOR MEMORY AND SECURITY

            // Create CryptoUtils object
            cryptoUtils = new Cryptography_Utilities();
        }

        public static Data_Access_Layer getInstance()
        {
            return instance;
        }

        private SqliteConnection getDatabaseConnection()
        {

            String databaseConnectionString = new SqliteConnectionStringBuilder()
            {
                DataSource = Data_Access_Layer.databaseName,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Private // USE PRIVATE CASHE FOR STRICTER ISOLATION
            }.ToString();

            return new SqliteConnection(databaseConnectionString);
        }

        private void initialiseDatabase()
        {
            try
            {
                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    //command.CommandText =
                    //@"
                    //    CREATE TABLE IF NOT EXISTS Bank_Accounts(    
                    //        accountNo TEXT PRIMARY KEY,
                    //        name TEXT NOT NULL,
                    //        address_line_1 TEXT,
                    //        address_line_2 TEXT,
                    //        address_line_3 TEXT,
                    //        town TEXT NOT NULL,
                    //        balance REAL NOT NULL,
                    //        accountType INTEGER NOT NULL,
                    //        overdraftAmount REAL,
                    //        interestRate REAL
                    //    ) WITHOUT ROWID
                    //";
                    command.CommandText =
                        @"
                    CREATE TABLE Encrypted_Bank_Accounts (
                        accountNo BLOB PRIMARY KEY,
                        name BLOB NOT NULL,      
                        addressLine1 BLOB,      
                        addressLine2 BLOB,         
                        addressLine3 BLOB,          
                        town BLOB NOT NULL,        
                        balance REAL NOT NULL,
                        accountType INTEGER NOT NULL, 
                        overdraftAmount REAL,    
                        interestRate REAL,
                        hashValue BLOB NOT NULL,
                        hashAccountNo BLOB NOT NULL
                    ) WITHOUT ROWID;";

                    command.ExecuteNonQuery();
                }
            }
            catch (SqliteException e)
            {
                // Handle SQLite-specific exceptions and log the error
                Console.Error.WriteLine($"SQLite Error: {e.Message}");
                Logger.LogError("An error occurred while initializing the database. " + e);
                throw new ApplicationException("An error occurred while initializing the database.", e);
            }
            catch (Exception e)
            {
                // Handle other unexpected exceptions and log the error
                Console.Error.WriteLine($"Unexpected Error: {e.Message}");
                Logger.LogError("An unexpected error occurred while initializing the database. " + e);
                throw new ApplicationException("An unexpected error occurred while initializing the database.", e);
            }
        }

        //NO LONGER NECESSARY - GETTING INDIVIDUAL FILES NOW
        //public void loadBankAccounts()
        //{
        //    if (!File.Exists(Data_Access_Layer.databaseName))
        //        initialiseDatabase();
        //    else
        //    {
        //        using (var connection = getDatabaseConnection())
        //        {
        //            connection.Open();
        //            var command = connection.CreateCommand();
        //            command.CommandText = "SELECT * FROM Bank_Accounts";
        //            SqliteDataReader dr = command.ExecuteReader();
        //            while(dr.Read())
        //            {
        //                int accountType = dr.GetInt16(7);
        //                if(accountType == Account_Type.Current_Account)
        //                {
        //                    Current_Account ca = new Current_Account();
        //                    ca.accountNo = dr.GetString(0);
        //                    ca.name = dr.GetString(1);
        //                    ca.address_line_1 = dr.GetString(2);
        //                    ca.address_line_2 = dr.GetString(3);
        //                    ca.address_line_3 = dr.GetString(4);
        //                    ca.town = dr.GetString(5);
        //                    ca.balance = dr.GetDouble(6);
        //                    ca.overdraftAmount = dr.GetDouble(8);
        //                    accounts.Add(ca);
        //                }
        //                else
        //                {
        //                    Savings_Account sa = new Savings_Account();
        //                    sa.accountNo = dr.GetString(0);
        //                    sa.name = dr.GetString(1);
        //                    sa.address_line_1 = dr.GetString(2);
        //                    sa.address_line_2 = dr.GetString(3);
        //                    sa.address_line_3 = dr.GetString(4);
        //                    sa.town = dr.GetString(5);
        //                    sa.balance = dr.GetDouble(6);
        //                    sa.interestRate = dr.GetDouble(9);
        //                    accounts.Add(sa);
        //                }
        //            }
        //        }
        //    }
        //}

        //public String addBankAccount(Bank_Account ba)
        //{
        //    if (ba.GetType() == typeof(Current_Account))
        //        ba = (Current_Account)ba;
        //    else
        //        ba = (Savings_Account)ba;
        //    accounts.Add(ba);
        //    using (var connection = getDatabaseConnection())
        //    {
        //        connection.Open();
        //        var command = connection.CreateCommand();
        //        command.CommandText =
        //        @"
        //            INSERT INTO Bank_Accounts VALUES(" +
        //            "'" + ba.accountNo + "', " +
        //            "'" + ba.name + "', " +
        //            "'" + ba.address_line_1 + "', " +
        //            "'" + ba.address_line_2 + "', " +
        //            "'" + ba.address_line_3 + "', " +
        //            "'" + ba.town + "', " +
        //            ba.balance + ", " +
        //            (ba.GetType() == typeof(Current_Account) ? 1 : 2) + ", ";
        //        if (ba.GetType() == typeof(Current_Account))
        //        {
        //            Current_Account ca = (Current_Account)ba;
        //            command.CommandText += ca.overdraftAmount + ", NULL)";
        //        }
        //        else
        //        {
        //            Savings_Account sa = (Savings_Account)ba;
        //            command.CommandText += "NULL," + sa.interestRate + ")";
        //        }
        //        command.ExecuteNonQuery();
        //    }
        //    return ba.accountNo;
        //}

        public bool addBankAccount(Bank_Account ba)
        {
            try
            {
                // Check for null fields in the Bank_Account object
                if (ba == null || string.IsNullOrEmpty(ba.AccountNo) || string.IsNullOrEmpty(ba.Name) || string.IsNullOrEmpty(ba.Town))
                {
                    throw new ArgumentException("Bank account data is incomplete. Account No, Name, and Town are required.");
                }

                if (ba.GetType() == typeof(Current_Account))
                    ba = (Current_Account)ba;
                else
                    ba = (Savings_Account)ba;

                Console.WriteLine(ba.AccountNo); // REMOVE THIS

                // Encrypt each field
                byte[] encryptedAccountNo = cryptoUtils.Encrypt(ba.AccountNo);
                byte[] encryptedName = cryptoUtils.Encrypt(ba.Name);
                byte[] encryptedAddressLine1 = cryptoUtils.Encrypt(ba.AddressLine1);
                byte[] encryptedAddressLine2 = cryptoUtils.Encrypt(ba.AddressLine2);
                byte[] encryptedAddressLine3 = cryptoUtils.Encrypt(ba.AddressLine3);
                byte[] encryptedTown = cryptoUtils.Encrypt(ba.Town);

                // Compute the hash for the entry
                byte[] hashValue = cryptoUtils.GenerateHashForSendingToDB(ba);

                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    // USED PREPARED STATEMENTS TO PREVENT SQL INJECTION
                    command.CommandText = @"
                    INSERT INTO Encrypted_Bank_Accounts 
                    (accountNo, name, addressLine1, addressLine2, addressLine3, town, balance, accountType, overdraftAmount, interestRate, hashValue, hashAccountNo)
                    VALUES (@accountNo, @name, @addressLine1, @addressLine2, @addressLine3, @town, @balance, @accountType, @overdraftAmount, @interestRate, @hashValue, @hashAccountNo)";

                    // Add common parameters
                    command.Parameters.AddWithValue("@accountNo", encryptedAccountNo);
                    command.Parameters.AddWithValue("@name", encryptedName);
                    command.Parameters.AddWithValue("@addressLine1", encryptedAddressLine1);
                    command.Parameters.AddWithValue("@addressLine2", encryptedAddressLine2);
                    command.Parameters.AddWithValue("@addressLine3", encryptedAddressLine3);
                    command.Parameters.AddWithValue("@town", encryptedTown);
                    command.Parameters.AddWithValue("@balance", ba.Balance);
                    command.Parameters.AddWithValue("@accountType", ba.GetType() == typeof(Current_Account) ? 1 : 2);
                    command.Parameters.AddWithValue("@hashValue", hashValue);
                    command.Parameters.AddWithValue("@hashAccountNo", cryptoUtils.ComputeHash(ba.AccountNo));

                    // Add type-specific parameters
                    if (ba.GetType() == typeof(Current_Account))
                    {
                        Current_Account ca = (Current_Account)ba;
                        command.Parameters.AddWithValue("@overdraftAmount", ca.OverdraftAmount);
                        command.Parameters.AddWithValue("@interestRate", DBNull.Value);
                    }
                    else
                    {
                        Savings_Account sa = (Savings_Account)ba;
                        command.Parameters.AddWithValue("@overdraftAmount", DBNull.Value);
                        command.Parameters.AddWithValue("@interestRate", sa.InterestRate);
                    }

                    command.ExecuteNonQuery();

                    // ADD LOGGING
                }
                ba = null;
                // CALL GB COLLECTOR??
                return true;
            }
            catch (ArgumentException e)
            {
                // Specific error handling for missing or invalid data
                Console.Error.WriteLine($"Error: {e.Message}");
                Logger.LogError("An error occurred while adding a bank account. " + e);
            }
            catch (SqliteException e)
            {
                // Handle SQLite-specific errors (e.g., connection or SQL issues)
                Console.Error.WriteLine($"SQLite Error: {e.Message}");
                Logger.LogError("An error occurred while adding a bank account. " + e);
            }
            catch (Exception e)
            {
                // Handle unexpected errors
                Console.Error.WriteLine($"Unexpected Error: {e.Message}");
                Logger.LogError("An unexpected error occurred while adding a bank account. " + e);
            }
            return false;
        }

        //public Bank_Account findBankAccountByAccNo(String accNo)
        //{

        //    foreach (Bank_Account ba in accounts)
        //    {
        //        if (ba.accountNo.Equals(accNo))
        //        {
        //            return ba;
        //        }
        //    }
        //    return null;
        //}

        public Bank_Account findBankAccountByAccNo(String accNo)
        {
            try
            {
                if (!File.Exists(Data_Access_Layer.databaseName))
                    initialiseDatabase();
                else
                {
                    // UPDATED THIS SO IT ONLY GETS THE INDIVIDUAL RECORD WHEN REQUIRED
                    using (var connection = getDatabaseConnection())
                    {
                        connection.Open();
                        var command = connection.CreateCommand();
                        command.CommandText = "SELECT * FROM Encrypted_Bank_Accounts WHERE hashAccountNo = @hashAccountNo";
                        command.Parameters.AddWithValue("@hashAccountNo", cryptoUtils.ComputeHash(accNo));
                        SqliteDataReader dr = command.ExecuteReader();

                        while (dr.Read())
                        {
                            // Read the encrypted fields
                            byte[] encryptedAccountNo = (byte[])dr["accountNo"];
                            byte[] encryptedName = (byte[])dr["name"];
                            byte[]? encryptedAddressLine1 = dr["addressLine1"] as byte[];
                            byte[]? encryptedAddressLine2 = dr["addressLine2"] as byte[];
                            byte[]? encryptedAddressLine3 = dr["addressLine3"] as byte[];
                            byte[] encryptedTown = (byte[])dr["town"];
                            double balance = dr.GetDouble(dr.GetOrdinal("balance"));
                            int accountType = dr.GetInt32(dr.GetOrdinal("accountType"));
                            double? overdraftAmount = dr.IsDBNull(dr.GetOrdinal("overdraftAmount")) ? (double?)null : dr.GetDouble(dr.GetOrdinal("overdraftAmount"));
                            double? interestRate = dr.IsDBNull(dr.GetOrdinal("interestRate")) ? (double?)null : dr.GetDouble(dr.GetOrdinal("interestRate"));
                            byte[] storedHashValue = (byte[])dr["hashValue"];
                            byte[] accountNoHash = cryptoUtils.ComputeHash(accNo);

                            // Decrypt the fields
                            string decryptedAccountNo = Encoding.UTF8.GetString(cryptoUtils.Decrypt(encryptedAccountNo));
                            string decryptedName = Encoding.UTF8.GetString(cryptoUtils.Decrypt(encryptedName));
                            string? decryptedAddressLine1 = encryptedAddressLine1 != null ? Encoding.UTF8.GetString(cryptoUtils.Decrypt(encryptedAddressLine1)) : null;
                            string? decryptedAddressLine2 = encryptedAddressLine2 != null ? Encoding.UTF8.GetString(cryptoUtils.Decrypt(encryptedAddressLine2)) : null;
                            string? decryptedAddressLine3 = encryptedAddressLine3 != null ? Encoding.UTF8.GetString(cryptoUtils.Decrypt(encryptedAddressLine3)) : null;
                            string decryptedTown = Encoding.UTF8.GetString(cryptoUtils.Decrypt(encryptedTown));

                            // Combine decrypted fields for hash computation
                            byte[] computedHashValue = cryptoUtils.ComputeHash(
                                cryptoUtils.CombineByteArrays(
                                    Encoding.UTF8.GetBytes(decryptedAccountNo), // Use decrypted account number
                                    Encoding.UTF8.GetBytes(decryptedName),     // Use decrypted name
                                    decryptedAddressLine1 != null ? Encoding.UTF8.GetBytes(decryptedAddressLine1) : new byte[0], // Replace null with empty byte array
                                    decryptedAddressLine2 != null ? Encoding.UTF8.GetBytes(decryptedAddressLine2) : new byte[0],
                                    decryptedAddressLine3 != null ? Encoding.UTF8.GetBytes(decryptedAddressLine3) : new byte[0],
                                    Encoding.UTF8.GetBytes(decryptedTown),     // Use decrypted town
                                    BitConverter.GetBytes(balance),           // Use consistent binary representation
                                    BitConverter.GetBytes(accountType),
                                    BitConverter.GetBytes(overdraftAmount ?? 0.0), // Default null to 0.0
                                    BitConverter.GetBytes(interestRate ?? 0.0),   // Default null to 0.0
                                    cryptoUtils.ComputeHash(accNo) // Compute hash of decrypted account number
                                )
                            );

                            // Verify hash value
                            if (!cryptoUtils.CompareHashes(computedHashValue, storedHashValue))
                            {
                                throw new CryptographicException("Data integrity check failed. The hash value does not match.");
                            }

                            // Create and return the appropriate account type
                            if (accountType == Account_Type.Current_Account)
                            {
                                return new Current_Account
                                {
                                    AccountNo = decryptedAccountNo,
                                    Name = decryptedName,
                                    AddressLine1 = decryptedAddressLine1,
                                    AddressLine2 = decryptedAddressLine2,
                                    AddressLine3 = decryptedAddressLine3,
                                    Town = decryptedTown,
                                    Balance = balance,
                                    OverdraftAmount = overdraftAmount ?? 0.0
                                };
                            }
                            else if (accountType == Account_Type.Savings_Account)
                            {
                                return new Savings_Account
                                {
                                    AccountNo = decryptedAccountNo,
                                    Name = decryptedName,
                                    AddressLine1 = decryptedAddressLine1,
                                    AddressLine2 = decryptedAddressLine2,
                                    AddressLine3 = decryptedAddressLine3,
                                    Town = decryptedTown,
                                    Balance = balance,
                                    InterestRate = interestRate ?? 0.0
                                };
                            }
                            // LOGGING
                        }
                    }
                }
            }
            catch (SqliteException sqlEx)
            {
                // Handle database-specific errors
                Console.WriteLine($"Database error: {sqlEx.Message}");
                Logger.LogError("An error occurred while retrieving a bank account. " + sqlEx);
            }
            catch (CryptographicException cryptoEx)
            {
                // Handle encryption/decryption issues
                Console.WriteLine($"Cryptographic error: {cryptoEx.Message}");
                Logger.LogError("An error occurred while retrieving a bank account. " + cryptoEx);
            }
            catch (Exception ex)
            {
                // Catch all other exceptions
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                Logger.LogError("An unexpected error occurred while retrieving a bank account. " + ex);
            }
            return null;
        }

        //public bool closeBankAccount(String accNo)
        //{
        //    Bank_Account toRemove = null;

        //    foreach (Bank_Account ba in accounts)
        //    {
        //        if (ba.accountNo.Equals(accNo))
        //        {
        //            toRemove = ba;
        //            break;
        //        }
        //    }
        //    if (toRemove == null)
        //        return false;
        //    else
        //    {
        //        accounts.Remove(toRemove);
        //        using (var connection = getDatabaseConnection())
        //        {
        //            connection.Open();
        //            var command = connection.CreateCommand();
        //            command.CommandText = "DELETE FROM Bank_Accounts WHERE accountNo = '" + toRemove.accountNo + "'";
        //            command.ExecuteNonQuery();
        //        }
        //        return true;
        //    }
        //}

        public bool closeBankAccount(String accNo)
        {
            try
            {
                if (findBankAccountByAccNo(accNo) != null)
                {
                    using (var connection = getDatabaseConnection())
                    {
                        connection.Open();
                        var command = connection.CreateCommand();
                        // PREPARED STATEMENT FOR SQL INJECTION PREVENTION
                        command.CommandText = "DELETE * FROM Encrypted_Bank_Accounts WHERE hashAccountNo = @hashAccountNo";
                        command.Parameters.AddWithValue("@hashAccountNo", cryptoUtils.ComputeHash(accNo));
                        command.ExecuteNonQuery();

                        // ADD LOGGING
                    }
                    return true;
                }
                else { return false; }
            }
            catch (SqliteException sqlEx)
            {
                // Handle database-specific errors
                Console.WriteLine($"Database error: {sqlEx.Message}");
                Logger.LogError("An error occurred while closing a bank account. " + sqlEx);
            }
            catch (Exception ex)
            {
                // Log unexpected errors
                Console.WriteLine($"An unexpected error occurred when trying to close a bank account: {ex.Message}");
                Logger.LogError("An unexpected error occurred while closing a bank account. " + ex);
            }
            return false;
        }

        //public bool lodge(String accNo, double amountToLodge)
        //{
        //    Bank_Account toLodgeTo = null;
        //    foreach (Bank_Account ba in accounts)
        //    {
        //        if (ba.accountNo.Equals(accNo))
        //        {
        //            ba.lodge(amountToLodge);
        //            toLodgeTo = ba;
        //            break;
        //        }
        //    }
        //    if (toLodgeTo == null)
        //        return false;
        //    else
        //    {
        //        using (var connection = getDatabaseConnection())
        //        {
        //            connection.Open();
        //            var command = connection.CreateCommand();
        //            command.CommandText = "UPDATE Bank_Accounts SET balance = " + toLodgeTo.balance + " WHERE accountNo = '" + toLodgeTo.accountNo + "'";
        //            command.ExecuteNonQuery();
        //        }
        //        return true;
        //    }
        //}


        public bool lodge(String accNo, double amountToLodge)
        {
            Bank_Account toLodgeTo = findBankAccountByAccNo(accNo);
            try
            {
                if (toLodgeTo != null)
                {
                    toLodgeTo.lodge(amountToLodge);

                    // Recalculate the hash value
                    byte[] updatedHashValue = cryptoUtils.GenerateHashForSendingToDB(toLodgeTo);

                    // Update the database with the new balance and hash value
                    using (var connection = getDatabaseConnection())
                    {
                        connection.Open();
                        var command = connection.CreateCommand();
                        command.CommandText = @"UPDATE Encrypted_Bank_Accounts SET balance = @balance, hashValue = @hashValue WHERE hashAccountNo = @hashAccountNo";
                        command.Parameters.AddWithValue("@balance", toLodgeTo.Balance);
                        command.Parameters.AddWithValue("@hashValue", updatedHashValue);
                        command.Parameters.AddWithValue("@hashAccountNo", cryptoUtils.ComputeHash(accNo));
                        command.ExecuteNonQuery();
                    }
                    return true;
                }
                else { return false; }
                // CALL GB COLLECTOR??
            }
            catch (SqliteException sqlEx)
            {
                // Handle database-specific errors
                Console.WriteLine($"Database error: {sqlEx.Message}");
                Logger.LogError("An error occurred while trying to lodge to a bank account. " + sqlEx);
            }
            catch (Exception ex)
            {
                // Log unexpected errors
                Console.WriteLine($"An unexpected error occurred when trying to lodge to a bank account: {ex.Message}");
                Logger.LogError("An unexpected error occurred trying to lodge to a bank account. " + ex);
            }
            return false;
        }

        //public bool withdraw(String accNo, double amountToWithdraw)
        //{
        //    Bank_Account toWithdrawFrom = null;
        //    bool result = false;
        //    foreach (Bank_Account ba in accounts)
        //    {
        //        if (ba.accountNo.Equals(accNo))
        //        {
        //            result = ba.withdraw(amountToWithdraw);
        //            toWithdrawFrom = ba;
        //            break;
        //        }
        //    }
        //    if (toWithdrawFrom == null || result == false)
        //        return false;
        //    else
        //    {
        //        using (var connection = getDatabaseConnection())
        //        {
        //            connection.Open();
        //            var command = connection.CreateCommand();
        //            command.CommandText = "UPDATE Bank_Accounts SET balance = " + toWithdrawFrom.balance + " WHERE accountNo = '" + toWithdrawFrom.accountNo + "'";
        //            command.ExecuteNonQuery();
        //        }
        //        return true;
        //    }
        //}

        public bool withdraw(String accNo, double amountToWithdraw)
        {
            bool result = false;
            Bank_Account toWithdrawFrom = findBankAccountByAccNo(accNo);
            result = toWithdrawFrom.withdraw(amountToWithdraw);

            try
            {
                if (toWithdrawFrom != null || result == false)
                {
                    byte[] updatedHashValue = cryptoUtils.GenerateHashForSendingToDB(toWithdrawFrom);

                    using (var connection = getDatabaseConnection())
                    {
                        connection.Open();
                        var command = connection.CreateCommand();
                        // Use prepared statement to prevent SQL injection
                        command.CommandText = "UPDATE Encrypted_Bank_Accounts SET balance = @balance, hashValue = @hashValue WHERE hashAccountNo = @hashAccountNo";
                        command.Parameters.AddWithValue("@balance", toWithdrawFrom.Balance);
                        command.Parameters.AddWithValue("@hashValue", updatedHashValue);
                        command.Parameters.AddWithValue("@hashAccountNo", cryptoUtils.ComputeHash(accNo));
                        command.ExecuteNonQuery();
                    }
                    return true;
                }
                else { return false; }
            }
            catch (SqliteException sqlEx)
            {
                // Handle database-specific errors
                Console.WriteLine($"Database error: {sqlEx.Message}");
                Logger.LogError("An error occurred while trying to withdraw from a bank account. " + sqlEx);
            }
            catch (Exception ex)
            {
                // Log unexpected errors
                Console.WriteLine($"An unexpected error occurred when trying to withdraw from a bank account: {ex.Message}");
                Logger.LogError("An unexpected error occurred trying to withdraw from a bank account. " + ex);
            }
            return false;
        }
    }
}
