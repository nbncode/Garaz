USE [Garaaz]
GO
/****** Object:  Table [dbo].[__MigrationHistory]    Script Date: 16-3-2020 11:05:58 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__MigrationHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ContextKey] [nvarchar](300) NOT NULL,
	[Model] [varbinary](max) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK_dbo.__MigrationHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC,
	[ContextKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[AccountLedger]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AccountLedger](
	[AccountLeaderId] [int] IDENTITY(1,1) NOT NULL,
	[Location] [nvarchar](500) NULL,
	[PartyName] [nvarchar](500) NULL,
	[Code] [nvarchar](200) NULL,
	[Date] [datetime] NULL,
	[Particulars] [nvarchar](500) NULL,
	[VchType] [nvarchar](200) NULL,
	[VchNo] [nvarchar](200) NULL,
	[Debit] [decimal](18, 2) NULL,
	[Credit] [decimal](18, 2) NULL,
	[DistributorId] [int] NULL,
	[WorkshopId] [int] NULL,
	[IsClosing] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[AccountLeaderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[AspNetRoles]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoles](
	[Id] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetRoles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[AspNetUserClaims]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.AspNetUserClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[AspNetUserLogins]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserLogins](
	[LoginProvider] [nvarchar](128) NOT NULL,
	[ProviderKey] [nvarchar](128) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetUserLogins] PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC,
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[AspNetUserRoles]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserRoles](
	[UserId] [nvarchar](128) NOT NULL,
	[RoleId] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetUserRoles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[AspNetUsers]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUsers](
	[Id] [nvarchar](128) NOT NULL,
	[Email] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](max) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEndDateUtc] [datetime] NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
	[UserName] [nvarchar](256) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[AssuredGift]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AssuredGift](
	[AssuredGiftId] [int] IDENTITY(1,1) NOT NULL,
	[SchemeId] [int] NULL,
	[Target] [nvarchar](500) NULL,
	[Point] [nvarchar](500) NULL,
	[Reward] [nvarchar](500) NULL,
 CONSTRAINT [PK_AssuredGift] PRIMARY KEY CLUSTERED 
(
	[AssuredGiftId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[AssuredGiftCategory]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AssuredGiftCategory](
	[AssuredGiftCategoryId] [int] IDENTITY(1,1) NOT NULL,
	[AssuredGiftId] [int] NULL,
	[CategoryId] [int] NULL,
	[IsAll] [bit] NULL,
 CONSTRAINT [PK_AssuredGiftCategory] PRIMARY KEY CLUSTERED 
(
	[AssuredGiftCategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[BannerMobile]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BannerMobile](
	[BannerId] [int] IDENTITY(1,1) NOT NULL,
	[BannerImage] [nvarchar](max) NULL,
	[BannerName] [nvarchar](200) NULL,
	[Type] [nvarchar](500) NULL,
	[Data] [nvarchar](max) NULL,
	[SchemeId] [int] NULL,
	[CreateDate] [datetime] NULL,
	[DistributorId] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[BannerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[BestSeller]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BestSeller](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[GroupId] [int] NULL,
	[DistributorId] [int] NULL,
 CONSTRAINT [PK_BestSeller] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Brand]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Brand](
	[BrandId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Image] [nvarchar](600) NULL,
	[CreatedDate] [datetime] NULL,
	[IsOriparts] [bit] NULL,
 CONSTRAINT [PK_Brand] PRIMARY KEY CLUSTERED 
(
	[BrandId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CashBack]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CashBack](
	[CashbackId] [int] IDENTITY(1,1) NOT NULL,
	[SchemeId] [int] NULL,
	[FromAmount] [decimal](18, 2) NULL,
	[ToAmount] [decimal](18, 2) NULL,
	[Benifit] [nvarchar](max) NULL,
 CONSTRAINT [PK_CashBack] PRIMARY KEY CLUSTERED 
(
	[CashbackId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CashbackRange]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CashbackRange](
	[CashbackRangeId] [int] IDENTITY(1,1) NOT NULL,
	[StartRange] [decimal](18, 2) NULL,
	[EndRange] [decimal](18, 2) NULL,
	[SchemeId] [int] NULL,
 CONSTRAINT [PK_CashbackRange] PRIMARY KEY CLUSTERED 
(
	[CashbackRangeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CashbackRangeMix]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CashbackRangeMix](
	[CashbackRangeMixId] [int] IDENTITY(1,1) NOT NULL,
	[CashbackId] [int] NULL,
	[CashbackRangeId] [int] NULL,
	[SchemeId] [int] NULL,
	[Percentage] [decimal](18, 2) NULL,
 CONSTRAINT [PK_CashbackRangeMix] PRIMARY KEY CLUSTERED 
(
	[CashbackRangeMixId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CategorySchemes]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CategorySchemes](
	[CategoryId] [int] IDENTITY(1,1) NOT NULL,
	[Category] [nvarchar](500) NULL,
	[MinAmount] [decimal](18, 2) NULL,
	[MaxAmount] [decimal](18, 2) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[SchemeId] [int] NULL,
 CONSTRAINT [PK_CategorySchemes] PRIMARY KEY CLUSTERED 
(
	[CategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Coupons]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Coupons](
	[CouponId] [int] IDENTITY(1,1) NOT NULL,
	[SchemeId] [int] NOT NULL,
	[WorkshopId] [int] NOT NULL,
	[CouponNumber] [nvarchar](10) NOT NULL,
 CONSTRAINT [PK_Coupons] PRIMARY KEY CLUSTERED 
(
	[CouponId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CustomerBackOrder]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerBackOrder](
	[CustomerOrderId] [int] IDENTITY(1,1) NOT NULL,
	[DistributorId] [int] NULL,
	[WorkshopId] [int] NULL,
	[CONo] [nvarchar](50) NULL,
	[CODate] [date] NULL,
	[PartyType] [nvarchar](50) NULL,
	[PartyCode] [nvarchar](50) NULL,
	[PartyName] [nvarchar](50) NULL,
	[CustomerOrderType] [nvarchar](50) NULL,
	[OrderType] [nvarchar](50) NULL,
	[PartStatus] [nvarchar](50) NULL,
	[PartNum] [nvarchar](50) NULL,
	[PartDesc] [nvarchar](50) NULL,
	[BinLoc] [nvarchar](50) NULL,
	[OrderedQty] [nvarchar](50) NULL,
	[ProcessedQty] [nvarchar](50) NULL,
	[PendingOrCancelledQty] [nvarchar](50) NULL,
	[StockQty] [nvarchar](50) NULL,
	[SellingPrice] [nvarchar](50) NULL,
	[LocCode] [nvarchar](200) NULL,
	[Order] [nvarchar](200) NULL,
	[CBO] [nvarchar](200) NULL,
	[StkMW] [nvarchar](200) NULL,
	[ETA] [nvarchar](200) NULL,
	[Inv] [nvarchar](200) NULL,
	[Pick] [nvarchar](200) NULL,
	[Alloc] [nvarchar](200) NULL,
	[BO] [nvarchar](200) NULL,
	[AO] [nvarchar](200) NULL,
	[Action] [nvarchar](200) NULL,
	[Remark] [nvarchar](200) NULL,
	[PD] [nvarchar](200) NULL,
 CONSTRAINT [PK_CustomerBackOrder] PRIMARY KEY CLUSTERED 
(
	[CustomerOrderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DailySalesTrackerWithInvoiceData]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DailySalesTrackerWithInvoiceData](
	[DailySalesTrackerId] [int] IDENTITY(1,1) NOT NULL,
	[Region] [nvarchar](50) NULL,
	[DealerCode] [nvarchar](50) NULL,
	[LocCode] [nvarchar](50) NULL,
	[LocDesc] [nvarchar](50) NULL,
	[PartNum] [nvarchar](50) NULL,
	[PartDesc] [nvarchar](50) NULL,
	[RootPartNum] [nvarchar](50) NULL,
	[PartCategory] [nvarchar](50) NULL,
	[Day] [nvarchar](50) NULL,
	[PartGroup] [nvarchar](50) NULL,
	[CalMonthYear] [nvarchar](50) NULL,
	[ConsPartyCode] [nvarchar](50) NULL,
	[ConsPartyName] [nvarchar](50) NULL,
	[ConsPartyTypeDesc] [nvarchar](50) NULL,
	[DocumentNum] [nvarchar](50) NULL,
	[Remarks] [nvarchar](50) NULL,
	[RetailQty] [nvarchar](50) NULL,
	[ReturnQty] [nvarchar](50) NULL,
	[NetRetailQty] [nvarchar](50) NULL,
	[RetailSelling] [nvarchar](50) NULL,
	[ReturnSelling] [nvarchar](50) NULL,
	[NetRetailSelling] [nvarchar](50) NULL,
	[DiscountAmount] [nvarchar](50) NULL,
	[UserId] [nvarchar](128) NULL,
	[DistributorId] [int] NULL,
	[CreatedDate] [datetime] NULL,
	[WorkShopId] [int] NULL,
	[GroupId] [int] NULL,
	[ProductId] [int] NULL,
	[CoNo] [nvarchar](100) NULL,
 CONSTRAINT [PK_DailySalesTrackerWithInvoiceData] PRIMARY KEY CLUSTERED 
(
	[DailySalesTrackerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DailyStock]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DailyStock](
	[StockId] [int] IDENTITY(1,1) NOT NULL,
	[DistributorId] [int] NULL,
	[OutletId] [int] NULL,
	[ConsigneeCode] [nvarchar](500) NULL,
	[DealerCode] [nvarchar](500) NULL,
	[LocationCode] [nvarchar](500) NULL,
	[PartCategoryCode] [nvarchar](500) NULL,
	[PartNum] [nvarchar](500) NULL,
	[RootPartNum] [nvarchar](500) NULL,
	[StockQuantity] [nvarchar](500) NULL,
	[Mrp] [nvarchar](500) NULL,
 CONSTRAINT [PK_DailyStock] PRIMARY KEY CLUSTERED 
(
	[StockId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DefaultWinners]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DefaultWinners](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SchemeId] [int] NOT NULL,
	[WorkshopId] [int] NOT NULL,
	[GiftId] [int] NOT NULL,
	[CouponNumber] [nvarchar](10) NOT NULL,
 CONSTRAINT [PK_DefaultWinners] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DeliveryAddress]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DeliveryAddress](
	[DeliveryAddressId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
	[PinCode] [nvarchar](50) NOT NULL,
	[MobileNo] [nvarchar](50) NOT NULL,
	[FirstName] [nvarchar](500) NOT NULL,
	[LastName] [nvarchar](500) NOT NULL,
	[Address] [nvarchar](500) NOT NULL,
	[City] [nvarchar](100) NOT NULL,
	[state] [nvarchar](500) NULL,
 CONSTRAINT [PK_DeliveryAddress] PRIMARY KEY CLUSTERED 
(
	[DeliveryAddressId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DistributorBrand]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DistributorBrand](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DistributorId] [int] NOT NULL,
	[BrandId] [int] NOT NULL,
 CONSTRAINT [PK_DistributorBrand] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DistributorLocation]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DistributorLocation](
	[LocationId] [int] IDENTITY(1,1) NOT NULL,
	[DistributorId] [int] NULL,
	[Location] [nvarchar](500) NULL,
	[LocationCode] [nvarchar](100) NULL,
 CONSTRAINT [PK_DistributorLocation] PRIMARY KEY CLUSTERED 
(
	[LocationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Distributors]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Distributors](
	[DistributorId] [int] IDENTITY(1,1) NOT NULL,
	[DistributorName] [nvarchar](500) NULL,
	[Address] [nvarchar](max) NULL,
	[Latitude] [nvarchar](50) NULL,
	[Longitude] [nvarchar](50) NULL,
	[Gstin] [nvarchar](50) NULL,
	[Pincode] [nvarchar](50) NULL,
	[State] [nvarchar](500) NULL,
	[City] [nvarchar](500) NULL,
	[Gender] [nchar](10) NULL,
	[LandlineNumber] [nvarchar](50) NULL,
	[FValue] [decimal](18, 2) NULL,
	[MValue] [decimal](18, 2) NULL,
	[SValue] [decimal](18, 2) NULL,
	[Company] [nvarchar](500) NULL,
	[UPIID] [nvarchar](500) NULL,
	[BankName] [nvarchar](500) NULL,
	[IfscCode] [nvarchar](500) NULL,
	[AccountNumber] [nvarchar](500) NULL,
 CONSTRAINT [PK_Distributers] PRIMARY KEY CLUSTERED 
(
	[DistributorId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DistributorsOutlets]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DistributorsOutlets](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DistributorId] [int] NULL,
	[OutletId] [int] NULL,
	[UserId] [nvarchar](128) NULL,
 CONSTRAINT [PK_DistributorsOutlets] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DistributorUser]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DistributorUser](
	[DistributorUserId] [int] IDENTITY(1,1) NOT NULL,
	[DistributorId] [int] NULL,
	[UserId] [nvarchar](128) NULL,
 CONSTRAINT [PK_DistributerUser] PRIMARY KEY CLUSTERED 
(
	[DistributorUserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DistributorUserInfo]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DistributorUserInfo](
	[DistributorUserId] [int] IDENTITY(1,1) NOT NULL,
	[Address] [nvarchar](100) NULL,
	[Latitude] [nvarchar](50) NULL,
	[Longitude] [nvarchar](50) NULL,
	[UserId] [nvarchar](128) NULL,
 CONSTRAINT [PK_DistributorUserInfo] PRIMARY KEY CLUSTERED 
(
	[DistributorUserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DistributorWorkShops]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DistributorWorkShops](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DistributorId] [int] NULL,
	[WorkShopId] [int] NULL,
	[UserId] [nvarchar](128) NULL,
 CONSTRAINT [PK_DistributorWorkShops] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Features]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Features](
	[FeatureId] [int] IDENTITY(1,1) NOT NULL,
	[FeatureName] [nvarchar](500) NULL,
	[FeatureValue] [nvarchar](500) NULL,
	[IsDefault] [bit] NOT NULL,
 CONSTRAINT [PK_Features] PRIMARY KEY CLUSTERED 
(
	[FeatureId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[FeaturesRole]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FeaturesRole](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FeatureId] [int] NULL,
	[RoleId] [nvarchar](128) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[FmsGroupSale]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FmsGroupSale](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SchemeId] [int] NOT NULL,
	[GroupId] [int] NOT NULL,
	[PartCreation] [char](1) NOT NULL,
	[TotalSale] [decimal](18, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[FocusPart]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FocusPart](
	[FocusPartId] [int] IDENTITY(1,1) NOT NULL,
	[SchemeId] [int] NULL,
	[GroupId] [int] NULL,
	[ProductId] [int] NULL,
	[Qty] [int] NULL,
	[Type] [nvarchar](500) NULL,
	[Value] [nvarchar](500) NULL,
	[Price] [decimal](18, 2) NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_FocusPart] PRIMARY KEY CLUSTERED 
(
	[FocusPartId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[GeneralPurpose]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GeneralPurpose](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Heading1] [nvarchar](max) NULL,
	[Heading2] [nvarchar](max) NOT NULL,
	[Heading3] [nvarchar](max) NULL,
	[Heading4] [nvarchar](max) NULL,
	[Heading5] [nvarchar](max) NULL,
	[Heading6] [nvarchar](max) NULL,
	[Heading7] [nvarchar](max) NULL,
	[Heading8] [nvarchar](max) NULL,
	[Heading9] [nvarchar](max) NULL,
	[Heading10] [nvarchar](max) NULL,
	[Heading11] [nvarchar](max) NULL,
	[ShowSecondScreen] [bit] NULL,
	[SecondScreenText] [nvarchar](max) NULL,
	[EnableSignupMailDelay] [bit] NULL,
	[DelayTime] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[GiftCategory]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GiftCategory](
	[GiftCategoryId] [int] IDENTITY(1,1) NOT NULL,
	[GiftId] [int] NULL,
	[CategoryId] [int] NULL,
	[IsAll] [bit] NULL,
 CONSTRAINT [PK_GiftCategory] PRIMARY KEY CLUSTERED 
(
	[GiftCategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[GiftManagement]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GiftManagement](
	[GiftId] [int] IDENTITY(1,1) NOT NULL,
	[SchemeId] [int] NULL,
	[Gift] [nvarchar](500) NULL,
	[Qty] [int] NULL,
	[DrawOrder] [nvarchar](500) NULL,
	[ImagePath] [nvarchar](500) NULL,
 CONSTRAINT [PK_GiftManagement] PRIMARY KEY CLUSTERED 
(
	[GiftId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[GiftsCoupon]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GiftsCoupon](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[GiftId] [int] NOT NULL,
	[WorkshopId] [int] NOT NULL,
	[CouponNumber] [nvarchar](10) NOT NULL,
 CONSTRAINT [PK_GiftsCoupon] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[GrowthPercentage]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GrowthPercentage](
	[GrowthPercentageId] [int] IDENTITY(1,1) NOT NULL,
	[DistributorId] [int] NULL,
	[GrowthPercentage] [decimal](18, 2) NULL,
	[MinValue] [decimal](18, 2) NULL,
 CONSTRAINT [PK__GrowthPe__21D845480E13028B] PRIMARY KEY CLUSTERED 
(
	[GrowthPercentageId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[LabelCriteria]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LabelCriteria](
	[CriteriaId] [int] IDENTITY(1,1) NOT NULL,
	[LabelName] [nvarchar](500) NULL,
	[TypeOfCriteria] [nvarchar](500) NULL,
	[GroupId] [int] NULL,
	[ProductId] [int] NULL,
	[SchemeId] [int] NULL,
	[Condition] [nvarchar](500) NULL,
	[Value] [nvarchar](500) NULL,
	[SaleAmount] [decimal](18, 2) NULL,
	[SaleCondition] [nvarchar](500) NULL,
	[Qty] [int] NULL,
	[Operator] [nvarchar](5) NULL,
 CONSTRAINT [PK_LabelCriteria] PRIMARY KEY CLUSTERED 
(
	[CriteriaId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[LoginTime]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoginTime](
	[LoginId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](128) NULL,
	[IpAddress] [nvarchar](500) NULL,
	[CreatedDate] [datetime] NULL,
 CONSTRAINT [PK_LoginTime] PRIMARY KEY CLUSTERED 
(
	[LoginId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Logs]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Logs](
	[LogsId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](128) NULL,
	[Details] [nvarchar](max) NULL,
	[Link] [nvarchar](max) NULL,
	[CreatedDate] [datetime] NULL,
	[Status] [nvarchar](500) NULL,
	[OutstandingAmount] [decimal](18, 0) NULL,
	[PaymentId] [nvarchar](max) NULL,
 CONSTRAINT [PK__Logs__C920548E9E30554A] PRIMARY KEY CLUSTERED 
(
	[LogsId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MailTemplate]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MailTemplate](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[MailHeading] [nvarchar](500) NULL,
	[MailHtml] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[SmsText] [nvarchar](500) NULL,
	[Type] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NULL,
 CONSTRAINT [PK_MailTemplate] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MgaCatalougeBanner]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MgaCatalougeBanner](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ImagePath] [nvarchar](max) NULL,
	[CreateDate] [datetime] NULL,
	[DistributorId] [int] NULL,
	[ShortDescription] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MgaCatalougeProduct]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MgaCatalougeProduct](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[BannerId] [int] NULL,
	[ProductId] [int] NULL,
	[CreateDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Notifications]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Notifications](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
	[Type] [nvarchar](max) NOT NULL,
	[Message] [nvarchar](max) NOT NULL,
	[IsRead] [bit] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[RefUserId] [nvarchar](128) NULL,
	[WorkshopId] [int] NULL,
 CONSTRAINT [PK_Notifications] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[OrderDetails]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderDetails](
	[OrderDetailID] [int] IDENTITY(1,1) NOT NULL,
	[OrderID] [int] NOT NULL,
	[ProductID] [int] NULL,
	[ProductName] [nvarchar](max) NULL,
	[UnitPrice] [decimal](18, 2) NULL,
	[Qty] [int] NULL,
	[TotalPrice] [decimal](18, 2) NULL,
	[ImagePath] [nvarchar](600) NULL,
	[OutletId] [int] NULL,
	[AvailabilityType] [nvarchar](500) NULL,
 CONSTRAINT [PK_OrderDetails] PRIMARY KEY CLUSTERED 
(
	[OrderDetailID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[OrderTable]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderTable](
	[OrderID] [int] IDENTITY(1,1) NOT NULL,
	[OrderNo] [nvarchar](500) NULL,
	[DistributorId] [int] NULL,
	[WorkshopID] [int] NULL,
	[DeliveryAddressId] [int] NULL,
	[PaymentMethod] [nvarchar](500) NULL,
	[SubTotal] [decimal](18, 2) NULL,
	[Discount] [decimal](18, 2) NULL,
	[DeliveryCharge] [decimal](18, 2) NULL,
	[PackingCharge] [decimal](18, 2) NULL,
	[UserID] [nvarchar](128) NULL,
	[OrderTotal] [decimal](18, 2) NULL,
	[OrderDate] [datetime] NULL,
	[DiscountCode] [nvarchar](50) NULL,
	[OrderStatus] [nvarchar](50) NOT NULL,
	[OutletId] [int] NULL,
	[CancelDate] [datetime] NULL,
	[RazorpayOrderId] [nvarchar](100) NULL,
	[RazorpayPaymentId] [nvarchar](100) NULL,
	[RazorpaySignature] [nvarchar](max) NULL,
	[FromType] [nvarchar](50) NULL,
 CONSTRAINT [PK_OrderTable] PRIMARY KEY CLUSTERED 
(
	[OrderID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Outlets]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Outlets](
	[OutletId] [int] IDENTITY(1,1) NOT NULL,
	[OutletName] [nvarchar](500) NULL,
	[Address] [nvarchar](max) NULL,
	[Latitude] [nvarchar](50) NULL,
	[Longitude] [nvarchar](50) NULL,
	[LocationId] [int] NULL,
	[OutletCode] [nvarchar](500) NULL,
 CONSTRAINT [PK_Outlets] PRIMARY KEY CLUSTERED 
(
	[OutletId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[OutletsUsers]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OutletsUsers](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OutletId] [int] NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_OutletsUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Outstanding]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Outstanding](
	[OutstandingId] [int] IDENTITY(1,1) NOT NULL,
	[PartyCode] [nvarchar](200) NULL,
	[PartyName] [nvarchar](200) NULL,
	[CustomerType] [nvarchar](200) NULL,
	[SalesExecutiveOrBranchCode] [nvarchar](200) NULL,
	[PendingBills] [nvarchar](200) NULL,
	[LessThan7Days] [nvarchar](200) NULL,
	[7To14Days] [nvarchar](200) NULL,
	[14To21Days] [nvarchar](200) NULL,
	[21To28Days] [nvarchar](200) NULL,
	[28To35Days] [nvarchar](200) NULL,
	[35To50Days] [nvarchar](200) NULL,
	[50To70Days] [nvarchar](200) NULL,
	[MoreThan70Days] [nvarchar](200) NULL,
	[DistributorId] [int] NULL,
	[TotalOutstanding] [decimal](18, 2) NULL,
	[WorkshopId] [int] NULL,
	[CreatedDate] [datetime] NULL,
 CONSTRAINT [PK_Outstanding] PRIMARY KEY CLUSTERED 
(
	[OutstandingId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Product]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Product](
	[ProductId] [int] IDENTITY(1,1) NOT NULL,
	[DistributorId] [int] NULL,
	[GroupId] [int] NULL,
	[ProductName] [nvarchar](500) NULL,
	[PartNo] [nvarchar](500) NULL,
	[Price] [decimal](18, 2) NULL,
	[Description] [nvarchar](max) NULL,
	[Remarks] [nvarchar](max) NULL,
	[ImagePath] [nvarchar](500) NULL,
	[CreatedDate] [datetime] NULL,
	[ProductType] [nvarchar](500) NULL,
	[BinLocation] [nvarchar](100) NULL,
	[PurchasePrice] [decimal](18, 2) NULL,
	[LatestLandedCost] [decimal](18, 2) NULL,
	[TaxPaidSellingPrice] [decimal](18, 2) NULL,
	[TaxableSellingPrice] [decimal](18, 2) NULL,
	[CurrentStock] [int] NULL,
	[InventoryValue] [decimal](18, 2) NULL,
	[Abc] [nvarchar](10) NULL,
	[Fms] [nvarchar](10) NULL,
	[Xyz] [nvarchar](10) NULL,
	[MovementCode] [nvarchar](50) NULL,
	[PackQuantity] [int] NULL,
	[Margin] [int] NULL,
	[SequenceNo] [int] NULL,
	[IssueIndicator] [nvarchar](500) NULL,
	[StartDate] [date] NULL,
	[CloseDate] [date] NULL,
	[ModelsApplicable] [nvarchar](200) NULL,
	[SalesTaxCategory] [nvarchar](10) NULL,
	[TaxDesc] [nvarchar](200) NULL,
	[OnOrderQtyMul] [int] NULL,
	[InTransitQty] [int] NULL,
	[AllocQty] [int] NULL,
	[FloatStock] [int] NULL,
	[MinimumLevel] [nvarchar](200) NULL,
	[MaximumLevel] [nvarchar](200) NULL,
	[ReorderLevel] [nvarchar](200) NULL,
	[Last12MonthAvgConsumption] [int] NULL,
	[ReservationQty] [nvarchar](200) NULL,
	[SafetyStock] [nvarchar](200) NULL,
	[SeasonalPartYn] [nvarchar](200) NULL,
	[DeadStockYn] [nvarchar](10) NULL,
	[ReasonToEditInPo] [nvarchar](200) NULL,
	[VorPartYn] [nvarchar](10) NULL,
	[HsCode] [int] NULL,
	[QuarantineQty] [nvarchar](200) NULL,
	[BrandId] [int] NULL,
	[TaxValue] [nvarchar](500) NULL,
	[RootPartNum] [nvarchar](500) NULL,
	[PartCategoryCode] [nvarchar](500) NULL,
 CONSTRAINT [PK_Product] PRIMARY KEY CLUSTERED 
(
	[ProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ProductAvailabilityType]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductAvailabilityType](
	[Id] [int] NOT NULL,
	[Text] [nvarchar](500) NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_ProductAvailabilityType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ProductGroup]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductGroup](
	[GroupId] [int] IDENTITY(1,1) NOT NULL,
	[GroupName] [nvarchar](500) NULL,
	[CreatedDate] [datetime] NULL,
	[ParentId] [int] NULL,
	[VariantId] [int] NULL,
	[ImagePath] [nvarchar](600) NULL,
	[DistributorId] [int] NULL,
	[BrandId] [int] NULL,
 CONSTRAINT [PK_ProductGroup] PRIMARY KEY CLUSTERED 
(
	[GroupId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ProductType]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TypeName] [nvarchar](500) NULL,
	[TypeNameUseInFile] [nvarchar](500) NULL,
	[CreatedDate] [datetime] NULL,
 CONSTRAINT [PK_ProductType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[QualifyCriteria]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[QualifyCriteria](
	[QualifyCriteriaId] [int] IDENTITY(1,1) NOT NULL,
	[SchemeId] [int] NULL,
	[AmountUpto] [decimal](18, 2) NULL,
	[Type] [nvarchar](500) NULL,
	[NumberOfCoupons] [int] NULL,
	[TypeValue] [nvarchar](500) NULL,
	[IsAll] [bit] NULL,
	[AdditionalCouponAmount] [decimal](18, 2) NULL,
	[AdditionalNumberOfCoupons] [int] NULL,
 CONSTRAINT [PK_QualifyCriteria] PRIMARY KEY CLUSTERED 
(
	[QualifyCriteriaId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[RoSalesExecutive]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RoSalesExecutive](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RoUserId] [nvarchar](128) NOT NULL,
	[SeUserId] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_RoSalesExecutive] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SalesExecutiveWorkshop]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SalesExecutiveWorkshop](
	[SewId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
	[WorkshopId] [int] NOT NULL,
 CONSTRAINT [PK_SalesExecutiveWorkshop] PRIMARY KEY CLUSTERED 
(
	[SewId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SchemeLocations]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SchemeLocations](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SchemeId] [int] NULL,
	[LocationId] [int] NULL,
 CONSTRAINT [PK_Table1] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Schemes]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Schemes](
	[SchemeId] [int] IDENTITY(1,1) NOT NULL,
	[SchemeName] [nvarchar](500) NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[DispersalFrequency] [nvarchar](500) NULL,
	[SchemesType] [nvarchar](500) NULL,
	[ThumbnailImage] [nvarchar](500) NULL,
	[BannerImage] [nvarchar](500) NULL,
	[PartCategory] [nvarchar](500) NULL,
	[PartyType] [nvarchar](500) NULL,
	[PartCreations] [nvarchar](500) NULL,
	[SchemeFor] [nvarchar](500) NULL,
	[TargetWorkshopCriteria] [nvarchar](500) NULL,
	[AreBothCbAgApplicable] [bit] NULL,
	[AssuredGift] [bit] NULL,
	[CashBack] [bit] NULL,
	[CanTakeMoreThanOneGift] [bit] NULL,
	[FocusPartApplicable] [bit] NULL,
	[CreatedDate] [datetime] NULL,
	[Type] [nvarchar](500) NULL,
	[DistributorId] [int] NULL,
	[UserId] [nvarchar](128) NULL,
	[IsDeleted] [bit] NULL,
	[IsActive] [bit] NULL,
	[TargetCriteria] [nvarchar](50) NULL,
	[StartRange] [datetime] NULL,
	[EndRange] [datetime] NULL,
	[RoInchargeId] [nvarchar](128) NULL,
	[SalesExecutiveId] [nvarchar](max) NULL,
	[IsAllRoInchargeSelected] [bit] NULL,
	[IsAllSalesExecutiveSelected] [bit] NULL,
	[IsCouponAllocated] [bit] NULL,
	[MaxGiftsAllowed] [int] NULL,
	[CashbackCriteria] [nvarchar](100) NULL,
	[FValue] [decimal](18, 2) NULL,
	[MValue] [decimal](18, 2) NULL,
	[SValue] [decimal](18, 2) NULL,
	[FocusPartTarget] [decimal](10, 2) NULL,
	[FocusPartBenifitType] [nvarchar](50) NULL,
	[FocusPartBenifitTypeValue] [nvarchar](100) NULL,
	[FocusPartBenifitTypeNumber] [nvarchar](100) NULL,
	[PartType] [nvarchar](500) NULL,
	[SubSchemeType] [nvarchar](500) NULL,
	[BranchCode] [nvarchar](max) NULL,
	[PrevYearFromDate] [date] NULL,
	[PrevYearToDate] [date] NULL,
	[PrevMonthFromDate] [date] NULL,
	[PrevMonthToDate] [date] NULL,
	[LuckyDraw] [bit] NULL,
	[GrowthCompPercentMinValue] [decimal](18, 0) NULL,
	[GrowthCompPercentBaseValue] [decimal](18, 0) NULL,
	[AreBothAgLdApplicable] [bit] NOT NULL,
 CONSTRAINT [PK_Schemes] PRIMARY KEY CLUSTERED 
(
	[SchemeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[StockColor]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StockColor](
	[StockId] [int] IDENTITY(1,1) NOT NULL,
	[StockType] [nvarchar](50) NOT NULL,
	[Qty] [int] NULL,
	[Color] [nvarchar](10) NOT NULL,
	[Tag] [nvarchar](200) NULL,
 CONSTRAINT [PK_StockColor] PRIMARY KEY CLUSTERED 
(
	[StockId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Support]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Support](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Heading] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[CreatedDate] [datetime] NULL,
 CONSTRAINT [PK_Support] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SupportQuery]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SupportQuery](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Subject] [nvarchar](500) NULL,
	[Message] [nvarchar](max) NULL,
	[UserId] [nvarchar](128) NULL,
	[CreatedDate] [datetime] NULL,
 CONSTRAINT [PK_SupportQuery] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[TargetGrowth]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TargetGrowth](
	[TragetGrowthId] [int] IDENTITY(1,1) NOT NULL,
	[SchemeId] [int] NULL,
	[Min] [decimal](18, 2) NULL,
	[Max] [decimal](18, 2) NULL,
	[Growth] [decimal](18, 2) NULL,
 CONSTRAINT [PK_TargetGrowth] PRIMARY KEY CLUSTERED 
(
	[TragetGrowthId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[TargetWorkShop]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TargetWorkShop](
	[TargetWorkShopId] [int] IDENTITY(1,1) NOT NULL,
	[WorkShopId] [int] NULL,
	[SchemeId] [int] NULL,
	[NewTarget] [nvarchar](500) NULL,
	[IsQualifiedAsDefault] [bit] NULL,
	[GrowthPercentage] [decimal](18, 2) NULL,
	[PrevYearAvgSale] [decimal](18, 2) NULL,
	[PrevMonthAvgSale] [decimal](18, 2) NULL,
	[GrowthComparisonPercentage] [decimal](18, 2) NULL,
	[CustomerType] [nvarchar](max) NULL,
 CONSTRAINT [PK_TargetWorkShop] PRIMARY KEY CLUSTERED 
(
	[TargetWorkShopId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[TempOrder]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TempOrder](
	[TempOrderId] [int] IDENTITY(1,1) NOT NULL,
	[OrderNo] [nvarchar](500) NULL,
	[DistributorId] [int] NULL,
	[WorkShopId] [int] NULL,
	[DeliveryAddressId] [int] NULL,
	[PaymentMethod] [nvarchar](500) NULL,
	[SubTotal] [decimal](18, 2) NULL,
	[Discount] [decimal](18, 2) NULL,
	[DiscountCode] [nvarchar](50) NULL,
	[DeliveryCharge] [decimal](18, 2) NULL,
	[PackingCharge] [decimal](18, 2) NULL,
	[GrandTotal] [decimal](18, 2) NULL,
	[UserId] [nvarchar](128) NULL,
	[OrderDate] [datetime] NULL,
 CONSTRAINT [PK_TempOrder] PRIMARY KEY CLUSTERED 
(
	[TempOrderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[TempOrderDetails]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TempOrderDetails](
	[TempOrderDetailId] [int] IDENTITY(1,1) NOT NULL,
	[TempOrderId] [int] NULL,
	[ProductId] [int] NULL,
	[Qty] [int] NULL,
	[UnitPrice] [decimal](18, 2) NULL,
	[TotalPrice] [decimal](18, 2) NULL,
	[AvailabilityType] [nvarchar](500) NULL,
	[OutletId] [int] NULL,
 CONSTRAINT [PK_TempOrderDetails] PRIMARY KEY CLUSTERED 
(
	[TempOrderDetailId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[TicketOfJoy]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TicketOfJoy](
	[TicketId] [int] IDENTITY(1,1) NOT NULL,
	[SchemeId] [int] NULL,
	[GroupId] [int] NULL,
	[ProductId] [int] NULL,
	[Type] [nvarchar](500) NULL,
	[Value] [nvarchar](500) NULL,
	[CouponCode] [nvarchar](500) NULL,
 CONSTRAINT [PK_TicketOfJoy] PRIMARY KEY CLUSTERED 
(
	[TicketId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[UserDetails]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserDetails](
	[UserDetailId] [int] IDENTITY(1,1) NOT NULL,
	[ConsPartyCode] [nvarchar](200) NULL,
	[FirstName] [nvarchar](500) NULL,
	[LastName] [nvarchar](500) NULL,
	[UserId] [nvarchar](128) NULL,
	[DistributorId] [nvarchar](128) NULL,
	[IsDeleted] [bit] NULL,
	[OTP] [int] NULL,
	[Address] [nvarchar](max) NULL,
	[Latitude] [nvarchar](50) NULL,
	[Longitude] [nvarchar](50) NULL,
	[Password] [nvarchar](500) NULL,
	[Designations] [nvarchar](200) NULL,
	[WalletAmount] [decimal](18, 2) NULL,
	[UserImage] [nvarchar](max) NULL,
	[TempOrderId] [int] NULL,
 CONSTRAINT [PK_UserDetails] PRIMARY KEY CLUSTERED 
(
	[UserDetailId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[UserFeatures]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserFeatures](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FeatureId] [int] NULL,
	[UserId] [nvarchar](128) NULL,
	[Feature] [bit] NULL,
 CONSTRAINT [PK_UserFeatures] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[UserWorkshop]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserWorkshop](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DistributorId] [int] NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
	[WorkshopId] [int] NOT NULL,
 CONSTRAINT [PK_UserWorkshop] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Variants]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Variants](
	[VariantId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Region] [nvarchar](100) NULL,
	[ProductionYear] [nvarchar](50) NULL,
	[Engine] [nvarchar](200) NULL,
	[Description] [nvarchar](600) NULL,
	[ChassisType] [nvarchar](200) NULL,
	[VNo] [nvarchar](200) NULL,
	[ParentId] [int] NULL,
	[VehicleId] [int] NULL,
	[CreatedDate] [datetime] NULL,
 CONSTRAINT [PK_Variants] PRIMARY KEY CLUSTERED 
(
	[VariantId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Vehicle]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Vehicle](
	[VehicleId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Image] [nvarchar](600) NULL,
	[CreatedDate] [datetime] NULL,
	[BrandId] [int] NULL,
 CONSTRAINT [PK_Vehicle] PRIMARY KEY CLUSTERED 
(
	[VehicleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[WalletTransaction]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WalletTransaction](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](128) NULL,
	[WorkshopId] [int] NULL,
	[Type] [nvarchar](50) NULL,
	[Amount] [decimal](18, 2) NULL,
	[Description] [nvarchar](max) NULL,
	[RefId] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NULL,
 CONSTRAINT [PK_WalletTransaction] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[WorkshopBrand]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WorkshopBrand](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[WorkshopId] [int] NOT NULL,
	[BrandId] [int] NOT NULL,
 CONSTRAINT [PK_WorkshopBrand] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[WorkshopCoupon]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WorkshopCoupon](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](128) NULL,
	[WorkshopId] [int] NULL,
	[Amount] [decimal](18, 2) NULL,
	[CouponNo] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
 CONSTRAINT [PK_WorkshopCoupon] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[WorkShopData]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WorkShopData](
	[WorkShopId] [int] IDENTITY(1,1) NOT NULL,
	[CustomerType] [nvarchar](500) NULL,
	[CustomerCode] [nvarchar](500) NULL,
	[PartyOwner1] [nchar](500) NULL,
	[CustomerName] [nvarchar](500) NULL,
	[Address1] [nvarchar](500) NULL,
	[Address2] [nvarchar](500) NULL,
	[Address3] [nvarchar](500) NULL,
	[CityCd] [nvarchar](50) NULL,
	[City] [nvarchar](500) NULL,
	[ResPhone] [nvarchar](50) NULL,
	[AlternateNumber] [nvarchar](50) NULL,
	[MobilePhone] [nvarchar](50) NULL,
	[Fax] [nvarchar](100) NULL,
	[Email] [nvarchar](100) NULL,
	[Tin] [nvarchar](100) NULL,
	[GstinNo] [nvarchar](200) NULL,
	[PartsDiscountCateg] [nvarchar](200) NULL,
	[Designation] [nvarchar](200) NULL,
	[ActiveYn] [nvarchar](100) NULL,
	[Executive] [nvarchar](500) NULL,
	[BranchCode] [nvarchar](200) NULL,
	[CreditLimit] [nvarchar](200) NULL,
	[Days] [nvarchar](200) NULL,
	[ParentGroup] [nvarchar](500) NULL,
	[DealerMapCD] [nvarchar](500) NULL,
	[PartyTitle] [nvarchar](500) NULL,
	[State] [nvarchar](500) NULL,
	[Region] [nvarchar](250) NULL,
	[Country] [nvarchar](500) NULL,
	[PinCode] [nvarchar](100) NULL,
	[PanNo] [nvarchar](250) NULL,
	[DiscountPer] [nvarchar](100) NULL,
	[May_19] [nvarchar](100) NULL,
	[Jun_19] [nvarchar](100) NULL,
	[Jul_19] [nvarchar](100) NULL,
	[Avarage] [nvarchar](250) NULL,
	[CreditDays] [nvarchar](100) NULL,
	[ContactTitleCD] [nvarchar](500) NULL,
	[ContactPersion] [nvarchar](500) NULL,
	[Pjp] [nvarchar](100) NULL,
	[SalesExecName] [nvarchar](500) NULL,
	[SalesExecutiveROIncharge] [nvarchar](500) NULL,
	[SalesHeadName] [nvarchar](500) NULL,
	[SalesExecutiveROInchargeCode] [nvarchar](100) NULL,
	[SalesHeadCode] [nvarchar](100) NULL,
	[OMCEOCode] [nvarchar](500) NULL,
	[SNO] [nvarchar](100) NULL,
	[PartyOwner2] [nvarchar](500) NULL,
	[PaymentType] [nvarchar](200) NULL,
	[BranchName] [nvarchar](500) NULL,
	[SalesExecNumber] [nvarchar](200) NULL,
 CONSTRAINT [PK__WorkShop__5E8376C7817B49A4] PRIMARY KEY CLUSTERED 
(
	[WorkShopId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[WorkShopLabelSchemes]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WorkShopLabelSchemes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[WorkShopId] [int] NOT NULL,
	[SchemeId] [int] NOT NULL,
	[CriteriaId] [int] NULL,
 CONSTRAINT [PK_WorkShopLabelSchemes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[WorkShops]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WorkShops](
	[WorkShopId] [int] IDENTITY(1,1) NOT NULL,
	[WorkShopName] [nvarchar](500) NULL,
	[Address] [nvarchar](max) NULL,
	[Latitude] [nvarchar](50) NULL,
	[Longitude] [nvarchar](50) NULL,
	[Gstin] [nvarchar](50) NULL,
	[Pincode] [nvarchar](50) NULL,
	[State] [nvarchar](500) NULL,
	[City] [nvarchar](500) NULL,
	[Gender] [nchar](10) NULL,
	[LandlineNumber] [nvarchar](50) NULL,
	[CriticalOutstandingDays] [int] NULL,
	[OutstandingAmount] [decimal](18, 2) NULL,
	[outletId] [int] NULL,
	[CategoryName] [nvarchar](500) NULL,
	[CreditLimit] [decimal](18, 2) NULL,
	[TotalOutstanding] [decimal](18, 2) NULL,
	[BillingName] [nvarchar](500) NULL,
	[YearOfEstablishment] [int] NULL,
	[Type] [nvarchar](500) NULL,
	[Make] [nvarchar](max) NULL,
	[JobsUndertaken] [nvarchar](500) NULL,
	[Premise] [nvarchar](200) NULL,
	[GaraazArea] [nvarchar](100) NULL,
	[TwoPostLifts] [int] NULL,
	[WashingBay] [bit] NULL,
	[PaintBooth] [bit] NULL,
	[ScanningAndToolKit] [bit] NULL,
	[TotalOwners] [int] NULL,
	[TotalChiefMechanics] [int] NULL,
	[TotalEmployees] [int] NULL,
	[MonthlyVehiclesServiced] [int] NULL,
	[MonthlyPartPurchase] [decimal](18, 2) NULL,
	[MonthlyConsumablesPurchase] [decimal](18, 2) NULL,
	[WorkingHours] [nvarchar](100) NULL,
	[WeeklyOffDay] [nvarchar](100) NULL,
	[Website] [nvarchar](500) NULL,
	[InsuranceCompanies] [nvarchar](max) NULL,
	[IsMoreThanOneBranch] [bit] NULL,
 CONSTRAINT [PK_WorkShops] PRIMARY KEY CLUSTERED 
(
	[WorkShopId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[WorkshopSchemesSelectedType]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WorkshopSchemesSelectedType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](200) NULL,
	[WorkshopId] [int] NULL,
	[SchemeId] [int] NULL,
	[SelectedOption] [nvarchar](200) NULL,
	[CreatedDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[WorkshopsUsers]    Script Date: 16-3-2020 11:05:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WorkshopsUsers](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[WorkshopId] [int] NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_WorkshopsUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[AssuredGiftCategory] ADD  CONSTRAINT [DF_AssuredGiftCategory_IsAll]  DEFAULT ((0)) FOR [IsAll]
GO
ALTER TABLE [dbo].[BannerMobile] ADD  DEFAULT (getdate()) FOR [CreateDate]
GO
ALTER TABLE [dbo].[Brand] ADD  DEFAULT ((0)) FOR [IsOriparts]
GO
ALTER TABLE [dbo].[CategorySchemes] ADD  CONSTRAINT [DF_CategorySchemes_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Distributors] ADD  CONSTRAINT [DF_Distributors_FValue]  DEFAULT ((75)) FOR [FValue]
GO
ALTER TABLE [dbo].[Distributors] ADD  CONSTRAINT [DF_Distributors_MValue]  DEFAULT ((15)) FOR [MValue]
GO
ALTER TABLE [dbo].[Distributors] ADD  CONSTRAINT [DF_Distributors_SValue]  DEFAULT ((10)) FOR [SValue]
GO
ALTER TABLE [dbo].[Features] ADD  DEFAULT ((0)) FOR [IsDefault]
GO
ALTER TABLE [dbo].[GiftCategory] ADD  DEFAULT ((0)) FOR [IsAll]
GO
ALTER TABLE [dbo].[ProductAvailabilityType] ADD  CONSTRAINT [DF_ProductAvailabilityType_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[ProductGroup] ADD  CONSTRAINT [DF_ProductGroup_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Schemes] ADD  CONSTRAINT [DF_Schemes_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Schemes] ADD  CONSTRAINT [DF__Schemes__IsDelet__3FD07829]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Schemes] ADD  CONSTRAINT [DF__Schemes__IsActiv__65F62111]  DEFAULT ((0)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Schemes] ADD  DEFAULT ((0)) FOR [AreBothAgLdApplicable]
GO
ALTER TABLE [dbo].[AccountLedger]  WITH CHECK ADD  CONSTRAINT [FK__AccountLe__Works__1980B20F] FOREIGN KEY([DistributorId])
REFERENCES [dbo].[Distributors] ([DistributorId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AccountLedger] CHECK CONSTRAINT [FK__AccountLe__Works__1980B20F]
GO
ALTER TABLE [dbo].[AccountLedger]  WITH CHECK ADD  CONSTRAINT [FK__AccountLe__Works__1A74D648] FOREIGN KEY([WorkshopId])
REFERENCES [dbo].[WorkShops] ([WorkShopId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AccountLedger] CHECK CONSTRAINT [FK__AccountLe__Works__1A74D648]
GO
ALTER TABLE [dbo].[AspNetUserClaims]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserClaims] CHECK CONSTRAINT [FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserLogins]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserLogins] CHECK CONSTRAINT [FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AssuredGift]  WITH CHECK ADD  CONSTRAINT [FK_AssuredGift_Schemes] FOREIGN KEY([SchemeId])
REFERENCES [dbo].[Schemes] ([SchemeId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AssuredGift] CHECK CONSTRAINT [FK_AssuredGift_Schemes]
GO
ALTER TABLE [dbo].[AssuredGiftCategory]  WITH CHECK ADD  CONSTRAINT [FK_AssuredGiftCategory_AssuredGift] FOREIGN KEY([AssuredGiftId])
REFERENCES [dbo].[AssuredGift] ([AssuredGiftId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AssuredGiftCategory] CHECK CONSTRAINT [FK_AssuredGiftCategory_AssuredGift]
GO
ALTER TABLE [dbo].[AssuredGiftCategory]  WITH CHECK ADD  CONSTRAINT [FK_AssuredGiftCategory_CategorySchemes] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[CategorySchemes] ([CategoryId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AssuredGiftCategory] CHECK CONSTRAINT [FK_AssuredGiftCategory_CategorySchemes]
GO
ALTER TABLE [dbo].[BannerMobile]  WITH CHECK ADD  CONSTRAINT [FK__BannerMob__Creat__76D69450] FOREIGN KEY([SchemeId])
REFERENCES [dbo].[Schemes] ([SchemeId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[BannerMobile] CHECK CONSTRAINT [FK__BannerMob__Creat__76D69450]
GO
ALTER TABLE [dbo].[BestSeller]  WITH CHECK ADD  CONSTRAINT [FK_BestSeller_Distributors] FOREIGN KEY([DistributorId])
REFERENCES [dbo].[Distributors] ([DistributorId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[BestSeller] CHECK CONSTRAINT [FK_BestSeller_Distributors]
GO
ALTER TABLE [dbo].[CashBack]  WITH CHECK ADD  CONSTRAINT [FK_CashBack_Schemes] FOREIGN KEY([SchemeId])
REFERENCES [dbo].[Schemes] ([SchemeId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CashBack] CHECK CONSTRAINT [FK_CashBack_Schemes]
GO
ALTER TABLE [dbo].[CashbackRange]  WITH CHECK ADD  CONSTRAINT [FK_CashbackRange_Schemes] FOREIGN KEY([SchemeId])
REFERENCES [dbo].[Schemes] ([SchemeId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CashbackRange] CHECK CONSTRAINT [FK_CashbackRange_Schemes]
GO
ALTER TABLE [dbo].[CashbackRangeMix]  WITH CHECK ADD  CONSTRAINT [FK_CashbackRangeMix_CashBack] FOREIGN KEY([CashbackId])
REFERENCES [dbo].[CashBack] ([CashbackId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CashbackRangeMix] CHECK CONSTRAINT [FK_CashbackRangeMix_CashBack]
GO
ALTER TABLE [dbo].[CashbackRangeMix]  WITH CHECK ADD  CONSTRAINT [FK_CashbackRangeMix_CashbackRange] FOREIGN KEY([CashbackRangeId])
REFERENCES [dbo].[CashbackRange] ([CashbackRangeId])
GO
ALTER TABLE [dbo].[CashbackRangeMix] CHECK CONSTRAINT [FK_CashbackRangeMix_CashbackRange]
GO
ALTER TABLE [dbo].[CashbackRangeMix]  WITH CHECK ADD  CONSTRAINT [FK_CashbackRangeMix_Schemes] FOREIGN KEY([SchemeId])
REFERENCES [dbo].[Schemes] ([SchemeId])
GO
ALTER TABLE [dbo].[CashbackRangeMix] CHECK CONSTRAINT [FK_CashbackRangeMix_Schemes]
GO
ALTER TABLE [dbo].[CategorySchemes]  WITH CHECK ADD  CONSTRAINT [FK_CategorySchemes_Schemes] FOREIGN KEY([SchemeId])
REFERENCES [dbo].[Schemes] ([SchemeId])
GO
ALTER TABLE [dbo].[CategorySchemes] CHECK CONSTRAINT [FK_CategorySchemes_Schemes]
GO
ALTER TABLE [dbo].[Coupons]  WITH CHECK ADD  CONSTRAINT [FK_Coupons_Schemes] FOREIGN KEY([SchemeId])
REFERENCES [dbo].[Schemes] ([SchemeId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Coupons] CHECK CONSTRAINT [FK_Coupons_Schemes]
GO
ALTER TABLE [dbo].[Coupons]  WITH CHECK ADD  CONSTRAINT [FK_Coupons_WorkShops] FOREIGN KEY([WorkshopId])
REFERENCES [dbo].[WorkShops] ([WorkShopId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Coupons] CHECK CONSTRAINT [FK_Coupons_WorkShops]
GO
ALTER TABLE [dbo].[CustomerBackOrder]  WITH CHECK ADD  CONSTRAINT [FK_CustomerBackOrder_Distributors] FOREIGN KEY([DistributorId])
REFERENCES [dbo].[Distributors] ([DistributorId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CustomerBackOrder] CHECK CONSTRAINT [FK_CustomerBackOrder_Distributors]
GO
ALTER TABLE [dbo].[CustomerBackOrder]  WITH CHECK ADD  CONSTRAINT [FK_CustomerBackOrder_WorkShops] FOREIGN KEY([WorkshopId])
REFERENCES [dbo].[WorkShops] ([WorkShopId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CustomerBackOrder] CHECK CONSTRAINT [FK_CustomerBackOrder_WorkShops]
GO
ALTER TABLE [dbo].[DailySalesTrackerWithInvoiceData]  WITH CHECK ADD  CONSTRAINT [FK_DailySalesTrackerWithInvoiceData_AspNetUsers] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DailySalesTrackerWithInvoiceData] CHECK CONSTRAINT [FK_DailySalesTrackerWithInvoiceData_AspNetUsers]
GO
ALTER TABLE [dbo].[DailySalesTrackerWithInvoiceData]  WITH CHECK ADD  CONSTRAINT [FK_DailySalesTrackerWithInvoiceData_DailySalesTrackerWithInvoiceData] FOREIGN KEY([DailySalesTrackerId])
REFERENCES [dbo].[DailySalesTrackerWithInvoiceData] ([DailySalesTrackerId])
GO
ALTER TABLE [dbo].[DailySalesTrackerWithInvoiceData] CHECK CONSTRAINT [FK_DailySalesTrackerWithInvoiceData_DailySalesTrackerWithInvoiceData]
GO
ALTER TABLE [dbo].[DailySalesTrackerWithInvoiceData]  WITH CHECK ADD  CONSTRAINT [FK_DailySalesTrackerWithInvoiceData_Distributors] FOREIGN KEY([DistributorId])
REFERENCES [dbo].[Distributors] ([DistributorId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DailySalesTrackerWithInvoiceData] CHECK CONSTRAINT [FK_DailySalesTrackerWithInvoiceData_Distributors]
GO
ALTER TABLE [dbo].[DailySalesTrackerWithInvoiceData]  WITH CHECK ADD  CONSTRAINT [FK_DailySalesTrackerWithInvoiceData_ProductGroup] FOREIGN KEY([GroupId])
REFERENCES [dbo].[ProductGroup] ([GroupId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DailySalesTrackerWithInvoiceData] CHECK CONSTRAINT [FK_DailySalesTrackerWithInvoiceData_ProductGroup]
GO
ALTER TABLE [dbo].[DailySalesTrackerWithInvoiceData]  WITH CHECK ADD  CONSTRAINT [FK_DailySalesTrackerWithInvoiceData_WorkShops] FOREIGN KEY([WorkShopId])
REFERENCES [dbo].[WorkShops] ([WorkShopId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DailySalesTrackerWithInvoiceData] CHECK CONSTRAINT [FK_DailySalesTrackerWithInvoiceData_WorkShops]
GO
ALTER TABLE [dbo].[DailyStock]  WITH CHECK ADD  CONSTRAINT [FK_DailyStock_Distributors] FOREIGN KEY([DistributorId])
REFERENCES [dbo].[Distributors] ([DistributorId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DailyStock] CHECK CONSTRAINT [FK_DailyStock_Distributors]
GO
ALTER TABLE [dbo].[DailyStock]  WITH CHECK ADD  CONSTRAINT [FK_DailyStock_Outlets] FOREIGN KEY([OutletId])
REFERENCES [dbo].[Outlets] ([OutletId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DailyStock] CHECK CONSTRAINT [FK_DailyStock_Outlets]
GO
ALTER TABLE [dbo].[DefaultWinners]  WITH CHECK ADD  CONSTRAINT [FK_DefaultWinners_GiftManagement] FOREIGN KEY([GiftId])
REFERENCES [dbo].[GiftManagement] ([GiftId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DefaultWinners] CHECK CONSTRAINT [FK_DefaultWinners_GiftManagement]
GO
ALTER TABLE [dbo].[DefaultWinners]  WITH CHECK ADD  CONSTRAINT [FK_DefaultWinners_Schemes] FOREIGN KEY([SchemeId])
REFERENCES [dbo].[Schemes] ([SchemeId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DefaultWinners] CHECK CONSTRAINT [FK_DefaultWinners_Schemes]
GO
ALTER TABLE [dbo].[DefaultWinners]  WITH CHECK ADD  CONSTRAINT [FK_DefaultWinners_WorkShops] FOREIGN KEY([WorkshopId])
REFERENCES [dbo].[WorkShops] ([WorkShopId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DefaultWinners] CHECK CONSTRAINT [FK_DefaultWinners_WorkShops]
GO
ALTER TABLE [dbo].[DeliveryAddress]  WITH CHECK ADD  CONSTRAINT [FK_DeliveryAddress_AspNetUsers] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DeliveryAddress] CHECK CONSTRAINT [FK_DeliveryAddress_AspNetUsers]
GO
ALTER TABLE [dbo].[DistributorBrand]  WITH CHECK ADD  CONSTRAINT [FK_Brand_DistributorBrand] FOREIGN KEY([BrandId])
REFERENCES [dbo].[Brand] ([BrandId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DistributorBrand] CHECK CONSTRAINT [FK_Brand_DistributorBrand]
GO
ALTER TABLE [dbo].[DistributorBrand]  WITH CHECK ADD  CONSTRAINT [FK_Distributors_DistributorBrand] FOREIGN KEY([DistributorId])
REFERENCES [dbo].[Distributors] ([DistributorId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DistributorBrand] CHECK CONSTRAINT [FK_Distributors_DistributorBrand]
GO
ALTER TABLE [dbo].[DistributorLocation]  WITH CHECK ADD  CONSTRAINT [FK_DistributorLocation_Distributors] FOREIGN KEY([DistributorId])
REFERENCES [dbo].[Distributors] ([DistributorId])
GO
ALTER TABLE [dbo].[DistributorLocation] CHECK CONSTRAINT [FK_DistributorLocation_Distributors]
GO
ALTER TABLE [dbo].[Distributors]  WITH CHECK ADD  CONSTRAINT [FK_Distributors_Distributors] FOREIGN KEY([DistributorId])
REFERENCES [dbo].[Distributors] ([DistributorId])
GO
ALTER TABLE [dbo].[Distributors] CHECK CONSTRAINT [FK_Distributors_Distributors]
GO
ALTER TABLE [dbo].[DistributorsOutlets]  WITH CHECK ADD  CONSTRAINT [FK_DistributorsOutlets_AspNetUsers1] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DistributorsOutlets] CHECK CONSTRAINT [FK_DistributorsOutlets_AspNetUsers1]
GO
ALTER TABLE [dbo].[DistributorsOutlets]  WITH CHECK ADD  CONSTRAINT [FK_DistributorsOutlets_Distributors] FOREIGN KEY([DistributorId])
REFERENCES [dbo].[Distributors] ([DistributorId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DistributorsOutlets] CHECK CONSTRAINT [FK_DistributorsOutlets_Distributors]
GO
ALTER TABLE [dbo].[DistributorsOutlets]  WITH CHECK ADD  CONSTRAINT [FK_DistributorsOutlets_Outlets] FOREIGN KEY([OutletId])
REFERENCES [dbo].[Outlets] ([OutletId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DistributorsOutlets] CHECK CONSTRAINT [FK_DistributorsOutlets_Outlets]
GO
ALTER TABLE [dbo].[DistributorUser]  WITH CHECK ADD  CONSTRAINT [FK_DistributerUser_DistributorId] FOREIGN KEY([DistributorId])
REFERENCES [dbo].[Distributors] ([DistributorId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DistributorUser] CHECK CONSTRAINT [FK_DistributerUser_DistributorId]
GO
ALTER TABLE [dbo].[DistributorUser]  WITH CHECK ADD  CONSTRAINT [FK_DistributorUser_DistributorUser] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DistributorUser] CHECK CONSTRAINT [FK_DistributorUser_DistributorUser]
GO
ALTER TABLE [dbo].[DistributorUserInfo]  WITH CHECK ADD  CONSTRAINT [FK_DistributorUserInfo_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DistributorUserInfo] CHECK CONSTRAINT [FK_DistributorUserInfo_UserId]
GO
ALTER TABLE [dbo].[DistributorWorkShops]  WITH CHECK ADD  CONSTRAINT [FK_DistributorWorkShops_AspNetUsers] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DistributorWorkShops] CHECK CONSTRAINT [FK_DistributorWorkShops_AspNetUsers]
GO
ALTER TABLE [dbo].[DistributorWorkShops]  WITH CHECK ADD  CONSTRAINT [FK_DistributorWorkShops_Distributors] FOREIGN KEY([DistributorId])
REFERENCES [dbo].[Distributors] ([DistributorId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DistributorWorkShops] CHECK CONSTRAINT [FK_DistributorWorkShops_Distributors]
GO
ALTER TABLE [dbo].[DistributorWorkShops]  WITH CHECK ADD  CONSTRAINT [FK_DistributorWorkShops_WorkShops] FOREIGN KEY([WorkShopId])
REFERENCES [dbo].[WorkShops] ([WorkShopId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DistributorWorkShops] CHECK CONSTRAINT [FK_DistributorWorkShops_WorkShops]
GO
ALTER TABLE [dbo].[FeaturesRole]  WITH CHECK ADD  CONSTRAINT [FK__FeaturesR__Featu__13FCE2E3] FOREIGN KEY([FeatureId])
REFERENCES [dbo].[Features] ([FeatureId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[FeaturesRole] CHECK CONSTRAINT [FK__FeaturesR__Featu__13FCE2E3]
GO
ALTER TABLE [dbo].[FeaturesRole]  WITH CHECK ADD  CONSTRAINT [FK__FeaturesR__RoleI__14F1071C] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[FeaturesRole] CHECK CONSTRAINT [FK__FeaturesR__RoleI__14F1071C]
GO
ALTER TABLE [dbo].[FmsGroupSale]  WITH CHECK ADD  CONSTRAINT [FK__FmsGroupS__Group__2F3AE904] FOREIGN KEY([GroupId])
REFERENCES [dbo].[ProductGroup] ([GroupId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[FmsGroupSale] CHECK CONSTRAINT [FK__FmsGroupS__Group__2F3AE904]
GO
ALTER TABLE [dbo].[FmsGroupSale]  WITH CHECK ADD  CONSTRAINT [FK__FmsGroupS__Schem__2E46C4CB] FOREIGN KEY([SchemeId])
REFERENCES [dbo].[Schemes] ([SchemeId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[FmsGroupSale] CHECK CONSTRAINT [FK__FmsGroupS__Schem__2E46C4CB]
GO
ALTER TABLE [dbo].[FocusPart]  WITH CHECK ADD  CONSTRAINT [FK_FocusPart_ProductGroup] FOREIGN KEY([GroupId])
REFERENCES [dbo].[ProductGroup] ([GroupId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[FocusPart] CHECK CONSTRAINT [FK_FocusPart_ProductGroup]
GO
ALTER TABLE [dbo].[FocusPart]  WITH CHECK ADD  CONSTRAINT [FK_FocusPart_Schemes] FOREIGN KEY([SchemeId])
REFERENCES [dbo].[Schemes] ([SchemeId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[FocusPart] CHECK CONSTRAINT [FK_FocusPart_Schemes]
GO
ALTER TABLE [dbo].[GiftCategory]  WITH CHECK ADD  CONSTRAINT [FK_GiftCategory_CategorySchemes] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[CategorySchemes] ([CategoryId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GiftCategory] CHECK CONSTRAINT [FK_GiftCategory_CategorySchemes]
GO
ALTER TABLE [dbo].[GiftCategory]  WITH CHECK ADD  CONSTRAINT [FK_GiftCategory_GiftManagement] FOREIGN KEY([GiftId])
REFERENCES [dbo].[GiftManagement] ([GiftId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GiftCategory] CHECK CONSTRAINT [FK_GiftCategory_GiftManagement]
GO
ALTER TABLE [dbo].[GiftManagement]  WITH CHECK ADD  CONSTRAINT [FK_GiftManagement_Schemes] FOREIGN KEY([SchemeId])
REFERENCES [dbo].[Schemes] ([SchemeId])
GO
ALTER TABLE [dbo].[GiftManagement] CHECK CONSTRAINT [FK_GiftManagement_Schemes]
GO
ALTER TABLE [dbo].[GiftsCoupon]  WITH CHECK ADD  CONSTRAINT [FK_GiftsCoupon_GiftManagement] FOREIGN KEY([GiftId])
REFERENCES [dbo].[GiftManagement] ([GiftId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GiftsCoupon] CHECK CONSTRAINT [FK_GiftsCoupon_GiftManagement]
GO
ALTER TABLE [dbo].[GiftsCoupon]  WITH CHECK ADD  CONSTRAINT [FK_GiftsCoupon_WorkShops] FOREIGN KEY([WorkshopId])
REFERENCES [dbo].[WorkShops] ([WorkShopId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GiftsCoupon] CHECK CONSTRAINT [FK_GiftsCoupon_WorkShops]
GO
ALTER TABLE [dbo].[GrowthPercentage]  WITH CHECK ADD  CONSTRAINT [FK__GrowthPer__Distr__1FF8A574] FOREIGN KEY([DistributorId])
REFERENCES [dbo].[Distributors] ([DistributorId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GrowthPercentage] CHECK CONSTRAINT [FK__GrowthPer__Distr__1FF8A574]
GO
ALTER TABLE [dbo].[LabelCriteria]  WITH CHECK ADD  CONSTRAINT [FK_LabelCriteria_ProductGroup] FOREIGN KEY([GroupId])
REFERENCES [dbo].[ProductGroup] ([GroupId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[LabelCriteria] CHECK CONSTRAINT [FK_LabelCriteria_ProductGroup]
GO
ALTER TABLE [dbo].[LabelCriteria]  WITH CHECK ADD  CONSTRAINT [FK_LabelCriteria_Schemes] FOREIGN KEY([SchemeId])
REFERENCES [dbo].[Schemes] ([SchemeId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[LabelCriteria] CHECK CONSTRAINT [FK_LabelCriteria_Schemes]
GO
ALTER TABLE [dbo].[LoginTime]  WITH CHECK ADD  CONSTRAINT [FK_LoginTime_AspNetUsers] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[LoginTime] CHECK CONSTRAINT [FK_LoginTime_AspNetUsers]
GO
ALTER TABLE [dbo].[Logs]  WITH CHECK ADD  CONSTRAINT [FK_Logs_Logs] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Logs] CHECK CONSTRAINT [FK_Logs_Logs]
GO
ALTER TABLE [dbo].[MgaCatalougeBanner]  WITH CHECK ADD  CONSTRAINT [FK_MgaCatalougeBanner_Distributors] FOREIGN KEY([DistributorId])
REFERENCES [dbo].[Distributors] ([DistributorId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[MgaCatalougeBanner] CHECK CONSTRAINT [FK_MgaCatalougeBanner_Distributors]
GO
ALTER TABLE [dbo].[MgaCatalougeProduct]  WITH CHECK ADD  CONSTRAINT [FK__MgaCatalo__Banne__450A2E92] FOREIGN KEY([BannerId])
REFERENCES [dbo].[MgaCatalougeBanner] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[MgaCatalougeProduct] CHECK CONSTRAINT [FK__MgaCatalo__Banne__450A2E92]
GO
ALTER TABLE [dbo].[MgaCatalougeProduct]  WITH CHECK ADD  CONSTRAINT [FK__MgaCatalo__Produ__45FE52CB] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Product] ([ProductId])
GO
ALTER TABLE [dbo].[MgaCatalougeProduct] CHECK CONSTRAINT [FK__MgaCatalo__Produ__45FE52CB]
GO
ALTER TABLE [dbo].[Notifications]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUsers_Notifications] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Notifications] CHECK CONSTRAINT [FK_AspNetUsers_Notifications]
GO
ALTER TABLE [dbo].[OrderDetails]  WITH CHECK ADD  CONSTRAINT [FK__OrderDeta__Outle__5AC46587] FOREIGN KEY([OutletId])
REFERENCES [dbo].[Outlets] ([OutletId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[OrderDetails] CHECK CONSTRAINT [FK__OrderDeta__Outle__5AC46587]
GO
ALTER TABLE [dbo].[OrderDetails]  WITH CHECK ADD  CONSTRAINT [FK_OrderDetails_OrderTable] FOREIGN KEY([OrderID])
REFERENCES [dbo].[OrderTable] ([OrderID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[OrderDetails] CHECK CONSTRAINT [FK_OrderDetails_OrderTable]
GO
ALTER TABLE [dbo].[OrderTable]  WITH CHECK ADD  CONSTRAINT [FK__OrderTabl__outle__7F01C5FD] FOREIGN KEY([OutletId])
REFERENCES [dbo].[Outlets] ([OutletId])
GO
ALTER TABLE [dbo].[OrderTable] CHECK CONSTRAINT [FK__OrderTabl__outle__7F01C5FD]
GO
ALTER TABLE [dbo].[OrderTable]  WITH CHECK ADD  CONSTRAINT [FK_OrderTable_AspNetUsers] FOREIGN KEY([UserID])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[OrderTable] CHECK CONSTRAINT [FK_OrderTable_AspNetUsers]
GO
ALTER TABLE [dbo].[OrderTable]  WITH CHECK ADD  CONSTRAINT [FK_OrderTable_Distributors] FOREIGN KEY([DistributorId])
REFERENCES [dbo].[Distributors] ([DistributorId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[OrderTable] CHECK CONSTRAINT [FK_OrderTable_Distributors]
GO
ALTER TABLE [dbo].[OrderTable]  WITH CHECK ADD  CONSTRAINT [FK_OrderTable_WorkShops] FOREIGN KEY([WorkshopID])
REFERENCES [dbo].[WorkShops] ([WorkShopId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[OrderTable] CHECK CONSTRAINT [FK_OrderTable_WorkShops]
GO
ALTER TABLE [dbo].[Outlets]  WITH CHECK ADD  CONSTRAINT [FK_Outlets_DistributorLocation] FOREIGN KEY([LocationId])
REFERENCES [dbo].[DistributorLocation] ([LocationId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Outlets] CHECK CONSTRAINT [FK_Outlets_DistributorLocation]
GO
ALTER TABLE [dbo].[OutletsUsers]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUsers_OutletsUsers] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[OutletsUsers] CHECK CONSTRAINT [FK_AspNetUsers_OutletsUsers]
GO
ALTER TABLE [dbo].[OutletsUsers]  WITH CHECK ADD  CONSTRAINT [FK_Outlets_OutletsUsers] FOREIGN KEY([OutletId])
REFERENCES [dbo].[Outlets] ([OutletId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[OutletsUsers] CHECK CONSTRAINT [FK_Outlets_OutletsUsers]
GO
ALTER TABLE [dbo].[Outstanding]  WITH CHECK ADD  CONSTRAINT [FK__Outstandi__Distr__45C948A1] FOREIGN KEY([DistributorId])
REFERENCES [dbo].[Distributors] ([DistributorId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Outstanding] CHECK CONSTRAINT [FK__Outstandi__Distr__45C948A1]
GO
ALTER TABLE [dbo].[Product]  WITH CHECK ADD  CONSTRAINT [FK_Product_BrandId] FOREIGN KEY([BrandId])
REFERENCES [dbo].[Brand] ([BrandId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Product] CHECK CONSTRAINT [FK_Product_BrandId]
GO
ALTER TABLE [dbo].[Product]  WITH CHECK ADD  CONSTRAINT [FK_Product_Distributors] FOREIGN KEY([DistributorId])
REFERENCES [dbo].[Distributors] ([DistributorId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Product] CHECK CONSTRAINT [FK_Product_Distributors]
GO
ALTER TABLE [dbo].[Product]  WITH CHECK ADD  CONSTRAINT [FK_Product_Product] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Product] ([ProductId])
GO
ALTER TABLE [dbo].[Product] CHECK CONSTRAINT [FK_Product_Product]
GO
ALTER TABLE [dbo].[Product]  WITH CHECK ADD  CONSTRAINT [FK_Product_ProductGroup] FOREIGN KEY([GroupId])
REFERENCES [dbo].[ProductGroup] ([GroupId])
GO
ALTER TABLE [dbo].[Product] CHECK CONSTRAINT [FK_Product_ProductGroup]
GO
ALTER TABLE [dbo].[ProductGroup]  WITH CHECK ADD  CONSTRAINT [FK__ProductGr__Brand__4B4D17CD] FOREIGN KEY([BrandId])
REFERENCES [dbo].[Brand] ([BrandId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ProductGroup] CHECK CONSTRAINT [FK__ProductGr__Brand__4B4D17CD]
GO
ALTER TABLE [dbo].[ProductGroup]  WITH CHECK ADD  CONSTRAINT [FK_Variants_ProductGroup] FOREIGN KEY([VariantId])
REFERENCES [dbo].[Variants] ([VariantId])
GO
ALTER TABLE [dbo].[ProductGroup] CHECK CONSTRAINT [FK_Variants_ProductGroup]
GO
ALTER TABLE [dbo].[QualifyCriteria]  WITH CHECK ADD  CONSTRAINT [FK_QualifyCriteria_Schemes] FOREIGN KEY([SchemeId])
REFERENCES [dbo].[Schemes] ([SchemeId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[QualifyCriteria] CHECK CONSTRAINT [FK_QualifyCriteria_Schemes]
GO
ALTER TABLE [dbo].[RoSalesExecutive]  WITH CHECK ADD  CONSTRAINT [FK_RoSalesExecutive_AspNetUsersRo] FOREIGN KEY([RoUserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[RoSalesExecutive] CHECK CONSTRAINT [FK_RoSalesExecutive_AspNetUsersRo]
GO
ALTER TABLE [dbo].[RoSalesExecutive]  WITH CHECK ADD  CONSTRAINT [FK_RoSalesExecutive_AspNetUsersSe] FOREIGN KEY([SeUserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
GO
ALTER TABLE [dbo].[RoSalesExecutive] CHECK CONSTRAINT [FK_RoSalesExecutive_AspNetUsersSe]
GO
ALTER TABLE [dbo].[SalesExecutiveWorkshop]  WITH CHECK ADD  CONSTRAINT [FK_SalesExecutiveWorkshop_AspNetUsers] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[SalesExecutiveWorkshop] CHECK CONSTRAINT [FK_SalesExecutiveWorkshop_AspNetUsers]
GO
ALTER TABLE [dbo].[SalesExecutiveWorkshop]  WITH CHECK ADD  CONSTRAINT [FK_SalesExecutiveWorkshop_WorkShops] FOREIGN KEY([WorkshopId])
REFERENCES [dbo].[WorkShops] ([WorkShopId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[SalesExecutiveWorkshop] CHECK CONSTRAINT [FK_SalesExecutiveWorkshop_WorkShops]
GO
ALTER TABLE [dbo].[SchemeLocations]  WITH CHECK ADD  CONSTRAINT [FK_Table1_DistributorLocation] FOREIGN KEY([LocationId])
REFERENCES [dbo].[DistributorLocation] ([LocationId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[SchemeLocations] CHECK CONSTRAINT [FK_Table1_DistributorLocation]
GO
ALTER TABLE [dbo].[SchemeLocations]  WITH CHECK ADD  CONSTRAINT [FK_Table1_Schemes] FOREIGN KEY([SchemeId])
REFERENCES [dbo].[Schemes] ([SchemeId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[SchemeLocations] CHECK CONSTRAINT [FK_Table1_Schemes]
GO
ALTER TABLE [dbo].[Schemes]  WITH CHECK ADD  CONSTRAINT [FK_Schemes_Distributors] FOREIGN KEY([DistributorId])
REFERENCES [dbo].[Distributors] ([DistributorId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Schemes] CHECK CONSTRAINT [FK_Schemes_Distributors]
GO
ALTER TABLE [dbo].[SupportQuery]  WITH CHECK ADD  CONSTRAINT [FK_SupportQuery_AspNetUsers] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[SupportQuery] CHECK CONSTRAINT [FK_SupportQuery_AspNetUsers]
GO
ALTER TABLE [dbo].[TargetGrowth]  WITH CHECK ADD  CONSTRAINT [FK_TargetGrowth_Schemes] FOREIGN KEY([SchemeId])
REFERENCES [dbo].[Schemes] ([SchemeId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TargetGrowth] CHECK CONSTRAINT [FK_TargetGrowth_Schemes]
GO
ALTER TABLE [dbo].[TargetWorkShop]  WITH CHECK ADD  CONSTRAINT [FK_TargetWorkShop_Schemes] FOREIGN KEY([SchemeId])
REFERENCES [dbo].[Schemes] ([SchemeId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TargetWorkShop] CHECK CONSTRAINT [FK_TargetWorkShop_Schemes]
GO
ALTER TABLE [dbo].[TargetWorkShop]  WITH CHECK ADD  CONSTRAINT [FK_TargetWorkShop_WorkShops] FOREIGN KEY([WorkShopId])
REFERENCES [dbo].[WorkShops] ([WorkShopId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TargetWorkShop] CHECK CONSTRAINT [FK_TargetWorkShop_WorkShops]
GO
ALTER TABLE [dbo].[TempOrder]  WITH CHECK ADD  CONSTRAINT [FK_TempOrder_AspNetUsers] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
GO
ALTER TABLE [dbo].[TempOrder] CHECK CONSTRAINT [FK_TempOrder_AspNetUsers]
GO
ALTER TABLE [dbo].[TempOrder]  WITH CHECK ADD  CONSTRAINT [FK_TempOrder_DeliveryAddress] FOREIGN KEY([DeliveryAddressId])
REFERENCES [dbo].[DeliveryAddress] ([DeliveryAddressId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TempOrder] CHECK CONSTRAINT [FK_TempOrder_DeliveryAddress]
GO
ALTER TABLE [dbo].[TempOrder]  WITH CHECK ADD  CONSTRAINT [FK_TempOrder_Distributors] FOREIGN KEY([DistributorId])
REFERENCES [dbo].[Distributors] ([DistributorId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TempOrder] CHECK CONSTRAINT [FK_TempOrder_Distributors]
GO
ALTER TABLE [dbo].[TempOrder]  WITH CHECK ADD  CONSTRAINT [FK_TempOrder_WorkShops] FOREIGN KEY([WorkShopId])
REFERENCES [dbo].[WorkShops] ([WorkShopId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TempOrder] CHECK CONSTRAINT [FK_TempOrder_WorkShops]
GO
ALTER TABLE [dbo].[TempOrderDetails]  WITH CHECK ADD  CONSTRAINT [FK__TempOrder__Outle__5BB889C0] FOREIGN KEY([OutletId])
REFERENCES [dbo].[Outlets] ([OutletId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TempOrderDetails] CHECK CONSTRAINT [FK__TempOrder__Outle__5BB889C0]
GO
ALTER TABLE [dbo].[TempOrderDetails]  WITH CHECK ADD  CONSTRAINT [FK_TempOrderDetails_Product] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Product] ([ProductId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TempOrderDetails] CHECK CONSTRAINT [FK_TempOrderDetails_Product]
GO
ALTER TABLE [dbo].[TempOrderDetails]  WITH CHECK ADD  CONSTRAINT [FK_TempOrderDetails_TempOrder] FOREIGN KEY([TempOrderId])
REFERENCES [dbo].[TempOrder] ([TempOrderId])
GO
ALTER TABLE [dbo].[TempOrderDetails] CHECK CONSTRAINT [FK_TempOrderDetails_TempOrder]
GO
ALTER TABLE [dbo].[TicketOfJoy]  WITH CHECK ADD  CONSTRAINT [FK_TicketOfJoy_ProductGroup] FOREIGN KEY([GroupId])
REFERENCES [dbo].[ProductGroup] ([GroupId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TicketOfJoy] CHECK CONSTRAINT [FK_TicketOfJoy_ProductGroup]
GO
ALTER TABLE [dbo].[TicketOfJoy]  WITH CHECK ADD  CONSTRAINT [FK_TicketOfJoy_Schemes] FOREIGN KEY([SchemeId])
REFERENCES [dbo].[Schemes] ([SchemeId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TicketOfJoy] CHECK CONSTRAINT [FK_TicketOfJoy_Schemes]
GO
ALTER TABLE [dbo].[UserDetails]  WITH CHECK ADD  CONSTRAINT [FK_UserDetails_AspNetUsers] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[UserDetails] CHECK CONSTRAINT [FK_UserDetails_AspNetUsers]
GO
ALTER TABLE [dbo].[UserDetails]  WITH CHECK ADD  CONSTRAINT [FK_UserDetails_AspNetUsers1] FOREIGN KEY([DistributorId])
REFERENCES [dbo].[AspNetUsers] ([Id])
GO
ALTER TABLE [dbo].[UserDetails] CHECK CONSTRAINT [FK_UserDetails_AspNetUsers1]
GO
ALTER TABLE [dbo].[UserFeatures]  WITH CHECK ADD  CONSTRAINT [FK_UserFeatures_AspNetUsers] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[UserFeatures] CHECK CONSTRAINT [FK_UserFeatures_AspNetUsers]
GO
ALTER TABLE [dbo].[UserFeatures]  WITH CHECK ADD  CONSTRAINT [FK_UserFeatures_UserFeatures] FOREIGN KEY([FeatureId])
REFERENCES [dbo].[Features] ([FeatureId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[UserFeatures] CHECK CONSTRAINT [FK_UserFeatures_UserFeatures]
GO
ALTER TABLE [dbo].[UserWorkshop]  WITH CHECK ADD  CONSTRAINT [FK_UserWorkshop_AspNetUsers] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[UserWorkshop] CHECK CONSTRAINT [FK_UserWorkshop_AspNetUsers]
GO
ALTER TABLE [dbo].[UserWorkshop]  WITH CHECK ADD  CONSTRAINT [FK_UserWorkshop_Distributors] FOREIGN KEY([DistributorId])
REFERENCES [dbo].[Distributors] ([DistributorId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[UserWorkshop] CHECK CONSTRAINT [FK_UserWorkshop_Distributors]
GO
ALTER TABLE [dbo].[UserWorkshop]  WITH CHECK ADD  CONSTRAINT [FK_UserWorkshop_Workshops] FOREIGN KEY([WorkshopId])
REFERENCES [dbo].[WorkShops] ([WorkShopId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[UserWorkshop] CHECK CONSTRAINT [FK_UserWorkshop_Workshops]
GO
ALTER TABLE [dbo].[Variants]  WITH CHECK ADD  CONSTRAINT [FK_Vehicle_Variants] FOREIGN KEY([VehicleId])
REFERENCES [dbo].[Vehicle] ([VehicleId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Variants] CHECK CONSTRAINT [FK_Vehicle_Variants]
GO
ALTER TABLE [dbo].[Vehicle]  WITH CHECK ADD  CONSTRAINT [FK_Brand_Vehicle] FOREIGN KEY([BrandId])
REFERENCES [dbo].[Brand] ([BrandId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Vehicle] CHECK CONSTRAINT [FK_Brand_Vehicle]
GO
ALTER TABLE [dbo].[WalletTransaction]  WITH CHECK ADD  CONSTRAINT [FK_WalletTransaction_AspNetUsers] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[WalletTransaction] CHECK CONSTRAINT [FK_WalletTransaction_AspNetUsers]
GO
ALTER TABLE [dbo].[WalletTransaction]  WITH CHECK ADD  CONSTRAINT [FK_WalletTransaction_WorkShops] FOREIGN KEY([WorkshopId])
REFERENCES [dbo].[WorkShops] ([WorkShopId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[WalletTransaction] CHECK CONSTRAINT [FK_WalletTransaction_WorkShops]
GO
ALTER TABLE [dbo].[WorkshopBrand]  WITH CHECK ADD  CONSTRAINT [FK_WorkshopBrand_Brand] FOREIGN KEY([BrandId])
REFERENCES [dbo].[Brand] ([BrandId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[WorkshopBrand] CHECK CONSTRAINT [FK_WorkshopBrand_Brand]
GO
ALTER TABLE [dbo].[WorkshopBrand]  WITH CHECK ADD  CONSTRAINT [FK_WorkshopBrand_Workshops] FOREIGN KEY([WorkshopId])
REFERENCES [dbo].[WorkShops] ([WorkShopId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[WorkshopBrand] CHECK CONSTRAINT [FK_WorkshopBrand_Workshops]
GO
ALTER TABLE [dbo].[WorkshopCoupon]  WITH CHECK ADD  CONSTRAINT [FK_WorkshopCoupon_AspNetUsers] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[WorkshopCoupon] CHECK CONSTRAINT [FK_WorkshopCoupon_AspNetUsers]
GO
ALTER TABLE [dbo].[WorkshopCoupon]  WITH CHECK ADD  CONSTRAINT [FK_WorkshopCoupon_WorkShops] FOREIGN KEY([WorkshopId])
REFERENCES [dbo].[WorkShops] ([WorkShopId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[WorkshopCoupon] CHECK CONSTRAINT [FK_WorkshopCoupon_WorkShops]
GO
ALTER TABLE [dbo].[WorkShopLabelSchemes]  WITH CHECK ADD  CONSTRAINT [FK_WorkShopLabelSchemes_Schemes] FOREIGN KEY([CriteriaId])
REFERENCES [dbo].[LabelCriteria] ([CriteriaId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[WorkShopLabelSchemes] CHECK CONSTRAINT [FK_WorkShopLabelSchemes_Schemes]
GO
ALTER TABLE [dbo].[WorkShopLabelSchemes]  WITH CHECK ADD  CONSTRAINT [FK_WorkShopLabelSchemes_WorkShops] FOREIGN KEY([WorkShopId])
REFERENCES [dbo].[WorkShops] ([WorkShopId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[WorkShopLabelSchemes] CHECK CONSTRAINT [FK_WorkShopLabelSchemes_WorkShops]
GO
ALTER TABLE [dbo].[WorkShops]  WITH CHECK ADD  CONSTRAINT [FK_workshop_OutletId] FOREIGN KEY([outletId])
REFERENCES [dbo].[Outlets] ([OutletId])
GO
ALTER TABLE [dbo].[WorkShops] CHECK CONSTRAINT [FK_workshop_OutletId]
GO
ALTER TABLE [dbo].[WorkshopsUsers]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUsers_WorkshopsUsers] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[WorkshopsUsers] CHECK CONSTRAINT [FK_AspNetUsers_WorkshopsUsers]
GO
ALTER TABLE [dbo].[WorkshopsUsers]  WITH CHECK ADD  CONSTRAINT [FK_Workshops_WorkshopsUsers] FOREIGN KEY([WorkshopId])
REFERENCES [dbo].[WorkShops] ([WorkShopId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[WorkshopsUsers] CHECK CONSTRAINT [FK_Workshops_WorkshopsUsers]
GO
