
create table users (
id int  generated always as identity primary key,
name text not null check(length(trim(name)) > 0),
email text not null unique
 );

create table tasks(
id int  generated always as identity primary key,
title text not null check(length(trim(title)) > 0),
iscomplete boolean not null default false,
created_at timestamptz default CURRENT_TIMESTAMP,
owner_id int not null references public.users(id) on delete cascade
);


insert into public.users (name, email)
values ('Alex', 'alex@example.com'), 
('Maria', 'maria@example.com'),
('Ivan', 'ivan@example.com'),
('Oleg', 'oleg@example.com')
returning *;

--1	"Alex"	"alex@example.com"
--2	"Maria"	"maria@example.com"
--3	"Ivan"	"ivan@example.com"
--4	"Oleg"	"oleg@example.com"

insert into public.tasks (title, iscomplete, owner_id)
values ('A', false, 1),
('B', true, 1), ('C', true, 2),
('D', false, 4), ('E', false, 4)
returning *;

--1	"A"	false	"2026-09-26 11:25:23.830524+03"	1
--2	"B"	true	"2026-09-26 11:25:23.830524+03"	1
--3	"C"	true	"2026-09-26 11:25:23.830524+03"	2
--4	"D"	false	"2026-09-26 11:25:23.830524+03"	4
--5	"E"	false	"2026-09-26 11:25:23.830524+03"	4


insert into public.tasks (title, iscomplete, owner_id)
values ('O', false, 100)
returning *;

--ERROR:  INSERT или UPDATE в таблице "tasks" нарушает ограничение внешнего ключа "tasks_owner_id_fkey"
--Ключ (owner_id)=(100) отсутствует в таблице "users". 

insert into public.tasks (title, iscomplete)
values ('O', false)
returning *;

--ERROR:  значение NULL в столбце "owner_id" отношения "tasks" нарушает ограничение NOT NULL
--Ошибочная строка содержит (7, O, f, 2026-09-26 11:27:47.484207+03, null). 

insert into public.users (name, email)
values ('Alex', 'mast.Delete@gmail.com');

insert into public.tasks (title, iscomplete, owner_id)
values ('O', false, 5), ('l', true, 5);
select  *
from public.tasks;
delete from public.users
where id = 5;
select  *
from public.tasks;