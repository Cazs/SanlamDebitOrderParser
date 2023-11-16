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

    var dodGrouped = debitOrder.Deductions.GroupBy(d => d.BankName);

    foreach (var dod in dodGrouped)
    {
        var groupTotal = dod.Sum(d => d.Amount);
        var maxNameLength = dod.Key.Length <= 16 ? dod.Key.Length : 16;
        var maxRecordCountLength = dod.Count().ToString().Length <= 3 ? dod.Count().ToString().Length : 3;
        var maxGroupTotalLength = groupTotal.ToString().Replace(".", "").Length <= 10 ? groupTotal.ToString().Replace(".", "").Length : 10;

        Console.WriteLine(dod.Key.ToUpper().Substring(0, maxNameLength) + " " + String.Concat(dod.Count().ToString().Substring(0, maxRecordCountLength).PadLeft(3, '0'), groupTotal.ToString().Replace(".", "").Substring(0, maxGroupTotalLength).PadLeft(10, '0')));

        dod.OrderBy(d => d.getSurname())
            .OrderBy(d => d.Amount)
            .ToList()
            .ForEach(elem =>
                Console.WriteLine($"{String.Concat(elem.getInitial(), elem.getSurname())} {elem.getAccountNumber()} {elem.AccountType} {elem.getBranchName()} {elem.getAmount()} {elem.getDebitOrderDate()}"));
    }
}
