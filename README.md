# Finance Manager

### Functional Requirements
- [X] GET /transactions
  - [X] Should return a list of Transaction
- [ ] GET /transactions?date=2026-03-27
  - [ ] Should return a list of Transaction by date
- [X] POST /transactions
      Params: date, title, amound and status 
    - [X] Should register a Transaction in db and return your id
- [X] PUT /transaction/{id}
  - [X] Should update a Transaction in db and return ok
- [X] DELETE /transaction/{id}
  - [X] Should update a Transaction in db as deleted and return ok
- [X] POST /transactions/upload
      Params: file, type and organization
  - [X] Should import Transactions from a bank csv file
  - [X] Should register imported Transactions in db
  - [X] Should not register duplicate Transactions in db
  - [ ] Should process in a queue, without block the request
  - [ ] Should return ok with processing status
- [ ] GET /transactions/export?startDate=2026-03-01&endDate=2026-03-31
  - [ ] Should get Transactions from a date range 
  - [ ] Should register the Transactions in a xlsx file
  - [ ] Should process in a queue, without block the request
  - [ ] Should return ok with processing status
- [X] GET /persons
  - [X] Should return a list of Person
- [ ] GET /persons?name=john-doe
    - [ ] Should return a list of Person by name
- [X] POST /persons
  Params: name and phone number
  - [X] Should register a Person in db and return your id
- [X] PUT /persons/{id}
  - [X] Should update a Person in db and return ok
- [X] DELETE /persons/{id}
  - [X] Should update a Person in db as deleted and return ok

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
  sudo pacman -S maven
  mvn spring-boot:run -Dspring-boot.run.jvmArguments="-Duser.timezone=UTC"
```

Open [http://localhost:8080](http://localhost:8080) with your browser to see the result.
