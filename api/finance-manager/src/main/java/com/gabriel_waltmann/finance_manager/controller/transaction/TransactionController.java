package com.gabriel_waltmann.finance_manager.controller.transaction;

import com.gabriel_waltmann.finance_manager.domain.transaction.Transaction;
import com.gabriel_waltmann.finance_manager.domain.transaction.TransactionListRequest;
import com.gabriel_waltmann.finance_manager.domain.transaction.TransactionRequest;
import com.gabriel_waltmann.finance_manager.service.transaction.TransactionService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.data.domain.Sort;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.multipart.MultipartFile;

import java.time.LocalDate;
import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;
import java.util.List;
import java.util.UUID;

@RestController
@RequestMapping("/transactions")
public class TransactionController {
    @Autowired
    private TransactionService service;

    @GetMapping
    ResponseEntity<List<Transaction>> list(
            @RequestParam(required = false) String withDeleted,
            @RequestParam(required = false) String startDate,
            @RequestParam(required = false) String endDate,
            @RequestParam(defaultValue = "10") String limit,
            @RequestParam(defaultValue = "1") String page,
            @RequestParam(defaultValue = "desc") String orderBy
    ) {
        boolean withDeletedBool = withDeleted != null && withDeleted.equals("true");

        DateTimeFormatter parser = DateTimeFormatter.ofPattern("yyyy-MM-dd");

        LocalDateTime startDateSql = startDate != null && !startDate.isEmpty()
            ? LocalDate.parse(startDate, parser).atStartOfDay()
            : null;

        LocalDateTime endDateSql = endDate != null && !endDate.isEmpty()
            ? LocalDate.parse(endDate, parser)
                .atTime(23, 59, 59)
            : null;

        int limitInt = Integer.parseInt(limit);

        int pageInt = Integer.parseInt(page); // request page 1 => db page 0
        int pageSql = pageInt - 1;

        Sort.Direction direction = orderBy.equals("desc") ? Sort.Direction.DESC : Sort.Direction.ASC;

        TransactionListRequest request = new TransactionListRequest(
                withDeletedBool,
                startDateSql,
                endDateSql,
                limitInt,
                pageSql,
                direction
        );

        List<Transaction> list = service.list(request);

        return ResponseEntity.ok(list);
    }

    @GetMapping("/{id}")
    ResponseEntity<Transaction> get(@PathVariable UUID id) {
        Transaction transaction = service.get(id);

        return transaction == null
                ? new ResponseEntity<>(HttpStatus.BAD_REQUEST)
                : ResponseEntity.ok(transaction);
    }

    @PostMapping
    // TODO: Add field validation
    ResponseEntity<Transaction> create(@RequestBody TransactionRequest request) {
        Transaction transaction = service.create(request);

        return ResponseEntity.ok(transaction);
    }

    @PutMapping("{id}")
    // TODO: Add field validation
    ResponseEntity<Transaction> update(@PathVariable UUID id, @RequestBody TransactionRequest request) {
        Transaction transaction = service.update(id, request);

        return transaction == null
                ? new ResponseEntity<>(HttpStatus.BAD_REQUEST)
                : ResponseEntity.ok(transaction);
    }

    @DeleteMapping("{id}")
    ResponseEntity<HttpStatus> delete(@PathVariable UUID id) {
        service.delete(id);

        return new ResponseEntity<>(HttpStatus.OK);
    }

    @PostMapping("/upload")
    ResponseEntity<HttpStatus> upload (
            @RequestPart MultipartFile file,
            @RequestParam String type,
            @RequestParam String organization
    ) {
        service.upload(file);

        return new ResponseEntity<>(HttpStatus.OK);
    }
}
