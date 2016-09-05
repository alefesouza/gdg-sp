GDG-SP
=====

Código do programa de check-in do GDG-SP utilizando os aplicativos.

Esse programa gera uma base de dados local com informações da API do Meetup, e conforme um usuário passa o QR Code gerado pelos apps na WebCam, o programa detecta o usuário e registra o horário que ele chegou, para assim manter um relatório de quem veio ao evento, há a opção de enviar quem faltou para o servidor e fazer uma contagem para detectar quem confirma mas não comparece nos eventos.

Ele também imprime etiquetas usando o SDK do Dymo Label, podendo conter o nome, código e momento em que a pessoa chegou assim que ela passar o QR Code na WebCam.

As funções principais não dependem de conexão de internet, desde que a base de dados já tenha sido baixada anteriormente e os usuários já tenham feito login no aplicativo.

Requisitos para execução: .NET Framework 4.5

Caso você queira adaptá-lo para outro Meetup:

- Edite o arquivo GDGSPCheckin/App.cs com as informações do seu Meetup.
- Mude o ícone principal.
- Mantenha os créditos no rodapé do arquivo GDGSPCheckin/MainPage.xaml, algo como "Baseado no GDG-SP Check-in desenvolvido por Alefe Souza".

Você também precisará de um servidor PHP e MySQL para usar o back-end (apenas para registrar e exibir pessoas com faltas), para mais informações e os arquivos do back-end, [clique aqui](../Back-end).

Veja também o outro software de check-in do GDG-SP, o [self-checkin](https://github.com/gdg-sp/self-checkin) desenvolvido por [Luis Leão](https://github.com/luisleao).