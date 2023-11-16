namespace SanlamSkyDebitOrderParser.Models
{
    class Deduction
    {
        public Deduction(
            string accountholder,
            Int64 accountnumber,
            string accounttype,
            string bankname,
            string branch,
            double amount,
            DateTime date)
        {
            AccountHolder = accountholder;
            AccountNumber = accountnumber;
            AccountType = accounttype;
            BankName = bankname;
            Branch = branch;
            Amount = amount;
            Date = date;
        }

        public string AccountHolder { get; set; }
        public Int64 AccountNumber { get; set; }
        public string AccountType { get; set; }
        public string BankName { get; set; }
        public string Branch { get; set; }
        public double Amount { get; set; }
        public DateTime Date { get; set; }

        public char getInitial() {
            return AccountHolder.Split(' ')[0].ToCharArray()[0];
        }

        public string getSurname()
        {
            string surname = AccountHolder.Split(' ')[AccountHolder.Split(' ').Length - 1]?.Replace(" ", "");
            int maxSurnamePadding = surname.Length <= 15 ? surname.Length : 15;
            return surname.Substring(0, maxSurnamePadding).PadRight(15, ' ');
        }

        public string getAccountNumber()
        {
            string accountNumber = AccountNumber.ToString();
            int maxAccountNumberPadding = accountNumber.Length <= 14 ? accountNumber.Length : 14;
            return accountNumber.Substring(0, maxAccountNumberPadding).PadRight(14, ' ');
        }

        public string getAccountType()
        {
            if (AccountType != null & AccountType.Split(" ").Length > 0)
            {
                switch (AccountType.ToLower().Split(" ")[0])
                {
                    case "cheque":
                        return " CH";
                    case "savings":
                        return "SAV";
                    case "credit":
                        return " CR";
                    default:
                        return "OTH";
                }
            } else
            {
                return "OTH";
            }
        }

        public string getBranchName()
        {
            int maxBranchPadding = Branch.Length <= 10 ? Branch.Length : 10;
            return Branch.Substring(0, maxBranchPadding).PadRight(10, ' ');
        }

        public string getAmount()
        {
            string amount = (Amount * 100).ToString()?.Replace(".", "");
            int maxAmountPadding = amount.Length <= 7 ? amount.Length : 7;
            return amount.Substring(0, maxAmountPadding).PadLeft(7, '0');
        }

        public string getDebitOrderDate()
        {
            return Date.ToString("dd/yyyy/MM").Replace("/", "");
        }
    }
}
