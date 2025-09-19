# Bulls-Cows-Game (Console)

Классическая игра «Быки и коровы» в консольном исполнении. Генерируется секретная комбинация, игрок вводит догадки — программа отображает верные предположения пользователя и ведёт счёт попыток. Имеются пресеты сложности с разными правилами которую меняют геймплей.

[![.NET CI](https://github.com/cut9/Bulls-Cows-Game/actions/workflows/dotnet-ci-windows.yml/badge.svg)](https://github.com/cut9/Bulls-Cows-Game/actions)
[![Release](https://img.shields.io/github/v/release/cut9/Bulls-Cows-Game)](https://github.com/cut9/Bulls-Cows-Game/releases)

---

## Информация
- Платформа: кроссплатформенная консоль (.NET)
- Язык: C# .NET 8
- Тип: Консольное приложение (CLI)

---

## Быстрый старт — запустить релиз (рекомендуется для обычных пользователей)

1. Перейдите в раздел **Releases**: https://github.com/cut9/Bulls-Cows-Game/releases  
2. Скачайте архив `win-x64.rar` для вашей платформы.  
3. Распакуйте архив и дважды кликните `Bulls&Cows.exe`.

---

## Быстрый старт — запустить из исходников (для разработчиков)

### Команды (PowerShell / CMD)
```powershell
git clone https://github.com/cut9/Bulls-Cows-Game.git
cd Bulls-Cows-Game
dotnet restore
dotnet run --project ./"Bulls&Cows"/"Bulls&Cows.csproj"
```
