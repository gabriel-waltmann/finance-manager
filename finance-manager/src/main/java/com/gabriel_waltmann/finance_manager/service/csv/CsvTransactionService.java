package com.gabriel_waltmann.finance_manager.service.csv;

import com.gabriel_waltmann.finance_manager.domain.transaction.Transaction;
import lombok.RequiredArgsConstructor;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import java.io.IOException;
import java.io.InputStream;
import java.util.Set;

@Service
@RequiredArgsConstructor
public class CsvTransactionService {
    @Autowired
    private CsvService<Transaction, CsvTransaction> csvService;

    public Set<Transaction> parseCsv(InputStream inputStream) throws IOException {
        return csvService.parseCSV(inputStream, CsvTransaction.class);
    }
}
