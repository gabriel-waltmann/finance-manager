package com.gabriel_waltmann.finance_manager.service.person;

import com.gabriel_waltmann.finance_manager.domain.person.Person;
import com.gabriel_waltmann.finance_manager.domain.person.PersonListRequest;
import com.gabriel_waltmann.finance_manager.domain.person.PersonRequest;
import com.gabriel_waltmann.finance_manager.repository.person.PersonRepository;
import com.gabriel_waltmann.finance_manager.specification.person.PersonSpecification;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.domain.Specification;
import org.springframework.stereotype.Service;

import java.util.Date;
import java.util.List;
import java.util.UUID;

@Service
public class PersonService {
    @Autowired
    private PersonRepository repository;

    @Autowired
    PersonSpecification personSpecification;

    public Person create(PersonRequest request) {
        Person person = new Person();

        person.setName(request.name());
        person.setPhone(request.phone());

        return repository.save(person);
    }

    public Person get(UUID id) {
        Person person = repository.findById(id).orElse(null);

        if (person == null) {
            return null;
        }

        boolean isNotDeleted = person.getDeleted_at() == null;

        return isNotDeleted ? person : null;
    }

    public List<Person> list(PersonListRequest request) {
        Pageable pageable = PageRequest.of(
                request.page(),
                request.limit(),
                request.orderBy(),
                "name"
        );

        Specification<Person> specification = this.personSpecification.list();

        Page<Person> page = repository.findAll(specification, pageable);

        return page.stream().toList();
    }

    public Person update(UUID id, PersonRequest request) {
        Person person = get(id);

        if (person == null) {
            return null;
        }

        person.setName(request.name());
        person.setPhone(request.phone());

        repository.save(person);

        return person;
    }

    public void delete(UUID id) {
        Person person = get(id);

        if (person == null) {
            return;
        }

        person.setDeleted_at(new Date());

        repository.save(person);
    }
}
