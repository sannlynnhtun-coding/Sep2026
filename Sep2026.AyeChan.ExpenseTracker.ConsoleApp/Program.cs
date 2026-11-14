using Sep2026.AyeChan.ExpenseTracker.ConsoleApp;

var reportService = new ReportService();

reportService.MonthlyReport(2026, 11);
Console.WriteLine();
reportService.MonthlyReport(2026, 10);
Console.WriteLine();
reportService.MonthlyReport(2026, 1);
Console.WriteLine();
reportService.MonthlyReport(2026, 13);