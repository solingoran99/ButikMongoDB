using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace ButikMongoDB
{
	public class Customer
	{
		public ObjectId Id { get; set; }
		public string Name { get; private set; }
		[BsonElement("password")]
		public string Password { get; private set; }

		public List<CartItem> Cart { get; private set; }

		public Customer(string name, string password)
		{
			Name = name;
			Password = password ?? throw new ArgumentNullException(nameof(password));
			Cart = new List<CartItem>();
		}

		public bool VerifyPassword(string password)
		{
			return Password.Trim() == password.Trim();
		}

		public static Customer Login(IMongoCollection<Customer> customers)
		{
			string userName;
			string userPassword;
			bool userVerified = false;

			while (!userVerified)
			{
				Console.ForegroundColor = ConsoleColor.Magenta;
				Console.WriteLine("Lush Locks");
				Console.ResetColor();
				Console.WriteLine("Enter username:");
				userName = Console.ReadLine();

				Console.WriteLine("Enter password:");
				userPassword = Console.ReadLine();

				var customer = customers.Find(c => c.Name == userName).FirstOrDefault();

				if (customer != null && customer.VerifyPassword(userPassword))
				{
					Console.WriteLine($"Logged in as {userName}");
					userVerified = true;
					return customer;
				}
				else
				{
					Console.WriteLine("Invalid username or password. Try again.");
				}
			}

			return null;
		}

		public static void RegisterCustomer(IMongoCollection<Customer> customers)
		{
			Console.ForegroundColor = ConsoleColor.Magenta;
			Console.WriteLine("Lush Locks");
			Console.ResetColor();
			Console.WriteLine("Enter your username:");
			string username = Console.ReadLine();

			Console.WriteLine("Enter your password:");
			string password = Console.ReadLine();

			var newCustomer = new Customer(username, password);
			customers.InsertOne(newCustomer);

			Console.WriteLine("Registration successful!");
			Console.WriteLine("Press any key to continue.");
			Console.ReadKey();
		}

		public void UpdateCart(Product product)
		{
			Cart.Add(new CartItem { Product = product, Quantity = 1 });
		}

		public void ViewCart()
		{
			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Magenta;
			Console.WriteLine("Lush Locks");
			Console.ResetColor();
			Console.WriteLine("Your Cart:");
			foreach (var item in Cart)
			{
				Console.WriteLine($"{item.Product.Name} - {item.Quantity} x {item.Product.Price:C}");
			}
			Console.WriteLine($"Total: {CalculateTotal():C}");
			Console.WriteLine("Press any key to continue.");
			Console.ReadKey();
		}

		public decimal CalculateTotal()
		{
			decimal total = 0;
			foreach (var item in Cart)
			{
				total += item.Product.Price * item.Quantity;
			}
			return total;
		}

		public void CheckOut(IMongoCollection<Customer> customers)
		{
			Console.Clear();
			Console.ForegroundColor= ConsoleColor.Magenta;
			Console.WriteLine("Lush Locks");
			Console.ResetColor();
			decimal total = CalculateTotal();

			Console.WriteLine($"Total: {total:C}");

			Console.WriteLine("Do you want to confirm your purchase? (y/n)");
			string confirmation = Console.ReadLine().ToLower();

			if (confirmation == "y")
			{
				Cart.Clear();
				customers.ReplaceOne(c => c.Id == this.Id, this);
				Console.WriteLine("Purchase successful!");
			}
			else
			{
				Console.WriteLine("Purchase canceled.");
			}

			Console.WriteLine("Press any key to continue.");
			Console.ReadKey();
		}

		public override string ToString()
		{
			return $"Name: {Name}\nCart items: {Cart.Count}";
		}
	}

	public class CartItem
	{
		public Product Product { get; set; }
		public int Quantity { get; set; }
	}
}
