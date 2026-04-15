package com.gabriel_waltmann.finance_manager.domain.person;

import jakarta.persistence.*;
import lombok.AllArgsConstructor;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;
import org.hibernate.annotations.CreationTimestamp;
import org.hibernate.annotations.UpdateTimestamp;

import java.util.Date;
import java.util.UUID;

@Entity
@Getter
@Setter
@AllArgsConstructor
@NoArgsConstructor
@Table(name = "persons")
public class Person {
    @Id
    @GeneratedValue
    private UUID id;

    private String name;

    private String phone;

    @Column(name = "updated_at") @UpdateTimestamp
    private Date updated_at;

    @Column(name = "created_at") @CreationTimestamp
    private Date created_at;

    @Column(name = "deleted_at")
    private Date deleted_at;
}