using System.Text;
using System.Xml;
using System.Xml.XPath;
using SanlamSkyDebitOrderParser.Models;

XmlDocument xmlDoc = new XmlDocument();
xmlDoc.Load("debitorders file.xml");

XmlNodeList debitOrdersXmlElements = xmlDoc.GetElementsByTagName("debitorders");

List<DebitOrder> debitOrdersList = new List<DebitOrder>();
DebitOrder debitOrder;
HashSet<Deduction> deductionsSet;

foreach (XmlElement doitem in debitOrdersXmlElements)
{
    debitOrder = new DebitOrder();
    deductionsSet = new HashSet<Deduction>();

    XmlNodeList deductionsXml = doitem.GetElementsByTagName("deduction");

    foreach (XmlNode ditem in deductionsXml)
    {
        if (ditem != null)
        {
            try
            {
                string accountholder = ditem.SelectSingleNode("accountholder").InnerText;
                string accountnumber = ditem.SelectSingleNode("accountnumber").InnerText;
                string accounttype = ditem.SelectSingleNode("accounttype").InnerText;
                string bankname = ditem.SelectSingleNode("bankname").InnerText;
                string branch = ditem.SelectSingleNode("branch").InnerText;
                string amount = ditem.SelectSingleNode("amount").InnerText;
                string date = ditem.SelectSingleNode("date").InnerText;

                if (accountholder != null
                    & accountnumber != null
                    & accounttype != null
                    & bankname != null
                    & branch != null
                    & amount != null
                    & date != null)
                {
                    deductionsSet.Add(new Deduction(accountholder,
                                              Int64.Parse(accountnumber),
                                              accounttype,
                                              bankname,
                                              branch,
                                              Double.Parse(amount),
                                              DateTime.Parse(date)));
                }
            } catch (XPathException ex)
            {
                Console.WriteLine("[ERROR] Could not get XML Object due to a Path error: " + ex.Message);
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine("[ERROR] Could not get XML Object due to a null error: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERROR] Could not get XML Object due to an error: " + ex.Message);
            }
        }
    }

    debitOrder.Deductions = deductionsSet;
    debitOrdersList.Add(debitOrder);

    var groupedDebitOrderDeductions = debitOrder.Deductions.GroupBy(d => d.BankName);

    foreach (var debitOrderDeduction in groupedDebitOrderDeductions)
    {
        StringBuilder bankDebitOrders = new StringBuilder();

        var groupTotal = debitOrderDeduction.Sum(dod => dod.Amount);
        var maxNameLength = debitOrderDeduction.Key.Length <= 16 ? debitOrderDeduction.Key.Length : 16;
        var maxRecordCountLength = debitOrderDeduction.Count().ToString().Length <= 3 ? debitOrderDeduction.Count().ToString().Length : 3;
        var maxGroupTotalLength = groupTotal.ToString().Replace(".", "").Length <= 10 ? groupTotal.ToString().Replace(".", "").Length : 10;

        string bankHeaderInfo = debitOrderDeduction.Key.ToUpper().Substring(0, maxNameLength) + " " + String.Concat(groupedDebitOrderDeductions.Count().ToString().Substring(0, maxRecordCountLength).PadLeft(3, '0'), groupTotal.ToString().Replace(".", "").Substring(0, maxGroupTotalLength).PadLeft(10, '0'));
        bankDebitOrders.AppendLine(bankHeaderInfo);

        debitOrderDeduction
            .OrderBy(dod => dod.getSurname())
            .OrderBy(dod => dod.Amount)
            .ToList()
            .ForEach(dod => {
                var debitOrderInfo = String.Concat(dod.getInitial(), dod.getSurname())
                    + " " + dod.getAccountNumber()
                    + " " + dod.getAccountType()
                    + " " + dod.getBranchName()
                    + " " + dod.getAmount()
                    + dod.getDebitOrderDate();

                bankDebitOrders.AppendLine(debitOrderInfo);
            });

        string bankDebitOrdersFilePath = string.Format(".\\{0}.txt", debitOrderDeduction.Key);
        File.WriteAllText(bankDebitOrdersFilePath, bankDebitOrders.ToString());
    }
}
