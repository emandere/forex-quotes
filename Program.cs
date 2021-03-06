﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Linq;
using System.Text.Json;
using System.Globalization;
using forex_quotes.Models;
using Microsoft.Extensions.Configuration;

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

            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true);
            IConfigurationRoot configuration = builder.Build();
            string server = configuration.GetSection("Servers:Local").Value;
            var quote = GetQuotes(server);
            var candle = GetDailyCandles(server);
            await Task.WhenAll(quote,candle);
        }

        static async Task GetQuotes(string server)
        {
            var url = $"http://{server}/api/forexprices/quote/";
            var urlput = $"http://{server}/api/forexprices/";
            var urlpost = $"http://{server}/api/forexprices/adddailyrealprices";
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
                    Console.WriteLine("Quote updated");
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                await Task.Delay(1000 * 10);
            }
        }

        static async Task GetDailyCandles(string server)
        {
            string url=$"http://{server}/api/forexdailyrealprices";
            string urlpost=$"http://{server}/api/forexdailyprices";
            while(true)
            {
                try
                {
                    var currTime = DateTime.Now;
                    string currDay = currTime.ToString("yyyyMMdd");
                    DateTime currDayDate = DateTime.ParseExact(currDay,"yyyyMMdd",CultureInfo.InvariantCulture);
                    foreach(var pair in pairs)
                    {
                        var urlgetpair = $"{url}/{pair}/{currDay}";
                        var prices = await GetAsync<ForexPricesDTO>(urlgetpair);
                        if(prices.priceDTOs.Count() > 0)
                        {
                            var dailycandle = new ForexDailyPriceDTO()
                            {
                                Pair = pair,
                                Open = prices.priceDTOs.First().Ask,
                                High = prices.priceDTOs.Select(x => x.Ask).Max(),
                                Low = prices.priceDTOs.Select(x => x.Ask).Min(),
                                Close = prices.priceDTOs.Last().Ask,
                                Date = currDay,
                                Datetime = currDayDate
                            };

                            var listCandles = new List<ForexDailyPriceDTO>();
                            listCandles.Add(dailycandle);
                            var responsePOST = await PostAsync<List<ForexDailyPriceDTO>>(listCandles,urlpost);
                            Console.WriteLine("Daily Candle updated");
                        }
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                await Task.Delay(1000 * 20);
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


