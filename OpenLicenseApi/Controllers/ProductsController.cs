/* 
GET    /api/products <- lista os produtos do usuário autenticado (com jwt)
GET    /api/products/{id} <- detalhes do produto, só se o produto pertencer ao usuário autenticado (com jwt)

POST   /api/products <- criar um produto para o usuário autenticado (com jwt), deve receber nome e descrição. Retorna os dados do produto criado.

PUT    /api/products/{id} <- atualizar os dados do produto, só se o produto pertencer ao usuário autenticado (com jwt)

DELETE /api/products/{id} <- deletar o produto, só se o produto pertencer ao usuário autenticado (com jwt)

---------

POST   /api/products/{id}/apikey <- criar uma nova chave de API para o produto, só se o produto pertencer ao usuário autenticado (com jwt)
DELETE /api/products/{id}/apikey/{keyId} <- deletar uma chave de API do produto, só se o produto pertencer ao usuário autenticado (com jwt)

Cada produto pertence a um usuario.
Cada chave de api para usar os endpoints de licenças pertence a um produto.
*/