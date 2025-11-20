using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyOrderProcessor.Tests
{
    public class OrderProcessorTests
    {
        [Fact]
        public void ProcessOrder_Throws_WhenOrderIsNull()
        {
            var db = new Mock<IDatabase>();
            var email = new Mock<IEmailService>();
            var processor = new OrderProcessor(db.Object, email.Object);

            Assert.Throws<ArgumentNullException>(() => processor.ProcessOrder(null));
        }

        [Fact]
        public void ProcessOrder_ReturnsFalse_WhenTotalAmountIsZero()
        {
            var order = new Order { TotalAmount = 0 };
            var db = new Mock<IDatabase>();
            var email = new Mock<IEmailService>();
            var processor = new OrderProcessor(db.Object, email.Object);

            Assert.False(processor.ProcessOrder(order));
        }

        [Fact]
        public void ProcessOrder_ReturnsFalse_WhenTotalAmountIsNegative()
        {
            var order = new Order { TotalAmount = -5 };
            var db = new Mock<IDatabase>();
            var email = new Mock<IEmailService>();
            var processor = new OrderProcessor(db.Object, email.Object);

            Assert.False(processor.ProcessOrder(order));
        }

        [Fact]
        public void ProcessOrder_ConnectsToDatabase_WhenNotConnected()
        {
            var order = new Order { TotalAmount = 10 };
            var db = new Mock<IDatabase>();
            db.Setup(x => x.IsConnected).Returns(false);
            var email = new Mock<IEmailService>();
            var processor = new OrderProcessor(db.Object, email.Object);

            processor.ProcessOrder(order);

            db.Verify(x => x.Connect(), Times.Once);
        }

        [Fact]
        public void ProcessOrder_DoesNotConnect_WhenAlreadyConnected()
        {
            var order = new Order { TotalAmount = 10 };
            var db = new Mock<IDatabase>();
            db.Setup(x => x.IsConnected).Returns(true);
            var email = new Mock<IEmailService>();
            var processor = new OrderProcessor(db.Object, email.Object);

            processor.ProcessOrder(order);

            db.Verify(x => x.Connect(), Times.Never);
        }

        [Fact]
        public void ProcessOrder_SavesOrder()
        {
            var order = new Order { TotalAmount = 10 };
            var db = new Mock<IDatabase>();
            db.Setup(x => x.IsConnected).Returns(true);
            var email = new Mock<IEmailService>();
            var processor = new OrderProcessor(db.Object, email.Object);

            processor.ProcessOrder(order);

            db.Verify(x => x.Save(order), Times.Once);
        }

        [Fact]
        public void ProcessOrder_SendsEmail_WhenAmountGreaterThan100()
        {
            var order = new Order { TotalAmount = 150, CustomerEmail = "a@a.com", Id = 1 };
            var db = new Mock<IDatabase>();
            db.Setup(x => x.IsConnected).Returns(true);
            var email = new Mock<IEmailService>();
            var processor = new OrderProcessor(db.Object, email.Object);

            processor.ProcessOrder(order);

            email.Verify(x => x.SendOrderConfirmation(order.CustomerEmail, order.Id), Times.Once);
        }

        [Fact]
        public void ProcessOrder_DoesNotSendEmail_WhenAmountIs100OrLess()
        {
            var order = new Order { TotalAmount = 100, CustomerEmail = "a@a.com", Id = 1 };
            var db = new Mock<IDatabase>();
            db.Setup(x => x.IsConnected).Returns(true);
            var email = new Mock<IEmailService>();
            var processor = new OrderProcessor(db.Object, email.Object);

            processor.ProcessOrder(order);

            email.Verify(x => x.SendOrderConfirmation(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void ProcessOrder_MarksOrderProcessed_WhenSuccess()
        {
            var order = new Order { TotalAmount = 10 };
            var db = new Mock<IDatabase>();
            db.Setup(x => x.IsConnected).Returns(true);
            var email = new Mock<IEmailService>();
            var processor = new OrderProcessor(db.Object, email.Object);

            processor.ProcessOrder(order);

            Assert.True(order.IsProcessed);
        }

        [Fact]
        public void ProcessOrder_ReturnsFalse_WhenSaveThrows()
        {
            var order = new Order { TotalAmount = 10 };
            var db = new Mock<IDatabase>();
            db.Setup(x => x.IsConnected).Returns(true);
            db.Setup(x => x.Save(It.IsAny<Order>())).Throws(new Exception());
            var email = new Mock<IEmailService>();
            var processor = new OrderProcessor(db.Object, email.Object);

            var result = processor.ProcessOrder(order);

            Assert.False(result);
            Assert.False(order.IsProcessed);
        }
    }
}
