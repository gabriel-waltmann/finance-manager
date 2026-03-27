package com.gabriel_waltmann.finance_manager.controller.transaction;

import com.gabriel_waltmann.finance_manager.domain.transaction.Transaction;
import com.gabriel_waltmann.finance_manager.service.transaction.TransactionService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.util.List;

@RestController
@RequestMapping("/transactions")
public class TransactionController {
    @Autowired
    private TransactionService service;

    @GetMapping("/")
    ResponseEntity<List<Transaction>> list() {
        List<Transaction> list = service.list(1, 10);

        return ResponseEntity.ok(list);
    }
}
