using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using WebAppOrder.Domain;


namespace WebAppOrder.Controllers
{
	[Route("api/[controller]")]
	public class OrderController : Controller
	{
		private ILogger<OrderController> _logger;

		public OrderController(ILogger<OrderController> logger)
		{
			_logger = logger;
		}

		[HttpPost]
		public IActionResult InsertOrder(Order order)
		{
			#region MyRegion
			var factory = new ConnectionFactory() { HostName = "localhost" };
			using (var connection = factory.CreateConnection())
			using (var channel = connection.CreateModel())
			{
				channel.QueueDeclare(queue: "orderQueue",
									 durable: false,
									 exclusive: false,
									 autoDelete: false,
									 arguments: null);

				string message = JsonSerializer.Serialize(order);
				var body = Encoding.UTF8.GetBytes(message);

				channel.BasicPublish(exchange: "",
									 routingKey: "orderQueue",
									 basicProperties: null,
									 body: body);
			}
			#endregion

			try
			{
				return Accepted(order);
			}
			catch (Exception ex)
			{
				_logger.LogError("Erro ao tentar criar um novo pedido", ex);
				return new StatusCodeResult(500);
			}
		}
	}
}
