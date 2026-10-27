@echo off
title Sistema Biblioteca UNI
echo ====================================================
echo    SISTEMA DE CONTROL DE BIBLIOTECA - UNI
echo ====================================================
echo Abriendo navegador en http://localhost:5087/Index.html ...
start http://localhost:5087/Index.html
echo Iniciando backend .NET...
dotnet run --project BibliotecaAPI
pause
