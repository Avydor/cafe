using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CafeSimulation.Models;
using CafeSimulation.Services;

// ГОЛОВНА ПРОГРАМА
class Program
{
    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        CoffeeShop shop = new CoffeeShop();
        

        shop.AddRandomBotOrder();
        shop.AddRandomBotOrder();

        bool running = true;

        while (running)
        {
            Console.Clear();
            Console.WriteLine("=================================");
            Console.WriteLine("               CAFE              ");
            Console.WriteLine("=================================");
            Console.WriteLine("1. Переглянути меню");
            Console.WriteLine("2. Переглянути чергу");
            Console.WriteLine("3. Зробити замовлення");
            Console.WriteLine("4. Приготувати наступне замовлення");
            Console.WriteLine("5. Історія замовлень");
            Console.WriteLine("6. Виручка");
            Console.WriteLine("7. Скасувати замовлення");
            Console.WriteLine("8. Найпопулярніший напій");
            Console.WriteLine("0. Вихід");
            Console.WriteLine("---------------------------------");
            Console.Write("Виберіть дію: ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.Clear();
                    shop.ShowMenu();
                    Pause();
                    break;

                case "2":
                    Console.Clear();
                    shop.ShowActiveOrders();
                    Pause();
                    break;

                case "3":
                    Console.Clear();
                    shop.ShowMenu();
                    Console.WriteLine("\n--- ОФОРМЛЕННЯ ---");

                    string name;
                    while (true)
                    {
                        Console.Write("Введіть ім'я: ");
                        name = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(name) && !name.Any(char.IsDigit))
                            break;
                        Console.WriteLine("Помилка: ім'я не повинно містити цифр або бути порожнім!");
                    }

                    decimal balance;
                    while (true)
                    {
                        Console.Write("Введіть ваш баланс (грн): ");
                        if (decimal.TryParse(Console.ReadLine(), out balance))
                            break;
                        Console.WriteLine("Помилка: введіть коректне число для балансу!");
                    }

                    string isVip;
                    while (true)
                    {
                        Console.Write("Ви VIP-клієнт? (1 - так, 0 - ні): ");
                        isVip = Console.ReadLine()?.Trim();
                        if (isVip == "1" || isVip == "0")
                            break;
                        Console.WriteLine("Помилка: можна ввести лише 1 чи 0!");
                    }

                    Console.Write("Виберіть ID позиції або СЕТУ: ");
                    if (!int.TryParse(Console.ReadLine(), out int drinkId))
                    {
                        Console.WriteLine("\nНекоректний ID!");
                        Pause();
                        break;
                    }

                    Console.Write("Введіть кількість (шт.): ");
                    if (!int.TryParse(Console.ReadLine(), out int quantity) || quantity < 1)
                    {
                        quantity = 1;
                    }

                    MenuItem selectedItem = shop.Menu.FirstOrDefault(m => m.Id == drinkId);

                    if (selectedItem != null)
                    {
                        Customer customer = (isVip == "1")
                            ? new VipCustomer(name + " (VIP)", balance)
                            : new Customer(name, balance);

                        Order newOrder = new Order(shop.GetNextOrderNumber(), customer, selectedItem, quantity);

                        shop.OrderHistory.Add(newOrder);

                        if (newOrder.Status == "У черзі")
                        {
                            shop.ActiveOrders.Enqueue(newOrder);
                            Console.WriteLine($"\n[+] Замовлення додано! Код: #{newOrder.Code} (x{newOrder.Quantity}). До сплати: {newOrder.FinalPrice} грн. Ваша позиція в черзі: {shop.ActiveOrders.Count}");
                        }
                        else
                        {
                            Console.WriteLine($"\n[X] Недостатньо коштів! Код замовлення #{newOrder.Code} відхилено.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("\nПозицію з таким ID не знайдено!");
                    }

                    Pause();
                    break;

                case "4":
                    Console.Clear();
                    shop.ProcessOrder();
                    Pause();
                    break;

                case "5":
                    Console.Clear();
                    shop.ShowHistory();
                    Pause();
                    break;

                case "6":
                    Console.Clear();
                    Console.WriteLine($"Загальна виручка: {shop.TotalRevenue} грн");
                    Pause();
                    break;

                case "7":
                    Console.Clear();
                    shop.ShowActiveOrders();
                    Console.WriteLine("\n--- СКАСУВАННЯ ЗАМОВЛЕННЯ ---");
                    Console.Write("Введіть код замовлення для скасування (наприклад, ES-101): ");
                    string codeToCancel = Console.ReadLine()?.Trim();

                    var message = shop.CancelOrder(codeToCancel)
                        ? $"\n[✓] Замовлення #{codeToCancel} успішно скасовано, кошти повернуто клієнту."
                        : $"\n[X] Замовлення з кодом #{codeToCancel} не знайдено в черзі.";

                    Console.WriteLine(message);

                    Pause();
                    break;

                case "8":
                    Console.Clear();
                    shop.ShowMostPopularItem();
                    Pause();
                    break;

                case "0":
                    running = false;
                    break;

               
            }
        }
    }

    static void Pause()
    {
        Console.WriteLine("\nНатисніть Enter, щоб повернутися...");
        Console.ReadLine();
    }
}
