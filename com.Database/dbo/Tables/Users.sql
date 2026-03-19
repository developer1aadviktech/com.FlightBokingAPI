CREATE TABLE [dbo].[Users] (
    [Id]        INT            IDENTITY (1, 1) NOT NULL,
    [UserName]  NVARCHAR (100) NULL,
    [Email]     NVARCHAR (100) NULL,
    [CreatedBy] INT            NULL
);

