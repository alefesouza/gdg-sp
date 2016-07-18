GDG-SP
=====

Código do back-end dos aplicativos do GDG-SP.

Caso você queira adaptá-lo para outro Meetup:

- Você precisará de um servidor com PHP e MySQL.
- Faça upload desses arquivos no seu servidor, e nos aplicativos onde você edita as informações do Meetup coloque a URL de onde você fez upload dos arquivos no "backend_url" ou "BackendUrl", mantendo a / no final.
- Edite o arquivo connect_db.php com as informações do seu banco de dados, no código comentado na linha 28 coloque o ID do seu meetup as chaves do seu aplicativo:
- Para as chaves da API do Meetup, [clique aqui](https://secure.meetup.com/meetup_api/oauth_consumers/) e escolha "Create New Consumer", no campo "Redirect URI" coloque o caminho do arquivo api/login.php, ficando dessa forma: http://{seu back-end}/api/login.php?meetupid={id do seu meetup}
- Para as chaves do OneSignal, [faça uma conta](https://onesignal.com/) no site e crie um aplicativo no painel, após isso clique em "App Settings" na barra lateral e depois em Keys & IDs, para associar os aplicativos, leia a [documentação do OneSignal](https://documentation.onesignal.com).
- Para as chaves do WNS (usado no app para Windows 10), o OneSignal fornece um bom tutorial de como consegui-las, [clique aqui](https://documentation.onesignal.com/docs/windows-phone-client-sid-secret).
- No arquivo functions.php há uma variável $mapbox_token, esse token você consegue no [site do Mapbox](https://www.mapbox.com/studio/signup/).

Para que a notificação de novos meetups seja automática, coloque um cron job direcionado para o arquivo notifications/cron.php de tempos em tempo, no mesmo arquivo, coloque a id do seu meetup no array $meetupids.