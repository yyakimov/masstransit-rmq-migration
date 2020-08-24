# MassTransit RabbitMQ scheme migration example

Pretty simple example of change type of exchange from fanout to topic using MassTransit.

## Getting Started

Initialize RabbitMQ
```
docker-compose up -d
```
Run consumers
```
dotnet run -p ./src/Example.Consumers
```
Run producer
```
dotnet run -p ./src/Example.Producer
```

## Steps

### Initial state
![scheme map](docs/images/0_initial.png)

### First step
Add temporary exchange and queues for consume temporary event.
![scheme map](docs/images/1_first.png)

### Second step
Begin publish temporary event to temporary exchange. Stop publish original event.
Here we can easily rollback changes.
![scheme map](docs/images/2_second.png)

### Third step
Remove original exchanges and queues.
![scheme map](docs/images/3_third.png)

### Four step
Add original exchange as topic and original queues.
![scheme map](docs/images/4_four.png)

### Fifth step
Begin publish to new original exchange.
![scheme map](docs/images/5_fifth.png)

### Six step
Remove all tmp items.
![scheme map](docs/images/6_six.png)