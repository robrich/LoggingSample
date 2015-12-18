CREATE TABLE [dbo].[User]
(
[Id] [int] NOT NULL IDENTITY(1, 1),
[CreateDate] [datetime] NOT NULL,
[ModifyDate] [datetime] NOT NULL,
[IsActive] [bit] NOT NULL,
[CompanyId] [int] NULL,
[SessionToken] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Email] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[EmailConfirmed] [bit] NOT NULL,
[FirstName] [nvarchar] (49) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[LastName] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
) ON [PRIMARY]
ALTER TABLE [dbo].[User] ADD
CONSTRAINT [FK_User_Company] FOREIGN KEY ([CompanyId]) REFERENCES [dbo].[Company] ([Id])
ALTER TABLE [dbo].[User] ADD
CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED  ([Id]) ON [PRIMARY]


GO
