<?php
/*
 * Copyright (C) 2017 Alefe Souza <contact@alefesouza.com>
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

// Página que contém links para download nas lojas de aplicativos, eu recomendo utilizar um encurtador de links, ficando http://bit.ly/gdgsp por exemplo.
$app_name = "GDG-SP";

$isandroid = strpos($_SERVER['HTTP_USER_AGENT'],'Android');
$iswp = strpos($_SERVER['HTTP_USER_AGENT'],'Windows Phone');

$isios = (strpos($_SERVER['HTTP_USER_AGENT'],'iPhone') || strpos($_SERVER['HTTP_USER_AGENT'],'iPad') || strpos($_SERVER['HTTP_USER_AGENT'],'iPod')) && !(strpos($_SERVER['HTTP_USER_AGENT'],'like iPhone') || strpos($_SERVER['HTTP_USER_AGENT'],'like iPad') || strpos($_SERVER['HTTP_USER_AGENT'],'like iPod'));

$android_package = "org.gdgsp";

$itunes_name = "gdg-sp";
$itunes_id = "1135565491";

$windows_id = "9nblggh4xcj7";

$android_uri = "market://details?id=$android_package";
$ios_uri = "itmss://itunes.apple.com/app/$itunes_name/id$itunes_id";
$windows_uri = "ms-windows-store://pdp/?ProductId=$windows_id";

$android_url = "https://play.google.com/store/apps/details?id=$android_package";
$ios_url = "https://itunes.apple.com/app/$itunes_name/id$itunes_id";
$windows_url = "https://www.microsoft.com/store/apps/$windows_id";

if($isandroid) {
  $open = $android_uri;
} else if($isios) {
  $open = $ios_uri;
} else if($iswp) {
  $open = $windows_uri;
} ?>
<html>
  <head>
    <title><?php echo $app_name; ?> - Download do aplicativo</title>
    <link rel="shortcut icon" href="../images/favicon.png">
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no" />
    <style>
      body {
        background-color: #ffffff;
        overflow-x: hidden;
      }
      
      a {
        color: #ffffff;
        text-decoration: none;
      }

      #main {
        position: absolute;
        top: 50%;
        left: 50%;
        transform: translate(-50%, -50%);
      }

      #fone {
        float: left;
      }

      table {
        margin-top: 25px;
      }
      
      #table-desktop {
        display: none;
      }
      
      #table-mobile {
        display: table;
      }
      
      @media(max-width: 1040px) {
        #main {
          top: 50%;
          left: 5%;
          transform: translate(0%, -50%);
          width: 90%;
        }
        
        #logo {
          width: 100%;
        }
      }
      
      @media(min-width: 768px) {
        #table-desktop {
          display: table;
        }

        #table-mobile {
          display: none;
        }
      }
      
      td {
        text-align: center;
      }
    </style>
  </head>
  <body>
    <div id="main">
      <img src="../images/store_page.png" id="logo">
      
      <table align="center" id="table-desktop">
        <tr>
          <td>
            <a href="<?php echo $ios_url; ?>?mt=8" style="display:inline-block;overflow:hidden;background:url(../images/Download_on_the_App_Store_Badge_PTBR_135x40.svg) no-repeat;width:135px;height:40px;"></a>
          </td>
          <td>
            <a href="<?php echo $android_url; ?>&utm_source=global_co&utm_medium=prtnr&utm_content=Mar2515&utm_campaign=PartBadge&pcampaignid=MKT-Other-global-all-co-prtnr-py-PartBadge-Mar2515-1"><img alt="Disponível no Google Play" src="https://play.google.com/intl/en_us/badges/images/generic/pt-br_badge_web_generic.png" style="width: 157px;" /></a>
          </td>
          <td>
            <a href="<?php echo $windows_url; ?>?ocid=badge"><img src="https://assets.windowsphone.com/9f16ef3b-7540-40b2-977c-3d20a4c22060/Portuguese-Brazilian_get-it-from-MS_InvariantCulture_Default.png" alt="Baixe da Microsoft" style="width: 138px;" /></a>
          </td>
        </tr>
      </table>
      
      <table align="center" id="table-mobile">
        <tr>
          <td>
            <a href="<?php echo $ios_url; ?>?mt=8" style="display:inline-block;overflow:hidden;background:url(../images/Download_on_the_App_Store_Badge_PTBR_135x40.svg) no-repeat;width:135px;height:40px;"></a>
          </td>
        </tr>
        <tr>
          <td>
            <a href="<?php echo $android_url; ?>&utm_source=global_co&utm_medium=prtnr&utm_content=Mar2515&utm_campaign=PartBadge&pcampaignid=MKT-Other-global-all-co-prtnr-py-PartBadge-Mar2515-1"><img alt="Disponível no Google Play" src="https://play.google.com/intl/en_us/badges/images/generic/pt-br_badge_web_generic.png" style="width: 157px;" /></a>
          </td>
        </tr>
        <tr>
          <td>
            <a href="<?php echo $windows_url; ?>?ocid=badge"><img src="https://assets.windowsphone.com/9f16ef3b-7540-40b2-977c-3d20a4c22060/Portuguese-Brazilian_get-it-from-MS_InvariantCulture_Default.png" alt="Baixe da Microsoft" style="width: 138px;" /></a>
          </td>
        </tr>
      </table>
    </div>

    <?php if($open != "") { ?>
    <script>
    window.onload = function() {
      window.location.href = "<?php echo $open; ?>";
    }
    </script>
    <?php } ?>
  </body>
</html>