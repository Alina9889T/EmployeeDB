using EmployeeDB.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;

namespace EmployeeDB
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello! You are starting to work with Employees Database");

            bool inConfig = true;
            bool inMenu = false;
            while (inConfig)
            {
                Console.WriteLine("Please specify MS SQL Server name");
                string input = (Console.ReadLine() ?? string.Empty).Trim();
                string SQLServerName = input.IsNullOrEmpty() ? "localhost" : input;

                Console.WriteLine($"Connecting to MS SQL Server {SQLServerName}...");
                try
                {
                    using (var context = new Context(SQLServerName))
                    {
                        Console.WriteLine($"Connected to MS SQL Server {SQLServerName}\r\n");
                        Console.WriteLine("Press any key to go to Actions menu");
                        Console.ReadKey();
                        inMenu = true;

                        while (inMenu)
                        {
                            ShowActionsMenu();
                            string menuAction = Console.ReadLine() ?? string.Empty;
                            switch (menuAction)
                            {
                                case "1":
                                    await CreateNewEmployeeAsync(context);
                                    break;
                                
                                case "2":
                                    ShowEmployeesList(context.Employees);
                                    break;
                                
                                case "3":
                                    await UpdateEmployeeAsync(context);
                                    break;
                                
                                case "4":
                                    await DeleteEmployeeAsync(context);
                                    break;
                                
                                case "R": 
                                case "r":
                                    inMenu = false;
                                    Console.Clear();
                                    break;
                                
                                case "E": 
                                case "e":
                                    inMenu = false;
                                    inConfig = false;
                                    break;
                                
                                default:
                                    Console.WriteLine("Choose another option");
                                    break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\r\nCan't continue working due to an exception");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("\r\nPress any key to retry");
                    Console.ReadKey();
                    Console.Clear();
                }
            }
        }

        #region Create-Update-Delete
        static async Task CreateNewEmployeeAsync(Context context)
        {
            if (context == null)
            {
                Console.WriteLine("For some unexpected reason DB context is null");
                Console.ReadKey();
                return;
            }

            Console.Clear();

            var employee = new Employee();
            employee.FirstName = AskUserForStringValue("Employee's first name");
            employee.LastName = AskUserForStringValue("Employee's last name");
            employee.Email = AskUserForStringValue("Employee's email");  
            employee.DateOfBirth = DateOnly.FromDateTime(AskUserForDateValue("Employee's date of birth"));            
            employee.Salary = AskUserForDecimalValue("Employee's salary");

            await context.AddEmployeeAsync(employee);
            Console.WriteLine(" \r\nEmployee created: \r\n");
            ShowEmployee(employee);
            Console.WriteLine("\r\nPress any key to return to Actions menu");
            Console.ReadKey();
        }

        static async Task UpdateEmployeeAsync(Context context)
        {
            if (context == null)
            {
                Console.WriteLine("For some unexpected reason DB context is null");
                Console.ReadKey();
                return;
            }

            if (context.Employees.Count() == 0)
            {
                Console.WriteLine("There are no employees in the Database");
                Console.ReadKey();
                return;
            }

            Console.Clear();
            Employee employee = AskUserForEmployeeById(context);  

            Console.WriteLine("Employee found: \r\n");
            ShowEmployee(employee);
            Console.WriteLine();

            bool inUpdate = true;

            ShowUpdateMenu();
            while (inUpdate)
            { 
                string menuAction = Console.ReadLine() ?? string.Empty;
                switch (menuAction)
                {
                    case "1":
                        employee.FirstName = AskUserForStringValue("Employee's first name");
                        Console.WriteLine("Choose any action from Update menu to continue update");
                        break;

                    case "2":
                        employee.LastName = AskUserForStringValue("Employee's last name");
                        Console.WriteLine("Choose any action from Update menu to continue update");
                        break;

                    case "3":
                        employee.Email = AskUserForStringValue("Employee's email");
                        Console.WriteLine("Choose any action from Update menu to continue update");
                        break;

                    case "4":
                        employee.DateOfBirth = DateOnly.FromDateTime(AskUserForDateValue("Employee's date of birth"));
                        Console.WriteLine("Choose any action from Update menu to continue update");
                        break;

                    case "5":
                        employee.Salary = AskUserForDecimalValue("Employee's salary");
                        Console.WriteLine("Choose any action from Update menu to continue update");
                        break;

                    case "A":
                    case "a":
                        await context.UpdateEmployeeAsync(employee);

                        Console.WriteLine("\r\nEmployee updated: \r\n");
                        ShowEmployee(employee);
                        Console.WriteLine("\r\nPress any key to return to Actions menu");
                        Console.ReadKey();
                        inUpdate = false;
                        break;

                    case "C":
                    case "c":
                        context.Entry(employee).Reload();
                        Console.WriteLine("Employee is NOT updated");
                        Console.WriteLine("Press any key to return to Actions menu");
                        Console.ReadKey();
                        inUpdate = false;
                        break;

                    default:
                        Console.WriteLine("Choose another option");
                        break;
                }
            }
        }

        static async Task DeleteEmployeeAsync(Context context)
        {
            if (context == null)
            {
                Console.WriteLine("For some unexpected reason DB context is null");
                Console.ReadKey();
                return;
            }

            if (context.Employees.Count() == 0)
            {
                Console.WriteLine("There are no employees in the Database");
                Console.ReadKey();
                return;
            }

            Console.Clear();
            Employee employee = AskUserForEmployeeById(context);
            Console.WriteLine("Employee found: \r\n");
            ShowEmployee(employee);

            Console.WriteLine("\r\nDo you really want to delete this employee? Press Y to confrim or any other button to return to Actions menu");
            string input = Console.ReadLine() ?? string.Empty;
            if (input == "Y" || input == "y")
            {
                await context.DeleteEmployeeAsync(employee);
                Console.WriteLine("Employee deleted succesfully. Press any key to return to Actions menu");
                Console.ReadKey();
            }
            else
            {
                return;
            }
        }
        #endregion

        #region Utilities
        static void ShowActionsMenu()
        {
            Console.Clear();
            Console.WriteLine("Please choose action from menu:");
            Console.WriteLine("1 - Create new employee");
            Console.WriteLine("2 - Show all employees");
            Console.WriteLine("3 - Update employee");
            Console.WriteLine("4 - Delete employee");
            Console.WriteLine("R - Return to MS SQL Server configuration");
            Console.WriteLine("E - Exit");
        }

        static void ShowUpdateMenu()
        {
            Console.WriteLine("What information would you like to change?");
            Console.WriteLine("1 - First name");
            Console.WriteLine("2 - Last name");
            Console.WriteLine("3 - Email");
            Console.WriteLine("4 - Date of birth");
            Console.WriteLine("5 - Salary");
            Console.WriteLine("A - Apply changes");
            Console.WriteLine("C - Cancel changes");
        }

        static void PrintHeader()
        {
            Console.WriteLine(string.Format("|{0,6}|{1,15}|{2,15}|{3,30}|{4,15}|{5,15}|", 
                "Id", 
                "First name", 
                "Last name", 
                "Email", 
                "Date of birth", 
                "Salary"));
            Console.WriteLine(new string('-', 103));
        }

        static void PrintEmployee(Employee employee)
        {
            Console.WriteLine(string.Format("|{0,6}|{1,15}|{2,15}|{3,30}|{4,15}|{5,15}|", 
                employee.EmployeeID, 
                employee.FirstName, 
                employee.LastName, 
                employee.Email, 
                employee.DateOfBirth, 
                string.Format("{0:0.00}", 
                employee.Salary)));
        }

        static void ShowEmployee(Employee employee)
        {
            PrintHeader();
            PrintEmployee(employee);
        }

        static void ShowEmployeesList(DbSet<Employee> Employees)
        {
            Console.Clear();
            Console.WriteLine("Employees list: \r\n");
            PrintHeader();
            foreach (var emp in Employees)
            {
                PrintEmployee(emp);
            }
            Console.WriteLine("\r\nPress any key to return to Actions menu");
            Console.ReadKey();
        }

        static string AskUserForStringValue(string valueName)
        {
            Console.WriteLine($"Enter {valueName}:");
            string value = (Console.ReadLine() ?? string.Empty).Trim();
            while (value.IsNullOrEmpty())
            {
                Console.WriteLine($"{valueName} can't be empty. Please try again:");
                value = (Console.ReadLine() ?? string.Empty).Trim();
            }
            return value;
        }

        static DateTime AskUserForDateValue(string valueName)
        {
            DateTime value = default;
            while (true)
            {
                Console.WriteLine($"Enter {valueName} in format dd.MM.YYYY:");
                if (DateTime.TryParse(Console.ReadLine(), new CultureInfo("ru-RU", false), out value))
                {
                    return value;
                }
                else
                {
                    Console.WriteLine("You have entered an incorrect date. Please try again");
                }
            }
        }

        static decimal AskUserForDecimalValue(string valueName)
        {
            decimal value = 0;

            while (true)
            {
                Console.WriteLine($"Enter {valueName}:");
                if (decimal.TryParse(Console.ReadLine(),
                            NumberStyles.Any,
                            CultureInfo.InvariantCulture,
                            out value))
                {
                    return value;
                }
                else
                {
                    Console.WriteLine("You have entered an incorrect decimal value. Please try again");
                }
            }
        }

        static Employee AskUserForEmployeeById(Context context)
        {
            Console.WriteLine("Enter employee's Id:");
            int employeeId = -1;

            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out employeeId))
                {
                    var employee = context.GetEmployeeById(employeeId);
                    if (employee != null)
                    {
                        return employee;
                    }
                    else
                    {
                        Console.WriteLine("Employee with such Id doesn't exist. Please try again");
                    }
                }
                else
                {
                    Console.WriteLine("You have entered an incorrect employee's Id. Please try again");
                }
            }
        }
        
        #endregion
    }
}
