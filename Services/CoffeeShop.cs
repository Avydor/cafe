using System;
using System.Collections.Generic;
using System.Linq;
using CafeSimulation.Models; 
using System.IO;

namespace CafeSimulation.Services
{
    public class CoffeeShop
    {
        // 1. Поля и свойства (хранение данных)
        public List<MenuItem> Menu { get; private set; }
        public Queue<Order> ActiveOrders { get; private set; }
        public List<Order> OrderHistory { get; private set; }
        public decimal TotalRevenue { get; private set; }

        private int orderCounter = 100;

        // 2. Конструктор (инициализация и наполнение меню)
        public CoffeeShop()
        {
            Menu = new List<MenuItem>();
            ActiveOrders = new Queue<Order>();
            OrderHistory = new List<Order>();
            TotalRevenue = 0m;

            // Наполняем начальное меню
            Menu.Add(new CoffeeItem(1, "Еспресо", 45m, "ES", MilkType.Super));
            Menu.Add(new CoffeeItem(2, "Лате", 65m, "LA", MilkType.SuperPuper));
            Menu.Add(new BakeryItem(3, "Круасан", 55m, "KR", false));
            Menu.Add(new BakeryItem(4, "Печиво Безглютенове", 40m, "BG", true));

            var coffee = Menu[0];
            var bakery = Menu[2];
            Menu.Add(new ComboItem(5, "Кава + Круасан", coffee, bakery, 0.15m));
        }

        // 3. Генератор следующего номера заказа
        public int GetNextOrderNumber()
        {
            orderCounter++;
            return orderCounter;
        }

        // 4. Добавление случайного заказа от бота
        public void AddRandomBotOrder()
        {
            string[] names = { "Олег", "Ірина", "Максим", "Софія", "Данило" };
            string randomName = names[Random.Shared.Next(0, names.Length)];

            Customer bot = Random.Shared.Next(0, 2) == 0
                ? new VipCustomer(randomName, 500m)
                : new Customer(randomName, 300m);

            if (Menu.Count == 0) return;

            MenuItem item = Menu[Random.Shared.Next(0, Menu.Count)];
            int quantity = Random.Shared.Next(1, 3);

            Order botOrder = new Order(GetNextOrderNumber(), bot, item, quantity);

            ActiveOrders.Enqueue(botOrder);
            OrderHistory.Add(botOrder);
        }

        // 5. Показать меню
        public void ShowMenu()
        {
            Console.WriteLine("=== МЕНЮ КАФЕ ===");
            foreach (var item in Menu)
            {
                Console.WriteLine(item.GetDescription());
            }
        }

        // 6. Показать активную очередь
        public void ShowActiveOrders()
        {
            Console.WriteLine("=== АКТИВНІ ЗАМОВЛЕННЯ (У ЧЕРЗІ) ===");
            if (ActiveOrders.Count == 0)
            {
                Console.WriteLine("Черга порожня.");
                return;
            }

            foreach (var order in ActiveOrders)
            {
                Console.WriteLine($"[{order.Code}] {order.Customer.Name} | {order.Item.Name} x{order.Quantity} | До сплати: {order.FinalPrice} грн | Статус: {order.Status}");
            }
        }

        // 7. Показать всю историю
        public void ShowHistory()
        {
            Console.WriteLine("=== ІСТОРІЯ ВСІХ ЗАМОВЛЕНЬ ===");
            if (OrderHistory.Count == 0)
            {
                Console.WriteLine("Історія порожня.");
                return;
            }

            foreach (var order in OrderHistory)
            {
                Console.WriteLine($"[{order.Code}] {order.Customer.Name} | {order.Item.Name} x{order.Quantity} | {order.FinalPrice} грн | Статус: {order.Status}");
            }
            Console.WriteLine($"\nЗагальна виручка каси: {TotalRevenue} грн");
        }

        // 8. Обработать (приготовить) следующий заказ из очереди
        public void ProcessOrder()
        {
            if (ActiveOrders.Count == 0)
            {
                Console.WriteLine("Немає активних замовлень для обробки.");
                return;
            }

            Order order = ActiveOrders.Dequeue();
            order.Status = "Виконано";
            TotalRevenue += order.FinalPrice;

            Console.WriteLine($"[+] Замовлення {order.Code} ({order.Item.Name}) успішно виконано!");
        }

        // 9. Отмена заказа по коду
        public bool CancelOrder(string orderCode)
        {
            if (string.IsNullOrWhiteSpace(orderCode)) return false;

            var orderToCancel = ActiveOrders.FirstOrDefault(o =>
                o.Code.Equals(orderCode, StringComparison.OrdinalIgnoreCase));

            if (orderToCancel == null) return false;

            // Пересобираем очередь без отмененного заказа
            ActiveOrders = new Queue<Order>(
                ActiveOrders.Where(o => !o.Code.Equals(orderToCancel.Code, StringComparison.OrdinalIgnoreCase))
            );

            // Возвращаем деньги
            orderToCancel.Customer.Balance += orderToCancel.FinalPrice;
            orderToCancel.Status = "Скасовано";

            return true;
        }

        // 10. Показать самый популярный товар
        public void ShowMostPopularItem()
        {
            Console.WriteLine("=== ТОП ПРОДАЖІВ ===");
            var popular = OrderHistory
                .Where(o => o.Status.Equals("Виконано", StringComparison.OrdinalIgnoreCase) ||
                            o.Status.Equals("У черзі", StringComparison.OrdinalIgnoreCase))
                .GroupBy(o => o.Item.Name)
                .Select(g => new { Name = g.Key, TotalQuantity = g.Sum(o => o.Quantity) })
                .OrderByDescending(g => g.TotalQuantity)
                .FirstOrDefault();

            if (popular != null)
            {
                Console.WriteLine($"🔥 Найпопулярніший товар: '{popular.Name}' (замовлено: {popular.TotalQuantity} шт.)");
            }
            else
            {
                Console.WriteLine("Немає даних для аналізу.");
            }
        }
        
    }
}