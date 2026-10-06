select u.id,u.name, t.id, t.title, t.iscompleted
from  public.users as u
left join public.tasks as t
on u.id = t.owner_id
order by u.id, t.id asc;
-- 6

select u.id,u.name,u.email,  t.id, t.title, t.iscompleted
from  public.users as u
inner join public.tasks as t
on u.id = t.owner_id order by u.id, t.id asc;
-- 5

select u.id,u.name, t.id, t.title, t.iscompleted
from  public.users as u
left join public.tasks as t
on u.id = t.owner_id 
where t.id is null
order by u.id asc;;
-- 1

select u.id,u.name, t.id, t.title, t.iscompleted
from  public.users as u
left join public.tasks as t
on u.id = t.owner_id 
and iscompleted = false
order by u.id, t.id asc;;
-- 5

select u.id,u.name, t.id, t.title, t.iscompleted
from  public.users as u
left join public.tasks as t
on u.id = t.owner_id 
where t.iscompleted = false
order by u.id, t.id asc;;
-- 3
