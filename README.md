# SpecBox.WebApi

В этом репозитории находится БД и веб-API для структурированного хранения информации о функциональных требованиях продукта. Приложение реализовано на C# (.NET7) и хранит данные в PostgreSQL.

## Как запустить

1. Соберите проект
   ```shell
   dotnet restore
   dotnet build
   ```
2. Запустите СУБД
   ```shell
   docker run --name postgres -e POSTGRES_PASSWORD=123 -p 5432:5432 -d postgres
   ```
3. Обновите структуру БД
   ```shell
   migrate-database postgres "host=localhost;port=5432;database=tms;user name=postgres;password=123" ./SpecBox.Migrations/bin/Debug/net7.0/SpecBox.Migrations.dll
   ```
4. Запустите приложение

### Информация

- документация API: https://localhost:7264/swagger

