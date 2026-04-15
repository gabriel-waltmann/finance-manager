package com.gabriel_waltmann.finance_manager.specification.person;

import com.gabriel_waltmann.finance_manager.domain.person.Person;
import jakarta.persistence.criteria.Predicate;
import org.springframework.data.jpa.domain.Specification;
import org.springframework.stereotype.Component;

import java.util.ArrayList;
import java.util.List;

@Component
public class PersonSpecification {
    public Specification<Person> list() {
        return (root, query, cb) -> {
            List<Predicate> predicates = new ArrayList<>();

            predicates.add(cb.isNull(root.get("deleted_at")));

            return cb.and(predicates.toArray(new Predicate[0]));
        };
    }
}
