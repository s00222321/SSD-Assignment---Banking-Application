﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking_Application
{
    public sealed class Current_Account: Bank_Account // MARKED CLASS AS SEALED
    {
        // USE LOGGING!!!!

        public double overdraftAmount; // PUBLIC? - CHANGE TO PRIVATE OR CONTROLLED SETTER

        public Current_Account(): base()
        {

        }
        
        public Current_Account(String name, String address_line_1, String address_line_2, String address_line_3, String town, double balance, double overdraftAmount) : base(name, address_line_1, address_line_2, address_line_3, town, balance)
        {
            // VALIDATION?? CHECK FOR NULLS?
            this.overdraftAmount = overdraftAmount;
        }

        public override bool withdraw(double amountToWithdraw)
        {
            double avFunds = getAvailableFunds();

            if (avFunds >= amountToWithdraw)
            {
                balance -= amountToWithdraw;
                return true;
            }

            else
                return false;

        }

        public override double getAvailableFunds() // PROTECTED OR INTERNAL?
        {
            return (base.balance + overdraftAmount);
        }

        public override String ToString()
        {

            return base.ToString() +
                "Account Type: Current Account\n" +
                "Overdraft Amount: " + overdraftAmount + "\n"; // USE STRINGBUILDER?? DON'T GIVE SENSITIVE DATA

        }

    }
}
