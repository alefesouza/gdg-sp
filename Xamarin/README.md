GDG-SP
=====

Código do aplicativo do GDG-SP para Xamarin.Forms.

Apenas as versões para iOS, Windows Phone e Windows 8.1 estão publicadas, porém aqui também há versões para Android e UWP (plataformas que eu domino nativamente logo poderia produzir um aplicativo melhor) para caso alguém queira usar em alguma palestra sobre Xamarin.

Os 5 aplicativos nesse projeto compartilham praticamente o mesmo código, no código das plataformas eu apenas editei para receber informações básicas, como versão do aplicativo e sistema operacional, e o funcionamento das notificações, para usar o OneSignal no Android, iOS e Windows Phone, e notificações nativas no Windows 8.1 e UWP, também editei todos os Windows para usar Live Tiles.

Caso você queira adaptá-lo para outro Meetup:

- Edite o arquivo GDG-SP/GDG_SP/Resx/AppResources.resx com as informações do seu Meetup.
- Mude os ícones principais (procure deixar na mesma resolução, são recomendações do iOS e Windows Phone).
- Para habilitar as notificações, leia a documentação do One Signal [para Xamarin.iOS](https://documentation.onesignal.com/docs/using-onesignal-in-your-xamarin-sdk-app) e para [Windows Phone](https://documentation.onesignal.com/docs/windows-phone-native-sdk-overview).
- Edite o arquivo GDG-SP/GDG_SP/AboutPage.xaml como quiser, mas deixe os meus créditos escrevendo "Esse aplicativo foi baseado no aplicativo do GDG-SP desenvolvido por Alefe Souza", com "aplicativo do GDG-SP" com um link para http://github.com/alefesouza/gdg-sp e "Alefe Souza" com um link para http://github.com/alefesouza ou http://alefesouza.com

Você também precisará de um servidor PHP e MySQL para usar o back-end, para mais informações e os arquivos do back-end, [clique aqui](https://github.com/alefesouza/gdg-sp/tree/master/Back-end).