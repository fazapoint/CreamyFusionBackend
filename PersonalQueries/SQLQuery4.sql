select p.id as productid,p.name,  price, validto, pp.id as productpricesid,  p.deleted as pdeleted, pp.deleted as ppdeleted, * from Products p 
join ProductPrices pp on p.id = pp.ProductId
--where p.id = '00F41002-6DE0-4963-8D1E-690D1708E9C7'
order by p.id, pp.validto

select * from products
select * from ProductPrices order by productid

update products set deleted = 0

delete from Products where id in (
'DD07B146-88FA-4C38-8C12-86BF4FA47E4B',
'121400F4-B41B-4395-8F96-D6A1F25C4C19'
)

--delete from products
--delete from ProductPrices