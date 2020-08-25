using System;
using System.Threading.Tasks;
using Example.Contracts;
using MassTransit;
using RabbitMQ.Client;

namespace Example.Consumer
{
    internal static class Program
    {
        private const string Host = "rabbitmq://localhost:5673/vhost";
        private const string Username = "admin";
        private const string Password = "mypass";

        private static void Main(string[] args)
        {
            var bus = InitialState();
            bus.Start();
            Console.WriteLine("Initial state started. Press any key for move to next step");
            Console.ReadLine();
            bus.Stop();

            bus = FirstStep();
            bus.Start();
            Console.WriteLine("First step started. Press any key for move to next step");
            Console.ReadLine();
            bus.Stop();

            bus = SecondStep();
            bus.Start();
            Console.WriteLine("Second step started. Press any key for move to next step");
            Console.WriteLine("Delete TestMessage exchange and bind items");
            Console.ReadLine();
            bus.Stop();

            bus = ThirdStep();
            bus.Start();
            Console.WriteLine("Second third started. Press any key for move to next step");
            Console.ReadLine();
            bus.Stop();

            bus = FourStep();
            bus.Start();
            Console.WriteLine("Second four started. Press any key for move to next step");
            Console.WriteLine("Delete TestMessageTmp exchange and bind items");
            Console.ReadLine();
            bus.Stop();
        }

