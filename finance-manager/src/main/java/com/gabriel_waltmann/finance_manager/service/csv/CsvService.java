package com.gabriel_waltmann.finance_manager.service.csv;

import com.opencsv.bean.CsvToBean;
import com.opencsv.bean.CsvToBeanBuilder;
import com.opencsv.bean.HeaderColumnNameMappingStrategy;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;

import java.io.*;
import java.util.Set;
import java.util.stream.Collectors;

@Service
@RequiredArgsConstructor
public class CsvService<T, K> {
    final CsvMapper<T, K> csvMapper;

    final public Set<T> parseCSV(InputStream stream, Class<K> clazz) throws IOException {
        try (Reader reader = new BufferedReader(new InputStreamReader(stream))) {
            HeaderColumnNameMappingStrategy<K> strategy = new HeaderColumnNameMappingStrategy<>();
            strategy.setType(clazz);
            CsvToBean<K> csvToBean = new CsvToBeanBuilder<K>(reader)
                    .withMappingStrategy(strategy)
                    .withIgnoreEmptyLine(true)
                    .withIgnoreLeadingWhiteSpace(true)
                    .build();

            return csvToBean.parse()
                    .stream()
                    .map(csvMapper::mapTo)
                    .collect(Collectors.toSet());
        }
    }

}
