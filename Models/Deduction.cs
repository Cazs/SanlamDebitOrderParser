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
    }
}
