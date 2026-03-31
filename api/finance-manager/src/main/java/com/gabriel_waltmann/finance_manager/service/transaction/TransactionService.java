package com.gabriel_waltmann.finance_manager.service.transaction;

import com.gabriel_waltmann.finance_manager.domain.transaction.Transaction;
import com.gabriel_waltmann.finance_manager.domain.transaction.TransactionRequest;
import com.gabriel_waltmann.finance_manager.repository.transaction.TransactionRepository;
import com.gabriel_waltmann.finance_manager.service.csv.CsvTransactionService;
import jakarta.transaction.Transactional;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Pageable;
import org.springframework.stereotype.Service;
import org.springframework.web.multipart.MultipartFile;

import java.io.IOException;
import java.io.InputStream;
import java.math.BigDecimal;
import java.util.*;

@Service
public class TransactionService {
    @Autowired
    private TransactionRepository repository;

    @Autowired
    private CsvTransactionService csvTransactionService;

    private Optional<Transaction> getExists(Date date, String title, BigDecimal amount) {
        return repository.findDuplicatedNotDeleted(date, title, amount);
    }

    public Transaction create(TransactionRequest request) {
        Optional<Transaction> exists = getExists(request.date(), request.title(), request.amount());

        if (exists.isPresent()) {
            return exists.get();
        }

        Transaction transaction = new Transaction();

        transaction.setDate(request.date());
        transaction.setTitle(request.title());
        transaction.setAmount(request.amount());

        return repository.save(transaction);
    }

    public Transaction get(UUID id) {
        Transaction transaction = repository.findById(id).orElse(null);

        if (transaction == null) {
            return null;
        }

        boolean isNotDeleted = transaction.getDeleted_at() == null;

        return isNotDeleted ? transaction : null;
    }

    // TODO: Add orderBy
    public List<Transaction> list(int page, int limit, String filter) {
        Pageable pageable = PageRequest.of(page, limit);

        boolean withDeletedFilter = filter != null && filter.equals("withDeleted");

        Page<Transaction> transactionsPage = withDeletedFilter
            ? repository.findAllWithDeleted(pageable)
            : repository.findAll(pageable);

        return transactionsPage.stream().toList();
    }

    public Transaction update(UUID id, TransactionRequest request) {
        Transaction transaction = get(id);

        if (transaction == null) {
            return null;
        }

        transaction.setDate(request.date());
        transaction.setTitle(request.title());
        transaction.setAmount(request.amount());

        repository.save(transaction);

        return transaction;
    }

    public void delete(UUID id) {
        Transaction transaction = get(id);

        if (transaction == null) {
            return;
        }

        transaction.setDeleted_at(new Date());

        repository.save(transaction);
    }

    @Transactional
    public void upload(MultipartFile file)  {
        try {
            InputStream inputStream = file.getInputStream();

            Set<Transaction> transactions = csvTransactionService.parseCsv((inputStream));

            for (Transaction transaction : transactions) {
                Optional<Transaction> exists = getExists(
                    transaction.getDate(),
                    transaction.getTitle(),
                    transaction.getAmount()
                );

                if (exists.isEmpty()) {
                    repository.save(transaction);
                }
            }
        } catch (IOException exception) {
            System.out.println(exception.getMessage());
        }
    }
}
