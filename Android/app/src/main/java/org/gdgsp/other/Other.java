/*
 * Copyright (C) 2016 Alefe Souza <http://alefesouza.com>
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

package org.gdgsp.other;

import android.app.Activity;
import android.app.PendingIntent;
import android.content.ActivityNotFoundException;
import android.content.ClipData;
import android.content.ClipboardManager;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager.NameNotFoundException;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.net.Uri;
import android.os.Build;
import android.preference.PreferenceManager;
import android.support.customtabs.CustomTabsIntent;
import android.support.v7.app.AlertDialog;
import android.view.LayoutInflater;
import android.view.View;
import android.widget.Toast;
import org.gdgsp.R;
import org.gdgsp.lib.*;

/**
 * Aqui ficam métodos e atributos utilizados em várias classes do aplicativo.
 */
public class Other extends Activity {
    /**
     * Cor primária do aplicativo, usada como cor da toolbar da Custom Tab.
     */
	public static final int colorPrimary = 0xff008bf8;

	/**
	 * String que tem que aparecer no final de todas as URLs no aplicativo.
	 */
	public static String getFinalUrl(Context context) {
		return "appversion=" + getAppVersion(context) + "&platform=android&sdkint=" + getOSVersion();
	}

    /**
     * Métodos que retorna a URL de receber os eventos.
     * @param context Contexto usado para receber uma string no string.xml
     * @return A URL final para receber os eventos.
     */
    public static String getEventsUrl(Context context) {
		return "http://" + context.getString(R.string.backend_url) + "api/events.php?meetupid=" + context.getString(R.string.meetup_id) + "&" + getFinalUrl(context);
    }

    /**
     * Método que retorna a URL para fazer RSVP.
     * @param context Contexto usado para receber uma string no string.xml
     * @param id O id do evento que o usuário está fazendo RSVP.
     * @return A URL final para fazer RSVP no evento que o usuário quer ir.
     */
	public static String getRSVPUrl(Context context, int id) {
        return "http://" + context.getString(R.string.backend_url) + "api/rsvp.php?meetupid=" + context.getString(R.string.meetup_id) + "&eventid=" + id + "&" + getFinalUrl(context);
    }

    /**
     * Método que retorna a URL das pessoas que vão para o evento.
     * @param context Contexto usado para receber uma string no string.xml
     * @param id O id do evento.
     * @return A URL final para receber as respostas das pessoas sobre o evento.
     */
    public static String getRSVPSUrl(Context context, int id) {
        return "http://" + context.getString(R.string.backend_url) + "api/people.php?meetupid=" + context.getString(R.string.meetup_id) + "&eventid=" + id + "&" + getFinalUrl(context);
    }

    /**
     * Método que retorna a URL para fazer login.
     */
	public static String getLoginUrl(Context context) {
		return "http://" + context.getString(R.string.backend_url) + "api/login.php?meetupid=" + context.getString(R.string.meetup_id);
	}

	/**
	 * Método que retorna a URL para o administrador enviar notificação.
	 */
	public static String getNotificationUrl(Context context) {
		return "http://" + context.getString(R.string.backend_url) + "notifications/send.php?meetupid=" + context.getString(R.string.meetup_id);
	}

    /**
     * Método que retorna o refresh token armazenado nas configurações.
     * @param context Contexto usando para receber a configuração.
     */
	public static String getRefreshToken(Context context) {
		SharedPreferences preferences = PreferenceManager.getDefaultSharedPreferences(context);

		return preferences.getString("refresh_token", "");
	}

    /**
     * Método que abre um link em uma Custom Tab
     * @param context Contexto usado
     * @param url URL a ser aberta
     */
	public static void openSite(Context context, String url) {
		url = url.startsWith("http") ? url : "http://" + url;
		
		CustomTabsIntent.Builder intentBuilder = new CustomTabsIntent.Builder();
		intentBuilder.setToolbarColor(colorPrimary);
		intentBuilder.setShowTitle(true);

		String shareLabel = context.getString(R.string.share);
		Bitmap icon = BitmapFactory.decodeResource(context.getResources(), R.drawable.ic_share);

		Intent actionIntent = new Intent(context.getApplicationContext(), ShareBroadcastReceiver.class);

		PendingIntent pendingIntent = PendingIntent.getBroadcast(context, 0, actionIntent, 0);
		intentBuilder.setActionButton(icon, shareLabel, pendingIntent);

		CustomTabActivityHelper.openCustomTab((Activity)context, intentBuilder.build(), Uri.parse(url), new WebviewFallback());
	}

