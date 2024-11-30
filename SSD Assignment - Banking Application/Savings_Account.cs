using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking_Application
{
    public sealed class Savings_Account: Bank_Account  // MAKRED CLASS AS SEALED
    {

        public double interestRate; // SHOULD NOT BE PUBLIC?? CAN'T BE NON-NEGATIVE

       public Savings_Account(): base()
        {

        }
        
        public Savings_Account(String name, String address_line_1, String address_line_2, String address_line_3, String town, double balance, double interestRate) : base(name, address_line_1, address_line_2, address_line_3, town, balance)
        {
            // VALIDATION?? CHECK FOR NULLS?
            this.interestRate = interestRate;
        }
        public override double getAvailableFunds() // PUBLIC????
        {
            return base.balance;
        }

        public override bool withdraw(double amountToWithdraw)
        {
            // ADD LOGGING AND ERROR HANDLING
            double avFunds = getAvailableFunds();

            if (avFunds >= amountToWithdraw)
            {
                balance -= amountToWithdraw;
                return true;
            }

            else
                return false;
        }

        public override String ToString()
        {
            // NO SENSITIVE DATA AND BETTER CONCATINATION
            return base.ToString() + 
                "Account Type: Savings Account\n" +
                "Interest Rate: " + interestRate + "\n";

        }


    }
}
