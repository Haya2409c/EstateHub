-- =====================================================
-- Admin Panel Database Schema
-- RealtorsPortal – SQL Server
-- Generated: 2026-06-17
-- Tables: SiteSettings, Packages, Payments, EmailLogs, SubscriptionHistories
-- =====================================================

-- -----------------------------------------------
-- 1. SiteSettings
--    Key-value store for all admin-configurable
--    settings (currency, ad expiry, PayPal keys…)
-- -----------------------------------------------
CREATE TABLE [dbo].[SiteSettings] (
    [Id]        INT            NOT NULL IDENTITY(1,1),
    [Key]       NVARCHAR(100)  NOT NULL,
    [Value]     NVARCHAR(2000) NULL,
    [Group]     NVARCHAR(50)   NULL,            -- 'General' | 'Listings' | 'Payment'
    [UpdatedAt] DATETIME2      NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_SiteSettings] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_SiteSettings_Key] UNIQUE ([Key])
);

-- Seed default settings
INSERT INTO [dbo].[SiteSettings] ([Key], [Value], [Group]) VALUES
    ('currency',      'USD',         'General'),
    ('page_size',     '20',          'General'),
    ('ad_expiry',     '30',          'Listings'),
    ('auto_approve',  '1',           'Listings'),
    ('paypal_client', '',            'Payment'),
    ('paypal_secret', '',            'Payment'),
    ('paypal_mode',   'sandbox',     'Payment');
GO

-- -----------------------------------------------
-- 2. Packages
--    Subscription plans visible on packages.html
-- -----------------------------------------------
CREATE TABLE [dbo].[Packages] (
    [Id]           INT             NOT NULL IDENTITY(1,1),
    [Name]         NVARCHAR(100)   NOT NULL,
    [Description]  NVARCHAR(1000)  NULL,
    [Price]        DECIMAL(18,2)   NOT NULL DEFAULT 0.00,
    [DurationDays] INT             NOT NULL DEFAULT 30,
    [ListingLimit] INT             NULL,        -- NULL = unlimited
    [ImageLimit]   INT             NULL,
    [IsActive]     BIT             NOT NULL DEFAULT 1,
    [IsFeatured]   BIT             NOT NULL DEFAULT 0,
    [SortOrder]    INT             NOT NULL DEFAULT 0,
    [CreatedAt]    DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt]    DATETIME2       NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_Packages] PRIMARY KEY ([Id])
);

-- Seed default packages (matches packages.html mock data)
INSERT INTO [dbo].[Packages] ([Name], [Description], [Price], [DurationDays], [ListingLimit], [ImageLimit], [SortOrder]) VALUES
    ('Basic',    'Standard package for private sellers',          0.00,  30, 5,    5,  1),
    ('Gold',     'Featured listings and priority support',       49.00,  30, 50,   15, 2),
    ('Platinum', 'Unlimited listings, 24/7 dedicated support',  99.00,  30, NULL, 30, 3);
GO

-- -----------------------------------------------
-- 3. Payments
--    Transaction records from PayPal gateway
-- -----------------------------------------------
CREATE TABLE [dbo].[Payments] (
    [Id]              INT              NOT NULL IDENTITY(1,1),
    [TransactionId]   NVARCHAR(200)    NOT NULL,    -- PayPal order/capture ID
    [UserId]          NVARCHAR(450)    NOT NULL,
    [PackageId]       INT              NOT NULL,
    [Amount]          DECIMAL(18,2)    NOT NULL,
    [Currency]        NVARCHAR(10)     NOT NULL DEFAULT 'USD',
    [Status]          NVARCHAR(20)     NOT NULL DEFAULT 'Pending',
                                                    -- 'Pending' | 'Completed' | 'Failed' | 'Refunded'
    [GatewayResponse] NVARCHAR(MAX)    NULL,         -- raw JSON from PayPal
    [CreatedAt]       DATETIME2        NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_Payments_TransactionId] UNIQUE ([TransactionId]),
    CONSTRAINT [FK_Payments_Users]     FOREIGN KEY ([UserId])    REFERENCES [dbo].[AspNetUsers]([Id]),
    CONSTRAINT [FK_Payments_Packages]  FOREIGN KEY ([PackageId]) REFERENCES [dbo].[Packages]([Id])
);
CREATE INDEX [IX_Payments_UserId]    ON [dbo].[Payments]([UserId]);
CREATE INDEX [IX_Payments_PackageId] ON [dbo].[Payments]([PackageId]);
CREATE INDEX [IX_Payments_Status]    ON [dbo].[Payments]([Status]);
GO