    /**
     * Tenta abrir o app do Meetup com uma url, se não der certo usa o método acima.
     * @param context Contexto usado.
     * @param url URL a ser aberta.
     */
	public static void openMeetupApp(Context context, String url) {
		try {
			Intent intent = new Intent(Intent.ACTION_VIEW);
			intent.setPackage("com.meetup");
			intent.setData(Uri.parse(url));
			context.startActivity(intent);
		} catch (ActivityNotFoundException e) {
			openSite(context, url);
		}
	}

    /**
     * Retorna o SDKINT do Android
     */
	public static int getOSVersion() {
		return Build.VERSION.SDK_INT;
	}

    /**
     * Retorna a versão do aplicativo
     */
	public static String getAppVersion(Context context) {
		try {
			PackageInfo pInfo = context.getPackageManager().getPackageInfo(context.getPackageName(), 0);
			return pInfo.versionName;
		} catch (NameNotFoundException e) {}
		return null;
	}

    /**
     * Exibe uma caixa de mensagem simples.
     * @param context Contexto usado.
     * @param title Título da mensagem.
     * @param description Descrição da mensagem.
     */
	public static void showMessage(Context context, String title, String description) {
		final AlertDialog alertDialog = new AlertDialog.Builder(context)
			.setTitle(title)
			.setMessage(description)
			.setPositiveButton("OK", null)
			.create();
			
		alertDialog.show();
	}

    /**
     * Exibe um toast.
     * @param context Contexto usado.
     * @param message Mensagem do toast.
     */
	public static void showToast(Context context, String message) {
		Toast.makeText(context, message, Toast.LENGTH_LONG).show();
	}

    /**
     * Método que executa ao pressionar um card da tela inicial.
     * @param context Contexto usado.
     * @param title Título do evento.
     * @param url URL do evento.
     */
	public static void cardDialog(final Context context, final String title, final String url) {
		final String items[] ={"Compartilhar", "Copiar link", "Abrir no navegador"};
        AlertDialog.Builder alertDialog = new AlertDialog.Builder(context);
        LayoutInflater inflater = (LayoutInflater)context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
        View convertView = inflater.inflate(R.layout.list_pressed, null);
        alertDialog.setView(convertView);
        alertDialog.setTitle(title);

		alertDialog.setItems(items, new DialogInterface.OnClickListener() {
				public void onClick(DialogInterface dialog, int item) {
					switch(item) {
						case 0:
							Intent shareIntent = new Intent(Intent.ACTION_SEND);
							shareIntent.setType("text/plain");
							shareIntent.putExtra(Intent.EXTRA_TEXT, title + " " + url);

							Intent chooserIntent = Intent.createChooser(shareIntent, context.getString(R.string.share));
							chooserIntent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
							
							context.startActivity(chooserIntent);
							break;
						case 1:
                            if(Build.VERSION.SDK_INT <= 10) {
                                android.text.ClipboardManager clipboard = (android.text.ClipboardManager) context.getSystemService(Context.CLIPBOARD_SERVICE);
                                clipboard.setText(url);
                            } else {
                                ClipboardManager clipboardManager = (ClipboardManager)context.getSystemService(Context.CLIPBOARD_SERVICE);
                                ClipData clipData = ClipData.newPlainText(url, url);
                                clipboardManager.setPrimaryClip(clipData);
                            }
							showToast(context, context.getString(R.string.link_copyed));
							break;
						case 2:
							Intent intent = new Intent(Intent.ACTION_VIEW);
							intent.setData(Uri.parse(url));
							context.startActivity(intent);
							break;
					}
				}
			});

		alertDialog.create();
        alertDialog.show();
	}

    /**
     * Retorna se o aplicativo está sendo executado em um tablet
     */
	public static boolean isTablet(Context context) {
		return context.getResources().getBoolean(R.bool.isTablet);
	}

    /**
     * Método que retorna a um parâmetro de uma URL.
     * @param url URL para extrair o parâmetro.
     * @param want Parâmetro a ser extraído.
     * @return
     */
	public static String getQuery(String url, String want) {
		String[] a = url.split("\\?");
		String[] b = a[1].split("&");
		for(String c : b) {
			if(c.split("=")[0].equals(want)) {
				return c.split("=")[1];
			}
		}
		return "";
	}
}
