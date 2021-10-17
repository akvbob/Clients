using ClientsApp.Data;
using ClientsApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Nancy.Json;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Web;
using Microsoft.Extensions.Configuration;

namespace ClientsApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ClientsDbContext _context;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _config;

        public HomeController(ILogger<HomeController> logger, ClientsDbContext context, IHttpClientFactory clientFactory, IConfiguration config)
        {
            _logger = logger;
            _context = context;
            _clientFactory = clientFactory;
            _config = config;
        }

        public IActionResult Index()
        {
            var clients = _context.Client.ToList();
            return View(clients);
        }


        public IActionResult Import(string button)
        {
            string path = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "klientai.json");

            if (System.IO.File.Exists(path))
            {
                string clientsJson = System.IO.File.ReadAllText(path);

                Client[] clients = new JavaScriptSerializer().Deserialize<Client[]>(clientsJson);

                //save clients to DB (without dublicates)
                foreach (Client client in clients)
                {
                    _context.Client.AddIfNotExists(client, c => c.Name == client.Name && c.Address == client.Address);
                    _context.SaveChanges();                
                }
                _logger.LogInformation("Clients imported from klientai.json file");
            }        
            return RedirectToAction("Index");
        }

  
        public async Task<IActionResult> EditAsync(int id)
        {
            Client client = _context.Client.First(x => x.ClientId == id);

            // GET data from API by selected client id
            var result = await GetPostCodeFromAPI(client);
            if (result == null)
            {
                TempData["errormsg"] = "Error: could not update this client postCode";
                return RedirectToAction("Index");
            }
            
            // Update client PostCode in DB
            client.PostCode = result["post_code"].ToString();
            await _context.SaveChangesAsync();

            _logger.LogInformation("Client  record PostCode updated {clientName}", client.Name);
            return RedirectToAction("Index");
        }

        public async Task<JToken> GetPostCodeFromAPI(Client c)
        {
            //format url for API method request
            string url = BuildUrl(c);

            // send request to www.postit.lt web API
            var request = new HttpRequestMessage(HttpMethod.Get, "?" + url);        
            var clientF = _clientFactory.CreateClient("meta");
            var response = await clientF.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Get request from API is successful");

                // parse API data
                var responseString = await response.Content.ReadAsStringAsync();
                JObject parsedResponse = JObject.Parse(responseString);
                var clients = parsedResponse["data"];
                if (clients == null)
                {
                    _logger.LogInformation("GET request returned no results");
                    return null;
                }
                int resultCount = ((JArray)clients).Count;
                if (resultCount >= 0)
                {          
                    return clients[0];
                }
                _logger.LogInformation("GET request returned no results");
                return null;            
            }
            else
            {
                _logger.LogInformation("Get request from API is unsuccessful");
                return null;           
            }    
        }

        // returns "key1=value1&key2=value2"
        public string BuildUrl(Client client)
        {
            string [] address = client.Address.Split(", ");
            string[] urlkeys = new string[] { "address", "city", "municipality" };
            
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            int i = 0;
            foreach (string value in address)
            {
                queryString.Add(urlkeys[i], value);
                i++;
            }
            queryString.Add("key", _config.GetValue<string>("PostitKey"));
            return queryString.ToString(); 
        }
 

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
