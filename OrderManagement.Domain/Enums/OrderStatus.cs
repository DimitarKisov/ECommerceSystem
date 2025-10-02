namespace OrderManagement.Domain.Enums
{
    /// <summary>
    /// Статус на поръчката
    /// </summary>
    public enum OrderStatus
    {
        Pending = 0,        // Чака обработка
        Confirmed = 1,      // Потвърдена
        Processing = 2,     // В процес на обработка
        Shipped = 3,        // Изпратена
        Delivered = 4,      // Доставена
        Cancelled = 5,      // Отказана
        Returned = 6        // Върната
    }
}
