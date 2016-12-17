using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net; // For generating HTTP requests and getting responses.
using NiceHashBotLib; // Include this for Order class, which contains stats for our order.
using Newtonsoft.Json; // For JSON parsing of remote APIs.
using System.Globalization;
using System.Text.RegularExpressions;

public class HandlerClass
{
    /// <summary>
    /// This method is called every 0.5 seconds.
    /// </summary>
    /// <param name="OrderStats">Order stats - do not change any properties or call any methods. This is provided only as read-only object.</param>
    /// <param name="MaxPrice">Current maximal price. Change this, if you would like to change the price.</param>
    /// <param name="NewLimit">Current speed limit. Change this, if you would like to change the limit.</param>
    public static void HandleOrder(ref Order OrderStats, ref double MaxPrice, ref double NewLimit)
    {
        // Following line of code makes the rest of the code to run only once per minute.
        if ((++Tick % 120) != 0) return;

        // Perform check, if order has been created at all. If not, stop executing the code.
        if (OrderStats == null) return;

        try
        {
            // get LRP
            string algoName = APIWrapper.ALGORITHM_NAME[OrderStats.Algorithm];
            double LRP = getCurrencyLRP(algoName);

            // Set new maximal price.
            MaxPrice = LRP;

            // Example how to print some data on console...
            Console.WriteLine("Adjusting order #" + OrderStats.ID.ToString() + " maximal price to: " + MaxPrice.ToString("F4"));
        }
        catch
        {
            return;
        }

    }

    /// <summary>
    /// Data structure used for serializing JSON response from CoinWarz. 
    /// It allows us to parse JSON with one line of code and easily access every data contained in JSON message.
    /// </summary>
#pragma warning disable 0649
    class WhatToMineResponse
    {
        public string name;
        public string tag;
        public string algorithm;
        public string block_time;
        public double block_reward;
        public double block_reward24;
        public int last_block;
        public double difficulty;
        public double difficulty24;
        public string nethash;
        public double exchange_rate;
        public double exchange_rate24;
        public double exchange_rate_vol;
        public string exchange_rate_curr;
        public string market_cap;
        public string estimated_rewards;
        public string pool_fee;
        public string btc_revenue;
        public string revenue;
        public string cost;
        public string profit;
        public string status;
        public bool lagging;
        public int timestamp;
    }
#pragma warning restore 0649


    /// <summary>
    /// Property used for measuring time.
    /// </summary>
    private static int Tick = -10;


    // Following methods do not need to be altered.
    #region PRIVATE_METHODS

    /// <summary>
    /// Get HTTP JSON response for provided URL.
    /// </summary>
    /// <param name="URL">URL.</param>
    /// <returns>JSON data returned by webserver or null if error occured.</returns>
    private static string GetHTTPResponseInJSON(string URL)
    {
        try
        {
            HttpWebRequest WReq = (HttpWebRequest)WebRequest.Create(URL);
            WReq.Timeout = 60000;
            WebResponse WResp = WReq.GetResponse();
            Stream DataStream = WResp.GetResponseStream();
            DataStream.ReadTimeout = 60000;
            StreamReader SReader = new StreamReader(DataStream);
            string ResponseData = SReader.ReadToEnd();
            if (ResponseData[0] != '{')
                throw new Exception("Not JSON data.");

            return ResponseData;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    private static double getCurrencyLRP(string algo)
    {
        using (var client = new WebClient())
        {
            var html = client.DownloadString("https://www.nicehash.com/jq_getfirstpagetable.jsp");
            html = html.Substring(html.LastIndexOf(algo));
            html = html.Substring(html.IndexOf("class=\"tdstat\">"));
            html = html.Substring(html.IndexOf(">") + 1);
            html = html.Substring(0, html.IndexOf("<"));
            html = Regex.Replace(html, "[^0-9.]", "");
            return double.Parse(html);
        }
    }

    #endregion
}
