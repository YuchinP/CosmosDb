using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;



namespace cosmos_db_docdb_dotnet
{

    class Program
    {
        private const string EndpointUrl = "https://ysp.documents.azure.com:443/";
        private static readonly string AuthorizationKey = "OZmkIaqOg6tQeMuf1A48D0Tkf8YA3mkh9icJN1q6SN2EEVAEDkv1A7ySUHjhYCYhTYNY2f0j1J0LwU0hv3inJQ==";
        private DocumentClient client;

        static void Main(string[] args)
        {


        }

        private async Task GetStartedDemo()
        {
            this.client = new DocumentClient(new Uri(EndpointUrl), AuthorizationKey);
            await this.CreateDatabaseIfNotExists("PizzaDb");

            await this.CreateDocumentCollectionIfNotExists("PizzaDB", "PizzaDelivery");

            //Inserting a document by creating a pizza object

            //Create a Database if not exists
            //<param name="DbName"> The name/IDictionary of the database.</Param>
            //<returns>the Task for asynchornous execution.</returns>'

            Pizza supremePizza = new Pizza
            {
                Id = "1",
                Topping = "everything",
                Restaurant = "papa johns"
            };

            await this.CreatePizzaDocumentIfNotExists("PizzaDb", "PizzaCollection", supremePizza);
            
        }
        private async Task CreateDatabaseIfNotExists(string DbName)
        {
            try
            {
                await this.client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(DbName));
            }
            catch (DocumentClientException de)
            {
                //If db does not exist, create new one
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await this.client.CreateDatabaseAsync(new Database { Id = DbName });
                    this.WriteToConsoleAndPromptToContinue("Created {0}", DbName);
                }
                else
                {
                    throw;
                }
            }
        }

        // <summary>
        // Create a collection with the specified name if it doesn't exist
        //</summary>
        // <param name ="DbName">The name/ID of the database.</param>
        // <param name ="collectionName">The name/ID of the the collection.</param>
        // <returns> The Task for asynchoronous execution.</returns>
        private async Task CreateDocumentCollectionIfNotExists(string DbName, string collectionName)
        {
            try
            {
                await this.client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(DbName, collectionName));
            }
            catch (DocumentClientException de)
            {
                //If document collection does not exist, create
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    DocumentCollection collectionInfo = new DocumentCollection();
                    collectionInfo.Id = collectionName;

                    await this.client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(DbName),
                        new DocumentCollection { Id = collectionName },
                        new RequestOptions { OfferThroughput = 400 });

                    this.WriteToConsoleAndPromptToContinue("Created {0}", collectionName);
                }
                else
                {
                    throw;
                }
            }
        }
        // <summary>
        // Create Pizza doc in the collection if another by the same ID doesn't already exist.
        // </summary>
        // <param name ="DbName"> The Name/ID of the database.</param>
        // <param name = "collectionName"> The name/ID of the collection.</param>
        // <param name = "pizza"> The pizza doc to be created </param>
        // <returns> The Task for asynchronous execution. </returns>
        private async Task CreatePizzaDocumentIfNotExists(string DbName, string collectionName, Pizza pizza)
        {
            try
            {
                await this.client.ReadDocumentAsync(UriFactory.CreateDocumentUri(DbName, collectionName, pizza.Id));

            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await this.client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(DbName, collectionName), pizza);
                    this.WriteToConsoleAndPromptToContinue("Created Pizza {0}", pizza.Id);

                }
                else
                {
                    throw;
                }
            }
        }

        // <summary>
        // Execute a simple query using LINQ and SQL. 
        // </summary>
        // <param name="DbName">The Name/ID of the database.</param>
        // <param name="collectionName">The Name/ID of the collection.</param>
        private void ExecuteSimpleQuery(string DbName, string collectionName)
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true };

            IQueryable<Pizza> pizzaQuery = this.client.CreateDocumentQuery<Pizza>(
                UriFactory.CreateDocumentCollectionUri(DbName, collectionName), queryOptions)
                .Where(f => f.Topping == "Pepperoni");

            Console.WriteLine("Running LINQ query...");
            foreach (Pizza pizza in pizzaQuery)
            {
                Console.WriteLine("\t Read {0}", pizza);
            }

            //Same thing with direct SQL
            IQueryable<Pizza> pizzaQueryInSql = this.client.CreateDocumentQuery<Pizza>(
                UriFactory.CreateDocumentCollectionUri(DbName, collectionName),
                "SELECT * FROM Pizza WHERE Pizza.topping = 'Pepperoni'",
                queryOptions);

            Console.WriteLine("Running direct SQL query...");
            foreach (Pizza pizza in pizzaQuery)
            {
                Console.WriteLine("\tRead {0}", pizza);
            }
        }

        // <summary>
        // Write to the console, and prompt to continue.
        // </summary>
        // <param name="format"> The string to be displayed.</param>
        // <param name="args"> Optional arguments.</param>
        private void WriteToConsoleAndPromptToContinue(string format, params object[] args)
        {
            Console.WriteLine(format, args);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
        // <summary>
        // Pizza class, storing different kinds of topings
        // Example to show how to store objects in the application logic
        //directly as JSON within Azure DocumentDB.
        //</summary>
        public class Pizza
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }
            
            public string Topping { get; set; }

            public string Restaurant { get; set; }

            public string Type { get; set; }

            public override string ToString()
            {
                return JsonConvert.SerializeObject(this);
            }
        }

        public class Type
        {
            public string PizzaName { get; set; }
            public string Specialty { get; set; }
        }
    }
}