        private static IBusControl InitialState()
        {
            return Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                var host = cfg.Host(new Uri(Host), x =>
                {
                    x.Username(Username);
                    x.Password(Password);
                });

                cfg.ReceiveEndpoint(host, "legacy_ru", e =>
                {
                    e.PrefetchCount = 10;
                    e.Bind<TestMessage>();
                    e.Consumer(() => new TestGenericConsumer("RU legacy service"));
                });

                cfg.ReceiveEndpoint(host, "legacy_by", e =>
                {
                    e.PrefetchCount = 10;
                    e.Bind<TestMessage>();
                    e.Consumer(() => new TestGenericConsumer("BY legacy service"));
                });

                cfg.ReceiveEndpoint(host, "country_agnostic", e =>
                {
                    e.PrefetchCount = 10;
                    e.Bind<TestMessage>();
                    e.Consumer(() => new TestGenericConsumer("Country agnostic service"));
                });
            });
        }

        private static IBusControl FirstStep()
        {
            return Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                var host = cfg.Host(new Uri(Host), x =>
                {
                    x.Username(Username);
                    x.Password(Password);
                });

                cfg.ReceiveEndpoint(host, "legacy_ru", e =>
                {
                    e.PrefetchCount = 10;
                    e.Bind<TestMessage>();
                    e.Consumer(() => new TestGenericConsumer("RU legacy service"));
                });

                cfg.ReceiveEndpoint(host, "legacy_ru_tmp", e =>
                {
                    e.BindMessageExchanges = false;
                    e.Bind<TestMessageTmp>(x =>
                    {
                        x.ExchangeType = ExchangeType.Topic;
                        x.RoutingKey = "ru";
                    });

                    e.Consumer(() => new TestGenericConsumer("RU legacy service"));
                });

                cfg.ReceiveEndpoint(host, "legacy_by", e =>
                {
                    e.PrefetchCount = 10;
                    e.Bind<TestMessage>();
                    e.Consumer(() => new TestGenericConsumer("BY legacy service"));
                });

                cfg.ReceiveEndpoint(host, "legacy_by_tmp", e =>
                {
                    e.BindMessageExchanges = false;
                    e.Bind<TestMessageTmp>(x =>
                    {
                        x.ExchangeType = ExchangeType.Topic;
                        x.RoutingKey = "by";
                    });

                    e.Consumer(() => new TestGenericConsumer("BY legacy service"));
                });

                cfg.ReceiveEndpoint(host, "country_agnostic", e =>
                {
                    e.PrefetchCount = 10;
                    e.Bind<TestMessage>();
                    e.Consumer(() => new TestGenericConsumer("Country agnostic service"));
                });

                cfg.ReceiveEndpoint(host, "country_agnostic_tmp", e =>
                {
                    e.BindMessageExchanges = false;
                    e.Bind<TestMessageTmp>(x =>
                    {
                        x.ExchangeType = ExchangeType.Topic;
                        x.RoutingKey = "#";
                    });

                    e.Consumer(() => new TestGenericConsumer("Country agnostic service"));
                });
            });
        }

        private static IBusControl SecondStep()
        {
            return Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                var host = cfg.Host(new Uri(Host), x =>
                {
                    x.Username(Username);
                    x.Password(Password);
                });

                cfg.ReceiveEndpoint(host, "legacy_ru_tmp", e =>
                {
                    e.BindMessageExchanges = false;
                    e.Bind<TestMessageTmp>(x =>
                    {
                        x.ExchangeType = ExchangeType.Topic;
                        x.RoutingKey = "ru";
                    });

                    e.Consumer(() => new TestGenericConsumer("RU legacy service"));
                });

                cfg.ReceiveEndpoint(host, "legacy_by_tmp", e =>
                {
                    e.BindMessageExchanges = false;
                    e.Bind<TestMessageTmp>(x =>
                    {
                        x.ExchangeType = ExchangeType.Topic;
                        x.RoutingKey = "by";
                    });

                    e.Consumer(() => new TestGenericConsumer("RU legacy service"));
                });

                cfg.ReceiveEndpoint(host, "country_agnostic_tmp", e =>
                {
                    e.BindMessageExchanges = false;
                    e.Bind<TestMessageTmp>(x =>
                    {
                        x.ExchangeType = ExchangeType.Topic;
                        x.RoutingKey = "#";
                    });

                    e.Consumer(() => new TestGenericConsumer("Country agnostic service"));
                });
            });
        }

        private static IBusControl ThirdStep()
        {
            return Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                var host = cfg.Host(new Uri(Host), x =>
                {
                    x.Username(Username);
                    x.Password(Password);
                });

                cfg.ReceiveEndpoint(host, "legacy_ru", e =>
                {
                    e.BindMessageExchanges = false;
                    e.Bind<TestMessage>(x =>
                    {
                        x.ExchangeType = ExchangeType.Topic;
                        x.RoutingKey = "ru";
                    });

                    e.Consumer(() => new TestGenericConsumer("RU legacy service"));
                });

                cfg.ReceiveEndpoint(host, "legacy_ru_tmp", e =>
                {
                    e.BindMessageExchanges = false;
                    e.Bind<TestMessageTmp>(x =>
                    {
                        x.ExchangeType = ExchangeType.Topic;
                        x.RoutingKey = "ru";
                    });

                    e.Consumer(() => new TestGenericConsumer("RU legacy service"));
                });

                cfg.ReceiveEndpoint(host, "legacy_by", e =>
                {
                    e.BindMessageExchanges = false;
                    e.Bind<TestMessage>(x =>
                    {
                        x.ExchangeType = ExchangeType.Topic;
                        x.RoutingKey = "by";
                    });

                    e.Consumer(() => new TestGenericConsumer("BY legacy service"));
                });

                cfg.ReceiveEndpoint(host, "legacy_by_tmp", e =>
                {
                    e.BindMessageExchanges = false;
                    e.Bind<TestMessageTmp>(x =>
                    {
                        x.ExchangeType = ExchangeType.Topic;
                        x.RoutingKey = "by";
                    });

                    e.Consumer(() => new TestGenericConsumer("RU legacy service"));
                });

                cfg.ReceiveEndpoint(host, "country_agnostic", e =>
                {
                    e.BindMessageExchanges = false;
                    e.Bind<TestMessage>(x =>
                    {
                        x.ExchangeType = ExchangeType.Topic;
                        x.RoutingKey = "#";
                    });

                    e.Consumer(() => new TestGenericConsumer("Country agnostic service"));
                });

                cfg.ReceiveEndpoint(host, "country_agnostic_tmp", e =>
                {
                    e.BindMessageExchanges = false;
                    e.Bind<TestMessageTmp>(x =>
                    {
                        x.ExchangeType = ExchangeType.Topic;
                        x.RoutingKey = "#";
                    });

                    e.Consumer(() => new TestGenericConsumer("Country agnostic service"));
                });
            });
        }

        private static IBusControl FourStep()
        {
            return Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                var host = cfg.Host(new Uri(Host), x =>
                {
                    x.Username(Username);
                    x.Password(Password);
                });

                cfg.ReceiveEndpoint(host, "legacy_ru", e =>
                {
                    e.BindMessageExchanges = false;
                    e.Bind<TestMessage>(x =>
                    {
                        x.ExchangeType = ExchangeType.Topic;
                        x.RoutingKey = "ru";
                    });

                    e.Consumer(() => new TestGenericConsumer("RU legacy service"));
                });

                cfg.ReceiveEndpoint(host, "legacy_by", e =>
                {
                    e.BindMessageExchanges = false;
                    e.Bind<TestMessage>(x =>
                    {
                        x.ExchangeType = ExchangeType.Topic;
                        x.RoutingKey = "by";
                    });

                    e.Consumer(() => new TestGenericConsumer("BY legacy service"));
                });

                cfg.ReceiveEndpoint(host, "country_agnostic", e =>
                {
                    e.BindMessageExchanges = false;
                    e.Bind<TestMessage>(x =>
                    {
                        x.ExchangeType = ExchangeType.Topic;
                        x.RoutingKey = "#";
                    });

                    e.Consumer(() => new TestGenericConsumer("Country agnostic service"));
                });
            });
        }
    }

    public class TestGenericConsumer : IConsumer<TestMessage>
    {
        private readonly string _name;

        public TestGenericConsumer(string name)
        {
            _name = name;
        }

        public Task Consume(ConsumeContext<TestMessage> context)
        {
            Console.WriteLine($"{_name} received message \"{context.Message.Message}\"");
            return Task.CompletedTask;
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