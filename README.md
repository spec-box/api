

Поднять контейнер БД

```
docker run --name postgres -e POSTGRES_PASSWORD=123 -p 5432:5432 -d postgres
```

Инициализировать структуру БД

```sh
migrate-database postgres "host=localhost;port=5432;database=tms;user name=postgres;password=123" ./tms.Migrations/bin/Debug/net7.0/tms.Migrations.dll
```
