-- =============================================================
-- Farm-to-Table Distribution — SQL Server Schema
-- Run this script against an empty database before starting the API.
-- =============================================================

-- ---------------------------------------------------------------
-- Farm
-- ---------------------------------------------------------------
CREATE TABLE Farm (
    FarmId        INT            NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Name          NVARCHAR(150)  NOT NULL,
    Location      NVARCHAR(250)  NOT NULL,
    ContactEmail  NVARCHAR(150)  NULL,
    ContactPhone  NVARCHAR(30)   NULL,
    CreatedAt     DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME()
);

-- ---------------------------------------------------------------
-- Crop
-- ---------------------------------------------------------------
CREATE TABLE Crop (
    CropId  INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Name    NVARCHAR(100) NOT NULL,
    Unit    NVARCHAR(30)  NOT NULL   -- kg, lbs, dozen, bushel, etc.
);

-- ---------------------------------------------------------------
-- HarvestBatch
-- ---------------------------------------------------------------
CREATE TABLE HarvestBatch (
    BatchId            INT             NOT NULL IDENTITY(1,1) PRIMARY KEY,
    FarmId             INT             NOT NULL,
    CropId             INT             NOT NULL,
    QuantityAvailable  DECIMAL(10,2)   NOT NULL,
    QuantityRemaining  DECIMAL(10,2)   NOT NULL,
    HarvestDate        DATE            NOT NULL,
    PricePerUnit       DECIMAL(10,2)   NOT NULL,
    Status             NVARCHAR(20)    NOT NULL DEFAULT 'Available',
    CreatedAt          DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_HarvestBatch_Farm FOREIGN KEY (FarmId) REFERENCES Farm(FarmId),
    CONSTRAINT FK_HarvestBatch_Crop FOREIGN KEY (CropId) REFERENCES Crop(CropId),
    CONSTRAINT CHK_HarvestBatch_Status CHECK (Status IN ('Available', 'Sold', 'Expired')),
    CONSTRAINT CHK_HarvestBatch_Qty CHECK (QuantityRemaining >= 0 AND QuantityRemaining <= QuantityAvailable)
);

CREATE INDEX IX_HarvestBatch_FarmId    ON HarvestBatch(FarmId);
CREATE INDEX IX_HarvestBatch_CropId    ON HarvestBatch(CropId);
CREATE INDEX IX_HarvestBatch_HarvestDate ON HarvestBatch(HarvestDate);

-- ---------------------------------------------------------------
-- Restaurant
-- ---------------------------------------------------------------
CREATE TABLE Restaurant (
    RestaurantId  INT            NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Name          NVARCHAR(150)  NOT NULL,
    Address       NVARCHAR(300)  NOT NULL,
    ContactEmail  NVARCHAR(150)  NULL,
    ContactPhone  NVARCHAR(30)   NULL,
    CreatedAt     DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME()
);

-- ---------------------------------------------------------------
-- Driver
-- ---------------------------------------------------------------
CREATE TABLE Driver (
    DriverId      INT            NOT NULL IDENTITY(1,1) PRIMARY KEY,
    FullName      NVARCHAR(150)  NOT NULL,
    Phone         NVARCHAR(30)   NULL,
    LicensePlate  NVARCHAR(20)   NULL,
    CreatedAt     DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME()
);

-- ---------------------------------------------------------------
-- PurchaseOrder
-- ---------------------------------------------------------------
CREATE TABLE PurchaseOrder (
    OrderId       INT            NOT NULL IDENTITY(1,1) PRIMARY KEY,
    RestaurantId  INT            NOT NULL,
    DriverId      INT            NULL,
    Status        NVARCHAR(20)   NOT NULL DEFAULT 'Pending',
    OrderedAt     DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
    DeliveredAt   DATETIME2      NULL,
    Notes         NVARCHAR(500)  NULL,

    CONSTRAINT FK_PurchaseOrder_Restaurant FOREIGN KEY (RestaurantId) REFERENCES Restaurant(RestaurantId),
    CONSTRAINT FK_PurchaseOrder_Driver     FOREIGN KEY (DriverId)     REFERENCES Driver(DriverId),
    CONSTRAINT CHK_PurchaseOrder_Status    CHECK (Status IN ('Pending','Confirmed','Delivered','Cancelled'))
);

CREATE INDEX IX_PurchaseOrder_RestaurantId ON PurchaseOrder(RestaurantId);
CREATE INDEX IX_PurchaseOrder_DriverId     ON PurchaseOrder(DriverId);
CREATE INDEX IX_PurchaseOrder_OrderedAt    ON PurchaseOrder(OrderedAt);

-- ---------------------------------------------------------------
-- OrderBatch  (M:N bridge between PurchaseOrder and HarvestBatch)
-- ---------------------------------------------------------------
CREATE TABLE OrderBatch (
    OrderBatchId    INT            NOT NULL IDENTITY(1,1) PRIMARY KEY,
    OrderId         INT            NOT NULL,
    BatchId         INT            NOT NULL,
    QuantityOrdered DECIMAL(10,2)  NOT NULL,
    UnitPrice       DECIMAL(10,2)  NOT NULL,  -- snapshot of price at time of order

    CONSTRAINT FK_OrderBatch_Order FOREIGN KEY (OrderId) REFERENCES PurchaseOrder(OrderId),
    CONSTRAINT FK_OrderBatch_Batch FOREIGN KEY (BatchId) REFERENCES HarvestBatch(BatchId),
    CONSTRAINT UQ_OrderBatch UNIQUE (OrderId, BatchId),
    CONSTRAINT CHK_OrderBatch_Qty CHECK (QuantityOrdered > 0)
);

CREATE INDEX IX_OrderBatch_OrderId ON OrderBatch(OrderId);
CREATE INDEX IX_OrderBatch_BatchId ON OrderBatch(BatchId);

-- ---------------------------------------------------------------
-- DeliveryTrip
-- ---------------------------------------------------------------
CREATE TABLE DeliveryTrip (
    TripId        INT            NOT NULL IDENTITY(1,1) PRIMARY KEY,
    DriverId      INT            NOT NULL,
    OrderId       INT            NOT NULL,
    ScheduledAt   DATETIME2      NOT NULL,
    CompletedAt   DATETIME2      NULL,
    Notes         NVARCHAR(500)  NULL,

    CONSTRAINT FK_DeliveryTrip_Driver FOREIGN KEY (DriverId) REFERENCES Driver(DriverId),
    CONSTRAINT FK_DeliveryTrip_Order  FOREIGN KEY (OrderId)  REFERENCES PurchaseOrder(OrderId)
);

CREATE INDEX IX_DeliveryTrip_DriverId   ON DeliveryTrip(DriverId);
CREATE INDEX IX_DeliveryTrip_ScheduledAt ON DeliveryTrip(ScheduledAt);
