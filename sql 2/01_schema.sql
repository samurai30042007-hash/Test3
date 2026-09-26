
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

