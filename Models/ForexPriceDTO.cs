using System;
using System.Text.Json.Serialization;
namespace forex_quotes.Models
{
    public  class ForexPriceDTO
    {
        [JsonPropertyName("instrument")]
        public string Instrument { get; set; }

        [JsonPropertyName("time")]

        public string Time { get; set; }  

        [JsonPropertyName("bid")]
 
        public double Bid { get; set; }

        [JsonPropertyName("ask")]
        public double Ask { get; set; }
        public DateTime UTCTime{get => DateTime.Parse(Time);}
    }   
}