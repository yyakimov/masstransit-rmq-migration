using System;
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

            _messageBus = InitialState();
            _messageBus.Start();
            Console.WriteLine("Initial state started. Press any key for move to next step");
            Console.ReadLine();
            _messageBus.Stop();

            _messageBus = FirstStep();
            _messageBus.Start();
            _step = 1;
            Console.WriteLine("First step started. Press any key for move to next step");
            Console.ReadLine();
            _messageBus.Stop();

            _messageBus = SecondStep();
            _messageBus.Start();
            _step = 2;
            Console.WriteLine("Second step started. Press any key for move to next step");
            Console.ReadLine();
            _messageBus.Stop();
        }

        private static async Task StartInfiniteMessagePublish()
        {
            while (true)
            {
                if (_messageBus != null)
                {
                    foreach (var country in new[] {"ru", "by"})
                        try
                        {
                            if (_step == 1)
                                await _messageBus.Publish(new TestMessageTmp
                                {
                                    Country = country,
                                    Message = $"foo {DateTime.Now:s}"
                                });
                            else
                                await _messageBus.Publish(new TestMessage
                                {
                                    Country = country,
                                    Message = $"foo {DateTime.Now:s}"
                                });
                        }
                        catch
                        {
                            // ignored
                        }
                }

                await Task.Delay(TimeSpan.FromMilliseconds(150));
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
            });
        }

        private static IBusControl FirstStep()
        {
            return Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(new Uri(Host), x =>
                {
                    x.Username(Username);
                    x.Password(Password);
                });

                // configure send topology with country routing key and topic exchange
                cfg.Send<TestMessageTmp>(x => { x.UseRoutingKeyFormatter(context => context.Message.Country); });
                cfg.Publish<TestMessageTmp>(x => { x.ExchangeType = ExchangeType.Topic; });
            });
        }

        private static IBusControl SecondStep()
        {
            return Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(new Uri(Host), x =>
                {
                    x.Username(Username);
                    x.Password(Password);
                });

                // configure send topology with country routing key and topic exchange
                cfg.Send<TestMessage>(x => { x.UseRoutingKeyFormatter(context => context.Message.Country); });
                cfg.Publish<TestMessage>(x => { x.ExchangeType = ExchangeType.Topic; });
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

    public class TestMessageTmp : TestMessage
    {
    }
}