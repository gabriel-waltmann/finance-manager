package com.gabriel_waltmann.finance_manager.domain.person;

import org.springframework.data.domain.Sort;

public record PersonListRequest(
    int limit,
    int page,
    Sort.Direction orderBy
) { }
