# SEC4-lab3

## 1. Запуск через Docker

1. Збірка образу

   ```bash
   docker build -t sec4-lab3-tests .
   ```

2. Запуск тестів

   ```bash
   docker run --rm -v "$(pwd):/app" -w /app sec4-lab3-tests
   ```
