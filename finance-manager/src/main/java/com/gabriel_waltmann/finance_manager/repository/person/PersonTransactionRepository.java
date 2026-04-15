package com.gabriel_waltmann.finance_manager.repository.person;

import com.gabriel_waltmann.finance_manager.domain.person.PersonTransaction;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.JpaSpecificationExecutor;

import java.util.UUID;

public interface PersonTransactionRepository extends JpaRepository<PersonTransaction, UUID>, JpaSpecificationExecutor<PersonRepository> {
}
