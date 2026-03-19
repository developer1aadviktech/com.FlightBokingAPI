CREATE TABLE [dbo].[Flight_baggage] (
    [bookingid]      NVARCHAR (50)  NULL,
    [departure_iata] NVARCHAR (50)  NULL,
    [arrival_iata]   NVARCHAR (50)  NULL,
    [paxtype]        NVARCHAR (50)  NULL,
    [id]             INT            IDENTITY (1, 1) NOT NULL,
    [baggage_type]   NVARCHAR (300) NULL,
    [value]          NVARCHAR (600) NULL
);

