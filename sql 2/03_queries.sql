select u.id,u.name, t.id, t.title, t.iscomplete
from  public.users as u
left join public.tasks as t
on u.id = t.owner_id;


select u.id,u.name, t.id, t.title, t.iscomplete
from  public.users as u
inner join public.tasks as t
on u.id = t.owner_id;


select u.id,u.name, t.id, t.title, t.iscomplete
from  public.users as u
left join public.tasks as t
on u.id = t.owner_id 
where t.id is null;

select u.id,u.name, t.id, t.title, t.iscomplete
from  public.users as u
left join public.tasks as t
on u.id = t.owner_id 
and iscomplete = false;

select u.id,u.name, t.id, t.title, t.iscomplete
from  public.users as u
left join public.tasks as t
on u.id = t.owner_id 
where iscomplete = false;

