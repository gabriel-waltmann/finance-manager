package com.gabriel_waltmann.finance_manager.service.transaction;

import com.gabriel_waltmann.finance_manager.domain.transaction.Transaction;
import com.gabriel_waltmann.finance_manager.domain.transaction.TransactionRequest;
import com.gabriel_waltmann.finance_manager.repository.transaction.TransactionRepository;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Pageable;
import org.springframework.data.domain.Page;
import org.springframework.stereotype.Service;

import java.util.Date;
import java.util.List;
import java.util.Optional;
import java.util.UUID;

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

    public Transaction get(UUID id) {
        Transaction transaction = repository.findById(id).orElse(null);

        if (transaction == null) {
            return null;
        }

        boolean isNotDeleted = transaction.getDeleted_at() == null;

        return isNotDeleted ? transaction : null;
    }

    // TODO: Add pagination
    public List<Transaction> list(boolean withDeleted) {
        if (withDeleted) {
            return repository.findAllWithDeleted().stream().toList();
        }

        return repository.findAll().stream().toList();
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
}
