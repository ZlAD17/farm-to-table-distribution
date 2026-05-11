-- =============================================================
-- Farm-to-Table Distribution — Seed / Sample Data
-- Run AFTER schema.sql
-- =============================================================

-- Farms
INSERT INTO Farm (Name, Location, ContactEmail, ContactPhone) VALUES
    ('Green Valley Farm',    'Springfield, IL',   'contact@greenvalley.com',    '555-1001'),
    ('Sunrise Organics',     'Portland, OR',      'hello@sunriseorganics.com',  '555-1002'),
    ('Meadow Creek Ranch',   'Austin, TX',        'info@meadowcreek.com',       '555-1003');

-- Crops
INSERT INTO Crop (Name, Unit) VALUES
    ('Tomatoes',    'kg'),
    ('Lettuce',     'kg'),
    ('Carrots',     'kg'),
    ('Strawberries','kg'),
    ('Corn',        'dozen');

-- HarvestBatches
INSERT INTO HarvestBatch (FarmId, CropId, QuantityAvailable, QuantityRemaining, HarvestDate, PricePerUnit, Status) VALUES
    (1, 1, 500.00, 500.00, '2026-05-01', 2.50, 'Available'),  -- BatchId 1
    (1, 2, 300.00, 300.00, '2026-05-03', 1.80, 'Available'),  -- BatchId 2
    (2, 3, 400.00, 400.00, '2026-05-02', 1.20, 'Available'),  -- BatchId 3
    (2, 4, 200.00, 200.00, '2026-05-04', 4.00, 'Available'),  -- BatchId 4
    (3, 5, 600.00, 600.00, '2026-05-01', 3.00, 'Available');  -- BatchId 5

-- Restaurants
INSERT INTO Restaurant (Name, Address, ContactEmail, ContactPhone) VALUES
    ('The Garden Table',  '123 Elm St, Chicago, IL',   'orders@gardentable.com',  '555-2001'),
    ('Farm Bistro',       '456 Oak Ave, Portland, OR', 'info@farmbistro.com',     '555-2002'),
    ('Root & Branch',     '789 Pine Rd, Austin, TX',   'hello@rootandbranch.com', '555-2003');

-- Drivers
INSERT INTO Driver (FullName, Phone, LicensePlate) VALUES
    ('Alice Johnson', '555-3001', 'IL-ABC-123'),
    ('Bob Martinez',  '555-3002', 'OR-XYZ-789'),
    ('Carol Smith',   '555-3003', 'TX-DEF-456');

-- PurchaseOrder (RestaurantId=1, DriverId=1)
INSERT INTO PurchaseOrder (RestaurantId, DriverId, Status, Notes) VALUES
    (1, 1, 'Confirmed', 'First test order');

DECLARE @OrderId INT = SCOPE_IDENTITY();

-- OrderBatch entries for that order (2 batches)
INSERT INTO OrderBatch (OrderId, BatchId, QuantityOrdered, UnitPrice) VALUES
    (@OrderId, 1, 50.00, 2.50),   -- 50 kg Tomatoes from Green Valley
    (@OrderId, 2, 30.00, 1.80);   -- 30 kg Lettuce from Green Valley

-- Decrement QuantityRemaining for used batches
UPDATE HarvestBatch SET QuantityRemaining = QuantityRemaining - 50.00 WHERE BatchId = 1;
UPDATE HarvestBatch SET QuantityRemaining = QuantityRemaining - 30.00 WHERE BatchId = 2;

-- DeliveryTrip for the order
INSERT INTO DeliveryTrip (DriverId, OrderId, ScheduledAt, Notes) VALUES
    (1, @OrderId, '2026-05-12 09:00:00', 'Morning delivery run');
