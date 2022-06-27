using Application.IService;
using Confluent.Kafka;
using Data.Entities;
using Data.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Ultilities
{
    public static class KafkaSettings
    {
        public static string Server = "moped-01.srvs.cloudkafka.com:9094";
        public static string UserName = "fhpfa4az";
        public static string Password = "SLQnaV1eVq-h4KdxzBVmZ-1Z7V9HVHkV";

        public static string Topic = "fhpfa4az-music"; // fhpfa4az là tiền tố của cloud
    }

    public static class KafkaProducerService
    {
        public static async Task SendMusicsAsync(Song song)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = KafkaSettings.Server,
                SaslUsername = KafkaSettings.UserName,
                SaslPassword = KafkaSettings.Password,
                SaslMechanism = SaslMechanism.ScramSha256,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SocketKeepaliveEnable = true
            };
            var musicElastic = new MusicSuggest
            {
                AlbumId = song.AlbumId,
                Author = song.Author,
                CreateDate = song.CreateDate,
                Id = song.Id,
                Image = song.Image,
                IsActive = song.IsActive,
                Lyric = song.Lyric,
                Name = song.Name,
                Url = song.Url,
                Suggest = new CompletionField()
                {
                    Input = new[] { song.Name, song.Author }
                }
            };

            using (var producer = new ProducerBuilder<Null, string>(config).Build())
            {
                try
                {
                    var result = await producer.ProduceAsync(KafkaSettings.Topic, 
                                        new Message<Null, string>{ Value = JsonConvert.SerializeObject(musicElastic) });
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Oops, something went wrong: {e}");
                }
            }
        }
    }

    public class KafkaConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ConsumerConfig _config = new ConsumerConfig
        {
            BootstrapServers = KafkaSettings.Server,
            SaslUsername = KafkaSettings.UserName,
            SaslPassword = KafkaSettings.Password,
            SaslMechanism = SaslMechanism.ScramSha256,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SocketKeepaliveEnable = true,
            GroupId = "test_group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        public IServiceProvider Services { get; }

        public KafkaConsumer(IServiceProvider services, IConfiguration configuration)
        {
            Services = services;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                using (var consumerBuilder = new ConsumerBuilder<Ignore, string>(_config).Build())
                {
                    consumerBuilder.Subscribe(KafkaSettings.Topic);
                    var cancelToken = new CancellationTokenSource();

                    try
                    {
                        while (true)
                        {
                            var consumer = consumerBuilder.Consume(cancelToken.Token);
                            var music = JsonConvert.DeserializeObject<MusicSuggest>(consumer.Message.Value);
                            using (var scope = Services.CreateScope()) //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-3.1&tabs=visual-studio
                            {
                                var scopedProcessingService = scope.ServiceProvider.GetRequiredService<IElasticSearchService>();
                                await scopedProcessingService.AddDocument("musics", music);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        consumerBuilder.Close();
                        using (var scope = Services.CreateScope())
                        {
                            var scopedProcessingService = scope.ServiceProvider.GetRequiredService<IMailService>();
                            scopedProcessingService.SendMail(_configuration["CCEmailAddress"], $"exception: {ex.Message}", "Kafka Service something wrong");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
