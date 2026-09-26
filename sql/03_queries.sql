insert into public.users (title, iscomplete, owner_id)
values ('A', false, 1),
('B', true, 1), ('C', true, 2),
('D', false, 4), ('E', false, 4)
returning *;