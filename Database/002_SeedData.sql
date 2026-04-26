USE ECommerceDb;
GO

INSERT INTO Categories (Name, Description) VALUES
('Electronics',   'Electronic devices and accessories'),
('Clothing',      'Apparel and fashion items'),
('Books',         'Books and educational materials'),
('Home & Garden', 'Home improvement and garden supplies'),
('Sports',        'Sports and outdoor equipment');

INSERT INTO Products (Name, Description, Price, StockQuantity, CategoryId, ImageUrl) VALUES
('Wireless Headphones',    'Premium noise-cancelling wireless headphones',   89.99,  50,  1, 'https://placehold.co/300x300?text=Headphones'),
('Laptop Stand',           'Adjustable aluminum laptop stand',               29.99, 100,  1, 'https://placehold.co/300x300?text=LaptopStand'),
('USB-C Hub',              '7-in-1 USB-C hub with HDMI and SD card reader', 49.99,  75,  1, 'https://placehold.co/300x300?text=USBHub'),
('Classic Cotton T-Shirt', '100% cotton classic-fit t-shirt',               19.99, 200,  2, 'https://placehold.co/300x300?text=TShirt'),
('Slim Denim Jeans',       'Slim fit denim jeans in multiple washes',       49.99, 150,  2, 'https://placehold.co/300x300?text=Jeans'),
('Clean Code',             'A handbook of agile software craftsmanship',    34.99,  30,  3, 'https://placehold.co/300x300?text=CleanCode'),
('The Pragmatic Programmer','Your journey to mastery, 20th anniversary ed.',39.99,  25,  3, 'https://placehold.co/300x300?text=PragProg'),
('Yoga Mat',               'Non-slip premium TPE yoga mat, 6mm thick',     24.99,  80,  5, 'https://placehold.co/300x300?text=YogaMat'),
('Resistance Bands Set',   'Set of 5 resistance bands with carry bag',      14.99, 120,  5, 'https://placehold.co/300x300?text=Bands'),
('Ceramic Plant Pot Set',  'Set of 3 hand-glazed ceramic plant pots',       34.99,  60,  4, 'https://placehold.co/300x300?text=PlantPots');
GO
