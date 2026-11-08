-- 1) Look at the sections first and pick the one the products page belongs to.
SELECT c.Id, c.Name, c.CategoryType,
       (SELECT COUNT(*) FROM SubCategories s WHERE s.CategoryId = c.Id) AS SubCount
FROM Categories c ORDER BY c.Id;

-- 2) Put the Id of the products section here (NULL = the first section whose name has "منتجات").
DECLARE @CategoryId INT = NULL;

IF @CategoryId IS NULL
    SELECT TOP 1 @CategoryId = Id FROM Categories WHERE Name LIKE N'%منتجات%' ORDER BY Id;

IF @CategoryId IS NULL
    PRINT N'Set @CategoryId to the Id of the products section.';
ELSE IF NOT EXISTS (SELECT 1 FROM SubCategories WHERE CategoryId = @CategoryId AND Name LIKE N'%عقار%')
BEGIN
    INSERT INTO SubCategories (Name, CategoryId) VALUES (N'عقار', @CategoryId);
    PRINT N'Added the sub-section "عقار".';
END
ELSE PRINT N'"عقار" already exists in this section.';