-- -----------------------------------------------
-- 4. EmailLogs
--    Audit trail for all outbound emails
-- -----------------------------------------------
CREATE TABLE [dbo].[EmailLogs] (
    [Id]                INT             NOT NULL IDENTITY(1,1),
    [RecipientUserId]   NVARCHAR(450)   NULL,
    [ToEmail]           NVARCHAR(200)   NOT NULL,
    [Subject]           NVARCHAR(300)   NOT NULL,
    [Body]              NVARCHAR(MAX)   NULL,
    [EmailType]         NVARCHAR(50)    NOT NULL,
                                        -- 'WelcomeEmail' | 'PaymentConfirmation' | 'SubscriptionExpiry' | 'PasswordReset'
    [Status]            NVARCHAR(20)    NOT NULL DEFAULT 'Pending',
                                        -- 'Pending' | 'Sent' | 'Failed'
    [ErrorMessage]      NVARCHAR(1000)  NULL,
    [RetryCount]        INT             NOT NULL DEFAULT 0,
    [SentAt]            DATETIME2       NULL,
    [CreatedAt]         DATETIME2       NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_EmailLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EmailLogs_Users] FOREIGN KEY ([RecipientUserId])
        REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE SET NULL
);
CREATE INDEX [IX_EmailLogs_Status]    ON [dbo].[EmailLogs]([Status]);
CREATE INDEX [IX_EmailLogs_EmailType] ON [dbo].[EmailLogs]([EmailType]);
GO

-- -----------------------------------------------
-- 5. SubscriptionHistories
--    Active/expired plan history per user
-- -----------------------------------------------
CREATE TABLE [dbo].[SubscriptionHistories] (
    [Id]         INT             NOT NULL IDENTITY(1,1),
    [UserId]     NVARCHAR(450)   NOT NULL,
    [PackageId]  INT             NOT NULL,
    [PaymentId]  INT             NULL,
    [StartDate]  DATETIME2       NOT NULL,
    [EndDate]    DATETIME2       NOT NULL,
    [Status]     NVARCHAR(20)    NOT NULL DEFAULT 'Active',
                                 -- 'Active' | 'Expired' | 'Cancelled'
    [Notes]      NVARCHAR(1000)  NULL,
    [CreatedAt]  DATETIME2       NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_SubscriptionHistories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SubscriptionHistories_Users]    FOREIGN KEY ([UserId])    REFERENCES [dbo].[AspNetUsers]([Id]),
    CONSTRAINT [FK_SubscriptionHistories_Packages] FOREIGN KEY ([PackageId]) REFERENCES [dbo].[Packages]([Id]),
    CONSTRAINT [FK_SubscriptionHistories_Payments] FOREIGN KEY ([PaymentId]) REFERENCES [dbo].[Payments]([Id]) ON DELETE SET NULL,
    CONSTRAINT [UQ_SubscriptionHistories_PaymentId] UNIQUE ([PaymentId])     -- 1-to-1 with Payment
);
CREATE INDEX [IX_SubscriptionHistories_UserId]    ON [dbo].[SubscriptionHistories]([UserId]);
CREATE INDEX [IX_SubscriptionHistories_Status]    ON [dbo].[SubscriptionHistories]([Status]);
CREATE INDEX [IX_SubscriptionHistories_EndDate]   ON [dbo].[SubscriptionHistories]([EndDate]);
GO
