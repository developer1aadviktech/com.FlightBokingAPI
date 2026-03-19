CREATE TABLE [dbo].[Temp_price_breakdown] (
    [bookingid]    NVARCHAR (50)   NULL,
    [paxtype]      NVARCHAR (50)   NULL,
    [api_basefare] DECIMAL (18, 2) NULL,
    [api_tax]      DECIMAL (18, 2) NULL,
    [api_total]    DECIMAL (18, 2) NULL,
    [basefare]     DECIMAL (18, 2) NULL,
    [tax]          DECIMAL (18, 2) NULL,
    [total]        DECIMAL (18, 2) NULL,
    [api_currency] NVARCHAR (50)   NULL,
    [currency]     NVARCHAR (50)   NULL,
    [quantity]     INT             NULL,
    [id]           INT             IDENTITY (1, 1) NOT NULL
);

