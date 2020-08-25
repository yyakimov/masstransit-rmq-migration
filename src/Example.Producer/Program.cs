using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Example.Contracts;
using MassTransit;
using RabbitMQ.Client;

namespace Example.Producer
{
    internal class Program
    {
        private const string Host = "rabbitmq://localhost:5673/vhost";
        private const string Username = "admin";
        private const string Password = "mypass";

        private static IBusControl _messageBus;
        private static int _step;

        private static void Main(string[] args)
        {
            StartInfiniteMessagePublish().ConfigureAwait(false);

            if (args.Length == 0)
            {
                _step = 1;
            }
            else if (args.Contains("--initial"))
            {
                _step = 0;
            }
            else if (args.Contains("--second"))
            {
                _step = 2;
            }

            _messageBus = _step switch
            {
                0 => InitialState(),
                1 => FirstStep(),
                2 => SecondStep(),
                _ => throw new InvalidEnumArgumentException()
            };
            _messageBus.Start();
            switch (_step)
            {
                case 0: Console.WriteLine("Initial state started. Press any key to finish");
                    break;
                case 1: Console.WriteLine("First step started. Press any key to finish");
                    break;
                case 2: Console.WriteLine("Second step started. Press any key to finish");
                    break;
            }
            Console.ReadLine();
            _messageBus.Stop();
        }

        private static string GetTmpExchangeName(Type type)
        {
            return type.Namespace+":"+type.Name + "Tmp";
        }

        private static async Task StartInfiniteMessagePublish()
        {
            while (true)
            {
                if (_messageBus != null)
                {
                    Console.WriteLine($"Send message {_step} {DateTime.Now:s}");
                    foreach (var country in new[] {"ru", "by"})
                        try
                        {
                            await _messageBus.Publish(new TestMessage
                            {
                                Country = country,
                                Message = $"{_step} country: {country} {DateTime.Now:s}"
                            });
                        }
                        catch
                        {
                            // ignored
                        }
                }

                await Task.Delay(TimeSpan.FromMilliseconds(1000));
            }
        }

        private static IBusControl InitialState()
        {
            return Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(new Uri(Host), x =>
                {
                    x.Username(Username);
                    x.Password(Password);
                });
                
                cfg.Publish<TestMessage>(x =>
                {
                    x.ExchangeType = ExchangeType.Fanout;
                });
            });
        }

        private static IBusControl FirstStep()
        {
            var rabbitClient = new RabbitApiClient("http://localhost:15673", Username, Password);
            rabbitClient.AddTmpTopicExchange<TestMessage>().Wait();
            return Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(new Uri(Host), x =>
                {
                    x.Username(Username);
                    x.Password(Password);
                });

                cfg.Message<TestMessage>(x =>
                {
                    x.SetEntityName(GetTmpExchangeName(typeof(TestMessage)));
                });
                // configure send topology with country routing key and topic exchange
                cfg.Send<TestMessage>(x =>
                {
                    x.UseRoutingKeyFormatter(context => context.Message.Country);
                });
                cfg.Publish<TestMessage>(x =>
                {
                    x.ExchangeType = ExchangeType.Topic;
                });
            });
        }

        private static IBusControl SecondStep()
        {
            var rabbitClient = new RabbitApiClient("http://localhost:15673", Username, Password);
            rabbitClient.RecreateExchange<TestMessage>().Wait();
            return Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(new Uri(Host), x =>
                {
                    x.Username(Username);
                    x.Password(Password);
                });
                
                // configure send topology with country routing key and topic exchange
                cfg.Send<TestMessage>(x =>
                {
                    x.UseRoutingKeyFormatter(context => context.Message.Country);
                });
                cfg.Publish<TestMessage>(x =>
                {
                    x.ExchangeType = ExchangeType.Topic;
                });
            });
        }
    }
}

namespace Example.Contracts
{
    public class TestMessage
    {
        public string Country { get; set; }
        public string Message { get; set; }
    }
}