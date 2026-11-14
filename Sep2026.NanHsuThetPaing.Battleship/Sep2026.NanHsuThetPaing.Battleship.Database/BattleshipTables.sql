-- Creates the 3 Battleship tables in Sep2026Db.
-- Run this once in SSMS (or let it be) - it skips tables that already exist.

USE [Sep2026Db]
GO

IF OBJECT_ID(N'[dbo].[BattleshipGame]', N'U') IS NULL
CREATE TABLE [dbo].[BattleshipGame]
           ([GameId] INT IDENTITY(1,1) PRIMARY KEY
           ,[Status] NVARCHAR(20) NOT NULL
           ,[ShotsUsed] INT NOT NULL DEFAULT 0
           ,[CreatedAt] DATETIME NOT NULL DEFAULT GETDATE())
GO

IF OBJECT_ID(N'[dbo].[BattleshipShip]', N'U') IS NULL
CREATE TABLE [dbo].[BattleshipShip]
           ([ShipId] INT IDENTITY(1,1) PRIMARY KEY
           ,[GameId] INT NOT NULL FOREIGN KEY REFERENCES [dbo].[BattleshipGame]([GameId])
           ,[IsPlayerShip] BIT NOT NULL
           ,[Type] NVARCHAR(20) NOT NULL
           ,[Size] INT NOT NULL
           ,[StartRow] INT NOT NULL
           ,[StartCol] INT NOT NULL
           ,[IsHorizontal] BIT NOT NULL
           ,[Hits] INT NOT NULL DEFAULT 0
           ,[IsSunk] BIT NOT NULL DEFAULT 0)
GO

IF OBJECT_ID(N'[dbo].[BattleshipShot]', N'U') IS NULL
CREATE TABLE [dbo].[BattleshipShot]
           ([ShotId] INT IDENTITY(1,1) PRIMARY KEY
           ,[GameId] INT NOT NULL FOREIGN KEY REFERENCES [dbo].[BattleshipGame]([GameId])
           ,[ByComputer] BIT NOT NULL
           ,[Row] INT NOT NULL
           ,[Col] INT NOT NULL
           ,[Result] NVARCHAR(10) NOT NULL)
GO
