using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace ButikMongoDB
{
	public class Program
	{
		private static IMongoCollection<Customer> customerCollection;
		private static IMongoCollection<Product> productCollection;

		static void Main(string[] args)
		{
			string connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
			var client = new MongoClient(connectionString);
			var database = client.GetDatabase("ButikDB");

			try
			{
				var command = new BsonDocument { { "ping", 1 } };
				var result = database.RunCommand<BsonDocument>(command);
				Console.WriteLine("Successfully connected to MongoDB!");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error connecting to MongoDB: {ex.Message}");
			}

			customerCollection = database.GetCollection<Customer>("customers");
			productCollection = database.GetCollection<Product>("products");

			InitializeStore();

			bool running = true;
			Customer loggedInCustomer = null;

			while (running)
			{
				Console.Clear();
				Console.ForegroundColor = ConsoleColor.Magenta;
				Console.WriteLine("Welcome to Lush Locks! To see our newest releases, hurry up and join!");
				Console.ResetColor();

				Console.WriteLine("1. Log in\n2. Become a member\n3. Exit\nEnter the corresponding number:");
				string userInput = Console.ReadLine();
				int userChoice;

				if (int.TryParse(userInput, out userChoice))
				{
					switch (userChoice)
					{
						case 1:
							Console.Clear();
							loggedInCustomer = Customer.Login(customerCollection);
							if (loggedInCustomer != null)
							{
								ShoppingMenu(loggedInCustomer);
							}
							break;
						case 2:
							Console.Clear();
							Customer.RegisterCustomer(customerCollection);
							break;
						case 3:
							running = false;
							break;
						default:
							Console.WriteLine("Invalid choice. Choose 1, 2 or 3.");
							break;
					}
				}
				else
				{
					Console.WriteLine("OOPS error! Please enter a number.");
				}
			}
		}

		static void InitializeStore()
		{
			if (productCollection.CountDocuments(Builders<Product>.Filter.Empty) == 0)
			{
				var defaultProducts = new List<Product>
				{
					new Product("Shampoo", 119.00m),
					new Product("Conditioner", 112.00m),
					new Product("Hair Mask", 219.20m),
					new Product("Hair Serum", 99.00m),
					new Product("Hair Oil", 269.00m)
				};

				productCollection.InsertMany(defaultProducts);
			}
		}

		static void ShoppingMenu(Customer customer)
		{
			bool shopping = true;

			while (shopping)
			{
				Console.Clear();
				Console.ForegroundColor = ConsoleColor.Magenta;
				Console.WriteLine("Lush Locks");
				Console.ResetColor();
				Console.WriteLine("1. Profile\n2. Shop\n3. View Cart\n4. Check Out\n5. Log Out\n6. Manage Products");

				if (int.TryParse(Console.ReadLine(), out int shoppingChoice))
				{
					switch (shoppingChoice)
					{
						case 1:
							Console.Clear();
							Console.ForegroundColor = ConsoleColor.Magenta;
							Console.WriteLine("Lush Locks - Profile");
							Console.ResetColor();
							Console.WriteLine(customer.ToString());
							Console.WriteLine("Press enter to go back to the menu.");
							Console.ReadKey();
							break;
						case 2:
							ShoppingLoop(customer);
							break;
						case 3:
							customer.ViewCart();
							break;
						case 4:
							customer.CheckOut(customerCollection);
							break;
						case 5:
							shopping = false;
							break;
						case 6:
							ManageProducts();
							break;
						default:
							Console.WriteLine("Invalid choice");
							break;
					}
				}
				else
				{
					Console.WriteLine("Invalid input, enter a number between 1-6.");
				}
			}
		}

		static void ShoppingLoop(Customer customer)
		{
			bool continueShopping = true;
			List<Product> products = productCollection.Find(Builders<Product>.Filter.Empty).ToList();

			while (continueShopping)
			{
				Console.Clear();
				Product.DisplayProducts(products);

				Console.WriteLine("\nEnter the number of the product to add to your cart:\nType 'exit' to stop shopping.");
				string productInput = Console.ReadLine();
				int productNumber;

				if (productInput.Trim().ToLower().Equals("exit", StringComparison.OrdinalIgnoreCase))
				{
					continueShopping = false;
				}
				else if (int.TryParse(productInput, out productNumber) && productNumber > 0 && productNumber <= products.Count)
				{
					Product selectedProduct = products[productNumber - 1];
					customer.UpdateCart(selectedProduct);
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine($"\n{selectedProduct.Name} has been added to your cart.");
					Console.ResetColor();

					System.Threading.Thread.Sleep(1500);
				}
				else
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Please enter a valid number from the list or type 'exit' to go back.");
					Console.ResetColor();
					System.Threading.Thread.Sleep(1500);
				}
			}
		}

		static void ManageProducts()
		{
			Console.Clear();
			Console.ForegroundColor= ConsoleColor.Magenta;
			Console.WriteLine("Lush Locks");
			Console.ResetColor();
			Console.WriteLine("Manage Store Products:");
			Console.WriteLine("1. Add Product\n2. Remove Product\n3. View All Products\n4. Back");

			if (int.TryParse(Console.ReadLine(), out int manageChoice))
			{
				switch (manageChoice)
				{
					case 1:
						AddProduct();
						break;
					case 2:
						RemoveProduct();
						break;
					case 3:
						ViewAllProducts();
						break;
					case 4:
						return;
					default:
						Console.WriteLine("Invalid choice.");
						break;
				}
			}
		}

		static void AddProduct()
		{
			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Magenta;
			Console.WriteLine("Lush Locks");
			Console.ResetColor();
			Console.WriteLine("Enter the product name:");
			string productName = Console.ReadLine();

			Console.WriteLine("Enter the product price:");
			decimal productPrice;
			while (!decimal.TryParse(Console.ReadLine(), out productPrice))
			{
				Console.WriteLine("Invalid price. Please enter a valid number:");
			}

			var newProduct = new Product(productName, productPrice);
			productCollection.InsertOne(newProduct);

			Console.WriteLine($"{productName} has been added to the store.");
			Console.ReadKey();
		}

		static void RemoveProduct()
		{
			Console.Clear();
			Console.ForegroundColor= ConsoleColor.Magenta;
			Console.WriteLine("Lush Locks");
			Console.ResetColor();
			List<Product> products = productCollection.Find(Builders<Product>.Filter.Empty).ToList();
			Product.DisplayProducts(products);

			Console.WriteLine("Enter the number of the product to remove:");
			string input = Console.ReadLine();
			int productNumber;
			if (int.TryParse(input, out productNumber) && productNumber > 0 && productNumber <= products.Count)
			{
				Product productToRemove = products[productNumber - 1];
				productCollection.DeleteOne(Builders<Product>.Filter.Eq("_id", productToRemove.Id));
				Console.WriteLine($"{productToRemove.Name} has been removed from the store.");
			}
			else
			{
				Console.WriteLine("Invalid product number.");
			}
			Console.ReadKey();
		}

		static void ViewAllProducts()
		{
			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Magenta;
			Console.WriteLine("Lush Locks");
			Console.ResetColor();
			List<Product> products = productCollection.Find(Builders<Product>.Filter.Empty).ToList();
			Product.DisplayProducts(products);
			Console.ReadKey();
		}
	}
}
