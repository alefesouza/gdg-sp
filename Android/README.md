GDG-SP
=====

Código do aplicativo do GDG-SP para Android.

Caso você queira adaptá-lo para outro Meetup:

- Edite o arquivo app/src/main/res/values/strings.xml com as informações do seu Meetup.
- Escolha um nome de pacote (o desse é org.gdgsp) e mude no arquivo app/src/main/AndroidManifest.xml, você precisará adaptar todos os .java para esse pacote, mudando o "org.gdgsp" na primeira linha de todos os .java
- Mude os ícones principais (procure deixar na mesma resolução, são recomendações do Android).
- Para habilitar as notificações, [leia a documentação do One Signal](https://documentation.onesignal.com/docs/using-onesignal-in-your-android-app)
- Edite o arquivo app/src/main/java/org/gdgsp/fragment/AboutFragment.java como quiser, mas deixe os meus créditos escrevendo "Esse aplicativo foi baseado no aplicativo do GDG-SP desenvolvido por Alefe Souza", com "aplicativo do GDG-SP" com um link para http://github.com/alefesouza/gdg-sp e "Alefe Souza" com um link para http://github.com/alefesouza ou http://alefesouza.com

Você também precisará de um servidor PHP e MySQL para usar o back-end, para mais informações e os arquivos do back-end, [clique aqui](https://github.com/alefesouza/gdg-sp/tree/master/Back-end).

Caso você vá adaptá-lo para outro GDG, sugiro você pedir para um co-organizador te enviar um email e preencher [esse formulário](https://support.google.com/googleplay/android-developer/answer/6320428) antes de enviá-lo para a Play Store, pois o aplicativo poderá ser suspenso (isso aconteceu comigo) caso você não avise o Google Play que você possui permissão para desenvolvover o aplicativo.