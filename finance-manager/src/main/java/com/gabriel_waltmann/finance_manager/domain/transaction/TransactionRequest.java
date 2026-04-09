package com.gabriel_waltmann.finance_manager.domain.transaction;

import java.math.BigDecimal;
import java.util.Date;

public record TransactionRequest (
    Date date,
    String title,
    BigDecimal amount
) {}
