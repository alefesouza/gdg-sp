GDG-SP
=====

Código do back-end dos aplicativos do GDG-SP.

Caso você queira adaptá-lo para outro Meetup:

- Você precisará de um servidor com PHP e MySQL.
- Faça upload desses arquivos no seu servidor, e nos aplicativos onde você edita as informações do Meetup coloque a URL de onde você fez upload dos arquivos no "backend_url" ou "BackendUrl", mantendo a / no final.
- Edite a classe GDGSP/Database/DB.php e o arquivo cli/config.php com as informações do seu banco de dados, no arquivo cli/config.php insira também na linha 38 o ID do Meetup junto com as chaves do aplicativo:
- Para as chaves da API do Meetup, [clique aqui](https://secure.meetup.com/meetup_api/oauth_consumers/) e escolha "Create New Consumer", no campo "Redirect URI" coloque o caminho do arquivo api/login.php, ficando dessa forma: http://{seu back-end}/api/login.php?meetupid={ID do seu meetup}
- Para as chaves do OneSignal, [faça uma conta](https://onesignal.com/) no site e crie um aplicativo no painel, após isso clique em "App Settings" na barra lateral e depois em Keys & IDs, para associar os aplicativos, leia a [documentação do OneSignal](https://documentation.onesignal.com).
- Para as chaves do WNS (usado no app para Windows 10), o OneSignal fornece um bom tutorial de como consegui-las, [clique aqui](https://documentation.onesignal.com/docs/windows-phone-client-sid-secret).
- Na class GDGSP/Util/Utils.php há uma constante mapBoxToken, esse token você consegue no [site do Mapbox](https://www.mapbox.com/studio/signup/), também há outra variável $qrCode, nela você coloca o valor padrão dos QR Codes gerados, a ID do usuário será adicionada ao final desse valor e gerará o QR Code.
- Na classe GDGSP/API/MeetupAPI.php também há um array $extra_members no método checkIsAdmin, nele você pode colocar o ID do Meetup de outros usuários para poderem enviar notificações sem ser um organizador do Meetup, o meu ID está como exemplo.

Para que a notificação de novos meetups seja automática, coloque um cron job direcionado para o arquivo notifications/cron.php de tempo em tempo, no mesmo arquivo, coloque a ID do seu meetup na variável $meetup_ids.

No back-end foi utilizado:

- [Leaflet](http://leafletjs.com)
- [Fonte Roboto](https://www.google.com/fonts/specimen/Roboto)
- [goQR.me](http://goqr.me/api/)

Agradecimentos
-----

O back-end do aplicativo era em [PHP 5.5 puro](https://github.com/alefesouza/gdg-sp/tree/49da0408e62810c7d2996b9783b389ce89e02b68/Back-end), mas após assistir as palestras [Modernizando a Arquitetura de sua Aplicação](https://www.youtube.com/watch?v=8Oc22yq8O_I) de [Antonio Spinelli](https://github.com/tonicospinelli) e [O PHP7 chegou. E isso é só o começo!](https://www.youtube.com/watch?v=r260ueWFq2M&t=2045s) de [Anderson Casimiro](https://github.com/duodraco) do [PHP-SP](https://github.com/PHPSP) eu decidi reescrever o back-end e deixar em PHP 7 orientado a objetos.