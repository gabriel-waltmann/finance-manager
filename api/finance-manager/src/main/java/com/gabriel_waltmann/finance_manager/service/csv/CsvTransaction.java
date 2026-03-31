package com.gabriel_waltmann.finance_manager.service.csv;

import com.opencsv.bean.CsvBindByName;
import com.opencsv.bean.CsvDate;
import lombok.*;

import java.math.BigDecimal;
import java.util.Date;

@Builder
@Getter
@Setter
@AllArgsConstructor
@NoArgsConstructor
public class CsvTransaction {
    @CsvBindByName(column = "date")
    @CsvDate("yyyy-mm-dd")
    private Date date;

    @CsvBindByName(column = "title")
    private String title;

    @CsvBindByName(column = "amount")
    private BigDecimal amount;
}
