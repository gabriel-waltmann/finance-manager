# Finance Manager

### Run the development server
```bash
  docker compose up -d
  cd api
  dotnet run 
```
Open [Swagger](https://localhost:7026/swagger) with your browser to see the result.

### Create and apply migrations
```bash
# Create migration
dotnet ef migrations add MigrationName

# Apply migrations - development
dotnet ef database update

# Apply migrations - production 
dotnet ef migrations script --idempotent -o migrations.sql
```

More details: https://medium.com/@adrianbailador/getting-started-with-postgresql-in-net-from-zero-to-production-1df2e489cd43

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
