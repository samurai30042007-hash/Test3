insert into public.test_table(title, mail, priority)
values ('Task 1', 'user1@example.com', 1), 
       ('Task 2', 'user2@example.com', 2), 
       ('Task 3', 'user3@example.com', 3),
       ('Task 6', 'user3@example.com', 1)
returning *;
insert into public.test_table(title, is_close)
values ('Task 4',  true),
       ('Task 5', false)
returning *;
update public.test_table
set is_close = true
where id = 1
returning *;
--insert into public.test_table (title)
--values ('  ');
--ERROR:  новая строка в отношении "test_table" нарушает ограничение-проверку "test_table_title_check"
--Ошибочная строка содержит (7,   , null, 2, f, 2026-09-24 09:10:02.717324+03). 
--insert into public.test_table (title,priority)
--values ('12', 4);
--ERROR:  новая строка в отношении "test_table" нарушает ограничение-проверку "test_table_priority_check"
--  Ошибочная строка содержит (8, 12, null, 4, f, 2026-09-24 09:11:21.258891+03). 