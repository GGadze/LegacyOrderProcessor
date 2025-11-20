using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyOrderProcessor
{
    public class OrderProcessor
    {
        private readonly IDatabase _database;
        private readonly IEmailService _emailService;

        public OrderProcessor(IDatabase database, IEmailService emailService)
        {
            _database = database;
            _emailService = emailService;
        }

        public bool ProcessOrder(Order order)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            if (order.TotalAmount <= 0) return false;

            if (!_database.IsConnected)
            {
                _database.Connect();
            }

            try
            {
                _database.Save(order);
                if (order.TotalAmount > 100)
                {
                    _emailService.SendOrderConfirmation(order.CustomerEmail, order.Id);
                }
                order.IsProcessed = true;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

}
