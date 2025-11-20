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
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        public bool ProcessOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (!IsOrderAmountValid(order))
                return false;

            EnsureDatabaseConnected();

            try
            {
                SaveOrder(order);
                SendEmailIfNeeded(order);
                MarkOrderProcessed(order);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsOrderAmountValid(Order order)
            => order.TotalAmount > 0;

        private void EnsureDatabaseConnected()
        {
            if (!_database.IsConnected)
                _database.Connect();
        }

        private void SaveOrder(Order order)
            => _database.Save(order);

        private void SendEmailIfNeeded(Order order)
        {
            if (order.TotalAmount > 100)
                _emailService.SendOrderConfirmation(order.CustomerEmail, order.Id);
        }

        private static void MarkOrderProcessed(Order order)
            => order.IsProcessed = true;
    }


}
