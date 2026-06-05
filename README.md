# Finance Manager

### Functional Requirements
- [ ] GET /transactions
  - [ ] Should return a list of Transaction
- [ ] GET /transactions?date=2026-03-27
  - [ ] Should return a list of Transaction by date
- [ ] POST /transactions
      Params: date, title, amound and status 
    - [ ] Should register a Transaction in db and return your id
- [ ] PUT /transaction/{id}
  - [ ] Should update a Transaction in db and return ok
- [ ] DELETE /transaction/{id}
  - [ ] Should update a Transaction in db as deleted and return ok
- [ ] POST /transactions/upload
      Params: file, type and organization
  - [ ] Should import Transactions from a bank csv file
  - [ ] Should register imported Transactions in db
  - [ ] Should not register duplicate Transactions in db
  - [ ] Should process in a queue, without block the request
  - [ ] Should return ok with processing status
- [ ] GET /transactions/export?startDate=2026-03-01&endDate=2026-03-31
  - [ ] Should get Transactions from a date range 
  - [ ] Should register the Transactions in a xlsx file
  - [ ] Should process in a queue, without block the request
  - [ ] Should return ok with processing status
- [ ] GET /persons
  - [ ] Should return a list of Person
- [ ] GET /persons?name=john-doe
  - [ ] Should return a list of Transaction by date
- [ ] POST /persons
  Params: name and phone number
  - [ ] Should register a Person in db and return your id
- [ ] PUT /persons/{id}
  - [ ] Should update a Person in db and return ok
- [ ] DELETE /persons/{id}
  - [ ] Should update a Person in db as deleted and return ok

### Non-Functional Requirements
- [ ] Reliability
- [ ] Resilient

### Nomenclatures
- Transaction: Pix, Bank Slip, TED, Purchase with Credit Card, etc..
- Organization: A bank or financial organization
- Person: A person that can be assigned to a transaction

### Getting Started

Run the development server (Arch Linux):

```bash
  cd api
dotnet run --launch-profile https
```

Open [Swagger](https://localhost:7026/swagger) with your browser to see the result.
