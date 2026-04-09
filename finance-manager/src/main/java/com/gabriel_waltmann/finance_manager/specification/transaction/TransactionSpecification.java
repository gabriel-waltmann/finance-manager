package com.gabriel_waltmann.finance_manager.specification.transaction;

import com.gabriel_waltmann.finance_manager.domain.transaction.Transaction;
import com.gabriel_waltmann.finance_manager.domain.transaction.TransactionListRequest;
import org.springframework.data.jpa.domain.Specification;

import java.util.ArrayList;
import java.util.List;
import jakarta.persistence.criteria.Predicate;
import org.springframework.stereotype.Component;

@Component
public class TransactionSpecification {
    public Specification<Transaction> list(TransactionListRequest request) {
        return (root, query, cb) -> {
            List<Predicate> predicates = new ArrayList<>();

            if (!request.withDeleted()) {
                predicates.add(cb.isNull(root.get("deleted_at")));
            }
            if (request.startDate() != null) {
                predicates.add(cb.greaterThanOrEqualTo(root.get("date"), request.startDate()));
            }
            if (request.endDate() != null) {
                predicates.add(cb.lessThanOrEqualTo(root.get("date"), request.endDate()));
            }
            return cb.and(predicates.toArray(new Predicate[0]));
        };
    }
}
