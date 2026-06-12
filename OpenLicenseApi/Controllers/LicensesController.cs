/* 
GET    /api/licenses <- lista as licenças do produto do usuário autenticado (com chave de api)

GET    /api/licenses/{id} <- detalhes da licença, só se a licença pertencer a um produto do usuário autenticado (com chave de api)

POST   /api/licenses <- criar uma licença para um produto do usuário autenticado (com chave de api)

PUT    /api/licenses/{id} <- atualizar os dados da licença, só se a licença pertencer a um produto do usuário autenticado (com chave de api)

DELETE /api/licenses/{id} <- deletar a licença, só se a licença pertencer a um produto do usuário autenticado (com chave de api)

PATCH /api/licenses/{id}/status <- atualizar o status da licença, podendo ser active ou suspended, só se a licença pertencer a um produto do usuário autenticado (com chave de api)

-------------

POST /api/licenses/validate <- validar se a licença é válida, ativa e pertence ao produto, deve receber a chave de api do produto o código da licença e o hardware id do cliente. 
Esse endpoint será usado na implementação principal do software do cliente, deve validar se:
ApiKey existe?, ApiKey ativa? Não? Retornar erro de chave de api inválida ou inativa.
Licença existe?, Pertence ao produto da ApiKey? Não? Retornar erro de licença inválida.
Licença ativa? Não? Retornar erro de licença inativa.
Existe ativação com esse HardwareId? Sim, Atualiza LastSeen, Retorna válido.
Conta quantas ativações existem. Se for menor que o limite de ativações da licença, Cria nova ativação com esse HardwareId, Retorna válido.
Se for igual ou maior que o limite de ativações da licença, Retorna erro de limite.

POST /api/licenses/deactivate <- desativar uma licença, deve receber a chave de api do produto ou jwt, o código da licença e o hardware id do cliente.

POST /api/licenses/ping <- verifica se a api está online.

Cada licença pertence a um produto.
Padrão da licença: 45AH-4HJY-97MR-2O80 -> 4 blocos de 4 caracteres alfanuméricos separados por hífen.
Esses endpoints devem usar obrigatoriamente a chave de api do produto para autenticação. X-Api-Key: xxx
Esses endpoints são usados pelo software do cliente para validar e ativar a licença, serão os unicos a serem exibidos publicamente na ScalarUI.
Os outros endpoints de outras Contrllers são de uso interno do dashboard, para o usuário gerenciar seus produtos e licenças, e devem usar a autenticação JWT do usuário do dashboard.
*/