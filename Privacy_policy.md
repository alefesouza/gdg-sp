GDG-SP
=====

Política de privacidade
-----

Algumas funções do aplicativo requerem conexão com a conta do Meetup do usuário, durante todo o processo de login, nenhum dado como login e senha é armazenado pelo aplicativo, pois ele apenas abre uma WebView que carrega a página de login do Meetup, após isso, a API do Meetup retorna um token que é salvo pelo aplicativo e fica apenas no dispositivo, esse token é usado para retornar as informações do usuário após ele permitir acesso do aplicativo a conta dele.

Nesse processo o ID do Meetup do usuário é salvo em um banco de dados para poder realizar sorteios apenas para quem utiliza o aplicativo e enviar notificações direcionadas para quem vai a um determinado evento.

Você pode acompanhar como tudo isso funciona navegando nesse repositório do GitHub, a única coisa que aqui que se difere do código-fonte dos aplicativos nas lojas são as keys utilizadas para funcionamento do One Signal, pois divulgar isso atrapalharia nas estatísticas do aplicativo, e no back-end, as credenciais para conexão com o meu banco de dados.