/* 
POST   /api/auth/register <- registrar um novo usuário, deve receber nome, email e senha. Retorna os dados do usuário criado ou um erro se o email já estiver em uso.
POST   /api/auth/login <- autenticar um usuário, deve receber email e senha. Retorna um JWT se as credenciais forem válidas ou um erro se forem inválidas.
POST   /api/auth/logout <- encerrar a sessão do usuário, invalida o JWT.

GET    /api/auth/me <- retorna os dados do usuário autenticado.

PUT    /api/auth <- atualizar os dados do usuário autenticado, deve receber nome, email e/ou senha. Retorna os dados atualizados do usuário ou um erro se o email já estiver em uso por outro usuário.
DELETE /api/auth <- deletar o usuário autenticado.

JWT do usuário do dashboard.
*/