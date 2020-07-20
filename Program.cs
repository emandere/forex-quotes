using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Linq;
using System.Text.Json;
using forex_quotes.Models;
namespace forex_quotes
{
    class Program
    {
         static readonly HttpClient client = new HttpClient();

         static List<string> pairs = new List<string>()
        {
            "AUDUSD",
            "EURUSD",
            "GBPUSD",
            "NZDUSD",
            "USDCAD",
            "USDCHF",
            "USDJPY"
        };

        static Dictionary<string,string> pairToInstrument= new Dictionary<string,string>()
        {
            {"AUDUSD","AUD_USD"},
            {"EURUSD","EUR_USD"},
            {"GBPUSD","GBP_USD"},
            {"NZDUSD","NZD_USD"},
            {"USDCAD","USD_CAD"},
            {"USDCHF","USD_CHF"},
            {"USDJPY","USD_JPY"},
        };
        static async Task Main(string[] args)
        {
            var url = "http://localhost:5002/api/forexprices/quote/";
            var urlput = "http://localhost:5002/api/forexprices/";
            var urlpost = "http://localhost:5002/api/forexprices/adddailyrealprices";
            //var price = await GetAsync<ForexPriceDTO>(url);
            //var response = await PutAsync<ForexPriceDTO>(price,urlput);
            await GetQuotes(url,urlput,urlpost);
            Console.WriteLine("Hello World!");
        }

        static async Task GetQuotes(string url,string urlput,string urlpost)
        {
            while(true)
            { 
                try
                {
                    var prices = new List<ForexPriceDTO>();
                    foreach(var pair in pairs)
                    {
                        var urlputpair = urlput + pair;
                        var urlpair = url + pairToInstrument[pair];
                        var price = await GetAsync<ForexPriceDTO>(urlpair);
                        var responsePUT = await PutAsync<ForexPriceDTO>(price,urlputpair);

                        prices.Add(price);
                    }

                    var responsePOST = await PostAsync<List<ForexPriceDTO>>(prices,urlpost);
                    Console.WriteLine("Pairs updated");
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                await Task.Delay(1000 * 10);
            }
        }

        static async Task<T> GetAsync<T>(string url)
        {
            var responseBody = await client.GetStringAsync(url);
            var data = JsonSerializer.Deserialize<T>(responseBody);
            return data;
        }

        static async Task<HttpResponseMessage> PatchAsync<T>(T dto,string url)
        {
            var stringPrice= JsonSerializer.Serialize<T>(dto);
            var stringPriceContent = new StringContent(stringPrice,UnicodeEncoding.UTF8,"application/json");
            var responsePriceBody = await client.PatchAsync(url,stringPriceContent);
            return responsePriceBody;
        }

        static async Task<HttpResponseMessage> PutAsync<T>(T dto,string url)
        {
            var stringPrice= JsonSerializer.Serialize<T>(dto);
            var stringPriceContent = new StringContent(stringPrice,UnicodeEncoding.UTF8,"application/json");
            var responsePriceBody = await client.PutAsync(url,stringPriceContent);
            return responsePriceBody;
        }

        static async Task<HttpResponseMessage> PostAsync<T>(T dto,string url)
        {
            var stringPrice= JsonSerializer.Serialize<T>(dto);
            var stringPriceContent = new StringContent(stringPrice,UnicodeEncoding.UTF8,"application/json");
            var responsePriceBody = await client.PostAsync(url,stringPriceContent);
            return responsePriceBody;
        }
    }


}


