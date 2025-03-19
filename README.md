# Ifolor.ProducerService

## Usage

```bash
cd Ifolor.ProducerService
docker compose up
```

Once completed, the following services will be available:

- **RabbitMQ**: [http://localhost:15672/](http://localhost:15672/)  
  (user: `guest`; password: `guest`)
- **Producer Service**: [http://localhost:5001/](http://localhost:5001/)
- **Consumer Service**: [http://localhost:9090/](http://localhost:9090/)
- **Prometheus**: [http://localhost:9091/](http://localhost:9091/)
- **Grafana**: [http://localhost:3000/](http://localhost:3000/)  
  (User: `admin`, Password: `admin`)

To Start go to  

http://localhost:5001/swagger

and call start endpoint. 

Consumer Service can be found here: 
https://github.com/karmak40/Ifolor.ConsumerService
