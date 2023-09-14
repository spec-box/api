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
   # установите утилиту для миграции структуры БД
   dotnet tool install -g thinkinghome.migrator.cli
   
   # обновите структуру БД
   migrate-database postgres "host=localhost;port=5432;database=tms;user name=postgres;password=123" ./SpecBox.Migrations/bin/Debug/net7.0/SpecBox.Migrations.dll
   ```
4. Запустите приложение
   ```shell
   export ASPNETCORE_ENVIRONMENT=Development
   export ConnectionStrings__default="host=localhost;port=5432;database=tms;user name=postgres;password=123"
   dotnet ./SpecBox.WebApi/bin/Debug/net7.0/SpecBox.WebApi.dll --urls=http://+:8080
   ```
5. Откройте в браузере адрес http://localhost:8080/swagger

## Контейнер

```bash
# сборка
docker build -t spec-box-api:0.0.1 -f ./SpecBox.WebApi/Dockerfile .


# локальный запуск
docker run -p 8080:80 -ti \
 --link postgres:postgres \
 -e ConnectionStrings__default='host=postgres;port=5432;database=tms;user name=postgres;password=123' \
 spec-box-api:0.0.1

docker run -p 5000:80 -t spec-box-api:0.0.1
```

### Информация

- документация API: https://localhost:7264/swagger

