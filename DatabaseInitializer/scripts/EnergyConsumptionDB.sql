-- Script para crear la base de datos 'EnergyConsumption' si no existe
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'EnergyConsumption')
BEGIN
    EXEC('CREATE DATABASE EnergyConsumption');
END