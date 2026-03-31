package com.gabriel_waltmann.finance_manager.service.csv;

import org.springframework.stereotype.Repository;

@Repository
public interface CsvMapper<T, K> {
    T mapTo(K k);
    K unmapFrom(T t);
}
