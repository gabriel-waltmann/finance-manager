package com.gabriel_waltmann.finance_manager.controller.person;

import com.gabriel_waltmann.finance_manager.domain.person.Person;
import com.gabriel_waltmann.finance_manager.domain.person.PersonListRequest;
import com.gabriel_waltmann.finance_manager.domain.person.PersonRequest;
import com.gabriel_waltmann.finance_manager.service.person.PersonService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.data.domain.Sort;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.UUID;

@RestController
@RequestMapping("/persons")
public class PersonController {
    @Autowired
    private PersonService service;

    @GetMapping
    ResponseEntity<List<Person>> list(
        @RequestParam(defaultValue = "10") String limit,
        @RequestParam(defaultValue = "1") String page,
        @RequestParam(defaultValue = "desc") String orderBy
    ) {
        int limitInt = Integer.parseInt(limit);

        int pageInt = Integer.parseInt(page); // request page 1 => db page 0
        int pageSql = pageInt - 1;

        Sort.Direction direction = orderBy.equals("desc") ? Sort.Direction.DESC : Sort.Direction.ASC;

        PersonListRequest request = new PersonListRequest(
                limitInt,
                pageSql,
                direction
        );

        List<Person> list = service.list(request);

        return ResponseEntity.ok(list);
    }

    @GetMapping("/{id}")
    ResponseEntity<Person> get(@PathVariable UUID id) {
        Person person = service.get(id);

        return person == null
            ? new ResponseEntity<>(HttpStatus.BAD_REQUEST)
            : ResponseEntity.ok(person);
    }

    @PostMapping
    // TODO: Add field validation
    ResponseEntity<Person> create(@RequestBody PersonRequest request) {
        Person person = service.create(request);

        return ResponseEntity.ok(person);
    }

    @PutMapping("{id}")
    // TODO: Add field validation
    ResponseEntity<Person> update(@PathVariable UUID id, @RequestBody PersonRequest request) {
        Person person = service.update(id, request);

        return person == null
            ? new ResponseEntity<>(HttpStatus.BAD_REQUEST)
            : ResponseEntity.ok(person);
    }

    @DeleteMapping("{id}")
    ResponseEntity<HttpStatus> delete(@PathVariable UUID id) {
        service.delete(id);

        return new ResponseEntity<>(HttpStatus.OK);
    }
}
