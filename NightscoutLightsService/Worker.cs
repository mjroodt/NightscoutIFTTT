using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NightscoutLightsService.Helpers;

namespace NightscoutLightsService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private readonly float _lowBG = 99.99F;
        private readonly float _urgentLowBG = 99.99F;
        private readonly float _highBG = 99.99F;
        private readonly float _urgentHighBG = 99.99F;
        private readonly TimeSpan _activePeriodStart = default(TimeSpan);
        private readonly TimeSpan _activePeriodEnd = default(TimeSpan);
        private  float _currentBGReading = -1;
        private  int _RunningInterval = 300000; //five minutes
        private Nightscout _nightscout;

        public Worker(ILogger<Worker> logger , IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _nightscout = new Nightscout(configuration);

            float.TryParse(_configuration["Nightscout:LowBG"], out _lowBG);
            float.TryParse(_configuration["Nightscout:UrgentLowBG"], out _urgentLowBG);
            float.TryParse(_configuration["Nightscout:HighBG"], out _highBG);
            float.TryParse(_configuration["Nightscout:UrgentHighBG"], out _urgentHighBG);

            DateTime tempDateTime;
           if(!DateTime.TryParseExact(_configuration["Nightscout:ActivePeriodStart"], "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDateTime))
            {
                //default it
                _activePeriodStart = new TimeSpan(6, 0, 0); //06 AM
            }
            else
            {
                _activePeriodStart = tempDateTime.TimeOfDay;
            }
            if (!DateTime.TryParseExact(_configuration["Nightscout:ActivePeriodEnd"], "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDateTime))
            {
                //default it
                _activePeriodEnd = new TimeSpan(23, 0, 0); //11 PM
            }
            else
            {
                _activePeriodEnd = tempDateTime.TimeOfDay;
            }

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
        
                TimeSpan now = DateTime.Now.TimeOfDay;
                string ifftEvent = string.Empty;

                if (!(now > _activePeriodStart) && (now < +_activePeriodEnd))
                {
                    //downtime, no need to run this when everyone is asleep
                    Console.WriteLine($"NightScout --> Quiet time between {_activePeriodStart.ToString() } and {_activePeriodEnd.ToString()}");
                    return;
                }
                if (!float.TryParse(await _nightscout.Callnightscout(), out _currentBGReading))
                {
                    //reset it
                    _currentBGReading = -1;
                };

                ConsoleHelper.Write($"Reading {_currentBGReading}");

                switch (_currentBGReading)
                {
                   
                    case var BG when (_currentBGReading > 0 && _currentBGReading <= _urgentLowBG):

                        ifftEvent = "ns-urgent-low";
                        break;
                    case var BG when (_currentBGReading > _urgentLowBG && _currentBGReading <= _lowBG):
                        ifftEvent = "ns-warning-low";
                        break;
                    case var BG when (_currentBGReading < _urgentHighBG && _currentBGReading >= _highBG):
                        ifftEvent = "ns-warning-high";
                        break;
                    case var BG when (_currentBGReading >= _urgentHighBG ):
                        ifftEvent = "ns-urgent-high";
                        break;

                    default:
                        ifftEvent = "ns-allclear";
                        break;
                }

                await _nightscout.CallIFTTT(ifftEvent);
                ConsoleHelper.Write($"Sent {ifftEvent} to IFTTT");


                int.TryParse(_configuration["Nightscout:RunningInterval"], out _RunningInterval);
             

              await Task.Delay(_RunningInterval, stoppingToken);
            }
        }
    }
}
