package com.gabriel_waltmann.finance_manager.service.csv;

import com.gabriel_waltmann.finance_manager.domain.transaction.Transaction;
import org.springframework.stereotype.Component;

@Component
public class CsvTransactionMapper implements CsvMapper<Transaction, CsvTransaction> {
    @Override
    public Transaction mapTo(CsvTransaction transactionCsv) {
        Transaction transaction = new Transaction();

        transaction.setDate(transactionCsv.getDate());
        transaction.setTitle(transactionCsv.getTitle());
        transaction.setAmount(transactionCsv.getAmount());

        return transaction;
    }

    @Override
    public CsvTransaction unmapFrom(Transaction transaction) {
        CsvTransaction transactionCsv = new CsvTransaction();

        transactionCsv.setDate(transaction.getDate());
        transactionCsv.setTitle(transaction.getTitle());
        transactionCsv.setAmount(transaction.getAmount());

        return transactionCsv;
    }
}
