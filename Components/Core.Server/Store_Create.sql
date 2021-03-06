SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ItemData](
	[ItemId] [uniqueidentifier] ROWGUIDCOL  NOT NULL CONSTRAINT [DF_ItemData_ItemId]  DEFAULT (newid()),
	[ItemName] [nvarchar](255) NOT NULL,
	[ItemType] [int] NOT NULL,
	[AppScope] [nvarchar](255) NULL,
	[AppProps] [nvarchar](max) NULL,
	[Created] [nvarchar](50) NOT NULL,
	[Expires] [nvarchar](50) NOT NULL,
	[DataType] [nvarchar](255) NULL,
	[YData] [image] NULL,
	[YSign] [varbinary](max) NULL,
	[NetScope] [nvarchar](255) NULL,
	[SysProps] [nvarchar](max) NULL,
	[StoreSRN] [int] NOT NULL CONSTRAINT [DF_ItemData_StoreRev]  DEFAULT ((0)),
	[StoreUSN] [bigint] NOT NULL CONSTRAINT [DF_ItemData_StoreUSN]  DEFAULT ((0)),
 CONSTRAINT [PK_ItemData] PRIMARY KEY CLUSTERED 
(
	[ItemId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF