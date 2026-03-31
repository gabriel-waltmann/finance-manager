package com.gabriel_waltmann.finance_manager.repository.transaction;

import com.gabriel_waltmann.finance_manager.domain.transaction.Transaction;
import jakarta.annotation.Nonnull;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;

import java.math.BigDecimal;
import java.util.Date;
import java.util.List;
import java.util.Optional;
import java.util.UUID;

public interface TransactionRepository extends JpaRepository<Transaction, UUID> {
    @Override
    @Nonnull
    @Query("SELECT t FROM Transaction t WHERE t.deleted_at IS NULL")
    List<Transaction> findAll();

    @Query("SELECT t FROM Transaction t")
    List<Transaction> findAllWithDeleted();

    @Query("SELECT t FROM Transaction t WHERE t.date = :date AND t.title = :title AND t.amount = :amount AND t.deleted_at IS NULL")
    Optional<Transaction> findDuplicatedNotDeleted(Date date, String title, BigDecimal amount);
}
