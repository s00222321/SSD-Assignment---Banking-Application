using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking_Application
{
    public abstract class Bank_Account
    {
        // MAKE PRIVATE AND READ-ONLY?
        public String accountNo;
        public String name;
        public String address_line_1;
        public String address_line_2;
        public String address_line_3;
        public String town;
        public double balance;

        public Bank_Account()
        {

        }
        
        public Bank_Account(String name, String address_line_1, String address_line_2, String address_line_3, String town, double balance)
        {
            // VALIDATE AND SANITIZE
            this.accountNo = System.Guid.NewGuid().ToString(); // DIRECTLY MODIFYABLE - ENCAPSULATION - VALIDATION AND FORMATTING
            this.name = name;
            this.address_line_1 = address_line_1;
            this.address_line_2 = address_line_2;
            this.address_line_3 = address_line_3;
            this.town = town;
            this.balance = balance; // DIRECTLY MODIFYABLE - ENCAPSULATION
        }

        // AUTHORISATION!!!
        public void lodge(double amountIn)
        {
            // VALIDATE
            balance += amountIn;
            // EXCEPTION HANDLING??
            // LOGGING AND AUDITING
        }

        public abstract bool withdraw(double amountToWithdraw);

        public abstract double getAvailableFunds();

        public override String ToString()
        {
            // SHOULD NOT BE SHOWING SENSITIVE DATA
            return "\nAccount No: " + accountNo + "\n" +
            "Name: " + name + "\n" +
            "Address Line 1: " + address_line_1 + "\n" +
            "Address Line 2: " + address_line_2 + "\n" +
            "Address Line 3: " + address_line_3 + "\n" +
            "Town: " + town + "\n" +
            "Balance: " + balance + "\n";

    }

    }
}
