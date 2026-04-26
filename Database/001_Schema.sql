USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'ECommerceDb')
    CREATE DATABASE ECommerceDb;
GO

USE ECommerceDb;
GO

CREATE TABLE Users (
    Id            INT            IDENTITY(1,1) PRIMARY KEY,
    Username      NVARCHAR(50)   NOT NULL,
    Email         NVARCHAR(100)  NOT NULL,
    PasswordHash  NVARCHAR(512)  NOT NULL,
    PasswordSalt  NVARCHAR(512)  NOT NULL,
    Role          NVARCHAR(20)   NOT NULL DEFAULT 'User',
    CreatedAt     DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT UQ_Users_Username UNIQUE (Username),
    CONSTRAINT UQ_Users_Email    UNIQUE (Email)
);

CREATE TABLE RefreshTokens (
    Id         INT           IDENTITY(1,1) PRIMARY KEY,
    UserId     INT           NOT NULL,
    Token      NVARCHAR(512) NOT NULL,
    ExpiresAt  DATETIME2     NOT NULL,
    IsRevoked  BIT           NOT NULL DEFAULT 0,
    CreatedAt  DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_RefreshTokens_Token UNIQUE (Token)
);

CREATE TABLE Categories (
    Id          INT           IDENTITY(1,1) PRIMARY KEY,
    Name        NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    CONSTRAINT UQ_Categories_Name UNIQUE (Name)
);

CREATE TABLE Products (
    Id            INT            IDENTITY(1,1) PRIMARY KEY,
    Name          NVARCHAR(200)  NOT NULL,
    Description   NVARCHAR(2000),
    Price         DECIMAL(18,2)  NOT NULL,
    StockQuantity INT            NOT NULL DEFAULT 0,
    CategoryId    INT            NOT NULL,
    ImageUrl      NVARCHAR(500),
    IsActive      BIT            NOT NULL DEFAULT 1,
    CreatedAt     DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(Id),
    CONSTRAINT CK_Products_Price      CHECK (Price >= 0),
    CONSTRAINT CK_Products_Stock      CHECK (StockQuantity >= 0)
);

CREATE TABLE Carts (
    Id        INT       IDENTITY(1,1) PRIMARY KEY,
    UserId    INT       NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Carts_Users   FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_Carts_UserId  UNIQUE (UserId)
);

CREATE TABLE CartItems (
    Id        INT IDENTITY(1,1) PRIMARY KEY,
    CartId    INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity  INT NOT NULL DEFAULT 1,
    CONSTRAINT FK_CartItems_Carts      FOREIGN KEY (CartId)    REFERENCES Carts(Id)    ON DELETE CASCADE,
    CONSTRAINT FK_CartItems_Products   FOREIGN KEY (ProductId) REFERENCES Products(Id),
    CONSTRAINT UQ_CartItems_CartProduct UNIQUE (CartId, ProductId),
    CONSTRAINT CK_CartItems_Quantity   CHECK (Quantity > 0)
);

CREATE TABLE Orders (
    Id              INT            IDENTITY(1,1) PRIMARY KEY,
    UserId          INT            NOT NULL,
    TotalAmount     DECIMAL(18,2)  NOT NULL,
    Status          NVARCHAR(50)   NOT NULL DEFAULT 'Pending',
    ShippingAddress NVARCHAR(500)  NOT NULL,
    CreatedAt       DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Orders_Users  FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT CK_Orders_Status CHECK (Status IN ('Pending','Processing','Shipped','Delivered','Cancelled'))
);

CREATE TABLE OrderItems (
    Id          INT           IDENTITY(1,1) PRIMARY KEY,
    OrderId     INT           NOT NULL,
    ProductId   INT           NOT NULL,
    ProductName NVARCHAR(200) NOT NULL,
    Price       DECIMAL(18,2) NOT NULL,
    Quantity    INT           NOT NULL,
    CONSTRAINT FK_OrderItems_Orders   FOREIGN KEY (OrderId)   REFERENCES Orders(Id)   ON DELETE CASCADE,
    CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id)
);
GO
