using System.Text;
using System.Xml;
using System.Xml.XPath;
using SanlamSkyDebitOrderParser.Models;
using Serilog;

using var errorLogger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("error-log.txt")
    .CreateLogger();

XmlDocument xmlDoc = new XmlDocument();
xmlDoc.Load("debitorders file.xml");

XmlNodeList debitOrdersXmlElements = xmlDoc.GetElementsByTagName("debitorders");

List<DebitOrder> debitOrdersList = new List<DebitOrder>();
DebitOrder debitOrder;
HashSet<Deduction> deductionsSet;

foreach (XmlElement debitOrderXmlElem in debitOrdersXmlElements)
{
    debitOrder = new DebitOrder();
    deductionsSet = new HashSet<Deduction>();

    XmlNodeList deductionsXml = debitOrderXmlElem.GetElementsByTagName("deduction");

    foreach (XmlNode deductionXmlElem in deductionsXml)
    {
        if (deductionXmlElem != null)
        {
            try
            {
                string accountholder = deductionXmlElem.SelectSingleNode("accountholder").InnerText;
                string accountnumber = deductionXmlElem.SelectSingleNode("accountnumber").InnerText;
                string accounttype = deductionXmlElem.SelectSingleNode("accounttype").InnerText;
                string bankname = deductionXmlElem.SelectSingleNode("bankname").InnerText;
                string branch = deductionXmlElem.SelectSingleNode("branch").InnerText;
                string amount = deductionXmlElem.SelectSingleNode("amount").InnerText;
                string date = deductionXmlElem.SelectSingleNode("date").InnerText;

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
                } else
                {
                    errorLogger.Error("[ERROR] Invalid XML Data.");
                }
            } catch (XPathException ex)
            {
                Console.WriteLine("[ERROR] Could not get XML Object due to a path error: " + ex.Message);
                errorLogger.Error("[ERROR] Could not get XML Object due to a path error: " + ex.Message);
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine("[ERROR] Could not get XML Object due to a null error: " + ex.Message);
                errorLogger.Error("[ERROR] Could not get XML Object due to a null error: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERROR] Could not get XML Object due to an error: " + ex.Message);
                errorLogger.Error("[ERROR] Could not get XML Object due to an error: " + ex.Message);
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
 
        // File.WriteAllText(bankDebitOrdersFilePath, bankDebitOrders.ToString());

        using var bankDebitOrderLogger = new LoggerConfiguration()
            //.WriteTo.Console()
            .WriteTo.File(bankDebitOrdersFilePath)
            .CreateLogger();

        bankDebitOrderLogger.Write(Serilog.Events.LogEventLevel.Information, String.Concat("\r\n", bankDebitOrders.ToString()));
        await bankDebitOrderLogger.DisposeAsync();
    }
}
