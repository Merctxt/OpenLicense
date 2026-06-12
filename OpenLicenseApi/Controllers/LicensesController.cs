/* 
GET    /api/licenses <- lista as licenças do produto do usuário autenticado (com chave de api)
GET    /api/licenses/{id} <- detalhes da licença, só se a licença pertencer a um produto do usuário autenticado (com chave de api)

POST   /api/licenses <- criar uma licença para um produto do usuário autenticado (com chave de api)

PUT    /api/licenses/{id} <- atualizar os dados da licença, só se a licença pertencer a um produto do usuário autenticado (com chave de api)

DELETE /api/licenses/{id} <- deletar a licença, só se a licença pertencer a um produto do usuário autenticado (com chave de api)

PATCH /api/licenses/{id}/status <- atualizar o status da licença, podendo ser active ou suspended, só se a licença pertencer a um produto do usuário autenticado (com chave de api)

-------------

POST /api/licenses/validate <- validar se a licença é válida, ativa e pertence ao produto, deve receber a chave de api do produto e o código da licença. Retorna os dados da licença se for válida ou um erro se for inválida.
POST /api/licenses/activate <- ativa a licença no computador do cliente, gravando o hardware id do cliente, só se a licença for válida e ativa. Deve receber a chave de api do produto, o código da licença e o hardware id do cliente. Retorna os dados da licença se for ativada com sucesso ou um erro se for inválida ou já estiver ativa em outro hardware.
POST /api/licenses/ping <- verifica se a api está online.

Cada licença pertence a um produto.
Esses endpoints devem usar obrigatoriamente a chave de api do produto para autenticação. X-Api-Key: xxx
Esses endpoints são usados pelo software do cliente para validar e ativar a licença, serão os unicos a serem exibidos publicamente na ScalarUI.
Os outros endpoints de outras Contrllers são de uso interno do dashboard, para o usuário gerenciar seus produtos e licenças, e devem usar a autenticação JWT do usuário do dashboard.
*/