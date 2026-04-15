package com.gabriel_waltmann.finance_manager.repository.person;


import com.gabriel_waltmann.finance_manager.domain.person.Person;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.JpaSpecificationExecutor;

import java.util.UUID;

public interface PersonRepository extends JpaRepository<Person, UUID>, JpaSpecificationExecutor<Person> {
}
