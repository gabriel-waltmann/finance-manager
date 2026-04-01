package com.gabriel_waltmann.finance_manager.domain.transaction;

import org.springframework.data.domain.Sort;

import java.sql.Date;

public record TransactionListRequest(
        boolean withDeleted,
        Date startDate,
        Date endDate,
        int limit,
        int page,
        Sort.Direction orderBy
) { }
