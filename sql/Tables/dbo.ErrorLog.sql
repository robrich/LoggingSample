CREATE TABLE [dbo].[ErrorLog]
(
[Id] [int] NOT NULL IDENTITY(1, 1),
[CreateDate] [datetime] NOT NULL,
[ModifyDate] [datetime] NOT NULL,
[IsActive] [bit] NOT NULL,
[UserMessage] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[ExceptionDetails] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[HttpMethod] [nvarchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Url] [nvarchar] (2000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[ReferrerUrl] [nvarchar] (2000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[UserAgent] [nvarchar] (512) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[ClientAddress] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Headers] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Body] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[UserId] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[ErrorLog] ADD CONSTRAINT [PK_LogInfo] PRIMARY KEY CLUSTERED  ([Id]) ON [PRIMARY]
GO
