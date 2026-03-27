package com.gabriel_waltmann.finance_manager.service.transaction;

import com.gabriel_waltmann.finance_manager.domain.transaction.Transaction;
import com.gabriel_waltmann.finance_manager.domain.transaction.TransactionRequest;
import com.gabriel_waltmann.finance_manager.repository.transaction.TransactionRepository;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Pageable;
import org.springframework.data.domain.Page;
import org.springframework.stereotype.Service;

import java.util.List;

@Service
public class TransactionService {
    @Autowired
    private TransactionRepository repository;

    public Transaction create(TransactionRequest request) {
        Transaction transaction = new Transaction();

        transaction.setDate(request.date());
        transaction.setTitle(request.title());
        transaction.setAmount(request.amount());

        return repository.save(transaction);
    }

    public List<Transaction> list(int pageNumber, int limit) {
        Pageable pageable = PageRequest.of(pageNumber, limit);

        Page<Transaction> page = repository.findAll(pageable);

        return page.stream().toList();
    }
}
