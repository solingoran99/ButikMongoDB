using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ButikMongoDB
{
	public class Product
	{
		[BsonId]
		public ObjectId Id { get; set; }

		public string Name { get; set; }
		public decimal Price { get; set; }

		public Product(string name, decimal price)
		{
			Name = name;
			Price = price;
		}

		public static void DisplayProducts(List<Product> products)
		{
			Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Lush Locks");
			Console.ResetColor();
            Console.WriteLine("Available Products:");
			for (int i = 0; i < products.Count; i++)
			{
				var product = products[i];
				Console.WriteLine($"{i + 1}. {product.Name} - {product.Price} kr");
			}
		}

	}
}
