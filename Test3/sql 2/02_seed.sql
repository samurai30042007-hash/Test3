insert into public.users (name, email)
values ('Alex', 'alex@example.com'), 
('Maria', 'maria@example.com'),
('Ivan', 'ivan@example.com'),
('Oleg', 'oleg@example.com')
returning *;

insert into public.tasks (title, iscompleted, owner_id)
values ('A', false, 1),
('B', true, 1), ('C', true, 2),
('D', false, 4), ('E', false, 4)
returning *;
