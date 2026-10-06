# Finance Manager

### Documentation

See the [documentation index](./docs/README.md) for the API implementation, people and transactions, asynchronous file processing, and live import status updates.

### Run the development environment

Start PostgreSQL, Redis, RabbitMQ, the API, and the Vite development server:

```bash
docker compose up --build
```

The development services are available at:

- Frontend: [http://localhost:5173](http://localhost:5173)
- API/Swagger: [http://localhost:5266/swagger](http://localhost:5266/swagger)
- RabbitMQ management: [http://localhost:15672](http://localhost:15672)

The API and frontend source directories are mounted into their containers. `dotnet watch` restarts the API when C# files change, and Vite applies frontend changes through HMR.

The API container configuration is stored in `api/.env.docker`. These values are intended for local development only.

Use `docker compose down` to stop the environment. Add `-v` only when you also want to remove the database, queue, and development dependency volumes.

### Functional Requirements
- [X] GET /transactions should return a list of Transaction
- [ ] GET /transactions?date=2026-03-27 should return a list of Transaction by date
- [X] POST /transactions should register a Transaction with date, title and amound  
- [X] PUT /transaction/{id} should update a Transaction with date, title and amound
- [X] DELETE /transaction/{id} should mark a Transaction as deleted
- [X] POST /transactions/upload should create transaction by csv file
- [ ] POST /transactions/upload should not register duplicate Transactions
- [ ] POST /transactions/upload should process in a queue, without block the request
- [ ] GET /transactions/export?startDate=2026-03-01&endDate=2026-03-31 should return Transactions in a xlsx file from a date range 
- [ ] GET /persons should return a list of Person
- [ ] GET /persons?name=john-doe should return a list of Transaction by date
- [ ] POST /persons should register a Person with name and phone number
- [ ] PUT /persons/{id} should update a Person with name and phone number
- [ ] DELETE /persons/{id} should mark a Person as deleted

### Non-Functional Requirements
- [ ] Reliability
- [ ] Resilient

### Nomenclatures
- Transaction: Pix, Bank Slip, TED, Purchase with Credit Card, etc..
- Organization: A bank or financial organization
- Person: A person that can be assigned to a transaction
