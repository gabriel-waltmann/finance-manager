package com.gabriel_waltmann.finance_manager.domain.transaction;

import org.springframework.data.domain.Sort;

import java.time.LocalDateTime;

public record TransactionListRequest(
        boolean withDeleted,
        LocalDateTime startDate,
        LocalDateTime endDate,
        int limit,
        int page,
        Sort.Direction orderBy
) { }
