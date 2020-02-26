using Microsoft.Extensions.Configuration;
using NightscoutLightsService.Helpers;
using NightscoutLightsService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NightscoutLightsService
{
    class Nightscout
    {
        private readonly IConfiguration _configuration; 
        public Nightscout(IConfiguration configuration)
        {
            _configuration = configuration;
        }
   
        private  readonly HttpClient client = new HttpClient();

        public  async Task<string> Callnightscout()
        {       

           using (var httpResponse = await client.GetAsync($"https://{_configuration["Nightscout:NightscoutURL"]}/api/v1/entries/current.json"))
            {
                using (var stream = await httpResponse.Content.ReadAsStreamAsync())
                {
                    List<NightscoutStatus> status = null;
                    try
                    {
                         status = await JsonSerializer.DeserializeAsync<List<NightscoutStatus>>(stream);

                        if(status.Any())
                        {                          
                            DateTime eventDate = DateTime.Parse(status.Single().DateString);

                            if (DateTime.Now.Subtract(eventDate) >= TimeSpan.FromMinutes(20))
                            {
                                 return "-1";
                            }
                       
                            return (status.Single().Sgv/ 18).ToString("N1");
                        }                      

                    }catch
                    {
                        return "99";

                    }
                    return string.Empty;

                }              
            }       
        }


        public  async Task CallIFTTT(string ifttt_event = "ns-allclear")
        { 

        var iftttUrl = $"https://maker.ifttt.com/trigger/{ifttt_event}/with/key/{_configuration["Nightscout:IFTTTKey"]}";

            using (var httpResponse = await client.GetAsync(iftttUrl))
            {

                if (httpResponse.StatusCode != HttpStatusCode.OK)
                {
                    ConsoleHelper.Write($"Unable to call IFFT. Response code: {httpResponse.StatusCode}");
                }
          
            }    
        }
    }
}
