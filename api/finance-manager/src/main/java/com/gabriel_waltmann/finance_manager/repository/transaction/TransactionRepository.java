package com.gabriel_waltmann.finance_manager.repository.transaction;

import com.gabriel_waltmann.finance_manager.domain.transaction.Transaction;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.UUID;

public interface TransactionRepository extends JpaRepository<Transaction, UUID> {

}
