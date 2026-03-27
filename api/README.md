# Finance Manager

### Functional Requirements
- [ ] GET /transactions
  - [ ] Should return a list of Transaction
- [ ] GET /transactions?date=2026-03-27
  - [ ] Should return a list of Transaction by date
- [ ] POST /transactions
      Params: date, title and amound 
    - [ ] Should register a Transaction in db and return your id
- [ ] PUT /transaction/{id}
  - [ ] Should update a Transaction in db and return ok
- [ ] DELETE /transaction/{id}
  - [ ] Should update a Transaction in db as deleted and return ok
- [ ] POST /transactions/import
      Params: bankFile
  - [ ] Should import Transactions from a bank csv file, register in db
  - [ ] Should register imported Transactions in db
  - [ ] Should process in a queue, without block the request
  - [ ] Should return ok with processing status
- [ ] POST /transactions/import
    Params: bankFile
- [ ] Should import Transactions from a bank csv file, register in db
- [ ] Should register imported Transactions in db
- [ ] Should process in a queue, without block the request
- [ ] Should return ok with processing status
- [ ] GET /transactions/export?date=2026-03-27
  - [ ] Should get Transactions from a date range 
  - [ ] Should register the Transactions in a xlsx file
  - [ ] Should process in a queue, without block the request
  - [ ] Should return ok with processing status

### Non Functional Requirements
- [ ] Reliability
- [ ] Resilient

### Nomenclatures
- Transaction: Pix, Bank Slip, TED, Purchase with Credit Card, etc..
- Person: A generic definetion to person with name,   

### Getting Started

Run the development server:

```bash
yarn dev
```

Open [http://localhost:3000](http://localhost:3000) with your browser to see the result.
